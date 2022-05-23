using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Inv_Container : MonoBehaviour
{
    [Header("Assignables")]
    public string str_ContainerName;
    [SerializeField] private GameObject thePlayer;
    public GameObject par_ContainerItems;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private GameObject par_Managers;
    public List<GameObject> inventory;

    [Header("For containers")]
    public bool isProtected;
    public bool isLocked;
    public bool needsKey;
    public bool randomizeAllContent;
    [SerializeField] private Env_Lock LockScript;

    [Header("For lootable NPCs")]
    public GameObject discardableDeadNPC;

    //public but hidden variables
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
    [HideInInspector] public List<GameObject> buttons;

    //private variables
    private bool destroySelf;

    private void Start()
    {
        if (gameObject.name != str_ContainerName)
        {
            gameObject.name = str_ContainerName;
        }

        if (inventory.Count > 0)
        {
            foreach (GameObject item in inventory)
            {
                item.GetComponent<Env_Item>().isInContainer = true;
                item.GetComponent<Env_Item>().DeactivateItem();
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) 
            && isContainerInventoryOpen
            && !par_Managers.GetComponent<UI_PauseMenu>().isUIOpen)
        {
            par_Managers.GetComponent<UI_PauseMenu>().isInventoryOpen = false;
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

            par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.gameObject.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.onClick.AddListener(CloseContainerAndPlayerInventory);

            par_Managers.GetComponent<UI_PauseMenu>().isInventoryOpen = true;

            PlayerInventoryScript.CloseInventory();
            PlayerInventoryScript.isPlayerAndContainerOpen = true;
            par_Managers.GetComponent<Manager_UIReuse>().par_Inventory.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().par_Stats.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = str_ContainerName + " inventory";
            PlayerInventoryScript.UpdatePlayerInventoryStats();

            ShowAll();

            par_Managers.GetComponent<Manager_UIReuse>().btn_TakeFromContainer.gameObject.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().btn_TakeFromContainer.GetComponent<Button>().interactable = false;
            par_Managers.GetComponent<Manager_UIReuse>().btn_TakeFromContainer.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_PlaceIntoContainer.gameObject.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().btn_PlaceIntoContainer.GetComponent<Button>().interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_PlaceIntoContainer.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_PlaceIntoContainer.onClick.AddListener(PlayerInventoryScript.CloseContainer);

            //adds discardable dead npc button to dead npc loot
            if (discardableDeadNPC != null)
            {
                par_Managers.GetComponent<Manager_UIReuse>().btn_DiscardDeadBody.gameObject.SetActive(true);
                par_Managers.GetComponent<Manager_UIReuse>().btn_DiscardDeadBody.onClick.RemoveAllListeners();
                par_Managers.GetComponent<Manager_UIReuse>().btn_DiscardDeadBody.onClick.AddListener(DiscardDeadBody);
            }

            par_Managers.GetComponent<UI_PauseMenu>().PauseGameAndCloseUIAndResetBools();

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

            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAll.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowWeapons.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowArmor.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowConsumables.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowAmmo.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowGear.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_ShowMisc.onClick.RemoveAllListeners();

            par_Managers.GetComponent<Manager_UIReuse>().par_Inventory.SetActive(false);
            par_Managers.GetComponent<Manager_UIReuse>().par_Stats.SetActive(false);
            par_Managers.GetComponent<Manager_UIReuse>().btn_TakeFromContainer.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_PlaceIntoContainer.onClick.RemoveAllListeners();

            par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();

            par_Managers.GetComponent<UI_PauseMenu>().callPMCloseOnce = false;
            par_Managers.GetComponent<UI_PauseMenu>().UnpauseGame();

            PlayerInventoryScript.hasOpenedInventoryOnce = false;
            calledContainerOpenOnce = false;
        }
    }
    public void CloseContainerAndPlayerInventory()
    {
        par_Managers.GetComponent<Manager_UIReuse>().btn_TakeFromContainer.gameObject.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().btn_PlaceIntoContainer.gameObject.SetActive(false);

        PlayerInventoryScript.canOpenPlayerInventory = false;
        PlayerInventoryScript.closedInventoryThroughContainer = true;
        PlayerInventoryScript.CloseContainer();

        PlayerInventoryScript.hasOpenedInventoryOnce = true;
        PlayerInventoryScript.CloseInventory();
        PlayerInventoryScript.Container = null;
        PlayerInventoryScript.isPlayerInventoryOpenWithContainerOrTrader = false;

        calledContainerOpenOnce = false;
        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_CloseUI.gameObject.SetActive(false);
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

    //used only for dead AI
    public void DiscardDeadBody()
    {
        par_Managers.GetComponent<UI_PauseMenu>().isInventoryOpen = false;
        CloseContainerAndPlayerInventory();

        destroySelf = true;

        discardableDeadNPC.transform.position = new Vector3(0, -1000, 0);
    }

    //used to randomize this containers content
    public void RandomizeAllContent()
    {
        //get total item count
        int totalItemCount = par_Managers.GetComponent<Manager_Console>().spawnables.Count;
        //get random amount of items we want to spawn
        int selectedItemCount = Random.Range(3, 10);
        //create list for selected item indexes
        List<int> selectedItems = new();
        //pick selectedItemCount amount of random item indexes and assign to list
        for (int i = 0; i < selectedItemCount; i++)
        {
            selectedItems.Add(Random.Range(0, totalItemCount));
        }
        //look for duplicate indexes and remove them
        selectedItems = selectedItems.Distinct().ToList();

        //spawn items in container
        foreach (int i in selectedItems)
        {
            //get item by index
            GameObject foundItem = null;
            foreach (GameObject item in par_Managers.GetComponent<Manager_Console>().spawnables)
            {
                if (par_Managers.GetComponent<Manager_Console>().spawnables.IndexOf(item) == i)
                {
                    foundItem = item;
                    break;
                }
            }

            //spawn item if it isnt null
            if (foundItem != null)
            {
                GameObject newDuplicate = Instantiate(foundItem,
                                                      transform.position,
                                                      Quaternion.identity,
                                                      par_ContainerItems.transform);

                newDuplicate.name = newDuplicate.GetComponent<Env_Item>().str_ItemName;

                inventory.Add(newDuplicate);

                //item count
                if (!newDuplicate.GetComponent<Env_Item>().isStackable
                    || newDuplicate.GetComponent<Item_Consumable>() != null)
                {
                    newDuplicate.GetComponent<Env_Item>().int_itemCount = 1;
                }
                else
                {
                    newDuplicate.GetComponent<Env_Item>().int_itemCount = Random.Range(1, 35);
                }

                //item durability/remainder

                //if this item is a gun
                if (newDuplicate.GetComponent<Item_Gun>() != null)
                {
                    //gun durability
                    float gunMaxDurability = newDuplicate.GetComponent<Item_Gun>().maxDurability;

                    newDuplicate.GetComponent<Item_Gun>().durability =
                        Mathf.Round(Random.Range(gunMaxDurability / 20, gunMaxDurability / 10 * 6) * 10) / 10;
                    newDuplicate.GetComponent<Item_Gun>().LoadValues();
                }
                //if this item is a melee weapon
                else if (newDuplicate.GetComponent<Item_Melee>() != null)
                {
                    //melee weapon durability
                    float meleeWeaponMaxDurability = newDuplicate.GetComponent<Item_Melee>().maxDurability;

                    newDuplicate.GetComponent<Item_Melee>().durability =
                        Mathf.Round(Random.Range(meleeWeaponMaxDurability / 20, meleeWeaponMaxDurability / 10 * 6) * 10) / 10;
                    newDuplicate.GetComponent<Item_Melee>().LoadValues();
                }
                //if this item is a consumable
                else if (newDuplicate.GetComponent<Item_Consumable>() != null)
                {
                    //consumable remainder
                    float consumableMaxRemainder = newDuplicate.GetComponent<Item_Consumable>().maxConsumableAmount;

                    newDuplicate.GetComponent<Item_Consumable>().currentConsumableAmount =
                        Mathf.Round(Random.Range(consumableMaxRemainder / 20, consumableMaxRemainder / 10 * 6) * 10) / 10;
                    newDuplicate.GetComponent<Item_Consumable>().LoadValues();
                }
                //if this item is a battery
                else if (newDuplicate.GetComponent<Item_Battery>() != null)
                {
                    //battery remainder
                    float batteryMaxRemainder = newDuplicate.GetComponent<Item_Battery>().maxBattery;

                    newDuplicate.GetComponent<Item_Battery>().currentBattery =
                        Mathf.Round(Random.Range(batteryMaxRemainder / 20, batteryMaxRemainder / 10 * 6) * 10) / 10;
                    newDuplicate.GetComponent<Item_Battery>().LoadValues();
                }

                newDuplicate.GetComponent<Env_Item>().isInContainer = true;

                newDuplicate.GetComponent<MeshRenderer>().enabled = false;
                if (newDuplicate.GetComponent<Rigidbody>() != null)
                {
                    newDuplicate.GetComponent<Rigidbody>().isKinematic = true;
                }

                newDuplicate.GetComponent<Env_Item>().DeactivateItem();

                //Debug.Log("Spawned item " + newDuplicate.name + " with count " + newDuplicate.GetComponent<Env_Item>().int_itemCount + " in container " + name + ".");
            }
        }
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

        //adds discardable dead npc button to dead npc loot
        if (discardableDeadNPC != null)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_DiscardDeadBody.gameObject.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().btn_DiscardDeadBody.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_DiscardDeadBody.onClick.AddListener(DiscardDeadBody);
        }
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
        PlayerInventoryScript.Container.GetComponent<Inv_Container>().buttons.Add(btn_New.gameObject);

        item.GetComponent<Env_Item>().isInContainer = true;
        item.GetComponent<Env_Item>().isTaking = true;
    }
}