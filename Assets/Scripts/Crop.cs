using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crop : MonoBehaviour
{

    [SerializeField] private GameObject _progressBarPrefab;
     private Transform _globalCanvas;
     private Camera _mainCamera;
    private Image _progressFill;
    private Image _backgroundImage;
    private Text _timeText;

    private GameObject _progressBarInstance;

    private Vector3 progressBarOffset = new Vector3(0, 1.5f, 0);
    private int _currentGrowDay = 0; // 当前生长天数

    private CropData _cropData;
    private Renderer _renderer;
    private Material _material;

    public bool IsMature;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _material = _renderer.material;
        _cropData = (CropData)GetComponent<ItemModel>().itemData;
        _mainCamera = Camera.main;

        _globalCanvas = GameObject.Find("ProgressBarCanvas").transform;
    }

    void OnEnable()
    {

        // 订阅新的一天事件，每天自动生长
        TimeManager.OnDayChanged += GrowOneDay;
    }
    void Update()
    {
        //if(IsMature)return;
        UpdateProgressBar();
    }
    void Start()
    {
        SpawnProgressBar();
    }
    private void LateUpdate()
    {
        if (_progressBarInstance == null) return;

        // 同步世界位置
        _progressBarInstance.transform.position = transform.position + progressBarOffset;

        // 全局Canvas的Billboard效果
        _progressBarInstance.transform.forward = _mainCamera.transform.forward;
    }



    private void SpawnProgressBar()
    {
        // 进度条作为全局Canvas的子物体
        _progressBarInstance = Instantiate(_progressBarPrefab, _globalCanvas);
        _progressBarInstance.name = $"{gameObject.name}_ProgressBar";

        _progressFill = _progressBarInstance.transform.Find("FIllImage").GetComponent<Image>();
        _timeText = _progressBarInstance.transform.Find("TimeText").GetComponent<Text>();
        _backgroundImage = _progressBarInstance.transform.Find("BackgroundImage").GetComponent<Image>();


        UpdateProgressBar();
    }

        private void UpdateProgressBar()
    {
        if(IsMature)
        {
            _progressFill.gameObject.SetActive(false);
            _backgroundImage.gameObject.SetActive(false);
            _timeText.text = $"成熟";
            return;
        }
        float progress = (float)_currentGrowDay / _cropData.totalGrowDays;
        _progressFill.fillAmount = progress;
        float remainingDay = _cropData.totalGrowDays - _currentGrowDay;
        _timeText.text = $"剩余{remainingDay}天";
    }
    /// <summary>
    /// 生长一天
    /// </summary>
    private void GrowOneDay()
    {
        if (IsMature) return; // 成熟后不再生长

        _currentGrowDay++;
        UpdateGrowStage();

        // 达到总天数，成熟
        if (_currentGrowDay >= _cropData.totalGrowDays)
        {
            IsMature = true;
            Debug.Log("作物成熟了！");
        }
    }

    /// <summary>
    /// 更新生长阶段外观
    /// </summary>
    private void UpdateGrowStage()
    {
        // 计算当前阶段
        int stage = Mathf.Clamp(_currentGrowDay, 0, _cropData.growStageColor.Length - 1);


        // 3D作物更新材质（和你之前的农田变色逻辑一致）
        if (_renderer != null)
        {
            // 这里可以替换成你自己的材质变色逻辑
            _material.color = _cropData.growStageColor[stage];
        }
    }

    public GameObject GetProgressBarInstance()
    {
        return _progressBarInstance;
    }
    void OnDisable()
    {
        // 取消订阅，必须成对出现！
        TimeManager.OnDayChanged -= GrowOneDay;
    }
}
