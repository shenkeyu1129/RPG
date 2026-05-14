using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存档数据容器
/// 所有可序列化的游戏状态
/// </summary>
[System.Serializable]
public class SaveData
{
    // 玩家位置
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    public float playerRotY;

    // 时间
    public int year;
    public int season;   // 0=Spring, 1=Summer, 2=Autumn, 3=Winter
    public int day;
    public int hour;
    public int minute;

    // 玩家名称
    public string playerName = "";

    // 经济
    public int gold;

    // 工具栏（7 个槽位）
    public List<SlotSaveData> toolbarSlots = new();

    // 背包
    public List<SlotSaveData> inventorySlots = new();

    // 农田状态
    public List<FarmLandSaveData> farmLands = new();
}

/// <summary>
/// 槽位序列化数据
/// </summary>
[System.Serializable]
public class SlotSaveData
{
    public int itemID;   // -1 或 0 表示空
    public int count;
}

/// <summary>
/// 农田序列化数据
/// </summary>
[System.Serializable]
public class FarmLandSaveData
{
    public float posX;
    public float posY;
    public float posZ;
    public int status;   // FarmCurrentStatus 的 int 值
}
