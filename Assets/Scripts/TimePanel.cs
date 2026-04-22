using UnityEngine;
using UnityEngine.UI;


public class TimePanel : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField]private Text seasonText;
    [SerializeField]private Text dayText;
    [SerializeField]private Text  timeText;
    [SerializeField] private Text yearText;

    void OnEnable()
    {
        // 订阅时间变化事件
        TimeManager.OnMinuteChanged += UpdateTimeUI;
        TimeManager.OnDayChanged += UpdateDateUI;
        TimeManager.OnSeasonChanged += UpdateSeasonUI;
        TimeManager.OnYearChanged += UpdateYearUI;
    }

     void Start()
    {
        // 开局初始化UI
        UpdateSeasonUI();
        UpdateYearUI();
        UpdateDateUI();
        UpdateTimeUI();
        
    }

    /// <summary>
    /// 更新时间显示
    /// </summary>
    private void UpdateTimeUI()
    {
        // 补零格式化，比如6点5分显示为06:05
        timeText.text = $"{TimeManager.Instance.CurrentHour:D2}:{TimeManager.Instance.CurrentMinute:D2}";
    }

    /// <summary>
    /// 更新日期显示
    /// </summary>
    private void UpdateDateUI()
    {
        dayText.text = $"第 {TimeManager.Instance.CurrentDay} 天";
        UpdateTimeUI();
    }

    /// <summary>
    /// 更新季节显示
    /// </summary>
    private void UpdateSeasonUI()
    {
        seasonText.text = TimeManager.Instance.CurrentSeason.ToString();
        UpdateDateUI();
    }

    /// <summary>
    /// 更新年份显示
    /// </summary>
    private void UpdateYearUI()
    {
        yearText.text = $"第 {TimeManager.Instance.CurrentYear} 年";
        UpdateSeasonUI();
    }

    void OnDisable()
    {
        // 取消订阅事件，必须成对出现！
        TimeManager.OnMinuteChanged -= UpdateTimeUI;
        TimeManager.OnDayChanged -= UpdateDateUI;
        TimeManager.OnSeasonChanged -= UpdateSeasonUI;
        TimeManager.OnYearChanged -= UpdateYearUI;
    }

   
}