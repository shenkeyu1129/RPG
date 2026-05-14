using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    /// <summary>
    /// 是否有模态面板打开（商店/背包），PlayerController 据此阻止游戏内点击
    /// </summary>
    public static bool IsAnyPanelOpen { get; set; }

    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private GameObject _interactPanel;
    [SerializeField] private ToolTipPanel _toolTipPanel;
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private GameObject _questPanel;

    void OnEnable()
    {
        PlayerEvents.Center.AddListener(PlayerEvent.EnterShop,OpenShop);
        PlayerEvents.Center.AddListener(PlayerEvent.ExitShop,CloseShop);
        PlayerEvents.Center.AddListener<string,string>(PlayerEvent.EnterInteractPanel,OpenInteractPanel);
        PlayerEvents.Center.AddListener(PlayerEvent.ExitInteractPanel,CloseInteractPanel);
        PlayerEvents.Center.AddListener<Crop>(PlayerEvent.ShowCropProgress,OpenCropProgressPanel);
        PlayerEvents.Center.AddListener<Crop>(PlayerEvent.HideCropProgress,CloseCropProgressPanel);
        PlayerEvents.Center.AddListener<string>(PlayerEvent.ShowToolName,ShowToolTip);
        PlayerEvents.Center.AddListener(PlayerEvent.HideToolName,HideToolTip);
        PlayerEvents.Center.AddListener(PlayerEvent.ToggleInventory, ToggleInventoryPanel);
        PlayerEvents.Center.AddListener(PlayerEvent.ToggleQuest, ToggleQuestPanel);
    }




    void OpenShop()
    {
        _shopPanel.SetActive(true);
        IsAnyPanelOpen = true;
        var shopUI = _shopPanel.GetComponent<ShopPanelUI>();
        if (shopUI != null) shopUI.OnOpen();
        Debug.Log("Open Shop");
    }

    void CloseShop()
    {
        _shopPanel.SetActive(false);
        RefreshPanelState();
        StartCoroutine(DelayedCursorLock());
        Debug.Log("Close Shop");
    }

    private System.Collections.IEnumerator DelayedCursorLock()
    {
        yield return null;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OpenInteractPanel(String interactText,String interactButtonText)
    {
        _interactPanel.GetComponent<InteractPanel>().SetInteractText(interactText,interactButtonText);
        _interactPanel.SetActive(true);
        Debug.Log("Open Interact Panel");
    }

    void CloseInteractPanel()
    {
        _interactPanel.SetActive(false);
        Debug.Log("Close Interact Panel");
    }

    void OpenCropProgressPanel(Crop crop)
    {
        GameObject progressBar = crop.GetProgressBarInstance();
        progressBar.SetActive(true);
    }

    void CloseCropProgressPanel(Crop crop)
    {
        GameObject progressBar = crop.GetProgressBarInstance();
        progressBar.SetActive(false);
    }

    void ShowToolTip(string itemName)
    {
        if (_toolTipPanel != null)
            _toolTipPanel.Show(itemName);
    }

    void HideToolTip()
    {
        if (_toolTipPanel != null)
            _toolTipPanel.Hide();
    }

    void ToggleInventoryPanel()
    {
        if (_shopPanel != null && _shopPanel.activeSelf) return;
        if (_inventoryPanel != null && !_inventoryPanel.activeSelf)
        {
            _inventoryPanel.SetActive(true);
            IsAnyPanelOpen = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            AudioEvents.Center.Trigger<string>(AudioEvent.PlaySFX, "UIClick");
        }
    }

    void ToggleQuestPanel()
    {
        if (_questPanel == null)
        {
            var found = transform.Find("QuestPanel");
            if (found != null) _questPanel = found.gameObject;
        }
        if (_questPanel == null) return;
        bool isActive = !_questPanel.activeSelf;
        _questPanel.SetActive(isActive);
        IsAnyPanelOpen = isActive;
        Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isActive;
        if (isActive)
            AudioEvents.Center.Trigger<string>(AudioEvent.PlaySFX, "UIClick");
    }

    /// <summary>
    /// 刷新面板状态标记（面板关闭时调用）
    /// </summary>
    private void RefreshPanelState()
    {
        bool shopOpen = _shopPanel != null && _shopPanel.activeSelf;
        bool invOpen = _inventoryPanel != null && _inventoryPanel.activeSelf;
        IsAnyPanelOpen = shopOpen || invOpen;
    }

        void OnDisable()
    {
        PlayerEvents.Center.RemoveListener(PlayerEvent.EnterShop,OpenShop);
        PlayerEvents.Center.RemoveListener(PlayerEvent.ExitShop,CloseShop);
        PlayerEvents.Center.RemoveListener<string,string>(PlayerEvent.EnterInteractPanel,OpenInteractPanel);
        PlayerEvents.Center.RemoveListener(PlayerEvent.ExitInteractPanel,CloseInteractPanel);
        PlayerEvents.Center.RemoveListener<Crop>(PlayerEvent.ShowCropProgress,OpenCropProgressPanel);
        PlayerEvents.Center.RemoveListener<Crop>(PlayerEvent.HideCropProgress,CloseCropProgressPanel);
        PlayerEvents.Center.RemoveListener<string>(PlayerEvent.ShowToolName,ShowToolTip);
        PlayerEvents.Center.RemoveListener(PlayerEvent.HideToolName,HideToolTip);
        PlayerEvents.Center.RemoveListener(PlayerEvent.ToggleInventory, ToggleInventoryPanel);
        PlayerEvents.Center.RemoveListener(PlayerEvent.ToggleQuest, ToggleQuestPanel);
    }
}
