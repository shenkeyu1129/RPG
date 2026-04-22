using UnityEngine;

public class DayNightCycle : MonoBehaviour
{

    [Header("昼夜颜色配置")]
    public Color dayColor = new Color(1f, 0.95f, 0.85f);    // 白天暖白
    public Color duskColor = new Color(1f, 0.6f, 0.2f);     // 黄昏橙黄
    public Color nightColor = new Color(0.1f, 0.2f, 0.4f);   // 夜晚深蓝
    public Color dawnColor = new Color(0.8f, 0.9f, 1f);      // 黎明淡青

    private Light _sunLight;
    private float _timeProgress; // 0~1 时间进度

    void Awake()
    {
        // 获取当前物体上的灯光组件
        _sunLight = GetComponent<Light>();
    }

    void OnEnable()
    {
        TimeManager.OnHourChanged += UpdateLightColor;
    }

    void UpdateLightColor()
    {
  
        Color targetColor;

        _timeProgress = TimeManager.Instance.CurrentHour / 24f;
        // 四段式昼夜颜色插值
        if (_timeProgress < 0.25f)
        {
            // 0~0.25 → 黎明 → 白天
            float t = _timeProgress / 0.25f;
            targetColor = Color.Lerp(dawnColor, dayColor, t);
        }
        else if (_timeProgress < 0.5f)
        {
            // 0.25~0.5 → 白天 → 黄昏
            float t = (_timeProgress - 0.25f) / 0.25f;
            targetColor = Color.Lerp(dayColor, duskColor, t);
        }
        else if (_timeProgress < 0.75f)
        {
            // 0.5~0.75 → 黄昏 → 黑夜
            float t = (_timeProgress - 0.5f) / 0.25f;
            targetColor = Color.Lerp(duskColor, nightColor, t);
        }
        else
        {
            // 0.75~1 → 黑夜 → 黎明
            float t = (_timeProgress - 0.75f) / 0.25f;
            targetColor = Color.Lerp(nightColor, dawnColor, t);
        }

        // 应用颜色到灯光
        _sunLight.color = targetColor;
    }

        void OnDisable()
    {
        TimeManager.OnHourChanged -= UpdateLightColor;
    }
}