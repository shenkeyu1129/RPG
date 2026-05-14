using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 存档管理器单例
/// JSON 序列化，支持自动存档和手动存档
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("存档设置")]
    [SerializeField] private bool autoSaveOnDayChange = true;

    // 存档槽位由 AccountManager 管理，此处为 fallback
    private int _currentSlotIndex = 0;
    public int CurrentSlotIndex
    {
        get
        {
            if (AccountManager.Instance != null)
                return AccountManager.CurrentSlotIndex;
            return _currentSlotIndex;
        }
        set { _currentSlotIndex = value; }
    }

    private string SavePath => $"{Application.persistentDataPath}/save_{CurrentSlotIndex}.json";

    /// <summary>
    /// 设置当前存档槽位（由 MainMenuPanel 调用）
    /// </summary>
    public void SetSlotIndex(int index)
    {
        _currentSlotIndex = index;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (autoSaveOnDayChange)
            TimeManager.OnDayChanged += AutoSave;
    }

    private void OnDisable()
    {
        if (autoSaveOnDayChange)
            TimeManager.OnDayChanged -= AutoSave;
    }

    private void AutoSave()
    {
        Debug.Log("自动存档中...");
        SaveGame(CurrentSlotIndex);
    }

    /// <summary>
    /// 保存游戏到指定槽位
    /// </summary>
    public void SaveGame(int slotIndex)
    {
        CurrentSlotIndex = slotIndex;
        var data = CollectSaveData();
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"存档完成 -> {SavePath} (金币:{data.gold}, 工具栏物品:{data.toolbarSlots.Count}, 背包物品:{data.inventorySlots.Count})");
    }

    /// <summary>
    /// 从指定槽位读取存档
    /// </summary>
    public void LoadGame(int slotIndex)
    {
        CurrentSlotIndex = slotIndex;
        string path = SavePath;
        Debug.Log($"读档路径: {path}");
        if (!File.Exists(path))
        {
            Debug.LogWarning("没有找到存档文件");
            return;
        }

        string json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<SaveData>(json);
        if (data == null)
        {
            Debug.LogError("存档文件损坏");
            return;
        }

        ApplySaveData(data);
        Debug.Log("读档完成");
    }

    /// <summary>
    /// 检查指定槽位是否有存档
    /// </summary>
    public bool HasSaveData(int slotIndex)
    {
        string path = $"{Application.persistentDataPath}/save_{slotIndex}.json";
        return File.Exists(path);
    }

    /// <summary>
    /// 获取存档中的玩家名称
    /// </summary>
    public string GetPlayerName()
    {
        string path = SavePath;
        if (!File.Exists(path)) return "";
        string json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<SaveData>(json);
        return data?.playerName ?? "";
    }

    /// <summary>
    /// 收集所有当前游戏状态
    /// </summary>
    private SaveData CollectSaveData()
    {
        var data = new SaveData();

        data.playerName = MainMenuPanel.PlayerName;

        // 玩家位置
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerPosX = player.transform.position.x;
            data.playerPosY = player.transform.position.y;
            data.playerPosZ = player.transform.position.z;
            data.playerRotY = player.transform.eulerAngles.y;
        }

        // 时间
        if (TimeManager.Instance != null)
        {
            data.year = TimeManager.Instance.CurrentYear;
            data.season = (int)TimeManager.Instance.CurrentSeason;
            data.day = TimeManager.Instance.CurrentDay;
            data.hour = TimeManager.Instance.CurrentHour;
            data.minute = TimeManager.Instance.CurrentMinute;
        }

        // 金币
        if (Wallet.Instance != null)
            data.gold = Wallet.Instance.CurrentGold;

        // 工具栏
        if (ToolBarManager.Instance != null)
        {
            foreach (var slot in ToolBarManager.Instance.allToolSlots)
            {
                data.toolbarSlots.Add(new SlotSaveData
                {
                    itemID = slot.currentItemData != null ? slot.currentItemData.itemID : -1,
                    count = slot.currentItemCount
                });
            }
        }

        // 背包
        if (InventoryManager.Instance != null)
        {
            foreach (var slot in InventoryManager.Instance.Slots)
            {
                data.inventorySlots.Add(new SlotSaveData
                {
                    itemID = slot.itemData != null ? slot.itemData.itemID : -1,
                    count = slot.count
                });
            }
        }

        // 农田状态
        var farmLands = FindObjectsByType<FarmLand>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var fl in farmLands)
        {
            data.farmLands.Add(new FarmLandSaveData
            {
                posX = fl.transform.position.x,
                posY = fl.transform.position.y,
                posZ = fl.transform.position.z,
                status = (int)fl.GetCurrentStatus()
            });
        }

        return data;
    }

    /// <summary>
    /// 恢复所有游戏状态
    /// </summary>
    private void ApplySaveData(SaveData data)
    {
        // 玩家位置（先禁用 CharacterController 再设置，否则位置会被覆盖）
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
            player.transform.eulerAngles = new Vector3(0, data.playerRotY, 0);
            if (cc != null) cc.enabled = true;
        }

        // 金币（确保 Wallet 存在）
        // ⚠ 注意：时间恢复会触发 OnDayChanged → AutoSave，必须先恢复其他数据再恢复时间
        if (Wallet.Instance == null)
        {
            var walletGo = new GameObject("Wallet");
            walletGo.AddComponent<Wallet>();
        }
        Wallet.Instance.SetGold(data.gold);
        Debug.Log($"读档金币: {data.gold}");

        // 工具栏（需要 ItemData 查找表）
        if (ToolBarManager.Instance != null)
        {
            Debug.Log($"读档工具栏: {data.toolbarSlots.Count} 个槽位, allItemDatas={(allItemDatas != null ? allItemDatas.Length : 0)} 个");
            for (int i = 0; i < ToolBarManager.Instance.allToolSlots.Length && i < data.toolbarSlots.Count; i++)
            {
                var slotData = data.toolbarSlots[i];
                if (slotData.itemID > 0)
                {
                    var item = FindItemByID(slotData.itemID);
                    if (item == null) Debug.LogWarning($"找不到 itemID={slotData.itemID} 的物品，请检查 SaveManager.allItemDatas");
                    ToolBarManager.Instance.allToolSlots[i].currentItemData = item;
                    ToolBarManager.Instance.allToolSlots[i].currentItemCount = slotData.count;
                }
                else
                {
                    ToolBarManager.Instance.allToolSlots[i].currentItemData = null;
                    ToolBarManager.Instance.allToolSlots[i].currentItemCount = 0;
                }
                ToolBarManager.Instance.allToolSlots[i].RefreshSlot();
            }
        }

        // 背包（确保 InventoryManager 存在）
        InventoryManager.EnsureExists();
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearAll();
            // 直接遍历存档数据（动态背包初始 Slots 为空）
            foreach (var slotData in data.inventorySlots)
            {
                if (slotData.itemID > 0 && slotData.count > 0)
                {
                    var item = FindItemByID(slotData.itemID);
                    if (item != null)
                        InventoryManager.Instance.AddItem(item, slotData.count);
                }
            }
        }

        // 农田状态
        var farmLands = FindObjectsByType<FarmLand>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var fl in farmLands)
        {
            foreach (var flData in data.farmLands)
            {
                if (Vector3.Distance(fl.transform.position,
                    new Vector3(flData.posX, flData.posY, flData.posZ)) < 0.1f)
                {
                    fl.SetCurrentStatus((FarmCurrentStatus)flData.status);
                    break;
                }
            }
        }

        // 时间（放在最后，因为 RestoreTime 会触发 OnDayChanged → AutoSave）
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.RestoreTime(
                data.year,
                (TimeManager.Season)data.season,
                data.day,
                data.hour,
                data.minute
            );
        }
    }

    /// <summary>
    /// 通过 itemID 查找 ItemData 资源
    /// 需要在 Inspector 中配置所有物品的引用
    /// </summary>
    [Header("物品查找表")]
    [SerializeField] private ItemData[] allItemDatas;

    private ItemData FindItemByID(int id)
    {
        if (allItemDatas == null) return null;
        foreach (var item in allItemDatas)
        {
            if (item != null && item.itemID == id)
                return item;
        }
        return null;
    }
}
