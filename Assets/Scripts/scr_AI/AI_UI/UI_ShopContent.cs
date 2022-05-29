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
    public List<GameObject> inventory;

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
    [HideInInspector] public List<GameObject> buttons;

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

        ShowAll();

        PlayerInventoryScript.UpdatePlayerInventoryStats();

        par_Managers.GetComponent<Manager_UIReuse>().btn_BuyFromTrader.gameObject.SetActive(true);
        par_Managers.GetComponent<Manager_UIReuse>().btn_BuyFromTrader.interactable = false;
        par_Managers.GetComponent<Manager_UIReuse>().btn_BuyFromTrader.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.gameObject.SetActive(true);
        par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.onClick.AddListener(PlayerInventoryScript.CloseShop);

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
            item.GetComponent<Env_Item>().isInUpgradeMenu = false;
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

    public void ShowAll()
    {
        HideAll();

        showingAllItems = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = false;

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null)
            {
                RebuildInventory(item);
            }
        }
    }
    public void ShowWeapons()
    {
        HideAll();

        showingAllWeapons = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = false;

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null
                && (item.GetComponent<Item_Gun>() != null
                || item.GetComponent<Item_Grenade>() != null
                || item.GetComponent<Item_Melee>() != null))
            {
                RebuildInventory(item);
            }
        }
    }
    public void ShowArmor()
    {
        HideAll();

        showingAllArmor = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = false;

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null
                && item.GetComponent<Item_Armor>() != null)
            {
                RebuildInventory(item);
            }
        }
    }
    public void ShowConsumables()
    {
        HideAll();

        showingAllConsumables = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = false;

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null 
                && item.GetComponent<Item_Consumable>() != null)
            {
                RebuildInventory(item);
            }
        }
    }
    public void ShowAmmo()
    {
        HideAll();

        showingAllAmmo = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = false;

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null 
                && item.GetComponent<Item_Ammo>() != null)
            {
                RebuildInventory(item);
            }
        }
    }
    public void ShowGear()
    {
        HideAll();

        showingAllGear = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = false;

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null
                && item.GetComponent<Item_Gear>() != null)
            {
                RebuildInventory(item);
            }
        }
    }
    public void ShowMisc()
    {
        HideAll();

        showingAllMisc = true;
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
                RebuildInventory(item);
            }
        }
    }

    private void HideAll()
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

        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.interactable = true;
    }

    private void RebuildInventory(GameObject item)
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