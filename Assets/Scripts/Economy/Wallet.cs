using UnityEngine;

/// <summary>
/// 钱包系统单例
/// 管理玩家金币，提供 Earn/Spend 接口，通过事件通知 UI 刷新
/// </summary>
public class Wallet : MonoBehaviour
{
    public static Wallet Instance { get; private set; }

    [SerializeField] private int _startingGold = 100;

    private int _currentGold;
    public int CurrentGold => _currentGold;

    /// <summary>
    /// 金币变化事件（参数：变化后的新余额）
    /// </summary>
    public static event System.Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _currentGold = _startingGold;
    }

    private void Start()
    {
        OnGoldChanged?.Invoke(_currentGold);
    }

    /// <summary>
    /// 赚取金币
    /// </summary>
    public void Earn(int amount)
    {
        if (amount <= 0) return;
        _currentGold += amount;
        OnGoldChanged?.Invoke(_currentGold);
        AudioEvents.Center.Trigger<string>(AudioEvent.PlaySFX, "Coin");
    }

    /// <summary>
    /// 花费金币，返回是否成功
    /// </summary>
    public bool Spend(int amount)
    {
        if (amount <= 0) return true;
        if (_currentGold < amount) return false;
        _currentGold -= amount;
        OnGoldChanged?.Invoke(_currentGold);
        return true;
    }

    /// <summary>
    /// 检查是否有足够金币
    /// </summary>
    public bool HasFunds(int amount)
    {
        return _currentGold >= amount;
    }

    /// <summary>
    /// 直接设置余额（用于读档）
    /// </summary>
    public void SetGold(int amount)
    {
        _currentGold = Mathf.Max(0, amount);
        OnGoldChanged?.Invoke(_currentGold);
    }
}
