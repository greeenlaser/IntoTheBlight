using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inv_Container : MonoBehaviour
{
    [Header("General")]
    public bool isProtected;
    public bool isLocked;
    public bool needsKey;
    public string str_ContainerName;

    [Header("Assignables")]
    [SerializeField] private GameObject thePlayer;
    public GameObject discardableDeadNPC;
    public GameObject par_ContainerItems;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;
    [SerializeField] private UI_PauseMenu PauseMenuScript;
    [SerializeField] private Env_Lock LockScript;
    public List<GameObject> inventory = new List<GameObject>();

    //public but hidden variables
    [HideInInspector] public bool containerActivated;
    [HideInInspector] public bool isContainerInventoryOpen;
    [HideInInspector] public bool calledContainerOpenOnce;
    [HideInInspector] public bool hasLootedDeadAIInventoryOnce;
    [HideInInspector] public bool showingAllItems;
    [HideInInspector] public bool showingAllWeapons;
    [HideInInspector] public bool showingAllArmor;
    [HideInInspector] public bool showingAllConsumables;
    [HideInInspector] public bool showingAllAmmo;
    [HideInInspector] public bool showingAllGear;
    [HideInInspector] public bool showingAllMisc;
    [HideInInspector] public List<GameObject> buttons = new List<GameObject>();

    //private variables
    private bool destroySelf;

    private void Start()
    {
        foreach (GameObject item in inventory)
        {
            item.GetComponent<Env_Item>().isInContainer = true;
            item.GetComponent<Env_Item>().DeactivateItem();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) 
            && isContainerInventoryOpen
            && !PauseMenuScript.isUIOpen)
        {
            PauseMenuScript.isInventoryOpen = false;
            CloseContainerAndPlayerInventory();
        }
    }

    public void CheckIfLocked()
    {
        if (isLocked)
        {
            if (!needsKey)
            {
                bool hasLockpick = false;
                foreach (GameObject item in thePlayer.GetComponent<Inv_Player>().inventory)
                {
                    if (item.GetComponent<Item_Lockpick>() != null
                        && item.GetComponent<Item_Lockpick>().itemType
                        == Item_Lockpick.ItemType.lockpick)
                    {
                        hasLockpick = true;
                        break;
                    }
                }
                if (hasLockpick)
                {
                    LockScript.OpenLockUI();
                }
                else if (!hasLockpick)
                {
                    //<<< play locked sound here <<<
                    Debug.Log("Error: Did not find any lockpicks in players inventory!");
                }
            }
            else if (needsKey)
            {
                bool foundCorrectKey = false;
                foreach (GameObject item in thePlayer.GetComponent<Inv_Player>().inventory)
                {
                    if (item.GetComponent<Item_Lockpick>() != null
                        && item.GetComponent<Item_Lockpick>().itemType == Item_Lockpick.ItemType.key
                        && item.GetComponent<Item_Lockpick>().targetLock == gameObject)
                    {
                        foundCorrectKey = true;
                        break;
                    }
                }
                if (foundCorrectKey)
                {
                    //Debug.Log("Unlocked this door with key!");
                    LockScript.Unlock();
                }
                else if (!foundCorrectKey)
                {
                    //<<< play locked sound here <<<
                    Debug.Log("Error: Did not find correct key in players inventory!");
                }
            }
        }
        else
        {
            OpenInventory();
        }
    }
    private void OpenInventory()
    {
        if (!calledContainerOpenOnce)
        {
            //only deletes the dead AI after the player has gone 25 meters from the dead AI
            //after the player has looted the dead AI atleast once
            if (!hasLootedDeadAIInventoryOnce && discardableDeadNPC != null)
            {
                hasLootedDeadAIInventoryOnce = true;
            }

            PlayerInventoryScript.Container = gameObject;
            PlayerInventoryScript.isPlayerInventoryOpenWithContainerOrTrader = false;

            UIReuseScript.btn_CloseUI.gameObject.SetActive(true);
            UIReuseScript.btn_CloseUI.onClick.RemoveAllListeners();
            UIReuseScript.btn_CloseUI.onClick.AddListener(CloseContainerAndPlayerInventory);

            PauseMenuScript.isInventoryOpen = true;

            PlayerInventoryScript.CloseInventory();
            PlayerInventoryScript.isPlayerAndContainerOpen = true;
            UIReuseScript.par_Inventory.SetActive(true);
            UIReuseScript.par_Stats.SetActive(true);
            UIReuseScript.txt_InventoryName.text = str_ContainerName + " inventory";
            PlayerInventoryScript.UpdatePlayerInventoryStats();

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

            UIReuseScript.btn_TakeFromContainer.gameObject.SetActive(true);
            UIReuseScript.btn_TakeFromContainer.GetComponent<Button>().interactable = false;
            UIReuseScript.btn_TakeFromContainer.onClick.RemoveAllListeners();
            UIReuseScript.btn_PlaceIntoContainer.gameObject.SetActive(true);
            UIReuseScript.btn_PlaceIntoContainer.GetComponent<Button>().interactable = true;
            UIReuseScript.btn_PlaceIntoContainer.onClick.RemoveAllListeners();
            UIReuseScript.btn_PlaceIntoContainer.onClick.AddListener(PlayerInventoryScript.CloseContainer);

            //adds discardable dead npc button to dead npc loot
            if (discardableDeadNPC != null)
            {
                UIReuseScript.btn_DiscardDeadBody.gameObject.SetActive(true);
                UIReuseScript.btn_DiscardDeadBody.onClick.RemoveAllListeners();
                UIReuseScript.btn_DiscardDeadBody.onClick.AddListener(DiscardDeadBody);
            }

            RebuildInventory();
            PauseMenuScript.PauseGameAndCloseUIAndResetBools();

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
                item.GetComponent<Env_Item>().isInPlayerInventory = false;
                item.GetComponent<Env_Item>().isInContainer = true;
                item.GetComponent<Env_Item>().isInTraderShop = false;
                item.GetComponent<Env_Item>().isInRepairMenu = false;
                item.GetComponent<Env_Item>().isBuying = false;
                item.GetComponent<Env_Item>().isTaking = true;
            }

            isContainerInventoryOpen = true;
            //Debug.Log("Container inventory is open!");
            calledContainerOpenOnce = true;
        }
    }
    public void CloseInventory()
    {
        if (calledContainerOpenOnce)
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
            UIReuseScript.btn_TakeFromContainer.onClick.RemoveAllListeners();
            UIReuseScript.btn_PlaceIntoContainer.onClick.RemoveAllListeners();

            UIReuseScript.ClearAllInventories();
            UIReuseScript.ClearInventoryUI();
            UIReuseScript.ClearStatsUI();

            PauseMenuScript.callPMCloseOnce = false;
            PauseMenuScript.UnpauseGame();

            PlayerInventoryScript.hasOpenedInventoryOnce = false;
            calledContainerOpenOnce = false;
        }
    }
    public void CloseContainerAndPlayerInventory()
    {
        UIReuseScript.btn_TakeFromContainer.gameObject.SetActive(false);
        UIReuseScript.btn_PlaceIntoContainer.gameObject.SetActive(false);

        PlayerInventoryScript.canOpenPlayerInventory = false;
        PlayerInventoryScript.closedInventoryThroughContainer = true;
        PlayerInventoryScript.CloseContainer();

        PlayerInventoryScript.hasOpenedInventoryOnce = true;
        PlayerInventoryScript.CloseInventory();
        PlayerInventoryScript.Container = null;
        PlayerInventoryScript.isPlayerInventoryOpenWithContainerOrTrader = false;

        calledContainerOpenOnce = false;
        UIReuseScript.btn_CloseUI.onClick.RemoveAllListeners();
        UIReuseScript.btn_CloseUI.gameObject.SetActive(false);
        isContainerInventoryOpen = false;
        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        PlayerInventoryScript.canOpenPlayerInventory = false;
        yield return new WaitForSeconds(0.2f);
        PlayerInventoryScript.canOpenPlayerInventory = true;

        if (destroySelf)
        {
            Destroy(discardableDeadNPC);
        }
    }

    public void DiscardDeadBody()
    {
        PauseMenuScript.isInventoryOpen = false;
        CloseContainerAndPlayerInventory();
        destroySelf = true;
        discardableDeadNPC.transform.position = new Vector3(0, -1000, 0);
    }

    private void RebuildInventory()
    {
        foreach (GameObject item in inventory)
        {
            if (item != null)
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
                PlayerInventoryScript.Container.GetComponent<Inv_Container>().buttons.Add(btn_New.gameObject);

                item.GetComponent<Env_Item>().isInContainer = true;
                item.GetComponent<Env_Item>().isTaking = true;
            }
        }

        //adds discardable dead npc button to dead npc loot
        if (discardableDeadNPC != null)
        {
            UIReuseScript.btn_DiscardDeadBody.gameObject.SetActive(true);
            UIReuseScript.btn_DiscardDeadBody.onClick.RemoveAllListeners();
            UIReuseScript.btn_DiscardDeadBody.onClick.AddListener(DiscardDeadBody);
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

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        RebuildInventory();
        //Debug.Log("Showing all trader inventory items.");
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

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null
                && (item.GetComponent<Item_Gun>() != null
                || item.GetComponent<Item_Grenade>() != null
                || item.GetComponent<Item_Melee>() != null))
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

                item.GetComponent<Env_Item>().isInContainer = true;
                item.GetComponent<Env_Item>().isTaking = true;
            }
        }
        //Debug.Log("Showing container weapons.");
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

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null
                && item.GetComponent<Item_Armor>() != null)
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

                item.GetComponent<Env_Item>().isInContainer = true;
                item.GetComponent<Env_Item>().isTaking = true;
            }
        }
        //Debug.Log("Showing container armor.");
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

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null && item.GetComponent<Item_Consumable>() != null)
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

                item.GetComponent<Env_Item>().isInContainer = true;
                item.GetComponent<Env_Item>().isTaking = true;
            }
        }
        //Debug.Log("Showing container consumables.");
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

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null && item.GetComponent<Item_Ammo>() != null)
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

                item.GetComponent<Env_Item>().isInContainer = true;
                item.GetComponent<Env_Item>().isTaking = true;
            }
        }
        //Debug.Log("Showing container ammo.");
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

        PlayerInventoryScript.UpdatePlayerInventoryStats();
        foreach (GameObject item in inventory)
        {
            if (item != null
                && item.GetComponent<Item_Gear>() != null)
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

                item.GetComponent<Env_Item>().isInContainer = true;
                item.GetComponent<Env_Item>().isTaking = true;
            }
        }
        //Debug.Log("Showing container gear.");
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

                item.GetComponent<Env_Item>().isInContainer = true;
                item.GetComponent<Env_Item>().isTaking = true;
            }
        }
        //Debug.Log("Showing container misc items.");
    }
}