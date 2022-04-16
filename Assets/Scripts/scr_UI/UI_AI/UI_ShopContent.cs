using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ShopContent : MonoBehaviour
{
    public ShopType shopType;
    public enum ShopType
    {
        unassigned,
        general,
        armor,
        weapons,
        consumables,
        tools
    }

    [Header("Assignables")]
    public GameObject par_TraderItems;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private UI_AIContent AIScript;
    [SerializeField] private GameObject par_Managers;
    public List<GameObject> inventory = new List<GameObject>();

    //public but hidden variables
    [HideInInspector] public bool isShopOpen;
    [HideInInspector] public bool showingAllItems;
    [HideInInspector] public bool showingAllWeapons;
    [HideInInspector] public bool showingAllArmor;
    [HideInInspector] public bool showingAllConsumables;
    [HideInInspector] public bool showingAllAmmo;
    [HideInInspector] public bool showingAllGear;
    [HideInInspector] public bool showingAllMisc;
    [HideInInspector] public string str_ShopName;
    [HideInInspector] public List<GameObject> buttons = new List<GameObject>();

    private void Start()
    {
        foreach (GameObject item in inventory)
        {
            item.GetComponent<Env_Item>().isInTraderShop = true;
            item.GetComponent<Env_Item>().DeactivateItem();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && isShopOpen)
        {
            CloseShopAndPlayerInventory();
        }
    }

    public void OpenShopUI()
    {
        PlayerInventoryScript.Trader = gameObject;

        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.gameObject.SetActive(true);
        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.onClick.AddListener(CloseShopAndPlayerInventory);

        PlayerInventoryScript.CloseInventory();
        PlayerInventoryScript.isPlayerAndTraderOpen = true;
        par_Managers.GetComponent<Manager_UIReuse>().par_Inventory.SetActive(true);
        par_Managers.GetComponent<Manager_UIReuse>().par_Stats.SetActive(true);
        if (shopType != ShopType.unassigned)
        {
            if (shopType == ShopType.general)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = AIScript.str_NPCName + "s general shop";
                str_ShopName = par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text;
            }
            else if (shopType == ShopType.armor)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = AIScript.str_NPCName + "s armor shop";
                str_ShopName = par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text;
            }
            else if (shopType == ShopType.weapons)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = AIScript.str_NPCName + "s weapons shop";
                str_ShopName = par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text;
            }
            else if (shopType == ShopType.consumables)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = AIScript.str_NPCName + "s consumables shop";
                str_ShopName = par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text;
            }
            else if (shopType == ShopType.tools)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = AIScript.str_NPCName + "s tools shop";
                str_ShopName = par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text;
            }
        }
        PlayerInventoryScript.UpdatePlayerInventoryStats();

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

        par_Managers.GetComponent<Manager_UIReuse>().btn_BuyFromTrader.gameObject.SetActive(true);
        par_Managers.GetComponent<Manager_UIReuse>().btn_BuyFromTrader.interactable = false;
        par_Managers.GetComponent<Manager_UIReuse>().btn_BuyFromTrader.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.gameObject.SetActive(true);
        par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.onClick.AddListener(PlayerInventoryScript.CloseShop);

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
            item.GetComponent<Env_Item>().isInPlayerInventory = false;
            item.GetComponent<Env_Item>().isInContainer = false;
            item.GetComponent<Env_Item>().isInTraderShop = true;
            item.GetComponent<Env_Item>().isInRepairMenu = false;
            item.GetComponent<Env_Item>().isBuying = true;
            item.GetComponent<Env_Item>().isTaking = false;
        }

        isShopOpen = true;
    }
    public void CloseShopUI()
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
        par_Managers.GetComponent<Manager_UIReuse>().btn_BuyFromTrader.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.onClick.RemoveAllListeners();

        par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
        par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
        par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();

        PlayerInventoryScript.hasOpenedInventoryOnce = false;
    }
    public void CloseShopAndPlayerInventory()
    {
        par_Managers.GetComponent<Manager_UIReuse>().btn_BuyFromTrader.gameObject.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.gameObject.SetActive(false);

        PlayerInventoryScript.canOpenPlayerInventory = false;
        PlayerInventoryScript.closedInventoryThroughContainer = true;
        PlayerInventoryScript.CloseShop();

        PlayerInventoryScript.hasOpenedInventoryOnce = true;
        PlayerInventoryScript.CloseInventory();
        PlayerInventoryScript.Trader = null;

        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.gameObject.SetActive(false);
        AIScript.OpenNPCDialogue();

        isShopOpen = false;
    }

    private void RebuildInventory()
    {
        foreach (GameObject item in inventory)
        {
            if (item != null)
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
                PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().buttons.Add(btn_New.gameObject);

                item.GetComponent<Env_Item>().isInTraderShop = true;
                item.GetComponent<Env_Item>().isBuying = true;
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

        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = false;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = true;

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        RebuildInventory();
        //Debug.Log("Showing all trader inventory items.");
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

        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = false;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = true;

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null
                && (item.GetComponent<Item_Gun>() != null
                || item.GetComponent<Item_Grenade>() != null
                || item.GetComponent<Item_Melee>() != null))
            {
                if (item.GetComponent<Env_Item>().isProtected)
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

                    item.GetComponent<Env_Item>().isInTraderShop = true;
                    item.GetComponent<Env_Item>().isBuying = true;

                }
            }
        }
        //Debug.Log("Showing trader weapons.");
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

        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = false;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = true;

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null
                && item.GetComponent<Item_Armor>() != null)
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

                item.GetComponent<Env_Item>().isInContainer = true;
                item.GetComponent<Env_Item>().isTaking = true;
            }
        }
        //Debug.Log("Showing trader armor.");
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

        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = false;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = true;

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null && item.GetComponent<Item_Consumable>() != null)
            {
                if (item.GetComponent<Env_Item>().isProtected)
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

                    item.GetComponent<Env_Item>().isInTraderShop = true;
                    item.GetComponent<Env_Item>().isBuying = true;

                }
            }
        }
        //Debug.Log("Showing trader consumables.");
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

        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = false;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = true;

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null && item.GetComponent<Item_Ammo>() != null)
            {
                if (item.GetComponent<Env_Item>().isProtected)
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

                    item.GetComponent<Env_Item>().isInTraderShop = true;
                    item.GetComponent<Env_Item>().isBuying = true;

                }
            }
        }
        //Debug.Log("Showing trader ammo.");
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

        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = false;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = true;

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null
                && item.GetComponent<Item_Gear>() != null)
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

                item.GetComponent<Env_Item>().isInContainer = true;
                item.GetComponent<Env_Item>().isTaking = true;
            }
        }
        //Debug.Log("Showing trader gear.");
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

        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = false;

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null
                && item.GetComponent<Item_Gun>() == null
                && item.GetComponent<Item_Grenade>() == null
                && item.GetComponent<Item_Melee>() == null
                && item.GetComponent<Item_Armor>() == null
                && item.GetComponent<Item_Consumable>() == null
                && item.GetComponent<Item_Ammo>() == null
                && item.GetComponent<Item_Gear>() == null)
            {
                if (item.GetComponent<Env_Item>().isProtected)
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

                    item.GetComponent<Env_Item>().isInTraderShop = true;
                    item.GetComponent<Env_Item>().isBuying = true;

                }
            }
        }
        //Debug.Log("Showing trader misc items.");
    }
}