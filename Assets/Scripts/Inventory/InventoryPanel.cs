using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背包面板 UI
/// 无限下拉列表，有物品才显示格子
/// </summary>
public class InventoryPanel : MonoBehaviour
{
    [Header("网格设置")]
    [SerializeField] private Transform contentParent; // ScrollRect 的 Content
    [SerializeField] private ItemSlot slotPrefab;      // 复用 ItemSlot 预制体

    [Header("其他")]
    [SerializeField] private Button closeButton;
    [SerializeField] private ScrollRect scrollRect;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void ClosePanel()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        gameObject.SetActive(false);
        UIManager.IsAnyPanelOpen = false;
        AudioEvents.Center.Trigger<string>(AudioEvent.PlaySFX, "UIClick");
    }

    private void OnEnable()
    {
        RefreshGrid();
        InventoryManager.OnInventoryChanged += RefreshGrid;
    }

    private void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= RefreshGrid;
    }

    /// <summary>
    /// 刷新背包：清除旧格子，根据当前物品列表重新生成
    /// </summary>
    public void RefreshGrid()
    {
        if (contentParent == null || InventoryManager.Instance == null) return;

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        int index = 0;
        foreach (var data in InventoryManager.Instance.Slots)
        {
            if (data == null || data.itemData == null || data.count <= 0) continue;

            var slotUI = Instantiate(slotPrefab, contentParent);
            slotUI.SetAsInventorySlot(index);
            slotUI.RefreshInventorySlot(data);
            index++;
        }

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }
}
