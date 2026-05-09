using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private GameObject _interactPanel;
    void OnEnable()
    {
        PlayerEvents.Center.AddListener(PlayerEvent.EnterShop,OpenShop);
        PlayerEvents.Center.AddListener(PlayerEvent.ExitShop,CloseShop);
        PlayerEvents.Center.AddListener<string,string>(PlayerEvent.EnterInteractPanel,OpenInteractPanel);
        PlayerEvents.Center.AddListener(PlayerEvent.ExitInteractPanel,CloseInteractPanel);
        PlayerEvents.Center.AddListener<Crop>(PlayerEvent.ShowCropProgress,OpenCropProgressPanel);
        PlayerEvents.Center.AddListener<Crop>(PlayerEvent.HideCropProgress,CloseCropProgressPanel);
    }




    void OpenShop()
    {
        _shopPanel.SetActive(true);
        Debug.Log("Open Shop");
    }

    void CloseShop()
    {
        _shopPanel.SetActive(false);
        Debug.Log("Close Shop");
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

        void OnDisable()
    {
        PlayerEvents.Center.RemoveListener(PlayerEvent.EnterShop,OpenShop);
        PlayerEvents.Center.RemoveListener(PlayerEvent.ExitShop,CloseShop);
        PlayerEvents.Center.RemoveListener<string,string>(PlayerEvent.EnterInteractPanel,OpenInteractPanel);
        PlayerEvents.Center.RemoveListener(PlayerEvent.ExitInteractPanel,CloseInteractPanel);
        PlayerEvents.Center.RemoveListener<Crop>(PlayerEvent.ShowCropProgress,OpenCropProgressPanel);
        PlayerEvents.Center.RemoveListener<Crop>(PlayerEvent.HideCropProgress,CloseCropProgressPanel);
    }
}
