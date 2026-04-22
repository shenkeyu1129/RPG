using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 右键可直接创建物品数据资产
[CreateAssetMenu(fileName = "NewItem", menuName = "RPG/Pickaxe")]
public class Pickaxe : ItemData
{
    public override void UseItem()
    {
        Debug.Log($"使用了物品：{itemName}");
        PickaxeEvents.Center.Trigger(PickaxeEvent.UsePickaxe);
    }   
}

//镐子事件
public enum PickaxeEvent
{
    UsePickaxe,
    AddPickaxe
}

//镐子事件管理中心
public static class PickaxeEvents
{
    public static readonly EventCenter<PickaxeEvent> Center = new ();
}