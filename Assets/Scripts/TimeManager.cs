using System;
using UnityEngine;

/// <summary>
/// 农场全局时间管理器
/// 挂在场景里的GameManager空物体上即可
/// </summary>
public class TimeManager : MonoBehaviour
{
    // 单例实例，全局唯一访问入口
    public static TimeManager Instance { get; private set; }

    #region 可配置参数（Inspector面板直接调试）
    [Header("时间流速设置")]
    [Tooltip("现实1秒 = 游戏内多少分钟，推荐值：10（现实1秒=游戏10分钟）")]
    public float gameMinutesPerRealSecond = 10f;
    [Tooltip("时间步长：多少分钟触发一次分钟事件，推荐10分钟，减少性能消耗")]
    public int minuteStep = 10;

    [Header("日期设置")]
    [Tooltip("每个季节的天数，星露谷标准为28天")]
    public int daysPerSeason = 28;
    [Tooltip("开局初始年份")]
    public int startYear = 1;
    [Tooltip("开局初始季节")]
    public Season startSeason = Season.Spring;
    [Tooltip("开局初始日期")]
    public int startDay = 1;
    [Tooltip("开局初始小时")]
    public int startHour = 6;
    [Tooltip("开局初始分钟")]
    public int startMinute = 0;

    [Header("日出日落设置")]
    [Tooltip("日出时间（小时），触发日出事件")]
    public int sunriseHour = 6;
    [Tooltip("日落时间（小时），触发日落事件")]
    public int sunsetHour = 20;
    #endregion

    #region 当前时间数据
    public int CurrentYear { get; private set; }
    public Season CurrentSeason { get; private set; }
    public int CurrentDay { get; private set; }
    public int CurrentHour { get; private set; }
    public int CurrentMinute { get; private set; }
    public bool IsPaused { get; private set; } // 时间暂停开关
    #endregion

    #region 全局静态事件（所有系统都能订阅）
    // 基础时间变化事件
    public static event Action OnMinuteChanged;  // 分钟变化
    public static event Action OnHourChanged;    // 小时变化
    public static event Action OnDayChanged;     // 日期变化（新的一天）
    public static event Action OnSeasonChanged;  // 季节变化
    public static event Action OnYearChanged;    // 年份变化

    // 农场专属事件
    public static event Action OnSunrise;        // 日出触发
    public static event Action OnSunset;         // 日落触发
    #endregion

    #region 内部变量
    private float _minuteTimer; // 分钟累计计时器
    private bool _hasTriggeredSunrise; // 当天日出是否已触发（防止重复触发）
    private bool _hasTriggeredSunset;  // 当天日落是否已触发
    #endregion

    #region 季节枚举
    public enum Season
    {
        Spring, // 春
        Summer, // 夏
        Autumn, // 秋
        Winter  // 冬
    }
    #endregion

    #region 初始化
    private void Awake()
    {
        // 单例初始化，防止场景里有多个实例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 场景切换不销毁
        }

        // 初始化开局时间
        InitTimeData();
    }

    /// <summary>
    /// 初始化时间数据
    /// </summary>
    private void InitTimeData()
    {
        CurrentYear = startYear;
        CurrentSeason = startSeason;
        CurrentDay = Mathf.Clamp(startDay, 1, daysPerSeason); // 日期最小1，最大季节天数
        CurrentHour = Mathf.Clamp(startHour, 0, 23);
        CurrentMinute = Mathf.Clamp(startMinute, 0, 59);

        // 重置日出日落标记
        _hasTriggeredSunrise = CurrentHour >= sunriseHour;
        _hasTriggeredSunset = CurrentHour >= sunsetHour;
    }
    #endregion

    #region 核心时间流逝
    private void Update()
    {
        // 暂停状态不流逝时间
        if (IsPaused) return;

        // 累计时间
        _minuteTimer += Time.deltaTime * gameMinutesPerRealSecond;

        // 达到步长，触发分钟变化
        if (_minuteTimer >= minuteStep)
        {
            _minuteTimer -= minuteStep;
            CurrentMinute += minuteStep;
            HandleTimeCarry(); // 处理时间进位
            OnMinuteChanged?.Invoke(); // 广播分钟变化事件
        }
    }

    /// <summary>
    /// 处理时间进位（分→时→日→季节→年）
    /// </summary>
    private void HandleTimeCarry()
    {
        // 分钟满60，进1小时
        if (CurrentMinute >= 60)
        {
            CurrentMinute -= 60;
            CurrentHour += 1;
            OnHourChanged?.Invoke(); // 广播小时变化事件

            // 检测日出日落
            CheckSunriseSunset();
        }

        // 小时满24，进1天
        if (CurrentHour >= 24)
        {
            CurrentHour -= 24;
            CurrentDay += 1;
            // 新的一天，重置日出日落标记
            _hasTriggeredSunrise = false;
            _hasTriggeredSunset = false;
            OnDayChanged?.Invoke(); // 广播日期变化事件（核心！作物生长用这个）
        }

        // 日期满季节天数，进1个季节
        if (CurrentDay > daysPerSeason)
        {
            CurrentDay = 1;
            CurrentSeason = (Season)(((int)CurrentSeason + 1) % 4);
            OnSeasonChanged?.Invoke(); // 广播季节变化事件
        }

        // 季节满4个，进1年
        if ((int)CurrentSeason == 0 && CurrentDay == 1 && CurrentHour == 0 && CurrentMinute == 0)
        {
            CurrentYear += 1;
            OnYearChanged?.Invoke(); // 广播年份变化事件
        }
    }

    /// <summary>
    /// 检测日出日落，触发对应事件
    /// </summary>
    private void CheckSunriseSunset()
    {
        // 到日出时间，且当天没触发过
        if (CurrentHour == sunriseHour && !_hasTriggeredSunrise)
        {
            _hasTriggeredSunrise = true;
            OnSunrise?.Invoke();
        }

        // 到日落时间，且当天没触发过
        if (CurrentHour == sunsetHour && !_hasTriggeredSunset)
        {
            _hasTriggeredSunset = true;
            OnSunset?.Invoke();
        }
    }
    #endregion

    #region 对外公开方法
    /// <summary>
    /// 暂停/恢复时间
    /// </summary>
    public void SetPause(bool isPaused)
    {
        IsPaused = isPaused;
    }

    /// <summary>
    /// 直接跳转到指定时间（比如跳过夜晚）
    /// </summary>
    public void JumpToTime(int targetHour, int targetMinute)
    {
        CurrentHour = Mathf.Clamp(targetHour, 0, 23);
        CurrentMinute = Mathf.Clamp(targetMinute, 0, 59);
        _minuteTimer = 0;
        OnHourChanged?.Invoke();
        OnMinuteChanged?.Invoke();
        CheckSunriseSunset();
    }

    /// <summary>
    /// 跳过一天（比如睡觉）
    /// </summary>
    public void SkipOneDay()
    {
        CurrentHour = startHour;
        CurrentMinute = startMinute;
        CurrentDay += 1;
        _minuteTimer = 0;
        _hasTriggeredSunrise = false;
        _hasTriggeredSunset = false;
        HandleTimeCarry();
        OnDayChanged?.Invoke();
    }
    #endregion

    #region 生命周期
    private void OnDestroy()
    {
        // 场景销毁时清空所有事件，防止内存泄漏
        OnMinuteChanged = null;
        OnHourChanged = null;
        OnDayChanged = null;
        OnSeasonChanged = null;
        OnYearChanged = null;
        OnSunrise = null;
        OnSunset = null;

        if (Instance == this)
        {
            Instance = null;
        }
    }
    #endregion
}