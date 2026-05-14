using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 背包中的一个格子数据
/// </summary>
[System.Serializable]
public class InventorySlotData
{
    public ItemData itemData;
    public int count;
}

/// <summary>
/// 背包管理器单例
/// 动态列表，无固定格子限制，有物品才有条目
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private readonly List<InventorySlotData> _slots = new();
    public IReadOnlyList<InventorySlotData> Slots => _slots;

    public static event System.Action OnInventoryChanged;

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

    /// <summary>
    /// 自动创建单例（场景中没有时）
    /// </summary>
    public static void EnsureExists()
    {
        if (Instance == null)
        {
            var go = new GameObject("InventoryManager");
            go.AddComponent<InventoryManager>();
        }
    }

    /// <summary>
    /// 添加物品，优先堆叠，否则新增一条
    /// 堆叠上限使用物品自身的 maxStackCount
    /// </summary>
    public bool AddItem(ItemData item, int count = 1)
    {
        if (item == null || count <= 0) return false;

        int limit = item.maxStackCount;
        int remaining = count;

        // 1. 堆叠到已有物品
        foreach (var slot in _slots)
        {
            if (slot.itemData.itemID == item.itemID && slot.count < limit)
            {
                int space = limit - slot.count;
                int toAdd = Mathf.Min(remaining, space);
                slot.count += toAdd;
                remaining -= toAdd;
                if (remaining <= 0)
                {
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        // 2. 新增条目（不受数量限制）
        while (remaining > 0)
        {
            int toAdd = Mathf.Min(remaining, limit);
            _slots.Add(new InventorySlotData { itemData = item, count = toAdd });
            remaining -= toAdd;
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 移除物品，数量归零时自动删除条目
    /// </summary>
    public bool RemoveItem(ItemData item, int count = 1)
    {
        if (item == null || count <= 0) return false;

        int remaining = count;

        for (int i = _slots.Count - 1; i >= 0; i--)
        {
            if (_slots[i].itemData.itemID == item.itemID)
            {
                int toRemove = Mathf.Min(remaining, _slots[i].count);
                _slots[i].count -= toRemove;
                remaining -= toRemove;
                if (_slots[i].count <= 0)
                    _slots.RemoveAt(i);
                if (remaining <= 0)
                {
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        OnInventoryChanged?.Invoke();
        return false;
    }

    public bool HasItem(ItemData item, int count = 1)
    {
        int total = 0;
        foreach (var slot in _slots)
        {
            if (slot.itemData.itemID == item.itemID)
                total += slot.count;
        }
        return total >= count;
    }

    public bool HasSpaceFor(ItemData item, int count = 1)
    {
        int limit = item.maxStackCount;
        int available = 0;
        foreach (var slot in _slots)
        {
            if (slot.itemData.itemID == item.itemID)
                available += limit - slot.count;
        }
        // 新增条目不限制数量（无限背包）
        available += count * limit;
        return available >= count;
    }

    public int GetItemCount(ItemData item)
    {
        int total = 0;
        foreach (var slot in _slots)
        {
            if (slot.itemData.itemID == item.itemID)
                total += slot.count;
        }
        return total;
    }

    public void ClearAll()
    {
        _slots.Clear();
        OnInventoryChanged?.Invoke();
    }
}
