using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inv_Player : MonoBehaviour
{
    [Header("General")]
    public int maxInvSpace;
    public int money;

    [Header("Assignables")]
    public GameObject par_PlayerItems;
    public Transform pos_EquippedItem;
    [SerializeField] private GameObject par_Managers;
    public List<GameObject> inventory;

    //public but hidden variables
    [HideInInspector] public bool closedInventoryThroughContainer;
    [HideInInspector] public bool isPlayerInventoryOpen;
    [HideInInspector] public bool isPlayerInventoryOpenWithContainerOrTrader;
    [HideInInspector] public bool canOpenPlayerInventory;
    [HideInInspector] public bool isPlayerAndContainerOpen;
    [HideInInspector] public bool isPlayerAndTraderOpen;
    [HideInInspector] public bool isPlayerAndRepairOpen;
    [HideInInspector] public bool hasOpenedInventoryOnce;
    [HideInInspector] public bool showingAllItems;
    [HideInInspector] public bool showingAllWeapons;
    [HideInInspector] public bool showingAllArmor;
    [HideInInspector] public bool showingAllConsumables;
    [HideInInspector] public bool showingAllAmmo;
    [HideInInspector] public bool showingAllGear;
    [HideInInspector] public bool showingAllMisc;
    [HideInInspector] public bool showingAllUnusedBatteries;
    [HideInInspector] public int invSpace;
    [HideInInspector] public GameObject Container;
    [HideInInspector] public GameObject Workbench;
    [HideInInspector] public GameObject Trader;
    [HideInInspector] public GameObject equippedGun;
    [HideInInspector] public GameObject equippedFlashlight;
    [HideInInspector] public GameObject heldObject;
    [HideInInspector] public List<GameObject> buttons;

    private void Awake()
    {
        invSpace = maxInvSpace;
        canOpenPlayerInventory = true;
    }

    private void Start()
    {
        foreach (GameObject item in inventory)
        {
            int itemWeight = item.GetComponent<Env_Item>().int_ItemWeight;
            invSpace -= itemWeight;
            item.GetComponent<Env_Item>().isInPlayerInventory = true;
            par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(item.GetComponent<Env_Item>().str_ItemName);
            item.GetComponent<Env_Item>().DeactivateItem();
        }
    }

    public void UpdatePlayerInventoryStats()
    {
        par_Managers.GetComponent<Manager_UIReuse>().txt_PlayerMoney.text = money.ToString();

        int takenSpace = 0;

        for (int i = 0; i < inventory.Count; i++)
        {
            GameObject item = inventory[i];

            if (item == null)
            {
                inventory.Remove(item);
            }
            else
            {
                takenSpace += item.GetComponent<Env_Item>().int_ItemWeight * item.GetComponent<Env_Item>().int_itemCount;
            }
        }
        par_Managers.GetComponent<Manager_UIReuse>().txt_PlayerInventorySpace.text = takenSpace + " / " + maxInvSpace;
    }

    public void CloseContainer()
    {
        if (!closedInventoryThroughContainer)
        {
            Container.GetComponent<Inv_Container>().CloseInventory();

            par_Managers.GetComponent<Manager_UIReuse>().btn_PlaceIntoContainer.interactable = false;
            par_Managers.GetComponent<Manager_UIReuse>().btn_PlaceIntoContainer.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_TakeFromContainer.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_TakeFromContainer.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_TakeFromContainer.onClick.AddListener(Container.GetComponent<Inv_Container>().CheckIfLocked);
        }

        isPlayerInventoryOpenWithContainerOrTrader = true;
        OpenInventory();
    }
    public void CloseShop()
    {
        if (!closedInventoryThroughContainer)
        {
            foreach (GameObject item in Trader.GetComponent<UI_ShopContent>().inventory)
            {
                item.GetComponent<Env_Item>().RemoveListeners();
            }

            Trader.GetComponent<UI_ShopContent>().CloseShopUI();

            par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.interactable = false;
            par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_BuyFromTrader.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_BuyFromTrader.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_BuyFromTrader.onClick.AddListener(Trader.GetComponent<UI_ShopContent>().OpenShopUI);
        }

        OpenInventory();
    }
    public void CloseRepair()
    {
        if (!closedInventoryThroughContainer && Trader != null)
        {
            Trader.GetComponent<UI_ShopContent>().CloseShopUI();
        }

        OpenInventory();
    }

    public void OpenInventory()
    {
        if (!hasOpenedInventoryOnce && !closedInventoryThroughContainer)
        {
            par_Managers.GetComponent<Manager_UIReuse>().par_Inventory.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().par_Stats.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";
            UpdatePlayerInventoryStats();

            showingAllItems = true;
            showingAllWeapons = false;
            showingAllArmor = false;
            showingAllConsumables = false;
            showingAllAmmo = false;
            showingAllGear = false;
            showingAllMisc = false;

            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = false;
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = true;

            RebuildInventory();

            par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();

            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.onClick.AddListener(ShowAll);
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.onClick.AddListener(ShowWeapons);
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.onClick.AddListener(ShowArmor);
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.onClick.AddListener(ShowConsumables);
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.onClick.AddListener(ShowAmmo);
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.onClick.AddListener(ShowGear);
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.onClick.AddListener(ShowMisc);

            foreach (GameObject item in inventory)
            {
                if (item == null)
                {
                    inventory.Remove(item);
                }

                item.GetComponent<Env_Item>().isInPlayerInventory = true;
                item.GetComponent<Env_Item>().isInContainer = false;
                item.GetComponent<Env_Item>().isInTraderShop = false;
                item.GetComponent<Env_Item>().isInRepairMenu = false;
                item.GetComponent<Env_Item>().isBuying = false;
                item.GetComponent<Env_Item>().isTaking = false;
            }

            if (!isPlayerInventoryOpen
                && canOpenPlayerInventory
                && !closedInventoryThroughContainer
                && !par_Managers.GetComponent<UI_PauseMenu>().isUIOpen)
            {
                par_Managers.GetComponent<UI_PauseMenu>().isInventoryOpen = true;
            }

            par_Managers.GetComponent<UI_PauseMenu>().PauseGameAndCloseUIAndResetBools();
            if (!isPlayerAndContainerOpen && !isPlayerAndTraderOpen && !isPlayerAndRepairOpen)
            {
                par_Managers.GetComponent<UI_PlayerMenu>().OpenPlayerMenuUI();
                isPlayerInventoryOpen = true;
            }
            //Debug.Log("Player inventory is open!");
            hasOpenedInventoryOnce = true;
        }
    }
    public void CloseInventory()
    {
        if (hasOpenedInventoryOnce)
        {
            showingAllItems = false;
            showingAllWeapons = false;
            showingAllArmor = false;
            showingAllConsumables = false;
            showingAllAmmo = false;
            showingAllGear = false;
            showingAllMisc = false;

            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.onClick.RemoveAllListeners();

            par_Managers.GetComponent<Manager_UIReuse>().par_Inventory.SetActive(false);
            par_Managers.GetComponent<Manager_UIReuse>().par_Stats.SetActive(false);
            par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();

            if (!par_Managers.GetComponent<UI_PauseMenu>().isTalkingToAI && !par_Managers.GetComponent<UI_PlayerMenu>().isPlayerMenuOpen)
            {
                par_Managers.GetComponent<UI_PauseMenu>().callPMCloseOnce = false;
                par_Managers.GetComponent<UI_PauseMenu>().UnpauseGame();
            }
            else if (par_Managers.GetComponent<UI_PauseMenu>().isTalkingToAI)
            {
                par_Managers.GetComponent<UI_PauseMenu>().callPMCloseOnce = false;
                par_Managers.GetComponent<UI_PauseMenu>().CloseUI();
            }

            isPlayerInventoryOpen = false;
            par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
            if (closedInventoryThroughContainer)
            {
                isPlayerAndContainerOpen = false;
                isPlayerAndTraderOpen = false;
                isPlayerAndRepairOpen = false;
                canOpenPlayerInventory = true;
                closedInventoryThroughContainer = false;
            }
            //Debug.Log("Player inventory is closed...");
            hasOpenedInventoryOnce = false;
        }
    }

    private void RebuildInventory()
    {
        foreach (GameObject item in inventory)
        {
            if (item != null)
            {
                if (item.GetComponent<Env_Item>().isProtected
                    && isPlayerAndTraderOpen)
                {
                    Debug.Log(item.GetComponent<Env_Item>().str_ItemName + " was not listed in " +
                             "player inventory while selling to trader because it is protected!");
                }
                else
                {
                    Button btn_New = Instantiate(par_Managers.GetComponent<Manager_UIReuse>().btn_Template);
                    btn_New.transform.SetParent(par_Managers.GetComponent<Manager_UIReuse>().par_Panel.transform, false);

                    for (int i = 0; i < item.GetComponent<Env_Item>().str_ItemName.Length - 1; i++)
                    {
                        if (item.GetComponent<Env_Item>().str_ItemName[i] == '_')
                        {
                            item.GetComponent<Env_Item>().hasUnderscore = true;
                            break;
                        }
                    }
                    if (item.GetComponent<Env_Item>().hasUnderscore)
                    {
                        if (item.GetComponent<Env_Item>().int_itemCount == 1)
                        {
                            string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                            btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName;
                        }
                        else
                        {
                            string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                            btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                        }
                    }
                    else if (!item.GetComponent<Env_Item>().hasUnderscore)
                    {
                        if (item.GetComponent<Env_Item>().int_itemCount == 1)
                        {
                            btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName;
                        }
                        else
                        {
                            btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                        }
                    }

                    btn_New.onClick.AddListener(item.GetComponent<Env_Item>().ShowStats);
                    buttons.Add(btn_New.gameObject);

                    item.GetComponent<Env_Item>().isInPlayerInventory = true;
                }
            }
        }
    }
    public void ShowAll()
    {
        par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
        par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
        showingAllItems = true;
        showingAllWeapons = false;
        showingAllArmor = false;
        showingAllConsumables = false;
        showingAllAmmo = false;
        showingAllGear = false;
        showingAllMisc = false;
        showingAllUnusedBatteries = false;

        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = false;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = true;

        UpdatePlayerInventoryStats();
        RebuildInventory();
        //Debug.Log("Showing all player inventory items.");
    }
    public void ShowWeapons()
    {
        par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
        par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
        showingAllItems = false;
        showingAllWeapons = true;
        showingAllArmor = false;
        showingAllConsumables = false;
        showingAllAmmo = false;
        showingAllGear = false;
        showingAllMisc = false;
        showingAllUnusedBatteries = false;

        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = false;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = true;

        UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null 
                && (item.GetComponent<Item_Gun>() != null
                || item.GetComponent<Item_Grenade>() != null
                || item.GetComponent<Item_Melee>() != null))
            {
                if (item.GetComponent<Env_Item>().isProtected
                    && isPlayerAndTraderOpen)
                {
                    Debug.Log(item.GetComponent<Env_Item>().str_ItemName + " was not listed in " +
                             "player inventory while selling to trader because it is protected!");
                }
                else
                {
                    Button btn_New = Instantiate(par_Managers.GetComponent<Manager_UIReuse>().btn_Template);
                    btn_New.transform.SetParent(par_Managers.GetComponent<Manager_UIReuse>().par_Panel.transform, false);

                    for (int i = 0; i < item.GetComponent<Env_Item>().str_ItemName.Length - 1; i++)
                    {
                        if (item.GetComponent<Env_Item>().str_ItemName[i] == '_')
                        {
                            item.GetComponent<Env_Item>().hasUnderscore = true;
                            break;
                        }
                    }
                    if (item.GetComponent<Env_Item>().hasUnderscore)
                    {
                        if (item.GetComponent<Env_Item>().int_itemCount == 1)
                        {
                            string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                            btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName;
                        }
                        else
                        {
                            string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                            btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                        }
                    }
                    else if (!item.GetComponent<Env_Item>().hasUnderscore)
                    {
                        if (item.GetComponent<Env_Item>().int_itemCount == 1)
                        {
                            btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName;
                        }
                        else
                        {
                            btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                        }
                    }

                    btn_New.onClick.AddListener(item.GetComponent<Env_Item>().ShowStats);
                    buttons.Add(btn_New.gameObject);

                    item.GetComponent<Env_Item>().isInPlayerInventory = true;
                }
            }
        }
        //Debug.Log("Showing player weapons.");
    }
    public void ShowArmor()
    {
        par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
        par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
        showingAllItems = false;
        showingAllWeapons = false;
        showingAllArmor = true;
        showingAllConsumables = false;
        showingAllAmmo = false;
        showingAllGear = false;
        showingAllMisc = false;
        showingAllUnusedBatteries = false;

        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = false;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = true;

        UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null
                && item.GetComponent<Item_Armor>() != null)
            {
                if (item.GetComponent<Env_Item>().isProtected
                    && isPlayerAndTraderOpen)
                {
                    Debug.Log(item.GetComponent<Env_Item>().str_ItemName + " was not listed in " +
                             "player inventory while selling to trader because it is protected!");
                }
                else
                {
                    Button btn_New = Instantiate(par_Managers.GetComponent<Manager_UIReuse>().btn_Template);
                    btn_New.transform.SetParent(par_Managers.GetComponent<Manager_UIReuse>().par_Panel.transform, false);

                    for (int i = 0; i < item.GetComponent<Env_Item>().str_ItemName.Length - 1; i++)
                    {
                        if (item.GetComponent<Env_Item>().str_ItemName[i] == '_')
                        {
                            item.GetComponent<Env_Item>().hasUnderscore = true;
                            break;
                        }
                    }
                    if (item.GetComponent<Env_Item>().hasUnderscore)
                    {
                        if (item.GetComponent<Env_Item>().int_itemCount == 1)
                        {
                            string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                            btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName;
                        }
                        else
                        {
                            string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                            btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                        }
                    }
                    else if (!item.GetComponent<Env_Item>().hasUnderscore)
                    {
                        if (item.GetComponent<Env_Item>().int_itemCount == 1)
                        {
                            btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName;
                        }
                        else
                        {
                            btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                        }
                    }

                    btn_New.onClick.AddListener(item.GetComponent<Env_Item>().ShowStats);
                    buttons.Add(btn_New.gameObject);

                    item.GetComponent<Env_Item>().isInPlayerInventory = true;
                }
            }
        }
        //Debug.Log("Showing player armor.");
    }
    public void ShowConsumables()
    {
        par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
        par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
        showingAllItems = false;
        showingAllWeapons = false;
        showingAllArmor = false;
        showingAllConsumables = true;
        showingAllAmmo = false;
        showingAllGear = false;
        showingAllMisc = false;
        showingAllUnusedBatteries = false;

        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = false;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = true;

        UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null && item.GetComponent<Item_Consumable>() != null)
            {
                if (item.GetComponent<Env_Item>().isProtected
                    && isPlayerAndTraderOpen)
                {
                    Debug.Log(item.GetComponent<Env_Item>().str_ItemName + " was not listed in " +
                             "player inventory while selling to trader because it is protected!");
                }
                else
                {
                    Button btn_New = Instantiate(par_Managers.GetComponent<Manager_UIReuse>().btn_Template);
                    btn_New.transform.SetParent(par_Managers.GetComponent<Manager_UIReuse>().par_Panel.transform, false);

                    for (int i = 0; i < item.GetComponent<Env_Item>().str_ItemName.Length - 1; i++)
                    {
                        if (item.GetComponent<Env_Item>().str_ItemName[i] == '_')
                        {
                            item.GetComponent<Env_Item>().hasUnderscore = true;
                            break;
                        }
                    }
                    if (item.GetComponent<Env_Item>().hasUnderscore)
                    {
                        if (item.GetComponent<Env_Item>().int_itemCount == 1)
                        {
                            string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                            btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName;
                        }
                        else
                        {
                            string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                            btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                        }
                    }
                    else if (!item.GetComponent<Env_Item>().hasUnderscore)
                    {
                        if (item.GetComponent<Env_Item>().int_itemCount == 1)
                        {
                            btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName;
                        }
                        else
                        {
                            btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                        }
                    }

                    btn_New.onClick.AddListener(item.GetComponent<Env_Item>().ShowStats);
                    buttons.Add(btn_New.gameObject);

                    item.GetComponent<Env_Item>().isInPlayerInventory = true;
                }
            }
        }
        //Debug.Log("Showing player consumables.");
    }
    public void ShowAmmo()
    {
        par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
        par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
        showingAllItems = false;
        showingAllWeapons = false;
        showingAllArmor = false;
        showingAllConsumables = false;
        showingAllAmmo = true;
        showingAllGear = false;
        showingAllMisc = false;
        showingAllUnusedBatteries = false;

        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = false;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = true;

        UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null && item.GetComponent<Item_Ammo>() != null)
            {
                if (item.GetComponent<Env_Item>().isProtected
                    && isPlayerAndTraderOpen)
                {
                    Debug.Log(item.GetComponent<Env_Item>().str_ItemName + " was not listed in " +
                             "player inventory while selling to trader because it is protected!");
                }
                else
                {
                    Button btn_New = Instantiate(par_Managers.GetComponent<Manager_UIReuse>().btn_Template);
                    btn_New.transform.SetParent(par_Managers.GetComponent<Manager_UIReuse>().par_Panel.transform, false);

                    for (int i = 0; i < item.GetComponent<Env_Item>().str_ItemName.Length - 1; i++)
                    {
                        if (item.GetComponent<Env_Item>().str_ItemName[i] == '_')
                        {
                            item.GetComponent<Env_Item>().hasUnderscore = true;
                            break;
                        }
                    }
                    if (item.GetComponent<Env_Item>().hasUnderscore)
                    {
                        if (item.GetComponent<Env_Item>().int_itemCount == 1)
                        {
                            string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                            btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName;
                        }
                        else
                        {
                            string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                            btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                        }
                    }
                    else if (!item.GetComponent<Env_Item>().hasUnderscore)
                    {
                        if (item.GetComponent<Env_Item>().int_itemCount == 1)
                        {
                            btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName;
                        }
                        else
                        {
                            btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                        }
                    }

                    btn_New.onClick.AddListener(item.GetComponent<Env_Item>().ShowStats);
                    buttons.Add(btn_New.gameObject);

                    item.GetComponent<Env_Item>().isInPlayerInventory = true;
                }
            }
        }
        //Debug.Log("Showing player ammo.");
    }
    public void ShowGear()
    {
        par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
        par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
        showingAllItems = false;
        showingAllWeapons = false;
        showingAllArmor = false;
        showingAllConsumables = false;
        showingAllAmmo = false;
        showingAllGear = true;
        showingAllMisc = false;
        showingAllUnusedBatteries = false;

        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = false;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = true;

        UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null 
                && item.GetComponent<Item_Gear>() != null)
            {
                if (item.GetComponent<Env_Item>().isProtected
                    && isPlayerAndTraderOpen)
                {
                    Debug.Log(item.GetComponent<Env_Item>().str_ItemName + " was not listed in " +
                             "player inventory while selling to trader because it is protected!");
                }
                else
                {
                    Button btn_New = Instantiate(par_Managers.GetComponent<Manager_UIReuse>().btn_Template);
                    btn_New.transform.SetParent(par_Managers.GetComponent<Manager_UIReuse>().par_Panel.transform, false);

                    for (int i = 0; i < item.GetComponent<Env_Item>().str_ItemName.Length - 1; i++)
                    {
                        if (item.GetComponent<Env_Item>().str_ItemName[i] == '_')
                        {
                            item.GetComponent<Env_Item>().hasUnderscore = true;
                            break;
                        }
                    }
                    if (item.GetComponent<Env_Item>().hasUnderscore)
                    {
                        if (item.GetComponent<Env_Item>().int_itemCount == 1)
                        {
                            string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                            btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName;
                        }
                        else
                        {
                            string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                            btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                        }
                    }
                    else if (!item.GetComponent<Env_Item>().hasUnderscore)
                    {
                        if (item.GetComponent<Env_Item>().int_itemCount == 1)
                        {
                            btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName;
                        }
                        else
                        {
                            btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                        }
                    }

                    btn_New.onClick.AddListener(item.GetComponent<Env_Item>().ShowStats);
                    buttons.Add(btn_New.gameObject);

                    item.GetComponent<Env_Item>().isInPlayerInventory = true;
                }
            }
        }
        //Debug.Log("Showing player gear.");
    }
    public void ShowMisc()
    {
        par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
        par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
        showingAllItems = false;
        showingAllWeapons = false;
        showingAllArmor = false;
        showingAllConsumables = false;
        showingAllAmmo = false;
        showingAllGear = false;
        showingAllMisc = true; 
        showingAllUnusedBatteries = false;

        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = false;

        UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null 
                && item.GetComponent<Item_Gun>() == null
                && item.GetComponent<Item_Grenade>() == null
                && item.GetComponent<Item_Melee>() == null
                && item.GetComponent<Item_Armor>() == null
                && item.GetComponent<Item_Consumable>() == null
                && item.GetComponent<Item_Gear>() == null
                && item.GetComponent<Item_Ammo>() == null)
            {
                if (item.GetComponent<Env_Item>().isProtected
                    && isPlayerAndTraderOpen)
                {
                    Debug.Log(item.GetComponent<Env_Item>().str_ItemName + " was not listed in " +
                             "player inventory while selling to trader because it is protected!");
                }
                else
                {
                    Button btn_New = Instantiate(par_Managers.GetComponent<Manager_UIReuse>().btn_Template);
                    btn_New.transform.SetParent(par_Managers.GetComponent<Manager_UIReuse>().par_Panel.transform, false);

                    for (int i = 0; i < item.GetComponent<Env_Item>().str_ItemName.Length - 1; i++)
                    {
                        if (item.GetComponent<Env_Item>().str_ItemName[i] == '_')
                        {
                            item.GetComponent<Env_Item>().hasUnderscore = true;
                            break;
                        }
                    }
                    if (item.GetComponent<Env_Item>().hasUnderscore)
                    {
                        if (item.GetComponent<Env_Item>().int_itemCount == 1)
                        {
                            string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                            btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName;
                        }
                        else
                        {
                            string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                            btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                        }
                    }
                    else if (!item.GetComponent<Env_Item>().hasUnderscore)
                    {
                        if (item.GetComponent<Env_Item>().int_itemCount == 1)
                        {
                            btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName;
                        }
                        else
                        {
                            btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                        }
                    }

                    btn_New.onClick.AddListener(item.GetComponent<Env_Item>().ShowStats);
                    buttons.Add(btn_New.gameObject);

                    item.GetComponent<Env_Item>().isInPlayerInventory = true;
                }
            }
        }
        //Debug.Log("Showing player misc items.");
    }
    public void ShowUnequippedBatteries()
    {
        par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
        par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
        showingAllItems = false;
        showingAllWeapons = false;
        showingAllArmor = false;
        showingAllConsumables = false;
        showingAllAmmo = false;
        showingAllGear = false;
        showingAllMisc = false;
        showingAllUnusedBatteries = true;

        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = true;

        UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null
                && item.GetComponent<Item_Battery>() != null
                && !item.GetComponent<Item_Battery>().isInUse)
            {
                Button btn_New = Instantiate(par_Managers.GetComponent<Manager_UIReuse>().btn_Template);
                btn_New.transform.SetParent(par_Managers.GetComponent<Manager_UIReuse>().par_Panel.transform, false);

                for (int i = 0; i < item.GetComponent<Env_Item>().str_ItemName.Length - 1; i++)
                {
                    if (item.GetComponent<Env_Item>().str_ItemName[i] == '_')
                    {
                        item.GetComponent<Env_Item>().hasUnderscore = true;
                        break;
                    }
                }
                if (item.GetComponent<Env_Item>().hasUnderscore)
                {
                    if (item.GetComponent<Env_Item>().int_itemCount == 1)
                    {
                        string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                        btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName;
                    }
                    else
                    {
                        string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                        btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                    }
                }
                else if (!item.GetComponent<Env_Item>().hasUnderscore)
                {
                    if (item.GetComponent<Env_Item>().int_itemCount == 1)
                    {
                        btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName;
                    }
                    else
                    {
                        btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                    }
                }

                btn_New.onClick.AddListener(item.GetComponent<Env_Item>().ShowStats);
                btn_New.onClick.AddListener(ShowUnequippedBatteries);
                buttons.Add(btn_New.gameObject);

                item.GetComponent<Env_Item>().isInPlayerInventory = true;
            }
        }
        //Debug.Log("Showing player misc items.");
    }
}