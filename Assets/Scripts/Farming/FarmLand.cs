using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//田的所有事件
public enum FarmEvent
{
    FarmStatueChanged,
    FarmColorChanged,
}

//田的事件管理中心
public static class FarmEvents
{
    public static readonly EventCenter<FarmEvent> Center = new();
}
// 田地完整生命周期状态
public enum FarmCurrentStatus
{
    Empty,          // 空地：未开垦
    Tilled,         // 已开垦：用锄头翻土
    Watered,        // 已浇水
    Planted,        // 已种下种子
    Growing,        // 生长中
    Mature,         // 成熟可收获

}
public class FarmLand : MonoBehaviour
{
    public Transform PlantPosition;
    private FarmCurrentStatus _farmCurrentStatue;

    private Renderer _renderer;

    private Material _material;

    [SerializeField] private GameObject border;
    public FarmCurrentStatus FarmCurrentStatue
    {
        set
        {
            if (value != _farmCurrentStatue)
            {
                _farmCurrentStatue = value;
                FarmEvents.Center.Trigger(FarmEvent.FarmStatueChanged);
            }
        }
        get => _farmCurrentStatue;
    }
    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _material = _renderer.material;
        _material.color = new Color32(95, 160, 120, 255);//绿色
        _farmCurrentStatue = FarmCurrentStatus.Empty;

    }

    void OnEnable()
    {
        FarmEvents.Center.AddListener(FarmEvent.FarmStatueChanged, ChangeColor);
    }

    //改变土地颜色，不同状态对应不同颜色的土地
    public void ChangeColor()
    {
        switch (_farmCurrentStatue)
        {
            case FarmCurrentStatus.Empty:
                _material.color = new Color32(95, 160, 120, 255);//绿色
                break;
            case FarmCurrentStatus.Tilled:
                _material.color = new Color32(128, 89, 51, 255);//浅褐色
                break;
            case FarmCurrentStatus.Watered:
                _material.color = new Color32(102, 64, 26, 255);//泥土褐色
                break;
            default:
                _material.color = new Color32(128, 89, 51, 255);//浅褐色
                break;
        }
        FarmEvents.Center.Trigger(FarmEvent.FarmColorChanged);
    }

    //获取天地边框
    public GameObject GetBorder()
    {
        return border;
    }
   
    void OnDisable()
    {
        FarmEvents.Center.RemoveListener(FarmEvent.FarmStatueChanged, ChangeColor);
    }

}
