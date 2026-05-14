using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 商店交易逻辑
/// 挂在 ShopPanel 上，管理购买和出售
/// </summary>
public class ShopManager : MonoBehaviour
{
    [Header("商店商品列表")]
    public List<ItemData> shopItems; // 在 Inspector 中拖入可购买物品

    /// <summary>
    /// 购买物品（优先工具栏，工具栏满则进背包）
    /// </summary>
    public void CmdBuy(ItemData item, int count = 1)
    {
        if (item == null || item.buyPrice <= 0) return;

        int totalCost = item.buyPrice * count;

        if (!Wallet.Instance.HasFunds(totalCost))
        {
            Debug.Log("金币不足！");
            return;
        }

        Wallet.Instance.Spend(totalCost);

        // 优先放入工具栏（满时自动溢出到背包）
        if (ToolBarManager.Instance != null)
            ToolBarManager.Instance.PickUpItem(item, count);
        else
        {
            InventoryManager.EnsureExists();
            InventoryManager.Instance?.AddItem(item, count);
        }

        AudioEvents.Center.Trigger<string>(AudioEvent.PlaySFX, "Buy");
        Debug.Log($"购买了 {item.itemName} x{count}");
    }

    /// <summary>
    /// 出售物品
    /// </summary>
    public void CmdSell(ItemData item, int count = 1)
    {
        if (item == null || item.sellPrice <= 0) return;

        InventoryManager.EnsureExists();
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("背包系统初始化失败");
            return;
        }

        if (InventoryManager.Instance.HasItem(item, count))
        {
            InventoryManager.Instance.RemoveItem(item, count);
            int revenue = item.sellPrice * count;
            Wallet.Instance.Earn(revenue);
            AudioEvents.Center.Trigger<string>(AudioEvent.PlaySFX, "Sell");
            Debug.Log($"出售了 {item.itemName} x{count}，获得 {revenue} 金币");
        }
        else
        {
            Debug.Log("背包中没有足够的物品！");
        }
    }
}
