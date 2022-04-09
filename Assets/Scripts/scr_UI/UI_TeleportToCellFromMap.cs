using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_TeleportToCellFromMap : MonoBehaviour, IPointerClickHandler
{ 
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private Manager_CurrentCell CurrentCellScript;
    [SerializeField] private GameObject par_Managers;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (par_Managers.GetComponent<UI_PlayerMenu>().openedMapUI)
        {
            par_Managers.GetComponent<Manager_UIReuse>().par_TeleportCheck.SetActive(true);

            par_Managers.GetComponent<Manager_UIReuse>().txt_Teleport.text = "Teleport to " + CurrentCellScript.str_CellName + "?";

            par_Managers.GetComponent<Manager_UIReuse>().btn_ConfirmTeleport.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_ConfirmTeleport.onClick.AddListener(ConfirmTeleport);

            par_Managers.GetComponent<Manager_UIReuse>().btn_CancelTeleport.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_CancelTeleport.onClick.AddListener(CancelTeleport);
        }
    }

    public void ConfirmTeleport()
    {
        thePlayer.transform.position = CurrentCellScript.currentCellSpawnpoint.position;

        par_Managers.GetComponent<Manager_UIReuse>().par_TeleportCheck.SetActive(false);
        par_Managers.GetComponent<UI_PlayerMenu>().ClosePlayerMenuUI();

        Debug.Log("Teleported to cell " + CurrentCellScript.str_CellName + "!");
    }
    public void CancelTeleport()
    {
        par_Managers.GetComponent<Manager_UIReuse>().par_TeleportCheck.SetActive(false);
    }
}