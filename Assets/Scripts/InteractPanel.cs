using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractPanel : MonoBehaviour
{
    [SerializeField] private Text _interactText;
    [SerializeField] private Text _interactButtonText;

   public void SetInteractText(string interactText,string interactButtonText)
    {
        _interactText.text = interactText;
        _interactButtonText.text = interactButtonText;
    }
}
