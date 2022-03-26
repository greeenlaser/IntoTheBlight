using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_RepairContent : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private UI_AIContent AIScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;
    [SerializeField] private UI_PauseMenu PauseMenuScript;

    //public but hidden variables
    [HideInInspector] public bool isNPCRepairUIOpen;
    [HideInInspector] public List<GameObject> buttons = new List<GameObject>();


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

        UIReuseScript.CloseDialogueUI();

        UIReuseScript.btn_CloseUI.gameObject.SetActive(true);
        UIReuseScript.btn_CloseUI.onClick.RemoveAllListeners();
        UIReuseScript.btn_CloseUI.onClick.AddListener(CloseRepairAndPlayerInventory);

        PlayerInventoryScript.CloseInventory();
        PlayerInventoryScript.isPlayerAndRepairOpen = true;
        UIReuseScript.par_Inventory.SetActive(true);
        UIReuseScript.par_Stats.SetActive(true);
        UIReuseScript.txt_InventoryName.text = gameObject.GetComponent<UI_AIContent>().str_NPCName + "'s repair shop";
        PlayerInventoryScript.UpdatePlayerInventoryStats();

        UIReuseScript.RebuildRepairMenu();

        isNPCRepairUIOpen = true;
    }
    public void CloseRepairUI()
    {
        UIReuseScript.par_Inventory.SetActive(false);
        UIReuseScript.par_Stats.SetActive(false);

        UIReuseScript.ClearAllInventories();
        UIReuseScript.ClearInventoryUI();
        UIReuseScript.ClearStatsUI();

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

        UIReuseScript.btn_CloseUI.onClick.RemoveAllListeners();
        UIReuseScript.btn_CloseUI.gameObject.SetActive(false);
        AIScript.CheckIfAnyQuestIsCompleted();

        isNPCRepairUIOpen = false;
    }
}