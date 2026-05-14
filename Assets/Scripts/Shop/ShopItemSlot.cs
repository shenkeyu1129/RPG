using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 商店网格中的单个商品条目
/// 每个条目自带购买和出售按钮
/// </summary>
public class ShopItemSlot : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Text itemNameText;
    [SerializeField] private Text buyPriceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Text sellPriceText;
    [SerializeField] private Button sellButton;

    private ItemData _itemData;
    private ShopManager _shopManager;

    private void Awake()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClick);
        if (sellButton != null)
            sellButton.onClick.AddListener(OnSellClick);
    }

    private void OnEnable()
    {
        InventoryManager.EnsureExists();
        RefreshSellButton();
        if (InventoryManager.Instance != null)
            InventoryManager.OnInventoryChanged += RefreshSellButton;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.OnInventoryChanged -= RefreshSellButton;
    }

    /// <summary>
    /// 初始化商品条目
    /// </summary>
    public void SetItem(ItemData item, ShopManager manager)
    {
        _itemData = item;
        _shopManager = manager;

        // 图标
        if (itemIcon != null)
        {
            itemIcon.sprite = item.itemIcon;
            itemIcon.enabled = item.itemIcon != null;
        }

        // 名称
        if (itemNameText != null)
            itemNameText.text = item.itemName;

        // 购买价格
        if (buyPriceText != null)
            buyPriceText.text = item.buyPrice > 0 ? $"购买 $ {item.buyPrice}" : "---";

        // 出售价格
        if (sellPriceText != null)
            sellPriceText.text = item.sellPrice > 0 ? $"出售 $ {item.sellPrice}" : "---";

        // 购买按钮
        if (buyButton != null)
            buyButton.interactable = item.buyPrice > 0;

        // 出售按钮状态
        RefreshSellButton();
    }

    private void RefreshSellButton()
    {
        if (sellButton == null) return;
        bool canSell = _itemData != null && _itemData.sellPrice > 0
                       && InventoryManager.Instance != null
                       && InventoryManager.Instance.HasItem(_itemData, 1);
        sellButton.interactable = canSell;
    }

    private void OnBuyClick()
    {
        if (_shopManager == null || _itemData == null) return;
        _shopManager.CmdBuy(_itemData, 1);
    }

    private void OnSellClick()
    {
        if (_shopManager == null || _itemData == null) return;
        _shopManager.CmdSell(_itemData, 1);
    }
}
