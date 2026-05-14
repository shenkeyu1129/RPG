using UnityEngine;

// 右键可直接创建物品数据资产
[CreateAssetMenu(fileName = "NewItem", menuName = "RPG/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("基础信息")]
    public int itemID; // 物品唯一ID
    public string itemName; // 物品名称
    public Sprite itemIcon; // 物品图标（核心，对应工具栏显示的Sprite）

    public GameObject itemPrefab; // 物品3D模型预制体（可选，用于世界中显示或装备）
    [TextArea] public string itemDesc; // 物品描述

    [Header("功能属性")]
    public ItemType itemType; // 物品类型（消耗品/装备/任务物品等）
    public int maxStackCount = 1; // 最大堆叠数量
    public int coolDownTime; // 冷却时间（秒）

    [Header("经济")]
    public int buyPrice;   // 购买价格（0=不可购买）
    public int sellPrice;  // 出售价格（0=不可出售）

    // 物品使用方法，可被子类重写
    public virtual void UseItem()
    {
        // 在这里写物品使用逻辑，如加血、加蓝等
        Debug.Log($"使用了物品：{itemName}");
    }
}

// 物品类型枚举
public enum ItemType
{
    Consumable,
    Equipment,
    Quest,
    Weapon
}

