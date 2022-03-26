using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Workbench : MonoBehaviour
{
    [Header("Assignables")]
    public string str_workbenchName;
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private UI_PlayerMenu PlayerMenuScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;
    [SerializeField] private UI_PauseMenu PauseMenuScript;

    //public but hidden variables
    [HideInInspector] public bool isActive;
    [HideInInspector] public bool isWorkbenchRepairUIOpen;
    [HideInInspector] public List<GameObject> buttons = new List<GameObject>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)
            && isWorkbenchRepairUIOpen
            && !PauseMenuScript.isUIOpen)
        {
            PauseMenuScript.isInventoryOpen = false;
            CloseRepairUI();
        }
    }

    public void OpenRepairUI()
    {
        PlayerInventoryScript.Workbench = gameObject;

        UIReuseScript.btn_CloseUI.gameObject.SetActive(true);
        UIReuseScript.btn_CloseUI.onClick.RemoveAllListeners();
        UIReuseScript.btn_CloseUI.onClick.AddListener(CloseRepairUI);

        PlayerInventoryScript.CloseInventory();
        PlayerInventoryScript.isPlayerAndRepairOpen = true;
        UIReuseScript.par_Inventory.SetActive(true);
        UIReuseScript.par_Stats.SetActive(true);
        UIReuseScript.txt_InventoryName.text = str_workbenchName.ToString();
        PlayerInventoryScript.UpdatePlayerInventoryStats();

        UIReuseScript.RebuildRepairMenu();
        PauseMenuScript.isInventoryOpen = true;
        PauseMenuScript.PauseGameAndCloseUIAndResetBools();

        isWorkbenchRepairUIOpen = true;
    }
    public void CloseRepairUI()
    {
        PlayerInventoryScript.canOpenPlayerInventory = false;
        PlayerInventoryScript.closedInventoryThroughContainer = true;

        PlayerInventoryScript.hasOpenedInventoryOnce = true;
        PlayerInventoryScript.CloseInventory();
        PlayerInventoryScript.Workbench = null;

        UIReuseScript.btn_CloseUI.onClick.RemoveAllListeners();
        UIReuseScript.btn_CloseUI.gameObject.SetActive(false);

        isWorkbenchRepairUIOpen = false;

        PlayerMenuScript.ClosePlayerMenuUI();
        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        PlayerInventoryScript.canOpenPlayerInventory = false;
        yield return new WaitForSeconds(0.2f);
        PlayerInventoryScript.canOpenPlayerInventory = true;
    }
}