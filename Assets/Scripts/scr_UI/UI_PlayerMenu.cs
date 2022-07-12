using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerMenu : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private Player_Movement PlayerMovementScript;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private Player_Health PlayerHealthScript;
    [SerializeField] private UI_Tooltip TooltipScript;
    [SerializeField] private UI_AbilityManager AbilityManagerScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool isPlayerMenuOpen;
    [HideInInspector] public bool openedInventoryUI;
    [HideInInspector] public bool openedQuestUI;
    [HideInInspector] public bool openedStatsUI;
    [HideInInspector] public bool openedUpgradeUI;
    [HideInInspector] public bool openedFactionUI;
    [HideInInspector] public bool openedRadioUI;
    [HideInInspector] public bool openedMapUI;
    [HideInInspector] public GameObject lockpickUI;

    //private variables
    private int currentUI;
    private readonly int inventoryUICode = 0;
    private readonly int questUICode = 1;
    private readonly int statsUICode = 2;
    private readonly int upgradeUICode = 3;
    private readonly int factionUICode = 4;
    private readonly int radioUICode = 5;
    private readonly int mapUICode = 6;
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        UIReuseScript = par_Managers.GetComponent<Manager_UIReuse>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) 
            && PlayerHealthScript.isPlayerAlive
            && !PlayerMovementScript.isStunned
            && !par_Managers.GetComponent<Manager_GameSaving>().isLoading)
        {
            //opens player inventory at start by default
            //if pressing tab and container isnt assigned
            if (PlayerInventoryScript.canOpenPlayerInventory
                && !PlayerInventoryScript.isPlayerAndContainerOpen
                && !PlayerInventoryScript.closedInventoryThroughContainer
                && !PlayerInventoryScript.isPlayerInventoryOpen
                && !isPlayerMenuOpen
                && !par_Managers.GetComponent<UI_PauseMenu>().isUIOpen
                && !par_Managers.GetComponent<UI_PauseMenu>().isWaitableUIOpen
                && lockpickUI == null)
            {
                openedInventoryUI = true;
                OpenPlayerMenuUI();
                OpenInventory();
            }
            //closes player menu UI and any other UI that is currently open
            else if (isPlayerMenuOpen
                    && (openedInventoryUI
                    || openedQuestUI
                    || openedStatsUI
                    || openedUpgradeUI
                    || openedFactionUI
                    || openedRadioUI
                    || openedMapUI))
            {
                ClosePlayerMenuUI();
            }
        }

        if (!par_Managers.GetComponent<Manager_Console>().consoleOpen)
        {
            //move up in "player menus list"
            if (Input.GetKeyDown(KeyCode.UpArrow) && isPlayerMenuOpen)
            {
                currentUI--;
                if (currentUI == -1)
                {
                    if (!AbilityManagerScript.hasExoskeleton)
                    {
                        currentUI = 1;
                    }
                    else if (AbilityManagerScript.hasExoskeleton)
                    {
                        currentUI = 6;
                    }
                }
                UpdateUI();
            }
            //move down in "player menus list"
            if (Input.GetKeyDown(KeyCode.DownArrow) && isPlayerMenuOpen)
            {
                currentUI++;
                if (!AbilityManagerScript.hasExoskeleton
                    && currentUI == 2)
                {
                    currentUI = 0;
                }
                else if (AbilityManagerScript.hasExoskeleton
                         && currentUI == 7)
                {
                    currentUI = 0;
                }
                UpdateUI();
            }
        }
    }
    private void UpdateUI()
    {
        if (currentUI == inventoryUICode && !openedInventoryUI)
        {
            OpenInventory();
        }
        else if (currentUI == questUICode && !openedQuestUI)
        {
            OpenQuests();
        }
        else if (currentUI == statsUICode && !openedStatsUI)
        {
            OpenStats();
        }
        else if (currentUI == upgradeUICode && !openedUpgradeUI)
        {
            OpenUpgrades();
        }
        else if (currentUI == factionUICode && !openedFactionUI)
        {
            OpenFactions();
        }
        else if (currentUI == radioUICode && !openedRadioUI)
        {
            OpenRadio();
        }
        else if (currentUI == mapUICode && !openedMapUI)
        {
            OpenMap();
        }
    }
    //open main player UI
    public void OpenPlayerMenuUI()
    {
        UIReuseScript.par_PlayerMenu.SetActive(true);

        UIReuseScript.btn_CloseUI.gameObject.SetActive(true);
        UIReuseScript.btn_CloseUI.onClick.RemoveAllListeners();
        UIReuseScript.btn_CloseUI.onClick.AddListener(ClosePlayerMenuUI);

        isPlayerMenuOpen = true;
        currentUI = 0;
    }
    //open player inventory
    public void OpenInventory()
    {
        openedInventoryUI = true;
        openedStatsUI = false;
        openedQuestUI = false;
        openedFactionUI = false;
        openedRadioUI = false;
        openedMapUI = false;

        UIReuseScript.btn_Inventory.GetComponent<Button>().interactable = false;
        UIReuseScript.btn_Quests.GetComponent<Button>().interactable = true;
        if (!AbilityManagerScript.hasExoskeleton)
        {
            UIReuseScript.btn_Stats.GetComponent<Button>().interactable = false;
            UIReuseScript.btn_Upgrades.GetComponent<Button>().interactable = false;
            UIReuseScript.btn_Factions.GetComponent<Button>().interactable = false;
            UIReuseScript.btn_Radio.GetComponent<Button>().interactable = false;
            UIReuseScript.btn_Map.GetComponent<Button>().interactable = false;
        }
        else if (AbilityManagerScript.hasExoskeleton)
        {
            UIReuseScript.btn_Stats.GetComponent<Button>().interactable = true;
            UIReuseScript.btn_Upgrades.GetComponent<Button>().interactable = true;
            UIReuseScript.btn_Factions.GetComponent<Button>().interactable = true;
            UIReuseScript.btn_Radio.GetComponent<Button>().interactable = true;
            UIReuseScript.btn_Map.GetComponent<Button>().interactable = true;
        }


        par_Managers.GetComponent<UI_AcceptedQuests>().CloseQuests();
        UIReuseScript.par_PlayerMenuStats.SetActive(false);
        TooltipScript.showTooltipUI = false;
        AbilityManagerScript.HideButtons();
        UIReuseScript.par_PlayerUpgrades.SetActive(false);
        UIReuseScript.par_PlayerFactionUI.SetActive(false);
        UIReuseScript.par_PlayerMenuRadio.SetActive(false);
        UIReuseScript.Minimap.transform.SetParent(UIReuseScript.par_MinimapMask.transform, false);
        UIReuseScript.MinimapPlayerPosition.transform.SetParent(UIReuseScript.par_MinimapMask.transform, false);
        UIReuseScript.MinimapPlayerPosition.transform.position = par_Managers.GetComponent<UI_Minimap>().playerPosition;
        UIReuseScript.par_PlayerMenuMap.SetActive(false);

        PlayerInventoryScript.OpenInventory();
        //Debug.Log("Opened player inventory UI");
    }
    //open quests
    public void OpenQuests()
    {
        openedInventoryUI = false;
        openedQuestUI = true;
        openedStatsUI = false;
        openedUpgradeUI = false;
        openedFactionUI = false;
        openedRadioUI = false;
        openedMapUI = false;

        UIReuseScript.btn_Inventory.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Quests.GetComponent<Button>().interactable = false;
        if (!AbilityManagerScript.hasExoskeleton)
        {
            UIReuseScript.btn_Stats.GetComponent<Button>().interactable = false;
            UIReuseScript.btn_Upgrades.GetComponent<Button>().interactable = false;
            UIReuseScript.btn_Factions.GetComponent<Button>().interactable = false;
            UIReuseScript.btn_Radio.GetComponent<Button>().interactable = false;
            UIReuseScript.btn_Map.GetComponent<Button>().interactable = false;
        }
        else if (AbilityManagerScript.hasExoskeleton)
        {
            UIReuseScript.btn_Stats.GetComponent<Button>().interactable = true;
            UIReuseScript.btn_Upgrades.GetComponent<Button>().interactable = true;
            UIReuseScript.btn_Factions.GetComponent<Button>().interactable = true;
            UIReuseScript.btn_Radio.GetComponent<Button>().interactable = true;
            UIReuseScript.btn_Map.GetComponent<Button>().interactable = true;
        }

        PlayerInventoryScript.CloseInventory();
        UIReuseScript.par_PlayerMenuStats.SetActive(false);
        TooltipScript.showTooltipUI = false;
        AbilityManagerScript.HideButtons();
        UIReuseScript.par_PlayerUpgrades.SetActive(false);
        UIReuseScript.par_PlayerFactionUI.SetActive(false);
        UIReuseScript.par_PlayerMenuRadio.SetActive(false);
        UIReuseScript.Minimap.transform.SetParent(UIReuseScript.par_MinimapMask.transform, false);
        UIReuseScript.MinimapPlayerPosition.transform.SetParent(UIReuseScript.par_MinimapMask.transform, false);
        UIReuseScript.MinimapPlayerPosition.transform.position = par_Managers.GetComponent<UI_Minimap>().playerPosition;
        UIReuseScript.par_PlayerMenuMap.SetActive(false);

        par_Managers.GetComponent<UI_AcceptedQuests>().OpenQuests();
        //Debug.Log("Opened quests UI");
    }
    //open stats
    public void OpenStats()
    {
        openedInventoryUI = false;
        openedQuestUI = false;
        openedStatsUI = true;
        openedUpgradeUI = false;
        openedFactionUI = false;
        openedRadioUI = false;
        openedMapUI = false;

        UIReuseScript.btn_Inventory.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Quests.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Stats.GetComponent<Button>().interactable = false;
        UIReuseScript.btn_Upgrades.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Factions.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Radio.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Map.GetComponent<Button>().interactable = true;

        PlayerInventoryScript.CloseInventory();
        TooltipScript.showTooltipUI = false;
        AbilityManagerScript.HideButtons();
        UIReuseScript.par_PlayerUpgrades.SetActive(false);
        UIReuseScript.par_PlayerFactionUI.SetActive(false);
        par_Managers.GetComponent<UI_AcceptedQuests>().CloseQuests();
        UIReuseScript.par_PlayerMenuRadio.SetActive(false);
        UIReuseScript.Minimap.transform.SetParent(UIReuseScript.par_MinimapMask.transform, false);
        UIReuseScript.MinimapPlayerPosition.transform.SetParent(UIReuseScript.par_MinimapMask.transform, false);
        UIReuseScript.MinimapPlayerPosition.transform.position = par_Managers.GetComponent<UI_Minimap>().playerPosition;
        UIReuseScript.par_PlayerMenuMap.SetActive(false);
        UIReuseScript.par_PlayerMenuStats.SetActive(true);

        par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
        //Debug.Log("Opened stats UI");
    }
    //open upgrades
    public void OpenUpgrades()
    {
        openedInventoryUI = false;
        openedQuestUI = false;
        openedStatsUI = false;
        openedUpgradeUI = true;
        openedFactionUI = false;
        openedRadioUI = false;
        openedMapUI = false;

        UIReuseScript.btn_Inventory.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Quests.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Stats.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Upgrades.GetComponent<Button>().interactable = false;
        UIReuseScript.btn_Factions.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Radio.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Map.GetComponent<Button>().interactable = true;

        PlayerInventoryScript.CloseInventory();
        par_Managers.GetComponent<UI_AcceptedQuests>().CloseQuests();
        UIReuseScript.par_PlayerMenuStats.SetActive(false);
        UIReuseScript.par_PlayerFactionUI.SetActive(false);
        UIReuseScript.par_PlayerMenuRadio.SetActive(false);
        UIReuseScript.Minimap.transform.SetParent(UIReuseScript.par_MinimapMask.transform, false);
        UIReuseScript.MinimapPlayerPosition.transform.SetParent(UIReuseScript.par_MinimapMask.transform, false);
        UIReuseScript.MinimapPlayerPosition.transform.position = par_Managers.GetComponent<UI_Minimap>().playerPosition;
        UIReuseScript.par_PlayerMenuMap.SetActive(false);

        UIReuseScript.par_PlayerUpgrades.SetActive(true);
        AbilityManagerScript.LoadUI();
        AbilityManagerScript.ShowUpgradeButtonPositions();

        //Debug.Log("Opened upgrades UI");
    }
    //open factions
    public void OpenFactions()
    {
        openedInventoryUI = false;
        openedQuestUI = false;
        openedStatsUI = false;
        openedUpgradeUI = false;
        openedFactionUI = true;
        openedRadioUI = false;
        openedMapUI = false;

        UIReuseScript.btn_Inventory.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Quests.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Stats.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Upgrades.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Factions.GetComponent<Button>().interactable = false;
        UIReuseScript.btn_Radio.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Map.GetComponent<Button>().interactable = true;

        PlayerInventoryScript.CloseInventory();
        par_Managers.GetComponent<UI_AcceptedQuests>().CloseQuests();
        UIReuseScript.par_PlayerMenuStats.SetActive(false);
        TooltipScript.showTooltipUI = false;
        AbilityManagerScript.HideButtons();
        UIReuseScript.par_PlayerUpgrades.SetActive(false);
        UIReuseScript.par_PlayerMenuRadio.SetActive(false);
        UIReuseScript.Minimap.transform.SetParent(UIReuseScript.par_MinimapMask.transform, false);
        UIReuseScript.MinimapPlayerPosition.transform.SetParent(UIReuseScript.par_MinimapMask.transform, false);
        UIReuseScript.MinimapPlayerPosition.transform.position = par_Managers.GetComponent<UI_Minimap>().playerPosition;
        UIReuseScript.par_PlayerMenuMap.SetActive(false);
        UIReuseScript.par_PlayerFactionUI.SetActive(true);

        UIReuseScript.UpdatePlayerFactionUI();
        //Debug.Log("Opened factions UI");
    }
    //open radio
    public void OpenRadio()
    {
        openedInventoryUI = false;
        openedQuestUI = false;
        openedStatsUI = false;
        openedUpgradeUI = false;
        openedFactionUI = false;
        openedRadioUI = true;
        openedMapUI = false;

        UIReuseScript.btn_Inventory.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Quests.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Stats.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Upgrades.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Factions.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Radio.GetComponent<Button>().interactable = false;
        UIReuseScript.btn_Map.GetComponent<Button>().interactable = true;

        PlayerInventoryScript.CloseInventory();
        par_Managers.GetComponent<UI_AcceptedQuests>().CloseQuests();
        UIReuseScript.par_PlayerMenuStats.SetActive(false);
        TooltipScript.showTooltipUI = false;
        AbilityManagerScript.HideButtons();
        UIReuseScript.par_PlayerUpgrades.SetActive(false);
        UIReuseScript.par_PlayerFactionUI.SetActive(false);
        UIReuseScript.Minimap.transform.SetParent(UIReuseScript.par_MinimapMask.transform, false);
        UIReuseScript.MinimapPlayerPosition.transform.SetParent(UIReuseScript.par_MinimapMask.transform, false);
        UIReuseScript.MinimapPlayerPosition.transform.position = par_Managers.GetComponent<UI_Minimap>().playerPosition;
        UIReuseScript.par_PlayerMenuMap.SetActive(false);

        UIReuseScript.par_PlayerMenuRadio.SetActive(true);
        //Debug.Log("Opened radio UI");
    }
    //open map
    public void OpenMap()
    {
        openedInventoryUI = false;
        openedQuestUI = false;
        openedStatsUI = false;
        openedUpgradeUI = false;
        openedFactionUI = false;
        openedRadioUI = false;
        openedMapUI = true;

        UIReuseScript.btn_Inventory.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Quests.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Stats.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Upgrades.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Factions.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Radio.GetComponent<Button>().interactable = true;
        UIReuseScript.btn_Map.GetComponent<Button>().interactable = false;

        PlayerInventoryScript.CloseInventory();
        par_Managers.GetComponent<UI_AcceptedQuests>().CloseQuests();
        UIReuseScript.par_PlayerMenuStats.SetActive(false);
        TooltipScript.showTooltipUI = false;
        AbilityManagerScript.HideButtons();
        UIReuseScript.par_PlayerUpgrades.SetActive(false);
        UIReuseScript.par_PlayerFactionUI.SetActive(false);
        UIReuseScript.par_PlayerMenuRadio.SetActive(false);
        UIReuseScript.par_PlayerMenuMap.SetActive(true);

        UIReuseScript.Minimap.transform.SetParent(UIReuseScript.par_MainMapMask.transform, false);
        UIReuseScript.MinimapPlayerPosition.transform.SetParent(UIReuseScript.par_MainMapMask.transform, false);

        //Debug.Log("Opened map UI");
    }

    public void ClosePlayerMenuUI()
    {
        if (openedInventoryUI)
        {
            PlayerInventoryScript.CloseInventory();
            openedInventoryUI = false;
        }
        else if (openedQuestUI)
        {
            par_Managers.GetComponent<UI_AcceptedQuests>().CloseQuests();
            openedQuestUI = false;
        }
        else if (openedStatsUI)
        {
            UIReuseScript.par_PlayerMenuStats.SetActive(false);
            openedStatsUI = false;
        }
        else if (openedUpgradeUI)
        {
            TooltipScript.showTooltipUI = false;
            AbilityManagerScript.HideButtons();
            UIReuseScript.par_PlayerUpgrades.SetActive(false);
            openedUpgradeUI = false;
        }
        else if (openedFactionUI)
        {
            UIReuseScript.par_PlayerFactionUI.SetActive(false);
            openedFactionUI = false;
        }
        else if (openedRadioUI)
        {
            UIReuseScript.par_PlayerMenuRadio.SetActive(false);
            openedRadioUI = false;
        }
        else if (openedMapUI)
        {
            UIReuseScript.Minimap.transform.SetParent(UIReuseScript.par_MinimapMask.transform, false);
            UIReuseScript.MinimapPlayerPosition.transform.SetParent(UIReuseScript.par_MinimapMask.transform, false);
            UIReuseScript.MinimapPlayerPosition.transform.position = par_Managers.GetComponent<UI_Minimap>().playerPosition;

            UIReuseScript.par_PlayerMenuMap.SetActive(false);
            openedMapUI = false;
        }

        PlayerInventoryScript.canOpenPlayerInventory = true;

        if (!par_Managers.GetComponent<Manager_Console>().consoleOpen)
        {
            par_Managers.GetComponent<UI_PauseMenu>().callPMCloseOnce = false;
            par_Managers.GetComponent<UI_PauseMenu>().UnpauseGame();
        }
        else if (par_Managers.GetComponent<Manager_Console>().consoleOpen)
        {
            par_Managers.GetComponent<UI_PauseMenu>().isInventoryOpen = false;
            par_Managers.GetComponent<UI_PauseMenu>().callPMCloseOnce = false;
        }

        isPlayerMenuOpen = false;

        UIReuseScript.btn_CloseUI.onClick.RemoveAllListeners();
        UIReuseScript.btn_CloseUI.gameObject.SetActive(false);

        UIReuseScript.par_PlayerMenu.SetActive(false);
    }

    public IEnumerator OpenAndClose()
    {
        yield return new WaitForSeconds(0.1f);

        openedInventoryUI = true;
        OpenPlayerMenuUI();
        OpenInventory();
        ClosePlayerMenuUI();
    }
}