using UnityEngine;
using UnityEngine.UI;
public class ItemSlot : MonoBehaviour
{



    [Header("UI组件引用")]
    [SerializeField] private Image itemIcon; // 物品图标组件
    [SerializeField] private Text countText; // 堆叠数量文本
    [SerializeField] private Image slotBackground; // 槽位背景



    [Header("当前槽位数据")]
    public ItemData currentItemData; // 当前槽位的物品数据
    public int currentItemCount; // 当前物品数量


    void OnEnable()
    {
        PlayerEvents.Center.AddListener<GameObject>(PlayerEvent.UseItem,OnSlotClick);
    }
    // 刷新槽位UI（核心方法，物品变化时调用）
    public void RefreshSlot()
    {
        // 有物品的情况
        if (currentItemData != null && currentItemCount > 0)
        {
            itemIcon.sprite = currentItemData.itemIcon; // 赋值图标
            itemIcon.enabled = true; // 显示图标

            // 堆叠数量大于1才显示文本
            countText.text = currentItemCount > 1 ? currentItemCount.ToString() : "";
            countText.gameObject.SetActive(currentItemCount > 1);
        }
        // 空槽位的情况
        else
        {
            ClearSlot();
        }
    }

    // 清空槽位
    public void ClearSlot()
    {
        currentItemData = null;
        currentItemCount = 0;

        itemIcon.sprite = null;
        itemIcon.enabled = false; // 隐藏图标
        countText.gameObject.SetActive(false); // 隐藏数量文本
    }

    // 槽位点击事件，绑定到Button组件的OnClick
    public void OnSlotClick(GameObject obj)
    {

        //currentItemData = obj.GetComponent<ItemModel>().itemData;
        if (currentItemData != null)
        {
            currentItemData.UseItem(); // 调用物品使用方法
            // 消耗品使用后减少数量，刷新UI
            if (currentItemData.itemType == ItemType.Consumable )
            {
                if(!PlayerController.CurrentFarmLand)return;
                if(PlayerController.CurrentFarmLand.FarmCurrentStatue != FarmCurrentStatus.Tilled)return;
                //种花
                GameObject flower = Instantiate(currentItemData.itemPrefab, PlayerController.CurrentFarmLand.PlantPosition.gameObject.transform);
                Crop crop = flower.GetComponent<Crop>();
                crop.IsMature = false;
                currentItemCount--;
                if (currentItemCount == 0)
                {
                    Destroy(PlayerController.EquipMentRoot.GetChild(0).gameObject); // 卸下当前装备的物品
                    if (ToolBarManager.Instance.allToolSlots[0].currentItemData != null)
                    {
                        ToolBarManager.Instance.currentItemSlot = ToolBarManager.Instance.allToolSlots[0];
                    }
                    else
                    {
                        ToolBarManager.Instance.currentItemSlot = null;
                    }
                }

                RefreshSlot();

                PlayerController.CurrentFarmLand.FarmCurrentStatue = FarmCurrentStatus.Planted;
            }
            else if (currentItemData.itemType == ItemType.Equipment)
            {

                if (PlayerController.CurrentFarmLand && PlayerController.CurrentFarmLand.FarmCurrentStatue == FarmCurrentStatus.Empty)
                {
                   PlayerController.CurrentFarmLand.FarmCurrentStatue = FarmCurrentStatus.Tilled;
                }
                else
                {
                    Debug.Log("未识别到未开垦的田");
                }

            }
        }
    }

    void OnDisable()
    {
        PlayerEvents.Center.RemoveListener<GameObject>(PlayerEvent.UseItem,OnSlotClick);
    }
}