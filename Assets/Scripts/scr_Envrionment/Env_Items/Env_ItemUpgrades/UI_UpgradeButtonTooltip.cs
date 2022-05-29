using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_UpgradeButtonTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //public but hidden variables
    [HideInInspector] public GameObject target;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (target != null)
        {
            if (target.GetComponent<Upgrade_Gun>() != null)
            {
                target.GetComponent<Upgrade_Gun>().ShowTooltipText();
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (target != null)
        {
            if (target.GetComponent<Upgrade_Gun>() != null)
            {
                target.GetComponent<Upgrade_Gun>().TooltipScript.showTooltipUI = false;
            }
        }
    }
}