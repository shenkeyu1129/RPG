using UnityEngine;
using System;


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
         currentItemSlot = allToolSlots[0]; // 默认选中第一个槽位
    }

    private void Start()
    {
        // 初始化所有槽位，清空显示
        InitAllSlots();

        RefreshAllSlots(); // 刷新显示，确保UI正确
        RefreshSlotHighlights(); // 初始化选中高亮
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
            RefreshSlotHighlights();
        }
    }

    /// <summary>
    /// 刷新所有槽位的选中高亮（选中项显示 Border，其余隐藏）
    /// </summary>
    private void RefreshSlotHighlights()
    {
        foreach (var slot in allToolSlots)
        {
            slot.SetHighlight(slot == currentItemSlot);
        }
    }

    //拾取到物体后放入槽位（受物品 maxStackCount 限制）
    public void PickUpItem(ItemData itemData, int count)
    {
        int remaining = count;

        // 1. 尝试堆叠到已有物品（检查 maxStackCount）
        foreach (var slot in allToolSlots)
        {
            if (slot.currentItemData != null && slot.currentItemData.itemID == itemData.itemID && slot.currentItemCount < itemData.maxStackCount)
            {
                int space = itemData.maxStackCount - slot.currentItemCount;
                int toAdd = Mathf.Min(remaining, space);
                slot.currentItemCount += toAdd;
                remaining -= toAdd;
                slot.RefreshSlot();
                if (remaining <= 0) return;
            }
        }

        // 2. 放入空槽位
        foreach (var slot in allToolSlots)
        {
            if (slot.currentItemData == null)
            {
                int toAdd = Mathf.Min(remaining, itemData.maxStackCount);
                slot.currentItemData = itemData;
                slot.currentItemCount = toAdd;
                remaining -= toAdd;
                slot.RefreshSlot();
                if (remaining <= 0) return;
            }
        }

        // 3. 剩余部分溢出到背包
        if (remaining > 0 && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(itemData, remaining);
            Debug.Log($"工具栏已满，{remaining} 个物品已放入背包");
        }
        else if (remaining > 0)
        {
            Debug.Log("工具栏已满，无法拾取更多物品");
        }
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