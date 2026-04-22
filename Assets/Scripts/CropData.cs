using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 右键可直接创建物品数据资产
[CreateAssetMenu(fileName = "NewItem", menuName = "RPG/CropData")]
public class CropData : ItemData
{
    [Header("作物设置")]
    public int totalGrowDays = 4; // 总生长天数
    public Color[] growStageColor; // 每个生长阶段的精灵/材质
    

   
    
}
