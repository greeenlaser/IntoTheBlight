using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_RepairContent : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private UI_AIContent AIScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool isNPCRepairUIOpen;
    [HideInInspector] public List<GameObject> buttons;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && isNPCRepairUIOpen)
        {
            CloseRepairAndPlayerInventory();
        }
    }

    public void OpenRepairUI()
    {
        PlayerInventoryScript.Trader = gameObject;

        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.gameObject.SetActive(true);
        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.onClick.AddListener(CloseRepairAndPlayerInventory);

        PlayerInventoryScript.CloseInventory();
        PlayerInventoryScript.isPlayerAndWorkbenchOpen = true;
        par_Managers.GetComponent<Manager_UIReuse>().par_Inventory.SetActive(true);
        par_Managers.GetComponent<Manager_UIReuse>().par_Stats.SetActive(true);
        par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = gameObject.GetComponent<UI_AIContent>().str_NPCName + "'s repair shop";
        PlayerInventoryScript.UpdatePlayerInventoryStats();

        par_Managers.GetComponent<Manager_UIReuse>().RebuildRepairMenu();

        isNPCRepairUIOpen = true;
    }
    public void CloseRepairUI()
    {
        par_Managers.GetComponent<Manager_UIReuse>().par_Inventory.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().par_Stats.SetActive(false);

        par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
        par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
        par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();

        PlayerInventoryScript.hasOpenedInventoryOnce = false;
    }
    public void CloseRepairAndPlayerInventory()
    {
        PlayerInventoryScript.canOpenPlayerInventory = false;
        PlayerInventoryScript.closedInventoryThroughContainer = true;
        PlayerInventoryScript.CloseRepair();

        PlayerInventoryScript.hasOpenedInventoryOnce = true;
        PlayerInventoryScript.CloseInventory();
        PlayerInventoryScript.Trader = null;

        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.gameObject.SetActive(false);
        AIScript.OpenNPCDialogue();

        isNPCRepairUIOpen = false;
    }
}