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
    [HideInInspector] public bool isActive;
    [HideInInspector] public bool isWorkbenchRepairUIOpen;
    [HideInInspector] public List<GameObject> buttons = new List<GameObject>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)
            && isWorkbenchRepairUIOpen
            && !par_Managers.GetComponent<UI_PauseMenu>().isUIOpen)
        {
            par_Managers.GetComponent<UI_PauseMenu>().isInventoryOpen = false;
            CloseRepairUI();
        }
    }

    public void OpenRepairUI()
    {
        PlayerInventoryScript.Workbench = gameObject;

        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.gameObject.SetActive(true);
        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.onClick.AddListener(CloseRepairUI);

        PlayerInventoryScript.CloseInventory();
        PlayerInventoryScript.isPlayerAndRepairOpen = true;
        par_Managers.GetComponent<Manager_UIReuse>().par_Inventory.SetActive(true);
        par_Managers.GetComponent<Manager_UIReuse>().par_Stats.SetActive(true);
        par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = str_workbenchName.ToString();
        PlayerInventoryScript.UpdatePlayerInventoryStats();

        par_Managers.GetComponent<Manager_UIReuse>().RebuildRepairMenu();
        par_Managers.GetComponent<UI_PauseMenu>().isInventoryOpen = true;
        par_Managers.GetComponent<UI_PauseMenu>().PauseGameAndCloseUIAndResetBools();

        isWorkbenchRepairUIOpen = true;
    }
    public void CloseRepairUI()
    {
        PlayerInventoryScript.canOpenPlayerInventory = false;
        PlayerInventoryScript.closedInventoryThroughContainer = true;

        PlayerInventoryScript.hasOpenedInventoryOnce = true;
        PlayerInventoryScript.CloseInventory();
        PlayerInventoryScript.Workbench = null;

        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.gameObject.SetActive(false);

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