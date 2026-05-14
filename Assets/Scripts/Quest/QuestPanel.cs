using UnityEngine;
using UnityEngine.UI;

public class QuestPanel : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Transform questListParent;
    [SerializeField] private GameObject questItemPrefab;
    [SerializeField] private Button closeButton;

    [Header("任务详情")]
    [SerializeField] private Text detailTitleText;
    [SerializeField] private Text detailDescText;
    [SerializeField] private Text detailProgressText;
    [SerializeField] private Text detailRewardText;
    [SerializeField] private GameObject detailPanel;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void ClosePanel()
    {
        AudioEvents.Center.Trigger<string>(AudioEvent.PlaySFX, "UIClick");
        gameObject.SetActive(false);
        UIManager.IsAnyPanelOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        RefreshList();
        if (detailPanel != null) detailPanel.SetActive(false);
        QuestManager.OnQuestChanged += RefreshList;
    }

    private void OnDisable()
    {
        QuestManager.OnQuestChanged -= RefreshList;
    }

    private void RefreshList()
    {
        if (questListParent == null || QuestManager.Instance == null) return;

        foreach (Transform child in questListParent)
            Destroy(child.gameObject);

        foreach (var quest in QuestManager.Instance.ActiveQuests)
        {

            if (quest == null) continue;
            var item = Instantiate(questItemPrefab, questListParent);
            SetupQuestItem(item, quest);
        }

        if (QuestManager.Instance.ActiveQuests.Count == 0)
        {
            var empty = Instantiate(questItemPrefab, questListParent);
            var texts = empty.GetComponentsInChildren<Text>(true);
            foreach (var t in texts)
            {
                if (t.name == "QuestNameText") t.text = "当前没有活跃任务";
                else if (t.name == "QuestProgressText") t.text = "";
            }
            var btn = empty.GetComponent<Button>();
            if (btn != null) btn.interactable = false;
        }
    }

    private void SetupQuestItem(GameObject item, QuestData quest)
    {
        var texts = item.GetComponentsInChildren<Text>(true);
        foreach (var t in texts)
        {
            if (t.name == "QuestNameText")
            {
                t.text = quest.questName;
            }
            else if (t.name == "QuestProgressText")
                t.text = quest.GetProgressText();
        }

        var btn = item.GetComponentsInChildren<Button>()[0];
        Debug.Log(btn);
        if (btn != null)
        { 
            btn.onClick.AddListener(() => ShowDetail(quest));
        }

    }

    private void ShowDetail(QuestData quest)
    {
        if (detailPanel == null) return;

        detailPanel.SetActive(true);

        if (detailTitleText != null)
            detailTitleText.text = quest.questName;
        if (detailDescText != null)
            detailDescText.text = quest.description;

        string progress = "";
        foreach (var obj in quest.objectives)
            progress += $"{obj.targetName}: {obj.currentCount}/{obj.targetCount}\n";
        if (detailProgressText != null)
            detailProgressText.text = progress;

        string reward = "";
        if (quest.reward.gold > 0)
            reward += $"金币: {quest.reward.gold}\n";
        if (quest.reward.itemReward != null)
            reward += $"物品: {quest.reward.itemReward.itemName} x{quest.reward.itemCount}";
        if (detailRewardText != null)
            detailRewardText.text = string.IsNullOrEmpty(reward) ? "无" : reward;
    }
}
