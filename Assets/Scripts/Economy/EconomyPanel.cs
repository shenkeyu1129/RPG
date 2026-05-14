using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 金币显示面板
/// 挂在 Canvas 下的 EconomyPanel 上，监听 Wallet 事件自动刷新
/// </summary>
public class EconomyPanel : MonoBehaviour
{
    [SerializeField] private Text goldText;

    private void Awake()
    {
        if (goldText == null)
            goldText = GetComponentInChildren<Text>();
    }

    private void OnEnable()
    {
        Wallet.OnGoldChanged += UpdateGoldDisplay;
    }

    private void OnDisable()
    {
        Wallet.OnGoldChanged -= UpdateGoldDisplay;
    }

    private void Start()
    {
        if (Wallet.Instance != null)
            UpdateGoldDisplay(Wallet.Instance.CurrentGold);
    }

    private void UpdateGoldDisplay(int amount)
    {
        if (goldText != null)
            goldText.text = $"$ {amount:N0}";
    }
}
