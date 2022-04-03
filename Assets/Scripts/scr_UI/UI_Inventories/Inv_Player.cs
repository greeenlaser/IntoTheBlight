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
    [SerializeField] private Manager_UIReuse UIReuseScript;
    [SerializeField] private UI_PauseMenu PauseMenuScript;
    [SerializeField] private Manager_Console ConsoleScript;
    [SerializeField] private UI_PlayerMenu PlayerMenuScript;
    public List<GameObject> inventory = new List<GameObject>();

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
    [HideInInspector] public int invSpace;
    [HideInInspector] public GameObject Container;
    [HideInInspector] public GameObject Workbench;
    [HideInInspector] public GameObject Trader;
    [HideInInspector] public GameObject equippedGun;
    [HideInInspector] public GameObject heldObject;
    [HideInInspector] public List<GameObject> buttons = new List<GameObject>();

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
            ConsoleScript.playeritemnames.Add(item.GetComponent<Env_Item>().str_ItemName);
            item.GetComponent<Env_Item>().DeactivateItem();
        }
    }

    public void UpdatePlayerInventoryStats()
    {
        UIReuseScript.txt_PlayerMoney.text = money.ToString();

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
        UIReuseScript.txt_PlayerInventorySpace.text = takenSpace + " / " + maxInvSpace;
    }

    public void CloseContainer()
    {
        if (!closedInventoryThroughContainer)
        {
            Container.GetComponent<Inv_Container>().CloseInventory();

            UIReuseScript.btn_PlaceIntoContainer.interactable = false;
            UIReuseScript.btn_PlaceIntoContainer.onClick.RemoveAllListeners();
            UIReuseScript.btn_TakeFromContainer.interactable = true;
            UIReuseScript.btn_TakeFromContainer.onClick.RemoveAllListeners();
            UIReuseScript.btn_TakeFromContainer.onClick.AddListener(Container.GetComponent<Inv_Container>().CheckIfLocked);
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

            UIReuseScript.btn_SellToTrader.interactable = false;
            UIReuseScript.btn_SellToTrader.onClick.RemoveAllListeners();
            UIReuseScript.btn_BuyFromTrader.interactable = true;
            UIReuseScript.btn_BuyFromTrader.onClick.RemoveAllListeners();
            UIReuseScript.btn_BuyFromTrader.onClick.AddListener(Trader.GetComponent<UI_ShopContent>().OpenShopUI);
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
            UIReuseScript.par_Inventory.SetActive(true);
            UIReuseScript.par_Stats.SetActive(true);
            UIReuseScript.txt_InventoryName.text = "Player inventory";
            UpdatePlayerInventoryStats();

            showingAllItems = true;
            showingAllWeapons = false;
            showingAllArmor = false;
            showingAllConsumables = false;
            showingAllAmmo = false;
            showingAllGear = false;
            showingAllMisc = false;

            UIReuseScript.btn_ShowAll.interactable = false;
            UIReuseScript.btn_ShowWeapons.interactable = true;
            UIReuseScript.btn_ShowArmor.interactable = true;
            UIReuseScript.btn_ShowConsumables.interactable = true;
            UIReuseScript.btn_ShowAmmo.interactable = true;
            UIReuseScript.btn_ShowGear.interactable = true;
            UIReuseScript.btn_ShowMisc.interactable = true;

            RebuildInventory();

            UIReuseScript.EnableInventorySortButtons();

            UIReuseScript.btn_ShowAll.onClick.AddListener(ShowAll);
            UIReuseScript.btn_ShowWeapons.onClick.AddListener(ShowWeapons);
            UIReuseScript.btn_ShowArmor.onClick.AddListener(ShowArmor);
            UIReuseScript.btn_ShowConsumables.onClick.AddListener(ShowConsumables);
            UIReuseScript.btn_ShowAmmo.onClick.AddListener(ShowAmmo);
            UIReuseScript.btn_ShowGear.onClick.AddListener(ShowGear);
            UIReuseScript.btn_ShowMisc.onClick.AddListener(ShowMisc);

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
                && !PauseMenuScript.isUIOpen)
            {
                PauseMenuScript.isInventoryOpen = true;
            }

            PauseMenuScript.PauseGameAndCloseUIAndResetBools();
            if (!isPlayerAndContainerOpen && !isPlayerAndTraderOpen && !isPlayerAndRepairOpen)
            {
                PlayerMenuScript.OpenPlayerMenuUI();
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

            UIReuseScript.btn_ShowAll.onClick.RemoveAllListeners();
            UIReuseScript.btn_ShowWeapons.onClick.RemoveAllListeners();
            UIReuseScript.btn_ShowArmor.onClick.RemoveAllListeners();
            UIReuseScript.btn_ShowConsumables.onClick.RemoveAllListeners();
            UIReuseScript.btn_ShowAmmo.onClick.RemoveAllListeners();
            UIReuseScript.btn_ShowGear.onClick.RemoveAllListeners();
            UIReuseScript.btn_ShowMisc.onClick.RemoveAllListeners();

            UIReuseScript.par_Inventory.SetActive(false);
            UIReuseScript.par_Stats.SetActive(false);
            UIReuseScript.ClearAllInventories();
            UIReuseScript.ClearInventoryUI();
            UIReuseScript.ClearStatsUI();

            if (!PauseMenuScript.isTalkingToAI && !PlayerMenuScript.isPlayerMenuOpen)
            {
                PauseMenuScript.callPMCloseOnce = false;
                PauseMenuScript.UnpauseGame();
            }
            else if (PauseMenuScript.isTalkingToAI)
            {
                PauseMenuScript.callPMCloseOnce = false;
                PauseMenuScript.CloseUI();
            }

            isPlayerInventoryOpen = false;
            UIReuseScript.ClearAllInventories();
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
                    Button btn_New = Instantiate(UIReuseScript.btn_Template);
                    btn_New.transform.SetParent(UIReuseScript.par_Panel.transform, false);

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
        UIReuseScript.ClearAllInventories();
        UIReuseScript.EnableInventorySortButtons();
        showingAllItems = true;
        showingAllWeapons = false;
        showingAllArmor = false;
        showingAllConsumables = false;
        showingAllAmmo = false;
        showingAllGear = false;
        showingAllMisc = false;

        UIReuseScript.btn_ShowAll.interactable = false;
        UIReuseScript.btn_ShowWeapons.interactable = true;
        UIReuseScript.btn_ShowArmor.interactable = true;
        UIReuseScript.btn_ShowConsumables.interactable = true;
        UIReuseScript.btn_ShowAmmo.interactable = true;
        UIReuseScript.btn_ShowGear.interactable = true;
        UIReuseScript.btn_ShowMisc.interactable = true;

        UpdatePlayerInventoryStats();
        RebuildInventory();
        //Debug.Log("Showing all player inventory items.");
    }
    public void ShowWeapons()
    {
        UIReuseScript.ClearAllInventories();
        UIReuseScript.EnableInventorySortButtons();
        showingAllItems = false;
        showingAllWeapons = true;
        showingAllArmor = false;
        showingAllConsumables = false;
        showingAllAmmo = false;
        showingAllGear = false;
        showingAllMisc = false;

        UIReuseScript.btn_ShowAll.interactable = true;
        UIReuseScript.btn_ShowWeapons.interactable = false;
        UIReuseScript.btn_ShowArmor.interactable = true;
        UIReuseScript.btn_ShowConsumables.interactable = true;
        UIReuseScript.btn_ShowAmmo.interactable = true;
        UIReuseScript.btn_ShowGear.interactable = true;
        UIReuseScript.btn_ShowMisc.interactable = true;

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
                    Button btn_New = Instantiate(UIReuseScript.btn_Template);
                    btn_New.transform.SetParent(UIReuseScript.par_Panel.transform, false);

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
        UIReuseScript.ClearAllInventories();
        UIReuseScript.EnableInventorySortButtons();
        showingAllItems = false;
        showingAllWeapons = false;
        showingAllArmor = true;
        showingAllConsumables = false;
        showingAllAmmo = false;
        showingAllGear = false;
        showingAllMisc = false;

        UIReuseScript.btn_ShowAll.interactable = true;
        UIReuseScript.btn_ShowWeapons.interactable = true;
        UIReuseScript.btn_ShowArmor.interactable = false;
        UIReuseScript.btn_ShowConsumables.interactable = true;
        UIReuseScript.btn_ShowAmmo.interactable = true;
        UIReuseScript.btn_ShowGear.interactable = true;
        UIReuseScript.btn_ShowMisc.interactable = true;

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
                    Button btn_New = Instantiate(UIReuseScript.btn_Template);
                    btn_New.transform.SetParent(UIReuseScript.par_Panel.transform, false);

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
        UIReuseScript.ClearAllInventories();
        UIReuseScript.EnableInventorySortButtons();
        showingAllItems = false;
        showingAllWeapons = false;
        showingAllArmor = false;
        showingAllConsumables = true;
        showingAllAmmo = false;
        showingAllGear = false;
        showingAllMisc = false;

        UIReuseScript.btn_ShowAll.interactable = true;
        UIReuseScript.btn_ShowWeapons.interactable = true;
        UIReuseScript.btn_ShowArmor.interactable = true;
        UIReuseScript.btn_ShowConsumables.interactable = false;
        UIReuseScript.btn_ShowAmmo.interactable = true;
        UIReuseScript.btn_ShowGear.interactable = true;
        UIReuseScript.btn_ShowMisc.interactable = true;

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
                    Button btn_New = Instantiate(UIReuseScript.btn_Template);
                    btn_New.transform.SetParent(UIReuseScript.par_Panel.transform, false);

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
        UIReuseScript.ClearAllInventories();
        UIReuseScript.EnableInventorySortButtons();
        showingAllItems = false;
        showingAllWeapons = false;
        showingAllArmor = false;
        showingAllConsumables = false;
        showingAllAmmo = true;
        showingAllGear = false;
        showingAllMisc = false;

        UIReuseScript.btn_ShowAll.interactable = true;
        UIReuseScript.btn_ShowWeapons.interactable = true;
        UIReuseScript.btn_ShowArmor.interactable = true;
        UIReuseScript.btn_ShowConsumables.interactable = true;
        UIReuseScript.btn_ShowAmmo.interactable = false;
        UIReuseScript.btn_ShowGear.interactable = true;
        UIReuseScript.btn_ShowMisc.interactable = true;

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
                    Button btn_New = Instantiate(UIReuseScript.btn_Template);
                    btn_New.transform.SetParent(UIReuseScript.par_Panel.transform, false);

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
        UIReuseScript.ClearAllInventories();
        UIReuseScript.EnableInventorySortButtons();
        showingAllItems = false;
        showingAllWeapons = false;
        showingAllArmor = false;
        showingAllConsumables = false;
        showingAllAmmo = false;
        showingAllGear = true;
        showingAllMisc = false;

        UIReuseScript.btn_ShowAll.interactable = true;
        UIReuseScript.btn_ShowWeapons.interactable = true;
        UIReuseScript.btn_ShowArmor.interactable = true;
        UIReuseScript.btn_ShowConsumables.interactable = true;
        UIReuseScript.btn_ShowAmmo.interactable = true;
        UIReuseScript.btn_ShowGear.interactable = false;
        UIReuseScript.btn_ShowMisc.interactable = true;

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
                    Button btn_New = Instantiate(UIReuseScript.btn_Template);
                    btn_New.transform.SetParent(UIReuseScript.par_Panel.transform, false);

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
        UIReuseScript.ClearAllInventories();
        UIReuseScript.EnableInventorySortButtons();
        showingAllItems = false;
        showingAllWeapons = false;
        showingAllArmor = false;
        showingAllConsumables = false;
        showingAllAmmo = false;
        showingAllGear = false;
        showingAllMisc = true;

        UIReuseScript.btn_ShowAll.interactable = true;
        UIReuseScript.btn_ShowWeapons.interactable = true;
        UIReuseScript.btn_ShowArmor.interactable = true;
        UIReuseScript.btn_ShowConsumables.interactable = true;
        UIReuseScript.btn_ShowAmmo.interactable = true;
        UIReuseScript.btn_ShowGear.interactable = true;
        UIReuseScript.btn_ShowMisc.interactable = false;

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
                    Button btn_New = Instantiate(UIReuseScript.btn_Template);
                    btn_New.transform.SetParent(UIReuseScript.par_Panel.transform, false);

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
}