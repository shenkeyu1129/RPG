using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("可用任务列表")]
    public List<QuestData> availableQuests;

    private readonly List<QuestData> _activeQuests = new();
    private readonly List<QuestData> _completedQuests = new();

    public IReadOnlyList<QuestData> ActiveQuests => _activeQuests;
    public IReadOnlyList<QuestData> CompletedQuests => _completedQuests;

    public static event System.Action OnQuestChanged;

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
        InventoryManager.OnInventoryChanged += OnInventoryChanged;
        if (Wallet.Instance != null)
            Wallet.OnGoldChanged += OnGoldChanged;
        PlayerEvents.Center.AddListener(PlayerEvent.GameStarted, OnGameStarted);
    }

    private void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= OnInventoryChanged;
        if (Wallet.Instance != null)
            Wallet.OnGoldChanged -= OnGoldChanged;
        PlayerEvents.Center.RemoveListener(PlayerEvent.GameStarted, OnGameStarted);
    }

    private void OnGameStarted()
    {
        if (availableQuests.Count > 0 && _activeQuests.Count == 0)
            AcceptQuest(availableQuests[0]);
    }

    public void AcceptQuest(QuestData quest)
    {
        if (quest == null || quest.isCompleted) return;

        quest.isActive = true;
        foreach (var obj in quest.objectives)
            obj.currentCount = 0;

        _activeQuests.Add(quest);
        availableQuests.Remove(quest);
        OnQuestChanged?.Invoke();
        Debug.Log($"接受任务: {quest.questName}");
    }

    public void ProgressQuest(QuestObjectiveType type, int targetID = -1, int amount = 1)
    {
        bool changed = false;

        // 用 for 反向遍历，避免 CompleteQuest 中 Remove 修改集合导致迭代器报错
        for (int i = _activeQuests.Count - 1; i >= 0; i--)
        {
            var quest = _activeQuests[i];
            if (quest.isCompleted) continue;

            foreach (var obj in quest.objectives)
            {
                if (obj.type == type && (targetID < 0 || obj.targetID == targetID))
                {
                    obj.currentCount = Mathf.Min(obj.currentCount + amount, obj.targetCount);
                    changed = true;
                }
            }

            if (!quest.isCompleted && quest.IsAllObjectivesMet())
            {
                CompleteQuest(quest);
                changed = true;
            }
        }

        if (changed)
            OnQuestChanged?.Invoke();
    }

    private void CompleteQuest(QuestData quest)
    {
        quest.isCompleted = true;
        quest.isActive = false;
        _activeQuests.Remove(quest);
        _completedQuests.Add(quest);

        if (quest.reward.gold > 0 && Wallet.Instance != null)
            Wallet.Instance.Earn(quest.reward.gold);

        if (quest.reward.itemReward != null && quest.reward.itemCount > 0)
        {
            if (ToolBarManager.Instance != null)
                ToolBarManager.Instance.PickUpItem(quest.reward.itemReward, quest.reward.itemCount);
        }

        Debug.Log($"完成任务: {quest.questName}");
        OnQuestChanged?.Invoke();
    }

    private void OnInventoryChanged()
    {
        foreach (var quest in _activeQuests)
        {
            if (quest.isCompleted) continue;
            foreach (var obj in quest.objectives)
            {
                if (obj.type == QuestObjectiveType.GatherItem && InventoryManager.Instance != null)
                {
                    var item = FindItemByID(obj.targetID);
                    if (item != null)
                    {
                        int total = InventoryManager.Instance.GetItemCount(item);
                        if (ToolBarManager.Instance != null)
                        {
                            foreach (var slot in ToolBarManager.Instance.allToolSlots)
                            {
                                if (slot.currentItemData != null && slot.currentItemData.itemID == obj.targetID)
                                    total += slot.currentItemCount;
                            }
                        }
                        obj.currentCount = Mathf.Min(total, obj.targetCount);
                    }
                }
            }
            if (quest.IsAllObjectivesMet() && !quest.isCompleted)
                CompleteQuest(quest);
        }
        OnQuestChanged?.Invoke();
    }

    private void OnGoldChanged(int currentGold)
    {
        ProgressQuest(QuestObjectiveType.EarnGold, amount: 1);
        ProgressQuest(QuestObjectiveType.ReachGold, amount: currentGold);

        foreach (var quest in _activeQuests)
        {
            foreach (var obj in quest.objectives)
            {
                if (obj.type == QuestObjectiveType.EarnGold) { }
                if (obj.type == QuestObjectiveType.ReachGold)
                    obj.currentCount = Mathf.Min(currentGold, obj.targetCount);
            }
            if (quest.IsAllObjectivesMet() && !quest.isCompleted)
                CompleteQuest(quest);
        }
        OnQuestChanged?.Invoke();
    }

    private ItemData FindItemByID(int id)
    {
        foreach (var quest in availableQuests)
        {
            if (quest.reward.itemReward != null && quest.reward.itemReward.itemID == id)
                return quest.reward.itemReward;
        }
        foreach (var quest in _activeQuests)
        {
            if (quest.reward.itemReward != null && quest.reward.itemReward.itemID == id)
                return quest.reward.itemReward;
        }
        foreach (var quest in _completedQuests)
        {
            if (quest.reward.itemReward != null && quest.reward.itemReward.itemID == id)
                return quest.reward.itemReward;
        }
        return null;
    }
}
