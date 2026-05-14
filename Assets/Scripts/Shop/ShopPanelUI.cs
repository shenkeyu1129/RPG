using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 商店面板 UI 控制（简化版）
/// 单个统一列表，每个商品自带购买和出售按钮
/// </summary>
public class ShopPanelUI : MonoBehaviour
{
    [Header("商品网格")]
    [SerializeField] private Transform itemGrid;          // 商品网格父物体
    [SerializeField] private ShopItemSlot itemSlotPrefab; // 商品条目预制体

    [Header("其他")]
    [SerializeField] private Text goldText;
    [SerializeField] private Button closeButton;

    private ShopManager _shopManager;
    private bool _firstOpen = true;

    private void Awake()
    {
        _shopManager = GetComponent<ShopManager>();

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);
    }

    private void OnEnable()
    {
        Wallet.OnGoldChanged += UpdateGoldDisplay;
    }

    private void OnDisable()
    {
        Wallet.OnGoldChanged -= UpdateGoldDisplay;
    }

    /// <summary>
    /// 商店打开时调用（由 UIManager 触发）
    /// </summary>
    public void OnOpen()
    {
        if (_firstOpen)
        {
            PopulateGrid();
            _firstOpen = false;
        }
        UpdateGoldDisplay(Wallet.Instance != null ? Wallet.Instance.CurrentGold : 0);
    }

    /// <summary>
    /// 填充商品列表（一次性生成，后续不再重建）
    /// </summary>
    private void PopulateGrid()
    {
        if (itemGrid == null || _shopManager == null) return;

        foreach (var item in _shopManager.shopItems)
        {
            if (item == null) continue;
            var slot = Instantiate(itemSlotPrefab, itemGrid);
            slot.SetItem(item, _shopManager);
        }
    }

    private void UpdateGoldDisplay(int amount)
    {
        if (goldText != null)
            goldText.text = $"$ {amount:N0}";
    }

    private void CloseShop()
    {
        PlayerEvents.Center.Trigger(PlayerEvent.ExitShop);
    }
}
