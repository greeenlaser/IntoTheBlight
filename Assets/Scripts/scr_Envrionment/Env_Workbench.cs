using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Workbench : MonoBehaviour
{
    [Header("Assignables")]
    public string str_workbenchName;
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public List<GameObject> buttons;

    //private variables
    private bool isWorkbenchUIOpen;
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        UIReuseScript = par_Managers.GetComponent<Manager_UIReuse>();

        UIReuseScript.btn_ShowRepair.gameObject.SetActive(false);
        UIReuseScript.btn_ShowUpgrades.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)
            && isWorkbenchUIOpen
            && !par_Managers.GetComponent<UI_PauseMenu>().isUIOpen)
        {
            par_Managers.GetComponent<UI_PauseMenu>().isInventoryOpen = false;
            CloseWorkbenchUI();
        }
    }

    public void OpenWorkbenchUI()
    {
        isWorkbenchUIOpen = true;

        PlayerInventoryScript.Workbench = gameObject;

        UIReuseScript.btn_CloseUI.gameObject.SetActive(true);
        UIReuseScript.btn_CloseUI.onClick.RemoveAllListeners();
        UIReuseScript.btn_CloseUI.onClick.AddListener(CloseWorkbenchUI);

        UIReuseScript.btn_ShowRepair.gameObject.SetActive(true);
        UIReuseScript.btn_ShowRepair.interactable = false;
        UIReuseScript.btn_ShowRepair.onClick.RemoveAllListeners();
        UIReuseScript.btn_ShowRepair.onClick.AddListener(OpenRepairUI);

        UIReuseScript.btn_ShowUpgrades.gameObject.SetActive(true);
        UIReuseScript.btn_ShowUpgrades.interactable = false;
        UIReuseScript.btn_ShowUpgrades.onClick.RemoveAllListeners();
        UIReuseScript.btn_ShowUpgrades.onClick.AddListener(OpenUpgradeUI);

        PlayerInventoryScript.CloseInventory();
        PlayerInventoryScript.isPlayerAndWorkbenchOpen = true;
        UIReuseScript.par_Inventory.SetActive(true);
        UIReuseScript.par_Stats.SetActive(true);

        OpenRepairUI();

        par_Managers.GetComponent<UI_PauseMenu>().isInventoryOpen = true;
        par_Managers.GetComponent<UI_PauseMenu>().PauseGameAndCloseUIAndResetBools();
    }
    public void OpenRepairUI()
    {
        RebuildUI();
        UIReuseScript.RebuildRepairMenu();

        UIReuseScript.btn_ShowRepair.interactable = false;
        UIReuseScript.btn_ShowUpgrades.interactable = true;
    }
    public void OpenUpgradeUI()
    {
        RebuildUI();
        UIReuseScript.RebuildUpgradeUI();

        UIReuseScript.btn_ShowUpgrades.interactable = false;
        UIReuseScript.btn_ShowRepair.interactable = true;
    }
    private void RebuildUI()
    {
        UIReuseScript.ClearAllInventories();
        UIReuseScript.ClearStatsUI();
        UIReuseScript.ClearInventoryUI();

        UIReuseScript.txt_InventoryName.text = str_workbenchName.ToString();
        PlayerInventoryScript.UpdatePlayerInventoryStats();
    }

    public void CloseWorkbenchUI()
    {
        UIReuseScript.HideItemUpgradeUI();

        PlayerInventoryScript.canOpenPlayerInventory = false;
        PlayerInventoryScript.closedInventoryThroughContainer = true;

        PlayerInventoryScript.hasOpenedInventoryOnce = true;
        PlayerInventoryScript.CloseInventory();
        PlayerInventoryScript.Workbench = null;

        UIReuseScript.btn_CloseUI.gameObject.SetActive(false);

        UIReuseScript.btn_ShowRepair.gameObject.SetActive(false);
        UIReuseScript.btn_ShowUpgrades.gameObject.SetActive(false);

        isWorkbenchUIOpen = false;

        par_Managers.GetComponent<UI_PlayerMenu>().ClosePlayerMenuUI();
        UIReuseScript.StartCoroutine("Wait");
    }
}