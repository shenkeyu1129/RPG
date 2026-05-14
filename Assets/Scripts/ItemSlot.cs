using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通用槽位组件（工具栏 + 背包共用）
/// isInventoryMode = false → 工具栏模式（装备/种植）
/// isInventoryMode = true  → 背包模式（点击转移到工具栏）
/// </summary>
public class ItemSlot : MonoBehaviour
{
    [Header("UI组件引用")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Text countText;
    [SerializeField] private Image slotBackground;
    [SerializeField] private GameObject borderHighlight; // 选中边框

    [Header("当前槽位数据")]
    public ItemData currentItemData;
    public int currentItemCount;

    [Header("背包模式（由 InventoryPanel 设置）")]
    public bool isInventoryMode;
    [System.NonSerialized] public int inventoryIndex = -1;

    void Awake()
    {
        currentItemData = null;
        currentItemCount = 0;
    }

    void OnEnable()
    {
        if (!isInventoryMode)
            PlayerEvents.Center.AddListener<GameObject>(PlayerEvent.UseItem, OnSlotClick);
    }
    // 刷新槽位UI（核心方法，物品变化时调用）
    public void RefreshSlot()
    {
        // 有物品的情况
        if (currentItemData != null && currentItemCount > 0)
        {
            itemIcon.sprite = currentItemData.itemIcon; // 赋值图标
            itemIcon.enabled = true; // 显示图标

            // 堆叠数量大于1才显示文本
            countText.text = currentItemCount > 1 ? currentItemCount.ToString() : "";
            countText.gameObject.SetActive(currentItemCount > 1);
        }
        // 空槽位的情况
        else
        {
            ClearSlot();
        }
    }

    /// <summary>
    /// 初始化为背包格子（由 InventoryPanel 调用）
    /// </summary>
    public void SetAsInventorySlot(int index)
    {
        isInventoryMode = true;
        inventoryIndex = index;
        // 背包格子不需要监听 UseItem 事件
    }

    /// <summary>
    /// 刷新背包格子的显示
    /// </summary>
    public void RefreshInventorySlot(InventorySlotData data)
    {
        if (data != null && data.itemData != null && data.count > 0)
        {
            currentItemData = data.itemData;
            currentItemCount = data.count;
            if (itemIcon != null)
            {
                itemIcon.sprite = data.itemData.itemIcon;
                itemIcon.enabled = true;
            }
            if (countText != null)
            {
                countText.text = data.count > 1 ? data.count.ToString() : "";
                countText.gameObject.SetActive(data.count > 1);
            }
        }
        else
        {
            currentItemData = null;
            currentItemCount = 0;
            if (itemIcon != null) itemIcon.enabled = false;
            if (countText != null) countText.gameObject.SetActive(false);
        }
    }

    // 清空槽位
    public void ClearSlot()
    {
        currentItemData = null;
        currentItemCount = 0;

        itemIcon.sprite = null;
        itemIcon.enabled = false;
        countText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 设置选中高亮（选中时显示 Border，否则隐藏）
    /// </summary>
    public void SetHighlight(bool isSelected)
    {
        if (borderHighlight != null)
            borderHighlight.SetActive(isSelected);
    }

    // 槽位点击事件
    public void OnSlotClick(GameObject obj)
    {
        if (currentItemData == null) return;

        // === 背包模式：点击转移到工具栏 ===
        if (isInventoryMode)
        {
            TransferFromInventoryToToolbar();
            return;
        }

        // === 工具栏模式：使用物品 ===
        currentItemData.UseItem();

        if (currentItemData.itemType == ItemType.Consumable)
        {
            if (!PlayerController.CurrentFarmLand) return;
            if (PlayerController.CurrentFarmLand.FarmCurrentStatue != FarmCurrentStatus.Tilled) return;

            GameObject flower = Instantiate(currentItemData.itemPrefab, PlayerController.CurrentFarmLand.PlantPosition.gameObject.transform);
            Crop crop = flower.GetComponent<Crop>();
            crop.IsMature = false;
            currentItemCount--;
            if (currentItemCount == 0)
            {
                Destroy(PlayerController.EquipMentRoot.GetChild(0).gameObject);
                if (ToolBarManager.Instance.allToolSlots[0].currentItemData != null)
                    ToolBarManager.Instance.currentItemSlot = ToolBarManager.Instance.allToolSlots[0];
                else
                    ToolBarManager.Instance.currentItemSlot = null;
            }
            RefreshSlot();
            PlayerController.CurrentFarmLand.FarmCurrentStatue = FarmCurrentStatus.Planted;
        }
        else if (currentItemData.itemType == ItemType.Equipment)
        {
            if (PlayerController.CurrentFarmLand && PlayerController.CurrentFarmLand.FarmCurrentStatue == FarmCurrentStatus.Empty)
                PlayerController.CurrentFarmLand.FarmCurrentStatue = FarmCurrentStatus.Tilled;
            else
                Debug.Log("未识别到未开垦的田");
        }
    }

    /// <summary>
    /// 背包 → 工具栏 转移
    /// </summary>
    private void TransferFromInventoryToToolbar()
    {
        if (InventoryManager.Instance == null || ToolBarManager.Instance == null) return;

        // 尝试放入工具栏的空槽或堆叠到已有物品
        foreach (var toolSlot in ToolBarManager.Instance.allToolSlots)
        {
            // 空槽：直接放入
            if (toolSlot.currentItemData == null)
            {
                InventoryManager.Instance.RemoveItem(currentItemData, 1);
                toolSlot.currentItemData = currentItemData;
                toolSlot.currentItemCount = 1;
                toolSlot.RefreshSlot();
                return;
            }
            // 同物品堆叠
            else if (toolSlot.currentItemData.itemID == currentItemData.itemID)
            {
                int space = currentItemData.maxStackCount - toolSlot.currentItemCount;
                if (space > 0)
                {
                    int toMove = Mathf.Min(1, space);
                    InventoryManager.Instance.RemoveItem(currentItemData, toMove);
                    toolSlot.currentItemCount += toMove;
                    toolSlot.RefreshSlot();
                    return;
                }
            }
        }
        Debug.Log("工具栏已满，无法转移");
    }

    void OnDisable()
    {
        if (!isInventoryMode)
            PlayerEvents.Center.RemoveListener<GameObject>(PlayerEvent.UseItem, OnSlotClick);
    }
}