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
    [HideInInspector] public bool isWorkbenchRepairUIOpen;
    [HideInInspector] public bool isWorkbenchUpgradeUIOpen;
    [HideInInspector] public List<GameObject> buttons;

    //private variables
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
            && isWorkbenchRepairUIOpen
            && !par_Managers.GetComponent<UI_PauseMenu>().isUIOpen)
        {
            par_Managers.GetComponent<UI_PauseMenu>().isInventoryOpen = false;
            CloseWorkbenchUI();
        }
    }

    public void OpenWorkbenchUI()
    {
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

        OpenRepairUI();

        par_Managers.GetComponent<UI_PauseMenu>().PauseGameAndCloseUIAndResetBools();
    }
    public void OpenRepairUI()
    {
        isWorkbenchUpgradeUIOpen = false;
        isWorkbenchRepairUIOpen = true;

        RebuildUI();
        UIReuseScript.RebuildRepairMenu();

        UIReuseScript.btn_ShowRepair.interactable = false;
        UIReuseScript.btn_ShowUpgrades.interactable = true;
    }
    public void OpenUpgradeUI()
    {
        isWorkbenchRepairUIOpen = false;
        isWorkbenchUpgradeUIOpen = true;

        RebuildUI();
        UIReuseScript.RebuildRepairMenu();

        UIReuseScript.btn_ShowUpgrades.interactable = false;
        UIReuseScript.btn_ShowRepair.interactable = true;
    }
    private void RebuildUI()
    {
        UIReuseScript.ClearAllInventories();
        UIReuseScript.ClearStatsUI();
        UIReuseScript.ClearInventoryUI();

        PlayerInventoryScript.CloseInventory();
        PlayerInventoryScript.isPlayerAndRepairOpen = true;
        UIReuseScript.par_Inventory.SetActive(true);
        UIReuseScript.par_Stats.SetActive(true);
        UIReuseScript.txt_InventoryName.text = str_workbenchName.ToString();
        PlayerInventoryScript.UpdatePlayerInventoryStats();

        par_Managers.GetComponent<UI_PauseMenu>().isInventoryOpen = true;
    }

    public void CloseWorkbenchUI()
    {
        PlayerInventoryScript.canOpenPlayerInventory = false;
        PlayerInventoryScript.closedInventoryThroughContainer = true;

        PlayerInventoryScript.hasOpenedInventoryOnce = true;
        PlayerInventoryScript.CloseInventory();
        PlayerInventoryScript.Workbench = null;

        UIReuseScript.btn_CloseUI.gameObject.SetActive(false);

        UIReuseScript.btn_ShowRepair.gameObject.SetActive(false);
        UIReuseScript.btn_ShowUpgrades.gameObject.SetActive(false);

        isWorkbenchRepairUIOpen = false;

        par_Managers.GetComponent<UI_PlayerMenu>().ClosePlayerMenuUI();
        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        PlayerInventoryScript.canOpenPlayerInventory = false;
        yield return new WaitForSeconds(0.2f);
        PlayerInventoryScript.canOpenPlayerInventory = true;
    }
}