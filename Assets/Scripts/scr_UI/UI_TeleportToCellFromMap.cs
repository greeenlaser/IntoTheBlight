using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_TeleportToCellFromMap : MonoBehaviour, IPointerClickHandler
{ 
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject noclipCamera;
    [SerializeField] private Manager_CurrentCell CurrentCellScript;
    [SerializeField] private Manager_Console ConsoleScript;
    [SerializeField] private UI_PlayerMenu PlayerMenuScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (PlayerMenuScript.openedMapUI)
        {
            UIReuseScript.par_TeleportCheck.SetActive(true);

            UIReuseScript.txt_Teleport.text = "Teleport to " + CurrentCellScript.str_CellName + "?";

            UIReuseScript.btn_ConfirmTeleport.onClick.RemoveAllListeners();
            UIReuseScript.btn_ConfirmTeleport.onClick.AddListener(ConfirmTeleport);

            UIReuseScript.btn_CancelTeleport.onClick.RemoveAllListeners();
            UIReuseScript.btn_CancelTeleport.onClick.AddListener(CancelTeleport);
        }
    }

    public void ConfirmTeleport()
    {
        if (!ConsoleScript.noclipEnabled)
        {
            thePlayer.transform.position = CurrentCellScript.currentCellSpawnpoint.position;
        }
        else if (ConsoleScript.noclipEnabled)
        {
            noclipCamera.transform.position = CurrentCellScript.currentCellSpawnpoint.position;
        }

        UIReuseScript.par_TeleportCheck.SetActive(false);
        PlayerMenuScript.ClosePlayerMenuUI();

        Debug.Log("Teleported to cell " + CurrentCellScript.str_CellName + "!");
    }
    public void CancelTeleport()
    {
        UIReuseScript.par_TeleportCheck.SetActive(false);
    }
}