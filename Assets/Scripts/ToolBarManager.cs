using UnityEngine;
using System;
using UnityEditorInternal.Profiling.Memory.Experimental;


public class ToolBarManager : MonoBehaviour
{
    // 单例模式，方便全局调用（如背包系统）
    public static ToolBarManager Instance;

    [Header("工具栏配置")]
    public ItemSlot[] allToolSlots; // 所有快捷栏槽位

    public ItemSlot currentItemSlot;

    private void Awake()
    {
        // 单例初始化
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 初始化所有槽位，清空显示
        InitAllSlots();

        RefreshAllSlots(); // 刷新显示，确保UI正确

        currentItemSlot = allToolSlots[0]; // 默认选中第一个槽位
    }

    void OnEnable()
    {
        PlayerEvents.Center.AddListener<int>(PlayerEvent.ChangeSlot, ChangeSlot);
        PlayerEvents.Center.AddListener<ItemData, int>(PlayerEvent.GetItem, PickUpItem);
    }

    // 初始化所有槽位
    public void InitAllSlots()
    {
        foreach (var slot in allToolSlots)
        {
            slot.ClearSlot();
        }
    }

    //切换槽位
    public void ChangeSlot(int slotIndex)
    {
        if (slotIndex < allToolSlots.Length)
        {
            currentItemSlot = allToolSlots[slotIndex];
        }
    }

    //拾取到物体后放入槽位
    public void PickUpItem(ItemData itemData, int count)
    {

        // 尝试放入已有相同物品的槽位
        foreach (var slot in allToolSlots)
        {
            if (slot.currentItemData != null && slot.currentItemData.itemID == itemData.itemID)
            {
                if (currentItemSlot.currentItemData == null)
                {
                    currentItemSlot = slot;
                }
                slot.currentItemCount += count;
                slot.RefreshSlot();
                return;
            }
        }

        // 否则放入第一个空槽位
        foreach (var slot in allToolSlots)
        {
            if (slot.currentItemData == null)
            {
                if (currentItemSlot.currentItemData == null)
                {
                    currentItemSlot = slot;
                }
                slot.currentItemData = itemData;
                slot.currentItemCount = count;
                slot.RefreshSlot();
                return;
            }
        }

        Debug.Log("工具栏已满，无法拾取更多物品");
    }

    // 全局刷新所有槽位（背包物品变化时调用）
    public void RefreshAllSlots()
    {
        foreach (var slot in allToolSlots)
        {
            slot.RefreshSlot();
        }
    }

    void OnDisable()
    {
        PlayerEvents.Center.RemoveListener<int>(PlayerEvent.ChangeSlot, ChangeSlot);
        PlayerEvents.Center.RemoveListener<ItemData, int>(PlayerEvent.GetItem, PickUpItem);
    }

}