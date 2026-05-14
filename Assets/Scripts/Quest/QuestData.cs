using System.Collections.Generic;
using UnityEngine;

public enum QuestObjectiveType
{
    GatherItem,
    EarnGold,
    PlantCrop,
    Harvest,
    TillLand,
    ReachGold,
}

[System.Serializable]
public class QuestObjective
{
    public QuestObjectiveType type;
    public int targetID;
    public string targetName;
    public int targetCount;
    public int currentCount;
}

[System.Serializable]
public class QuestReward
{
    public int gold;
    public ItemData itemReward;
    public int itemCount = 1;
}

[CreateAssetMenu(fileName = "NewQuest", menuName = "RPG/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("基础信息")]
    public int questID;
    public string questName;
    [TextArea(2, 4)] public string description;

    [Header("目标")]
    public List<QuestObjective> objectives = new();

    [Header("奖励")]
    public QuestReward reward = new();

    [Header("状态")]
    public bool isActive;
    public bool isCompleted;

    public bool IsAllObjectivesMet()
    {
        foreach (var obj in objectives)
            if (obj.currentCount < obj.targetCount) return false;
        return true;
    }

    public string GetProgressText()
    {
        if (objectives.Count == 0) return "";
        int total = 0, done = 0;
        foreach (var obj in objectives)
        {
            total += obj.targetCount;
            done += Mathf.Min(obj.currentCount, obj.targetCount);
        }
        return $"{done}/{total}";
    }
}
