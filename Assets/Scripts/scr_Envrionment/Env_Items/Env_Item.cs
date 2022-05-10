using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Env_Item : MonoBehaviour
{
    [Header("Stats")]
    [Tooltip("Items with this tag cannot be sold or destroyed.")]
    public bool isProtected;
    [Tooltip("Items without this tag only stack once.")]
    public bool isStackable;
    public string str_ItemName;
    public string str_ItemDescription;
    [Tooltip("How much is this item worth at its full durability/remainder?")]
    public int int_maxItemValue;
    [Tooltip("How much does this item weigh?")]
    public int int_ItemWeight;
    [Tooltip("How many of this item exist in this single item slot. Note: Randomized items or dead items dropped from enemies ignore this value.")]
    public int int_itemCount = 1;

    [Header("Assignables")]
    [SerializeField] private Transform pos_HoldItem;
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_DroppedItems;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private GameObject par_Managers;

    [Header("Repairable assignables")]
    public RepairKitTypeRequired repairKitRequired;
    public enum RepairKitTypeRequired
    {
        unassigned,
        Melee_Tier1,
        Melee_Tier2,
        Melee_Tier3,
        LightGun_Tier1,
        LightGun_Tier2,
        LightGun_Tier3,
        HeavyGun_Tier1,
        HeavyGun_Tier2,
        HeavyGun_Tier3,
        LightArmor_Tier1,
        LightArmor_Tier2,
        LightArmor_Tier3,
        HeavyArmor_Tier1,
        HeavyArmor_Tier2,
        HeavyArmor_Tier3,
        Gear_Tier1,
        Gear_Tier2,
        Gear_Tier3
    }

    [Header("Consumable assignables")]
    [SerializeField] private Player_Health PlayerHealthScript;

    [Header("Quest assignables")]
    public bool disableAtStart;
    [SerializeField] private QuestStage_General QuestStage;

    //public but hidden variables
    [HideInInspector] public bool isInPlayerInventory;
    [HideInInspector] public bool isInContainer;
    [HideInInspector] public bool isInTraderShop;
    [HideInInspector] public bool isInRepairMenu;
    [HideInInspector] public bool isBuying;
    [HideInInspector] public bool isTaking;
    [HideInInspector] public bool hasUnderscore;
    [HideInInspector] public bool droppedObject;
    [HideInInspector] public bool toBeDeleted;
    [HideInInspector] public int int_ItemValue;
    [HideInInspector] public float time;
    [HideInInspector] public GameObject theItem;

    //private variables
    private bool foundDuplicate;
    private bool canContinue;
    private float closestDistance;
    private string cellName;

    //multiple checkers
    private bool isPickingUpMultipleItems;
    private bool isBuyingMultipleItems;
    private bool isSellingMultipleItems;
    private bool isDroppingMultipleItems;
    private bool isDestroyingMultipleItems;
    private bool isTakingMultipleItems;
    private bool isPlacingMultipleItems;
    private bool hasBatteriesInInv;
    private int int_selectedCount;
    private int int_confirmedCount;
    private GameObject selectedGun;
    private GameObject correctAmmo;
    private GameObject duplicate;
    //buy, sell, space
    private int int_finalSpace;
    private int int_totalSpace;
    private int int_singlePrice;
    private int int_finalPrice;
    private string str_itemName;
    private string str_containerName;
    private string str_traderName;

    private void Start()
    {
        if (gameObject.name != str_ItemName)
        {
            gameObject.name = str_ItemName;
        }
    }

    private void Update()
    {
        //resets the stackable item bools
        if (!isInPlayerInventory 
            && !isInContainer 
            && !isInTraderShop)
        {
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPickingUpMultipleItems ||
                    isBuyingMultipleItems ||
                    isSellingMultipleItems ||
                    isDroppingMultipleItems ||
                    isDestroyingMultipleItems ||
                    isTakingMultipleItems ||
                    isPlacingMultipleItems)
                {
                    CancelCount();
                }
            }
        }

        //if the object was dropped and distance is over 25
        //or more than 2 irl minutes have passed after the object was dropped
        if (droppedObject
            && !isProtected
            && !par_Managers.GetComponent<UI_PauseMenu>().isGamePaused)
        {
            time += Time.deltaTime;

            if (time > 120)
            {
                Debug.Log("System: " + name + " was destroyed because it had been dropped for too long.");
                DestroyObject();
            }
            else if (Vector3.Distance(transform.position, thePlayer.transform.transform.position) > 25)
            {
                Debug.Log("System: " + name + " was destroyed after player went too far from it.");
                DestroyObject();
            }
        }
    }

    //allows to use certain buttons depending on which inventory
    //the player is in currently
    public void ShowStats()
    {
        RemoveListeners();
        par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
        par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
        par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();

        PlayerInventoryScript.UpdatePlayerInventoryStats();

        foundDuplicate = false;
        duplicate = null;

        int_selectedCount = 0;
        int_confirmedCount = 0;
        isPickingUpMultipleItems = false;
        isDroppingMultipleItems = false;
        isDestroyingMultipleItems = false;
        isPlacingMultipleItems = false;
        isTakingMultipleItems = false;
        isBuyingMultipleItems = false;
        isSellingMultipleItems = false;
        par_Managers.GetComponent<Manager_UIReuse>().ClearCountSliderUI();

        par_Managers.GetComponent<Manager_UIReuse>().txt_AmmoCount.text = "";
        par_Managers.GetComponent<Manager_UIReuse>().txt_protected.gameObject.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().txt_notStackable.gameObject.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().txt_tooHeavy.gameObject.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().txt_tooExpensive.gameObject.SetActive(false);

        //replacing underscores with space
        for (int i = 0; i < str_ItemName.Length - 1; i++)
        {
            if (str_ItemName[i] == '_')
            {
                hasUnderscore = true;
                break;
            }
        }
        if (hasUnderscore)
        {
            string str_fakeName = str_ItemName.Replace("_", " ");
            par_Managers.GetComponent<Manager_UIReuse>().txt_ItemName.text = str_fakeName;
        }
        else if (!hasUnderscore)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_ItemName.text = str_ItemName;
        }
        //item description
        par_Managers.GetComponent<Manager_UIReuse>().txt_ItemDescription.text = str_ItemDescription;

        if (gameObject.GetComponent<Item_Gun>() != null)
        {
            gameObject.GetComponent<Item_Gun>().LoadValues();
        }
        else if (gameObject.GetComponent<Item_Melee>() != null)
        {
            gameObject.GetComponent<Item_Melee>().LoadValues();
        }
        else if (gameObject.GetComponent<Item_Consumable>() != null)
        {
            gameObject.GetComponent<Item_Consumable>().LoadValues();
        }
        else if (gameObject.GetComponent<Item_Battery>() != null)
        {
            gameObject.GetComponent<Item_Battery>().LoadValues();
        }
        else
        {
            int_ItemValue = int_maxItemValue;
        }

        //single or multiple item value and weight
        if (int_itemCount == 1)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_ItemValue.text = int_ItemValue.ToString();
            par_Managers.GetComponent<Manager_UIReuse>().txt_ItemWeight.text = int_ItemWeight.ToString();
        }
        else if (int_itemCount > 1)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_ItemValue.text = int_ItemValue.ToString() + " (" + int_ItemValue * int_itemCount + ")";
            par_Managers.GetComponent<Manager_UIReuse>().txt_ItemWeight.text = int_ItemWeight.ToString() + " (" + int_ItemWeight * int_itemCount + ")";
        }

        //weapon damage
        if (gameObject.GetComponent<Item_Melee>() != null)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_WeaponDamage.text = "Damage: " + gameObject.GetComponent<Item_Melee>().damage.ToString();
        }
        //gun ammo count
        if (gameObject.GetComponent<Item_Gun>() != null)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_WeaponDamage.text = "Damage: " + (Mathf.Floor(gameObject.GetComponent<Item_Gun>().damage * 10) / 10).ToString();
            par_Managers.GetComponent<Manager_UIReuse>().txt_AmmoCount.text = "Ammo: " + gameObject.GetComponent<Item_Gun>().currentClipSize.ToString();
        }
        if (gameObject.GetComponent<Item_Grenade>() != null
            && (gameObject.GetComponent<Item_Grenade>().grenadeType
            == Item_Grenade.GrenadeType.fragmentation
            || gameObject.GetComponent<Item_Grenade>().grenadeType
            == Item_Grenade.GrenadeType.plasma))
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_WeaponDamage.text = "Damage: " + gameObject.GetComponent<Item_Grenade>().maxDamage.ToString();
        }

        if (!isInRepairMenu)
        {
            par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
        }

        //if is in own inventory
        if (isInPlayerInventory
            && !PlayerInventoryScript.isPlayerAndContainerOpen
            && !PlayerInventoryScript.isPlayerAndTraderOpen
            && !PlayerInventoryScript.isPlayerAndRepairOpen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";
            par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();

            if (gameObject.GetComponent<Item_Consumable>() != null)
            {
                if (gameObject.GetComponent<Item_Consumable>().consumableType == Item_Consumable.ConsumableType.Repairkit)
                {
                    float finalPercentage = gameObject.GetComponent<Item_Consumable>().currentConsumableAmount
                                            / gameObject.GetComponent<Item_Consumable>().maxConsumableAmount * 100;
                    par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: " 
                        + Mathf.FloorToInt(finalPercentage).ToString() + "%";
                }
                else if (gameObject.GetComponent<Item_Consumable>().consumableType 
                    == Item_Consumable.ConsumableType.Healthkit
                         || gameObject.GetComponent<Item_Consumable>().consumableType 
                         == Item_Consumable.ConsumableType.Food)
                {
                    float finalPercentage = gameObject.GetComponent<Item_Consumable>().currentConsumableAmount
                                            / gameObject.GetComponent<Item_Consumable>().maxConsumableAmount * 100;
                    par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: " 
                        + Mathf.FloorToInt(finalPercentage).ToString() + "%";

                    par_Managers.GetComponent<Manager_UIReuse>().btn_Consume.gameObject.SetActive(true);
                    if (PlayerHealthScript.health < PlayerHealthScript.maxHealth)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().btn_Consume.interactable = true;
                    }
                    else if (PlayerHealthScript.health == PlayerHealthScript.maxHealth)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().btn_Consume.interactable = false;
                    }
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Consume.onClick.AddListener(gameObject.GetComponent<Item_Consumable>().Consume);
                }
            }
            else if (gameObject.GetComponent<Item_Lockpick>() != null
                     && gameObject.GetComponent<Item_Lockpick>().itemType
                     == Item_Lockpick.ItemType.lockpick)
            {
                float finalPercentage = gameObject.GetComponent<Item_Lockpick>().lockpickDurability
                                        / gameObject.GetComponent<Item_Lockpick>().maxLockpickDurability * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: " 
                    + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }

            else if (gameObject.GetComponent<Item_Gun>() != null)
            {
                float durability = gameObject.GetComponent<Item_Gun>().durability;
                float maxDurability = gameObject.GetComponent<Item_Gun>().maxDurability;

                float finalPercentage = durability / maxDurability * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemDurability.text = "Durability: " 
                    + Mathf.FloorToInt(finalPercentage).ToString() + "%";

                if (!gameObject.GetComponent<Item_Gun>().hasEquippedGun)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Equip.gameObject.SetActive(true);
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Equip.onClick.AddListener(gameObject.GetComponent<Item_Gun>().EquipGun);
                }
                else if (gameObject.GetComponent<Item_Gun>().hasEquippedGun)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Unequip.gameObject.SetActive(true);
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Unequip.onClick.AddListener(gameObject.GetComponent<Item_Gun>().UnequipGun);
                }
            }
            else if (gameObject.GetComponent<Item_Melee>() != null)
            {
                float durability = gameObject.GetComponent<Item_Melee>().durability;
                float maxDurability = gameObject.GetComponent<Item_Melee>().maxDurability;

                float finalPercentage = durability / maxDurability * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemDurability.text = "Durability: " 
                    + Mathf.FloorToInt(finalPercentage).ToString() + "%";

                if (!gameObject.GetComponent<Item_Melee>().hasEquippedMeleeWeapon)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Equip.gameObject.SetActive(true);
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Equip.onClick.AddListener(gameObject.GetComponent<Item_Melee>().EquipMeleeWeapon);
                }
                else if (gameObject.GetComponent<Item_Melee>().hasEquippedMeleeWeapon)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Unequip.gameObject.SetActive(true);
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Unequip.onClick.AddListener(gameObject.GetComponent<Item_Melee>().UnequipMeleeWeapon);
                }
            }
            else if (gameObject.GetComponent<Item_Grenade>() != null)
            {
                if (!gameObject.GetComponent<Item_Grenade>().hasEquippedGrenade)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Equip.gameObject.SetActive(true);
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Equip.onClick.AddListener(gameObject.GetComponent<Item_Grenade>().EquipGrenade);
                }
                else if (gameObject.GetComponent<Item_Grenade>().hasEquippedGrenade)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Unequip.gameObject.SetActive(true);
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Unequip.onClick.AddListener(gameObject.GetComponent<Item_Grenade>().UnequipGrenade);
                }
            }
            else if (gameObject.GetComponent<Item_Flashlight>() != null)
            {
                if (gameObject.GetComponent<Item_Flashlight>().battery != null)
                {
                    GameObject theBattery = gameObject.GetComponent<Item_Flashlight>().battery;

                    float finalPercentage = theBattery.GetComponent<Item_Battery>().currentBattery
                            / theBattery.GetComponent<Item_Battery>().maxBattery * 100;
                    par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: "
                            + Mathf.FloorToInt(finalPercentage).ToString() + "%";
                }
                else
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: 0%";
                }

                if (!gameObject.GetComponent<Item_Flashlight>().isFlashlightEquipped)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Equip.gameObject.SetActive(true);
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Equip.onClick.AddListener(gameObject.GetComponent<Item_Flashlight>().EquipFlashlight);
                }
                else if (gameObject.GetComponent<Item_Flashlight>().isFlashlightEquipped)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Unequip.gameObject.SetActive(true);
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Unequip.onClick.AddListener(gameObject.GetComponent<Item_Flashlight>().UnequipFlashlight);
                }

                foreach (GameObject fl in PlayerInventoryScript.inventory)
                {
                    if (fl.GetComponent<Item_Flashlight>() != null)
                    {
                        fl.GetComponent<Item_Flashlight>().isAssigningBattery = false;
                        break;
                    }
                }
                gameObject.GetComponent<Item_Flashlight>().isAssigningBattery = true;

                hasBatteriesInInv = false;
                foreach (GameObject battery in PlayerInventoryScript.inventory)
                {
                    if (battery.GetComponent<Item_Battery>() != null
                        && !battery.GetComponent<Item_Battery>().isInUse)
                    {
                        battery.GetComponent<Item_Battery>().target = gameObject;
                        hasBatteriesInInv = true;
                    }
                }

                par_Managers.GetComponent<Manager_UIReuse>().btn_AddBattery.gameObject.SetActive(true);
                if (hasBatteriesInInv
                    && gameObject.GetComponent<Item_Flashlight>().battery == null)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().btn_AddBattery.interactable = true;
                    par_Managers.GetComponent<Manager_UIReuse>().btn_AddBattery.onClick.AddListener(PlayerInventoryScript.ShowUnequippedBatteries);
                }
                else
                {
                    par_Managers.GetComponent<Manager_UIReuse>().btn_AddBattery.interactable = false;
                }

                par_Managers.GetComponent<Manager_UIReuse>().btn_RemoveBattery.gameObject.SetActive(true);
                if (gameObject.GetComponent<Item_Flashlight>().battery != null)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().btn_RemoveBattery.interactable = true;
                    par_Managers.GetComponent<Manager_UIReuse>().btn_RemoveBattery.onClick.AddListener(gameObject.GetComponent<Item_Flashlight>().RemoveBattery);
                }
                else
                {
                    par_Managers.GetComponent<Manager_UIReuse>().btn_RemoveBattery.interactable = false;
                }
            }
            else if (gameObject.GetComponent<Item_Battery>() != null)
            {
                float finalPercentage = gameObject.GetComponent<Item_Battery>().currentBattery
                        / gameObject.GetComponent<Item_Battery>().maxBattery * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: "
                        + Mathf.FloorToInt(finalPercentage).ToString() + "%";

                if (PlayerInventoryScript.showingAllUnusedBatteries)
                {
                    foreach (GameObject battery in PlayerInventoryScript.inventory)
                    {
                        if (battery.GetComponent<Item_Battery>() != null)
                        {
                            battery.GetComponent<Item_Battery>().isBeingAssigned = false;
                        }
                    }

                    gameObject.GetComponent<Item_Battery>().isBeingAssigned = true;

                    par_Managers.GetComponent<Manager_UIReuse>().btn_Equip.gameObject.SetActive(true);
                    GameObject target = gameObject.GetComponent<Item_Battery>().target;
                    Item_Flashlight flashlight = target.GetComponent<Item_Flashlight>();
                    par_Managers.GetComponent<Manager_UIReuse>().btn_Equip.onClick.AddListener(flashlight.AssignBattery);
                }
            }

            if (!isProtected)
            {
                par_Managers.GetComponent<Manager_UIReuse>().btn_Destroy.gameObject.SetActive(true);
                par_Managers.GetComponent<Manager_UIReuse>().btn_Destroy.onClick.AddListener(Destroy);
                par_Managers.GetComponent<Manager_UIReuse>().btn_Drop.gameObject.SetActive(true);
                par_Managers.GetComponent<Manager_UIReuse>().btn_Drop.onClick.AddListener(Drop);
            }
            else if (isProtected
                     || (gameObject.GetComponent<Item_Battery>() != null
                     && gameObject.GetComponent<Item_Battery>().isInUse))
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_protected.gameObject.SetActive(true);
            }

            if (!isStackable)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_notStackable.gameObject.SetActive(true);
            }
        }

        //if is looting container
        else if (PlayerInventoryScript.isPlayerAndContainerOpen
                && isInContainer
                && isTaking)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = 
                PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";

            par_Managers.GetComponent<Manager_UIReuse>().RebuildContainerInventory();

            par_Managers.GetComponent<Manager_UIReuse>().btn_PlaceIntoContainer.onClick.AddListener(
                PlayerInventoryScript.CloseContainer);

            par_Managers.GetComponent<Manager_UIReuse>().btn_Take.gameObject.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().btn_Take.onClick.AddListener(Take);

            if (gameObject.GetComponent<Item_Consumable>() != null)
            {
                if (gameObject.GetComponent<Item_Consumable>().consumableType
                    == Item_Consumable.ConsumableType.Repairkit
                    || gameObject.GetComponent<Item_Consumable>().consumableType
                    == Item_Consumable.ConsumableType.Healthkit)
                {
                    float finalPercentage = gameObject.GetComponent<Item_Consumable>().currentConsumableAmount
                                            / gameObject.GetComponent<Item_Consumable>().maxConsumableAmount * 100;
                    par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";
                }
            }
            else if (gameObject.GetComponent<Item_Lockpick>() != null
                    && gameObject.GetComponent<Item_Lockpick>().itemType
                    == Item_Lockpick.ItemType.lockpick)
            {
                float finalPercentage = gameObject.GetComponent<Item_Lockpick>().lockpickDurability
                                        / gameObject.GetComponent<Item_Lockpick>().maxLockpickDurability * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }

            else if (gameObject.GetComponent<Item_Gun>() != null)
            {
                float durability = gameObject.GetComponent<Item_Gun>().durability;
                float maxDurability = gameObject.GetComponent<Item_Gun>().maxDurability;

                float finalPercentage = durability / maxDurability * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemDurability.text = "Durability: "
                    + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }
            else if (gameObject.GetComponent<Item_Melee>() != null)
            {
                float durability = gameObject.GetComponent<Item_Melee>().durability;
                float maxDurability = gameObject.GetComponent<Item_Melee>().maxDurability;

                float finalPercentage = durability / maxDurability * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemDurability.text = "Durability: "
                    + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }

            else if (gameObject.GetComponent<Item_Flashlight>() != null)
            {
                if (gameObject.GetComponent<Item_Flashlight>().battery != null)
                {
                    GameObject theBattery = gameObject.GetComponent<Item_Flashlight>().battery;

                    float finalPercentage = theBattery.GetComponent<Item_Battery>().currentBattery
                            / theBattery.GetComponent<Item_Battery>().maxBattery * 100;
                    par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: "
                            + Mathf.FloorToInt(finalPercentage).ToString() + "%";
                }
                else
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: 0%";
                }
            }
            else if (gameObject.GetComponent<Item_Battery>() != null)
            {
                float finalPercentage = gameObject.GetComponent<Item_Battery>().currentBattery
                        / gameObject.GetComponent<Item_Battery>().maxBattery * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: "
                        + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }

            if (int_ItemWeight > PlayerInventoryScript.invSpace)
            {
                par_Managers.GetComponent<Manager_UIReuse>().btn_Take.interactable = false;
                par_Managers.GetComponent<Manager_UIReuse>().txt_tooHeavy.gameObject.SetActive(true);
            }
            else
            {
                par_Managers.GetComponent<Manager_UIReuse>().btn_Take.interactable = true;
                par_Managers.GetComponent<Manager_UIReuse>().txt_tooHeavy.gameObject.SetActive(false);
            }

            if (!isProtected)
            {
                par_Managers.GetComponent<Manager_UIReuse>().btn_Place.gameObject.SetActive(true);
                par_Managers.GetComponent<Manager_UIReuse>().btn_Place.interactable = true;
                par_Managers.GetComponent<Manager_UIReuse>().btn_Place.onClick.AddListener(Place);
            }
            else if (isProtected
                     || (gameObject.GetComponent<Item_Battery>() != null
                     && gameObject.GetComponent<Item_Battery>().isInUse))
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_protected.gameObject.SetActive(true);
            }

            if (!isStackable)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_notStackable.gameObject.SetActive(true);
            }
            //Debug.Log("Looting from " + PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + "!");
        }
        //if is placing into container
        else if (PlayerInventoryScript.isPlayerAndContainerOpen
                && isInPlayerInventory
                && !isTaking)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";
            par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();

            par_Managers.GetComponent<Manager_UIReuse>().btn_TakeFromContainer.onClick.AddListener(
                PlayerInventoryScript.Container.GetComponent<Inv_Container>().CheckIfLocked);

            if (gameObject.GetComponent<Item_Consumable>() != null)
            {
                if (gameObject.GetComponent<Item_Consumable>().consumableType 
                    == Item_Consumable.ConsumableType.Repairkit
                    || gameObject.GetComponent<Item_Consumable>().consumableType 
                    == Item_Consumable.ConsumableType.Healthkit)
                {
                    float finalPercentage = gameObject.GetComponent<Item_Consumable>().currentConsumableAmount
                                            / gameObject.GetComponent<Item_Consumable>().maxConsumableAmount * 100;
                    par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: " 
                        + Mathf.FloorToInt(finalPercentage).ToString() + "%";
                }
            }
            else if (gameObject.GetComponent<Item_Lockpick>() != null
                     && gameObject.GetComponent<Item_Lockpick>().itemType
                     == Item_Lockpick.ItemType.lockpick)
            {
                float finalPercentage = gameObject.GetComponent<Item_Lockpick>().lockpickDurability
                                        / gameObject.GetComponent<Item_Lockpick>().maxLockpickDurability * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: " 
                    + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }
            else if (gameObject.GetComponent<Item_Gun>() != null)
            {
                float durability = gameObject.GetComponent<Item_Gun>().durability;
                float maxDurability = gameObject.GetComponent<Item_Gun>().maxDurability;

                float finalPercentage = durability / maxDurability * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemDurability.text = "Durability: " 
                    + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }
            else if (gameObject.GetComponent<Item_Melee>() != null)
            {
                float durability = gameObject.GetComponent<Item_Melee>().durability;
                float maxDurability = gameObject.GetComponent<Item_Melee>().maxDurability;

                float finalPercentage = durability / maxDurability * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemDurability.text = "Durability: " 
                    + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }

            else if (gameObject.GetComponent<Item_Flashlight>() != null)
            {
                if (gameObject.GetComponent<Item_Flashlight>().battery != null)
                {
                    GameObject theBattery = gameObject.GetComponent<Item_Flashlight>().battery;

                    float finalPercentage = theBattery.GetComponent<Item_Battery>().currentBattery
                            / theBattery.GetComponent<Item_Battery>().maxBattery * 100;
                    par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: "
                            + Mathf.FloorToInt(finalPercentage).ToString() + "%";
                }
                else
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: 0%";
                }
            }
            else if (gameObject.GetComponent<Item_Battery>() != null)
            {
                float finalPercentage = gameObject.GetComponent<Item_Battery>().currentBattery
                        / gameObject.GetComponent<Item_Battery>().maxBattery * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: "
                        + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }

            if (!isProtected)
            {
                par_Managers.GetComponent<Manager_UIReuse>().btn_Place.gameObject.SetActive(true);
                par_Managers.GetComponent<Manager_UIReuse>().btn_Place.interactable = true;
                par_Managers.GetComponent<Manager_UIReuse>().btn_Place.onClick.AddListener(Place);
            }
            else if (isProtected
                     || (gameObject.GetComponent<Item_Battery>() != null
                     && gameObject.GetComponent<Item_Battery>().isInUse))
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_protected.gameObject.SetActive(true);
            }

            if (!isStackable)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_notStackable.gameObject.SetActive(true);
            }
            //Debug.Log("Placing into " + PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + "!");
        }
        //if is buying from npc
        else if (PlayerInventoryScript.isPlayerAndTraderOpen
                && isInTraderShop
                && isBuying)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().str_ShopName;
            par_Managers.GetComponent<Manager_UIReuse>().RebuildShopInventory();

            par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.onClick.AddListener(PlayerInventoryScript.CloseShop);

            if (gameObject.GetComponent<Item_Consumable>() != null)
            {
                if (gameObject.GetComponent<Item_Consumable>().consumableType == Item_Consumable.ConsumableType.Repairkit
                    || gameObject.GetComponent<Item_Consumable>().consumableType == Item_Consumable.ConsumableType.Healthkit)
                {
                    float finalPercentage = gameObject.GetComponent<Item_Consumable>().currentConsumableAmount
                                            / gameObject.GetComponent<Item_Consumable>().maxConsumableAmount * 100;
                    par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: " 
                        + Mathf.FloorToInt(finalPercentage).ToString() + "%";
                }
            }
            else if (gameObject.GetComponent<Item_Lockpick>() != null
                     && gameObject.GetComponent<Item_Lockpick>().itemType
                     == Item_Lockpick.ItemType.lockpick)
            {
                float finalPercentage = gameObject.GetComponent<Item_Lockpick>().lockpickDurability
                                        / gameObject.GetComponent<Item_Lockpick>().maxLockpickDurability * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: " 
                    + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }

            else if (gameObject.GetComponent<Item_Gun>() != null)
            {
                float durability = gameObject.GetComponent<Item_Gun>().durability;
                float maxDurability = gameObject.GetComponent<Item_Gun>().maxDurability;

                float finalPercentage = durability / maxDurability * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemDurability.text = "Durability: "
                    + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }
            else if (gameObject.GetComponent<Item_Melee>() != null)
            {
                float durability = gameObject.GetComponent<Item_Melee>().durability;
                float maxDurability = gameObject.GetComponent<Item_Melee>().maxDurability;

                float finalPercentage = durability / maxDurability * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemDurability.text = "Durability: " 
                    + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }

            else if (gameObject.GetComponent<Item_Flashlight>() != null)
            {
                if (gameObject.GetComponent<Item_Flashlight>().battery != null)
                {
                    GameObject theBattery = gameObject.GetComponent<Item_Flashlight>().battery;

                    float finalPercentage = theBattery.GetComponent<Item_Battery>().currentBattery
                            / theBattery.GetComponent<Item_Battery>().maxBattery * 100;
                    par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: "
                            + Mathf.FloorToInt(finalPercentage).ToString() + "%";
                }
                else
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: 0%";
                }
            }
            else if (gameObject.GetComponent<Item_Battery>() != null)
            {
                float finalPercentage = gameObject.GetComponent<Item_Battery>().currentBattery
                        / gameObject.GetComponent<Item_Battery>().maxBattery * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: "
                        + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }

            if (PlayerInventoryScript.money >= int_ItemValue)
            {
                par_Managers.GetComponent<Manager_UIReuse>().btn_BuyItem.gameObject.SetActive(true);
                par_Managers.GetComponent<Manager_UIReuse>().btn_BuyItem.interactable = true;
                par_Managers.GetComponent<Manager_UIReuse>().btn_BuyItem.onClick.AddListener(Buy);
            }
            else if (PlayerInventoryScript.money < int_ItemValue)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_tooExpensive.gameObject.SetActive(true);
            }

            if (!isStackable)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_notStackable.gameObject.SetActive(true);
            }
            //Debug.Log("Buying from str_traderName!");
        }
        //if is selling to npc
        else if (PlayerInventoryScript.isPlayerAndTraderOpen
                && isInPlayerInventory
                && !isBuying)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";
            par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();

            int_finalPrice /= 2;
            par_Managers.GetComponent<Manager_UIReuse>().txt_ItemValue.text = int_finalPrice.ToString();

            par_Managers.GetComponent<Manager_UIReuse>().btn_BuyFromTrader.onClick.AddListener(
                PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().OpenShopUI);

            if (gameObject.GetComponent<Item_Consumable>() != null)
            {
                if (gameObject.GetComponent<Item_Consumable>().consumableType 
                    == Item_Consumable.ConsumableType.Repairkit
                    || gameObject.GetComponent<Item_Consumable>().consumableType 
                    == Item_Consumable.ConsumableType.Healthkit)
                {
                    float finalPercentage = gameObject.GetComponent<Item_Consumable>().currentConsumableAmount
                                            / gameObject.GetComponent<Item_Consumable>().maxConsumableAmount * 100;
                    par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: " 
                        + Mathf.FloorToInt(finalPercentage).ToString() + "%";
                }
            }
            else if (gameObject.GetComponent<Item_Lockpick>() != null
                     && gameObject.GetComponent<Item_Lockpick>().itemType
                     == Item_Lockpick.ItemType.lockpick)
            {
                float finalPercentage = gameObject.GetComponent<Item_Lockpick>().lockpickDurability
                                        / gameObject.GetComponent<Item_Lockpick>().maxLockpickDurability * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: " 
                    + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }

            else if (gameObject.GetComponent<Item_Gun>() != null)
            {
                float durability = gameObject.GetComponent<Item_Gun>().durability;
                float maxDurability = gameObject.GetComponent<Item_Gun>().maxDurability;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemDurability.text = "Durability: " 
                    + Mathf.FloorToInt(durability / maxDurability * 100).ToString() + "%";
            }
            else if (gameObject.GetComponent<Item_Melee>() != null)
            {
                float durability = gameObject.GetComponent<Item_Melee>().durability;
                float maxDurability = gameObject.GetComponent<Item_Melee>().maxDurability;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemDurability.text = "Durability: "
                    + Mathf.FloorToInt(durability / maxDurability * 100).ToString() + "%";
            }

            else if (gameObject.GetComponent<Item_Flashlight>() != null)
            {
                if (gameObject.GetComponent<Item_Flashlight>().battery != null)
                {
                    GameObject theBattery = gameObject.GetComponent<Item_Flashlight>().battery;

                    float finalPercentage = theBattery.GetComponent<Item_Battery>().currentBattery
                            / theBattery.GetComponent<Item_Battery>().maxBattery * 100;
                    par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: "
                            + Mathf.FloorToInt(finalPercentage).ToString() + "%";
                }
                else
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: 0%";
                }
            }
            else if (gameObject.GetComponent<Item_Battery>() != null)
            {
                float finalPercentage = gameObject.GetComponent<Item_Battery>().currentBattery
                        / gameObject.GetComponent<Item_Battery>().maxBattery * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemRemainder.text = "Remainder: "
                        + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }

            if (!isProtected)
            {
                par_Managers.GetComponent<Manager_UIReuse>().btn_SellItem.gameObject.SetActive(true);
                par_Managers.GetComponent<Manager_UIReuse>().btn_SellItem.interactable = true;
                par_Managers.GetComponent<Manager_UIReuse>().btn_SellItem.onClick.AddListener(Sell);
            }
            else if (isProtected
                     || (gameObject.GetComponent<Item_Battery>() != null
                     && gameObject.GetComponent<Item_Battery>().isInUse))
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_protected.gameObject.SetActive(true);
            }

            if (!isStackable)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_notStackable.gameObject.SetActive(true);
            }
            //Debug.Log("Selling to str_traderName!");
        }
        //if is in repair menu
        if (PlayerInventoryScript.isPlayerAndRepairOpen
            && isInRepairMenu)
        {
            par_Managers.GetComponent<Manager_UIReuse>().RebuildRepairMenu();

            //trader repairs with money
            if (PlayerInventoryScript.Trader != null
                && PlayerInventoryScript.Workbench == null)
            {
                float maxRepairPrice = gameObject.GetComponent<Item_Gun>().maxRepairPrice;
                float priceToRepairOnePercent = Mathf.FloorToInt(maxRepairPrice / 100);

                par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = 
                    PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName + "(s) repair shop";

                if (gameObject.GetComponent<Item_Gun>() != null
                    && gameObject.GetComponent<Item_Gun>().durability
                    < gameObject.GetComponent<Item_Gun>().maxDurability)
                {
                    //looks for money in player inv and only enables repair button
                    //in trader repair menu if player actually
                    //has enough money to repair 1% or more of this item
                    if (PlayerInventoryScript.money >= priceToRepairOnePercent)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().btn_Repair.gameObject.SetActive(true);
                        par_Managers.GetComponent<Manager_UIReuse>().btn_Repair.interactable = true;
                        par_Managers.GetComponent<Manager_UIReuse>().btn_Repair.onClick.AddListener(RepairWithMoney);
                    }
                    else
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().txt_tooExpensive.gameObject.SetActive(true);
                    }
                }
            }
            //workbench repairs with repair kits
            else if (PlayerInventoryScript.Trader == null
                     && PlayerInventoryScript.Workbench != null)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = 
                    PlayerInventoryScript.Workbench.GetComponent<Env_Workbench>().str_workbenchName;

                if (gameObject.GetComponent<Item_Gun>() != null
                    && gameObject.GetComponent<Item_Gun>().durability
                     < gameObject.GetComponent<Item_Gun>().maxDurability)
                {
                    //looks for repair kits in player inv and only enables repair button
                    //in workbench repair menu if player actually the correct repair kit
                    foreach (GameObject item in PlayerInventoryScript.inventory)
                    {
                        if (item.GetComponent<Item_Consumable>() != null
                            && item.GetComponent<Item_Consumable>().consumableType
                            == Item_Consumable.ConsumableType.Repairkit
                            && item.GetComponent<Item_Consumable>().repairKitType.ToString()
                            == gameObject.GetComponent<Env_Item>().repairKitRequired.ToString())
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().btn_Repair.gameObject.SetActive(true);
                            par_Managers.GetComponent<Manager_UIReuse>().btn_Repair.interactable = true;
                            item.GetComponent<Item_Consumable>().item = gameObject;
                            par_Managers.GetComponent<Manager_UIReuse>().btn_Repair.onClick.AddListener(item.GetComponent<Item_Consumable>().Consume);
                            break;
                        }
                    }
                }
            }

            if (gameObject.GetComponent<Item_Gun>() != null)
            {
                float durability = gameObject.GetComponent<Item_Gun>().durability;
                float maxDurability = gameObject.GetComponent<Item_Gun>().maxDurability;

                float finalPercentage = durability / maxDurability * 100;
                par_Managers.GetComponent<Manager_UIReuse>().txt_ItemDurability.text = "Durability: " 
                    + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }
            if (!isStackable)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_notStackable.gameObject.SetActive(true);
            }
            if (isProtected)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_protected.gameObject.SetActive(true);
            }
        }
    }

    //repair with money in trader repair shop
    public void RepairWithMoney()
    {
        int currentPlayerMoney = PlayerInventoryScript.money;
        float currDurability = gameObject.GetComponent<Item_Gun>().durability;
        float maxDurability = gameObject.GetComponent<Item_Gun>().maxDurability;
        float priceToRepairOnePercent = gameObject.GetComponent<Item_Gun>().maxRepairPrice / 100;
        float currDurabilityPercent = Mathf.FloorToInt(currDurability / maxDurability * 100);
        int percentageToMaxDurability = 100 - Mathf.FloorToInt(currDurabilityPercent);
        int priceToFullyRepair = Mathf.FloorToInt(priceToRepairOnePercent * percentageToMaxDurability);

        //if player has enough money to repair this gun
        if (currentPlayerMoney >= priceToFullyRepair)
        {
            gameObject.GetComponent<Item_Gun>().durability = Mathf.RoundToInt(maxDurability);

            PlayerInventoryScript.money -= Mathf.FloorToInt(priceToFullyRepair);

            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            RemoveListeners();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildRepairMenu();
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = 
                PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName + "(s) repair shop";

            PlayerInventoryScript.UpdatePlayerInventoryStats();
            par_Managers.GetComponent<Manager_UIReuse>().durability = gameObject.GetComponent<Item_Gun>().maxDurability;
            par_Managers.GetComponent<Manager_UIReuse>().maxDurability = gameObject.GetComponent<Item_Gun>().maxDurability;
            par_Managers.GetComponent<Manager_UIReuse>().UpdateWeaponQuality();

            //Debug.Log("Fully repaired " + str_ItemName + " and removed " + priceToFullyRepair + " money from players inventory!");
        }
        //if player doesnt have enough money to fully repair this gun
        else if (currentPlayerMoney < priceToFullyRepair)
        {
            //repairs one percent at a time until player has less than enough money to repair this item or 0 money
            float durabilityIncreasePercent = 0;
            currentPlayerMoney = PlayerInventoryScript.money;
            while (currentPlayerMoney - priceToRepairOnePercent > 0)
            {
                currentPlayerMoney -= Mathf.FloorToInt(priceToRepairOnePercent);
                durabilityIncreasePercent += 1;
            }

            //get current gun durability %
            currDurabilityPercent = Mathf.FloorToInt(currDurability / maxDurability * 100);
            //add increased % to current %
            float increasedDurabilityPercent = currDurabilityPercent + durabilityIncreasePercent;
            //get single % durability real value 
            float singleDurability = Mathf.FloorToInt(gameObject.GetComponent<Item_Gun>().maxDurability / 100);
            //get durability new real value
            float increasedDurability = singleDurability * increasedDurabilityPercent;
            //update gun durability to new real value
            gameObject.GetComponent<Item_Gun>().durability = increasedDurability;

            //int moneyRemoved = PlayerInventoryScript.money - currentPlayerMoney;
            PlayerInventoryScript.money = currentPlayerMoney;

            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            RemoveListeners();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildRepairMenu();
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = 
                PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName + "(s) repair shop";

            PlayerInventoryScript.UpdatePlayerInventoryStats();
            par_Managers.GetComponent<Manager_UIReuse>().durability = gameObject.GetComponent<Item_Gun>().durability;
            par_Managers.GetComponent<Manager_UIReuse>().maxDurability = gameObject.GetComponent<Item_Gun>().maxDurability;
            par_Managers.GetComponent<Manager_UIReuse>().UpdateWeaponQuality();

            //float newDurabilityPercent = Mathf.FloorToInt(increasedDurability / maxDurability * 100);
            //Debug.Log("Repaired " + str_ItemName + " for " + durabilityIncreasePercent + "% and removed " + moneyRemoved + " money from players inventory!");
        }
    }

    //picking up an item from the ground
    public void PickUp()
    {
        //check if player inventory has gameobject with same name
        for (int i = 0; i < PlayerInventoryScript.inventory.Count; i++)
        {
            str_itemName = PlayerInventoryScript.inventory[i].name;
            if (str_itemName == str_ItemName && isStackable)
            {
                //Debug.Log("Found duplicate " + PlayerInventoryScript.inventory[i].name + " at player inventory!");

                foundDuplicate = true;
                duplicate = PlayerInventoryScript.inventory[i];
                break;
            }
        }

        int_finalSpace = PlayerInventoryScript.invSpace - int_ItemWeight;
        if (int_itemCount == 1 && isStackable)
        {
            if (int_finalSpace >= 0)
            {
                droppedObject = false;
                time = 0;
                if (foundDuplicate)
                {
                    duplicate.GetComponent<Env_Item>().int_itemCount += int_itemCount;
                    PlayerInventoryScript.invSpace -= int_ItemWeight;

                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    RemoveListeners();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();

                    par_Managers.GetComponent<Manager_UIReuse>().InteractUIDisabled();

                    //this is only called if queststage was assigned
                    //aka when this item is used in a quest
                    if (QuestStage != null)
                    {
                        QuestStage.PickedUpItem();
                    }

                    PlayerInventoryScript.UpdatePlayerInventoryStats();

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        UpdateGunsAndAmmo();
                    }

                    //Debug.Log("Picked up one duplicate " + str_ItemName + " from the ground! Removed " + int_ItemWeight.ToString() + " space from players inventory.");

                    Destroy(gameObject);
                }
                else if (!foundDuplicate)
                {
                    theItem = gameObject;

                    par_Managers.GetComponent<Manager_UIReuse>().InteractUIDisabled();

                    PlayerInventoryScript.inventory.Add(gameObject);
                    PlayerInventoryScript.invSpace -= int_ItemWeight;

                    par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(str_ItemName);

                    foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
                    {
                        if (cell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                        {
                            cell.GetComponent<Manager_CurrentCell>().items.Remove(gameObject);
                            break;
                        }
                    }

                    gameObject.GetComponent<Env_ObjectPickup>().isHolding = false;
                    gameObject.transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;
                    gameObject.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);
                    isInPlayerInventory = true;

                    foundDuplicate = false;
                    duplicate = null;

                    //this is only called if queststage was assigned
                    //aka when this item is used in a quest
                    if (QuestStage != null)
                    {
                        QuestStage.PickedUpItem();
                    }

                    PlayerInventoryScript.UpdatePlayerInventoryStats();

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        UpdateGunsAndAmmo();
                    }

                    //Debug.Log("Picked up one " + str_ItemName + " from the ground! Removed " + int_ItemWeight.ToString() + " space from players inventory.");
                    DeactivateItem();
                }
            }
            else
            {
                foundDuplicate = false;
                duplicate = null;

                //this is only called if queststage was assigned
                //aka when this item is used in a quest
                if (QuestStage != null)
                {
                    Debug.Log("Error: Could not pick up quest item!");
                }

                Debug.Log("Error: Not enough inventory space to pick up " + str_ItemName + "!");
            }
        }
        else if (int_itemCount > 1 && isStackable)
        {
            isPickingUpMultipleItems = true;

            par_Managers.GetComponent<Manager_UIReuse>().InteractUIDisabled();

            if (!par_Managers.GetComponent<UI_PauseMenu>().isGamePaused)
            {
                par_Managers.GetComponent<UI_PauseMenu>().PauseGame();
            }

            par_Managers.GetComponent<Manager_UIReuse>().par_ItemCount.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().itemCountSlider.maxValue = int_itemCount;

            par_Managers.GetComponent<Manager_UIReuse>().txt_CountInfo.text = "Picking up " + str_ItemName + "(s) ...";
            par_Managers.GetComponent<Manager_UIReuse>().itemCountSlider.onValueChanged.AddListener(SliderValue);
            par_Managers.GetComponent<Manager_UIReuse>().txt_CountValue.text = "Selected count: 1/" + int_itemCount;
            par_Managers.GetComponent<Manager_UIReuse>().txt_SliderInfo.text = "Total weight removed: " + int_ItemWeight;
            par_Managers.GetComponent<Manager_UIReuse>().btn_ConfirmCount.onClick.AddListener(ConfirmCount);
            par_Managers.GetComponent<Manager_UIReuse>().btn_CancelCount.onClick.AddListener(CancelCount);
        }
        else if (!isStackable)
        {
            if (int_finalSpace >= 0)
            {
                theItem = gameObject;

                par_Managers.GetComponent<Manager_UIReuse>().InteractUIDisabled();

                PlayerInventoryScript.inventory.Add(gameObject);
                PlayerInventoryScript.invSpace -= int_ItemWeight;

                par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(str_ItemName);

                foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
                {
                    if (cell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                    {
                        cell.GetComponent<Manager_CurrentCell>().items.Remove(gameObject);
                        break;
                    }
                }

                gameObject.GetComponent<Env_ObjectPickup>().isHolding = false;
                gameObject.transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;
                gameObject.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);
                isInPlayerInventory = true;

                foundDuplicate = false;
                duplicate = null;

                //this is only called if queststage was assigned
                //aka when this item is used in a quest
                if (QuestStage != null)
                {
                    QuestStage.PickedUpItem();
                }

                //this is only called if the player picked up the exoskeleton
                if (str_ItemName == "Exoskeleton")
                {
                    par_Managers.GetComponent<UI_AbilityManager>().hasExoskeleton = true;
                    par_Managers.GetComponent<Manager_UIReuse>().ShowExoskeletonUI();
                }

                PlayerInventoryScript.UpdatePlayerInventoryStats();

                par_Managers.GetComponent<Manager_UIReuse>().InteractUIDisabled();
                //Debug.Log("Picked up one non-stackable " + str_ItemName + " from the ground! Removed " + int_ItemWeight.ToString() + " space from players inventory.");

                DeactivateItem();
            }
            else
            {
                //this is only called if queststage was assigned
                //aka when this item is used in a quest
                if (QuestStage != null)
                {
                    Debug.Log("Error: Could not pick up quest item!");
                }

                Debug.Log("Error: Not enough inventory space to pick up " + str_ItemName + "!");
            }
        }
    }

    public void Buy()
    {
        //check if player inventory has gameobject with same name
        for (int i = 0; i < PlayerInventoryScript.inventory.Count; i++)
        {
            str_itemName = PlayerInventoryScript.inventory[i].name;
            if (str_itemName == str_ItemName && isStackable)
            {
                //Debug.Log("Found duplicate " + PlayerInventoryScript.inventory[i].name + " at player inventory!");

                foundDuplicate = true;
                duplicate = PlayerInventoryScript.inventory[i];
                break;
            }
        }

        str_traderName = PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName;
        if (int_itemCount == 1 && isStackable)
        {
            int_finalSpace = PlayerInventoryScript.invSpace - int_ItemWeight;
            if (int_finalSpace >= 0 && PlayerInventoryScript.money >= int_ItemValue)
            {
                if (foundDuplicate)
                {
                    PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().inventory.Remove(gameObject);

                    duplicate.GetComponent<Env_Item>().int_itemCount += int_itemCount;
                    PlayerInventoryScript.invSpace -= int_ItemWeight;
                    PlayerInventoryScript.money -= int_ItemValue;

                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    RemoveListeners();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildShopInventory();

                    par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.onClick.AddListener(PlayerInventoryScript.CloseShop);

                    par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().str_ShopName;

                    //Debug.Log("Bought one " + str_ItemName + " from " + str_traderName + " for " + int_ItemValue.ToString() + "! Removed " + int_ItemWeight.ToString() + " space from players inventory.");

                    PlayerInventoryScript.UpdatePlayerInventoryStats();

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        UpdateGunsAndAmmo();
                    }

                    Destroy(gameObject);
                }
                else if (!foundDuplicate)
                {
                    PlayerInventoryScript.inventory.Add(gameObject);
                    PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().inventory.Remove(gameObject);
                    PlayerInventoryScript.invSpace -= int_ItemWeight;
                    PlayerInventoryScript.money -= int_ItemValue;

                    par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(str_ItemName);

                    gameObject.transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;
                    GameObject pressedButton = EventSystem.current.currentSelectedGameObject;

                    PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().buttons.Remove(pressedButton);

                    pressedButton.SetActive(false);
                    isInPlayerInventory = true;
                    isInTraderShop = false;

                    gameObject.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);

                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    RemoveListeners();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildShopInventory();

                    par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.onClick.AddListener(PlayerInventoryScript.CloseShop);

                    par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().str_ShopName;

                    foundDuplicate = false;
                    duplicate = null;

                    PlayerInventoryScript.UpdatePlayerInventoryStats();

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        UpdateGunsAndAmmo();
                    }

                    //Debug.Log("Bought one " + str_ItemName + " from " + str_traderName + " for " + int_ItemValue.ToString() + "! Removed " + int_ItemWeight.ToString() + " space from players inventory.");
                }
            }
            else if (int_finalSpace < 0)
            {
                foundDuplicate = false;
                duplicate = null;

                Debug.Log("Error: Not enough inventory space to buy " + str_ItemName + "!");
            }
            else if (PlayerInventoryScript.money < int_ItemValue)
            {
                foundDuplicate = false;
                duplicate = null;

                Debug.Log("Error: Not enough money to buy " + str_ItemName + "!");
            }
        }
        else if (int_itemCount > 1 && isStackable)
        {
            isBuyingMultipleItems = true;

            par_Managers.GetComponent<Manager_UIReuse>().par_ItemCount.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().itemCountSlider.maxValue = int_itemCount;

            str_traderName = PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName;
            par_Managers.GetComponent<Manager_UIReuse>().txt_CountInfo.text = "Buying " + str_ItemName + "(s) from " + str_traderName + "...";
            par_Managers.GetComponent<Manager_UIReuse>().itemCountSlider.onValueChanged.AddListener(SliderValue);
            par_Managers.GetComponent<Manager_UIReuse>().txt_CountValue.text = "Selected count: 1/" + int_itemCount;
            par_Managers.GetComponent<Manager_UIReuse>().txt_SliderInfo.text = "Total weight removed: " + int_ItemWeight + " Total money removed: " + int_ItemValue;
            par_Managers.GetComponent<Manager_UIReuse>().btn_ConfirmCount.onClick.AddListener(ConfirmCount);
            par_Managers.GetComponent<Manager_UIReuse>().btn_CancelCount.onClick.AddListener(CancelCount);
        }
        else if (!isStackable)
        {
            PlayerInventoryScript.inventory.Add(gameObject);
            PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().inventory.Remove(gameObject);
            PlayerInventoryScript.invSpace -= int_ItemWeight;
            PlayerInventoryScript.money -= int_ItemValue;

            par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(str_ItemName);

            gameObject.transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;
            GameObject pressedButton = EventSystem.current.currentSelectedGameObject;

            PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().buttons.Remove(pressedButton);

            pressedButton.SetActive(false);
            isInPlayerInventory = true;
            isInTraderShop = false;

            gameObject.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);

            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            RemoveListeners();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildShopInventory();

            par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.onClick.AddListener(PlayerInventoryScript.CloseShop);

            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().str_ShopName;

            foundDuplicate = false;
            duplicate = null;

            PlayerInventoryScript.UpdatePlayerInventoryStats();

            str_traderName = PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName;
            //Debug.Log("Bought one non-stackable " + str_ItemName + " from " + str_traderName + " for " + int_ItemValue.ToString() + "! Removed " + int_ItemWeight.ToString() + " space from players inventory.");
        }

        if (!isInRepairMenu)
        {
            par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
        }
    }

    //selling to npc
    public void Sell()
    {
        for (int i = 0; i < PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().inventory.Count; i++)
        {
            str_itemName = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().inventory[i].name;
            if (str_itemName == str_ItemName && isStackable)
            {
                //Debug.Log("Found duplicate " + PlayerInventoryScript.Trader.GetComponent<Inv_Container>().inventory[i].name + " at trader shop!");

                foundDuplicate = true;
                duplicate = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().inventory[i];

                break;
            }
        }

        //cannot sell the equipped gun while it is reloading
        if (gameObject.GetComponent<Item_Gun>() != null
            && PlayerInventoryScript.equippedGun == gameObject
            && gameObject.GetComponent<Item_Gun>().isReloading)
        {
            Debug.Log("Error: Can't sell " + str_ItemName + " while it is reloading!");
            canContinue = false;
        }
        else if (gameObject.GetComponent<Item_Gun>() == null
                 || PlayerInventoryScript.equippedGun != gameObject
                 || !gameObject.GetComponent<Item_Gun>().isReloading)
        {
            canContinue = true;
        }

        if (canContinue)
        {
            //finds the equipped gun
            foreach (GameObject item in PlayerInventoryScript.inventory)
            {
                if (item.GetComponent<Item_Gun>() != null
                    && PlayerInventoryScript.equippedGun == item)
                {
                    selectedGun = item;
                    break;
                }
            }

            //cannot sell the equipped guns ammo while the gun is reloading
            if (selectedGun != null
                && gameObject.GetComponent<Item_Ammo>() != null
                && selectedGun.GetComponent<Item_Gun>().caseType.ToString() == gameObject.GetComponent<Item_Ammo>().caseType.ToString()
                && selectedGun.GetComponent<Item_Gun>().isReloading)
            {
                Debug.Log("Error: Can't sell " + gameObject.GetComponent<Env_Item>().str_ItemName + " while " + selectedGun.GetComponent<Env_Item>().str_ItemName + " is reloading!");
                canContinue = false;
            }
            else if (selectedGun == null
                     || gameObject.GetComponent<Item_Ammo>() == null
                     || selectedGun.GetComponent<Item_Gun>().caseType.ToString() != gameObject.GetComponent<Item_Ammo>().caseType.ToString()
                     || !selectedGun.GetComponent<Item_Gun>().isReloading)
            {
                canContinue = true;
            }
        }

        if (canContinue)
        {
            int_singlePrice = int_finalPrice * 2;
            str_traderName = PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName;
            int_totalSpace = int_ItemWeight * int_confirmedCount;

            if (int_itemCount == 1 && isStackable)
            {
                if (foundDuplicate)
                {
                    PlayerInventoryScript.inventory.Remove(gameObject);

                    duplicate.GetComponent<Env_Item>().int_itemCount += int_itemCount;
                    PlayerInventoryScript.invSpace += int_ItemWeight;
                    PlayerInventoryScript.money += int_singlePrice;

                    par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(str_ItemName);

                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    RemoveListeners();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();

                    par_Managers.GetComponent<Manager_UIReuse>().btn_BuyFromTrader.onClick.AddListener(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().OpenShopUI);

                    par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";

                    par_Managers.GetComponent<Manager_UIReuse>().InteractUIDisabled();

                    //Debug.Log("Sold one duplicate " + str_ItemName + " to " + str_traderName + " for " + int_singlePrice.ToString() + "! Added " + int_ItemWeight.ToString() + " space back to players inventory.");

                    PlayerInventoryScript.UpdatePlayerInventoryStats();

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        RemoveAmmotypeFromAllGuns();
                    }

                    Destroy(gameObject);
                }
                else if (!foundDuplicate)
                {
                    PlayerInventoryScript.inventory.Remove(gameObject);
                    PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().inventory.Add(gameObject);
                    PlayerInventoryScript.invSpace += int_ItemWeight;
                    PlayerInventoryScript.money += int_singlePrice;

                    par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(str_ItemName);

                    Vector3 pos_container = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().par_TraderItems.transform.position;
                    gameObject.transform.position = pos_container;
                    gameObject.transform.SetParent(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().par_TraderItems.transform);

                    isInPlayerInventory = false;
                    isInTraderShop = true;

                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    RemoveListeners();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();

                    par_Managers.GetComponent<Manager_UIReuse>().btn_BuyFromTrader.onClick.AddListener(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().OpenShopUI);

                    par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";

                    foundDuplicate = false;
                    duplicate = null;

                    PlayerInventoryScript.UpdatePlayerInventoryStats();

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        RemoveAmmotypeFromAllGuns();
                    }

                    //Debug.Log("Sold one " + str_ItemName + " to " + str_traderName + " for " + int_singlePrice.ToString() + "! Added " + int_ItemWeight.ToString() + " space back to players inventory.");
                }
            }
            else if (int_itemCount > 1 && isStackable)
            {
                isSellingMultipleItems = true;

                par_Managers.GetComponent<Manager_UIReuse>().par_ItemCount.SetActive(true);
                par_Managers.GetComponent<Manager_UIReuse>().itemCountSlider.maxValue = int_itemCount;

                str_traderName = PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName;
                par_Managers.GetComponent<Manager_UIReuse>().txt_CountInfo.text = "Selling " + str_ItemName + "(s) to " + str_traderName + "...";
                par_Managers.GetComponent<Manager_UIReuse>().itemCountSlider.onValueChanged.AddListener(SliderValue);
                par_Managers.GetComponent<Manager_UIReuse>().txt_CountValue.text = "Selected count: 1/" + int_itemCount;
                par_Managers.GetComponent<Manager_UIReuse>().txt_SliderInfo.text = "Total weight added: " + int_ItemWeight + " Total money added: " + int_singlePrice;
                par_Managers.GetComponent<Manager_UIReuse>().btn_ConfirmCount.onClick.AddListener(ConfirmCount);
                par_Managers.GetComponent<Manager_UIReuse>().btn_CancelCount.onClick.AddListener(CancelCount);
            }
            else if (!isStackable)
            {
                if (gameObject.GetComponent<Item_Gun>() != null)
                {
                    UnequipAndUnloadGun();
                }

                PlayerInventoryScript.inventory.Remove(gameObject);
                PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().inventory.Add(gameObject);
                PlayerInventoryScript.invSpace += int_ItemWeight;
                PlayerInventoryScript.money += int_singlePrice;

                par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(str_ItemName);

                Vector3 pos_container = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().par_TraderItems.transform.position;
                gameObject.transform.position = pos_container;
                gameObject.transform.SetParent(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().par_TraderItems.transform);

                isInPlayerInventory = false;
                isInTraderShop = true;

                par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                RemoveListeners();
                par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();

                par_Managers.GetComponent<Manager_UIReuse>().btn_BuyFromTrader.onClick.AddListener(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().OpenShopUI);

                par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";

                foundDuplicate = false;
                duplicate = null;

                PlayerInventoryScript.UpdatePlayerInventoryStats();

                //Debug.Log("Sold one non-stackable " + str_ItemName + " to " + str_traderName + " for " + int_singlePrice.ToString() + "! Added " + int_ItemWeight.ToString() + " space back to players inventory.");
            }
        }
        canContinue = true;

        if (!isInRepairMenu)
        {
            par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
        }
    }

    //taking from container
    public void Take()
    {
        //check if player inventory has gameobject with same name
        for (int i = 0; i < PlayerInventoryScript.inventory.Count; i++)
        {
            str_itemName = PlayerInventoryScript.inventory[i].name;
            if (str_itemName == str_ItemName && isStackable)
            {
                //Debug.Log("Found duplicate " + PlayerInventoryScript.inventory[i].name + " at player inventory!");

                foundDuplicate = true;
                duplicate = PlayerInventoryScript.inventory[i];
                break;
            }
        }

        if (str_ItemName == "money")
        {
            PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory.Remove(gameObject);
            isInContainer = false;

            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            RemoveListeners();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildContainerInventory();

            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";

            PlayerInventoryScript.money += int_itemCount;
            PlayerInventoryScript.UpdatePlayerInventoryStats();

            //Debug.Log("Took " + int_itemCount + " money from " + PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + "!");

            Destroy(gameObject);
        }

        if (int_itemCount == 1 && isStackable && str_ItemName != "money")
        {
            int_finalSpace = PlayerInventoryScript.invSpace - int_ItemWeight;
            if (int_finalSpace >= 0)
            {
                if (foundDuplicate)
                {
                    PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory.Remove(gameObject);

                    duplicate.GetComponent<Env_Item>().int_itemCount += int_itemCount;
                    PlayerInventoryScript.invSpace -= int_ItemWeight;

                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    RemoveListeners();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildContainerInventory();

                    par_Managers.GetComponent<Manager_UIReuse>().btn_PlaceIntoContainer.onClick.AddListener(PlayerInventoryScript.CloseContainer);

                    par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";

                    par_Managers.GetComponent<Manager_UIReuse>().InteractUIDisabled();

                    str_containerName = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName;
                    //Debug.Log("Took one duplicate " + str_ItemName + " from " + str_containerName + "! Removed " + int_ItemWeight.ToString() + " space from players inventory.");

                    PlayerInventoryScript.UpdatePlayerInventoryStats();

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        UpdateGunsAndAmmo();
                    }

                    Destroy(gameObject);
                }
                else if (!foundDuplicate)
                {
                    PlayerInventoryScript.inventory.Add(gameObject);
                    PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory.Remove(gameObject);
                    PlayerInventoryScript.invSpace -= int_ItemWeight;

                    par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(str_ItemName);

                    gameObject.transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;
                    GameObject pressedButton = EventSystem.current.currentSelectedGameObject;

                    PlayerInventoryScript.Container.GetComponent<Inv_Container>().buttons.Remove(pressedButton);

                    pressedButton.SetActive(false);
                    isInPlayerInventory = true;
                    isInContainer = false;

                    gameObject.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);

                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    RemoveListeners();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildContainerInventory();

                    par_Managers.GetComponent<Manager_UIReuse>().btn_PlaceIntoContainer.onClick.AddListener(PlayerInventoryScript.CloseContainer);

                    par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";

                    foundDuplicate = false;
                    duplicate = null;

                    PlayerInventoryScript.UpdatePlayerInventoryStats();

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        UpdateGunsAndAmmo();
                    }

                    str_containerName = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName;
                    //Debug.Log("Took one " + str_ItemName + " from " + str_containerName + "! Removed " + int_ItemWeight.ToString() + " space from players inventory.");
                }
            }
        }
        else if (int_itemCount > 1 && isStackable && str_ItemName != "money")
        {
            isTakingMultipleItems = true;

            par_Managers.GetComponent<Manager_UIReuse>().par_ItemCount.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().itemCountSlider.maxValue = int_itemCount;

            str_containerName = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName;
            par_Managers.GetComponent<Manager_UIReuse>().txt_CountInfo.text = "Taking " + str_ItemName + "(s) from " + str_containerName + "...";
            par_Managers.GetComponent<Manager_UIReuse>().itemCountSlider.onValueChanged.AddListener(SliderValue);
            par_Managers.GetComponent<Manager_UIReuse>().txt_CountValue.text = "Selected count: 1/" + int_itemCount;
            par_Managers.GetComponent<Manager_UIReuse>().txt_SliderInfo.text = "Total weight removed: " + int_ItemWeight;
            par_Managers.GetComponent<Manager_UIReuse>().btn_ConfirmCount.onClick.AddListener(ConfirmCount);
            par_Managers.GetComponent<Manager_UIReuse>().btn_CancelCount.onClick.AddListener(CancelCount);
        }
        else if (!isStackable)
        {
            PlayerInventoryScript.inventory.Add(gameObject);
            PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory.Remove(gameObject);
            PlayerInventoryScript.invSpace -= int_ItemWeight;

            par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(str_ItemName);

            gameObject.transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;
            GameObject pressedButton = EventSystem.current.currentSelectedGameObject;

            PlayerInventoryScript.Container.GetComponent<Inv_Container>().buttons.Remove(pressedButton);

            pressedButton.SetActive(false);
            isInPlayerInventory = true;
            isInContainer = false;

            gameObject.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);

            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            RemoveListeners();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildContainerInventory();

            par_Managers.GetComponent<Manager_UIReuse>().btn_PlaceIntoContainer.onClick.AddListener(PlayerInventoryScript.CloseContainer);

            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";

            foundDuplicate = false;
            duplicate = null;

            PlayerInventoryScript.UpdatePlayerInventoryStats();

            str_containerName = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName;
            //Debug.Log("Took one non-stackable " + str_ItemName + " from " + str_containerName + "! Removed " + int_ItemWeight.ToString() + " space from players inventory.");
        }

        if (!isInRepairMenu)
        {
            par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
        }
    }

    //placing into container
    public void Place()
    {
        for (int i = 0; i < PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory.Count; i++)
        {
            str_itemName = PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory[i].name;
            if (str_itemName == str_ItemName && isStackable)
            {
                //Debug.Log("Found duplicate " + PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory[i].name + " at container inventory!");

                foundDuplicate = true;
                duplicate = PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory[i];
                break;
            }
        }

        //cannot place the equipped gun to container while it is reloading
        if (gameObject.GetComponent<Item_Gun>() != null
            && PlayerInventoryScript.equippedGun == gameObject
            && gameObject.GetComponent<Item_Gun>().isReloading)
        {
            Debug.Log("Error: Can't place " + str_ItemName + " to container while it is reloading!");
            canContinue = false;
        }
        else if (gameObject.GetComponent<Item_Gun>() == null
                 || PlayerInventoryScript.equippedGun != gameObject
                 || !gameObject.GetComponent<Item_Gun>().isReloading)
        {
            canContinue = true;
        }

        if (canContinue)
        {
            //finds the equipped gun
            foreach (GameObject item in PlayerInventoryScript.inventory)
            {
                if (item.GetComponent<Item_Gun>() != null
                    && PlayerInventoryScript.equippedGun == item)
                {
                    selectedGun = item;
                    break;
                }
            }

            //cannot place the equipped guns ammo to container while the gun is reloading
            if (selectedGun != null
                && gameObject.GetComponent<Item_Ammo>() != null
                && selectedGun.GetComponent<Item_Gun>().caseType.ToString() == gameObject.GetComponent<Item_Ammo>().caseType.ToString()
                && selectedGun.GetComponent<Item_Gun>().isReloading)
            {
                Debug.Log("Error: Can't place " + gameObject.GetComponent<Env_Item>().str_ItemName + " to container while " + selectedGun.GetComponent<Env_Item>().str_ItemName + " is reloading!");
                canContinue = false;
            }
            else if (selectedGun == null
                     || gameObject.GetComponent<Item_Ammo>() == null
                     || selectedGun.GetComponent<Item_Gun>().caseType.ToString() != gameObject.GetComponent<Item_Ammo>().caseType.ToString()
                     || !selectedGun.GetComponent<Item_Gun>().isReloading)
            {
                canContinue = true;
            }

            if (!isInRepairMenu)
            {
                par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
            }
        }

        if (canContinue)
        {
            if (int_itemCount == 1 && isStackable)
            {
                if (foundDuplicate)
                {
                    PlayerInventoryScript.inventory.Remove(gameObject);

                    duplicate.GetComponent<Env_Item>().int_itemCount += int_itemCount;
                    PlayerInventoryScript.invSpace += int_ItemWeight;

                    par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(str_ItemName);

                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    RemoveListeners();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();

                    par_Managers.GetComponent<Manager_UIReuse>().btn_TakeFromContainer.onClick.AddListener(PlayerInventoryScript.Container.GetComponent<Inv_Container>().CheckIfLocked);

                    par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";

                    par_Managers.GetComponent<Manager_UIReuse>().InteractUIDisabled();

                    str_containerName = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName;
                    //Debug.Log("Placed one duplicate " + str_ItemName + " to " + str_containerName + "! Added " + int_ItemWeight.ToString() + " space back to players inventory.");

                    PlayerInventoryScript.UpdatePlayerInventoryStats();

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        RemoveAmmotypeFromAllGuns();
                    }

                    Destroy(gameObject);
                }
                else if (!foundDuplicate)
                {
                    PlayerInventoryScript.inventory.Remove(gameObject);
                    PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory.Add(gameObject);
                    PlayerInventoryScript.invSpace += int_ItemWeight;

                    par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(str_ItemName);

                    Vector3 pos_container = PlayerInventoryScript.Container.GetComponent<Inv_Container>().par_ContainerItems.transform.position;
                    gameObject.transform.position = pos_container;
                    gameObject.transform.SetParent(PlayerInventoryScript.Container.GetComponent<Inv_Container>().par_ContainerItems.transform);

                    isInPlayerInventory = false;
                    isInContainer = true;

                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    RemoveListeners();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();

                    par_Managers.GetComponent<Manager_UIReuse>().btn_TakeFromContainer.onClick.AddListener(PlayerInventoryScript.Container.GetComponent<Inv_Container>().CheckIfLocked);

                    par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";

                    foundDuplicate = false;
                    duplicate = null;

                    PlayerInventoryScript.UpdatePlayerInventoryStats();

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        RemoveAmmotypeFromAllGuns();
                    }

                    str_containerName = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName;
                    //Debug.Log("Placed one " + str_ItemName + " to " + str_containerName + "! Added " + int_ItemWeight.ToString() + " space back to players inventory.");
                }
            }
            else if (int_itemCount > 1 && isStackable)
            {
                isPlacingMultipleItems = true;

                par_Managers.GetComponent<Manager_UIReuse>().par_ItemCount.SetActive(true);
                par_Managers.GetComponent<Manager_UIReuse>().itemCountSlider.maxValue = int_itemCount;

                str_containerName = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName;
                par_Managers.GetComponent<Manager_UIReuse>().txt_CountInfo.text = "Placing " + str_ItemName + "(s) to " + str_containerName + "...";
                par_Managers.GetComponent<Manager_UIReuse>().itemCountSlider.onValueChanged.AddListener(SliderValue);
                par_Managers.GetComponent<Manager_UIReuse>().txt_CountValue.text = "Selected count: 1/" + int_itemCount;
                par_Managers.GetComponent<Manager_UIReuse>().txt_SliderInfo.text = "Total weight added: " + int_ItemWeight;
                par_Managers.GetComponent<Manager_UIReuse>().btn_ConfirmCount.onClick.AddListener(ConfirmCount);
                par_Managers.GetComponent<Manager_UIReuse>().btn_CancelCount.onClick.AddListener(CancelCount);
            }
            else if (!isStackable)
            {
                if (gameObject.GetComponent<Item_Gun>() != null)
                {
                    UnequipAndUnloadGun();
                }

                PlayerInventoryScript.inventory.Remove(gameObject);
                PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory.Add(gameObject);
                PlayerInventoryScript.invSpace += int_ItemWeight;

                par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(str_ItemName);

                Vector3 pos_container = PlayerInventoryScript.Container.GetComponent<Inv_Container>().par_ContainerItems.transform.position;
                gameObject.transform.position = pos_container;
                gameObject.transform.SetParent(PlayerInventoryScript.Container.GetComponent<Inv_Container>().par_ContainerItems.transform);

                isInPlayerInventory = false;
                isInContainer = true;

                par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                RemoveListeners();
                par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();

                par_Managers.GetComponent<Manager_UIReuse>().btn_TakeFromContainer.onClick.AddListener(PlayerInventoryScript.Container.GetComponent<Inv_Container>().CheckIfLocked);

                par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";

                foundDuplicate = false;
                duplicate = null;

                PlayerInventoryScript.UpdatePlayerInventoryStats();

                str_containerName = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName;
                //Debug.Log("Placed one non-stackable " + str_ItemName + " to " + str_containerName + "! Added " + int_ItemWeight.ToString() + " space back to players inventory.");
            }
        }
        canContinue = true;

        if (!isInRepairMenu)
        {
            par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
        }
    }

    //dropping from own inventory
    public void Drop()
    {
        //cannot drop the equipped gun while it is reloading
        if (gameObject.GetComponent<Item_Gun>() != null
            && PlayerInventoryScript.equippedGun == gameObject
            && gameObject.GetComponent<Item_Gun>().isReloading)
        {
            Debug.Log("Error: Can't drop " + str_ItemName + " while it is reloading!");
            canContinue = false;
        }
        else if (gameObject.GetComponent<Item_Gun>() == null
                 || PlayerInventoryScript.equippedGun != gameObject
                 || !gameObject.GetComponent<Item_Gun>().isReloading)
        {
            canContinue = true;
        }

        if (canContinue)
        {
            //finds the equipped gun
            foreach (GameObject item in PlayerInventoryScript.inventory)
            {
                if (item.GetComponent<Item_Gun>() != null
                    && PlayerInventoryScript.equippedGun == item)
                {
                    selectedGun = item;
                    break;
                }
            }

            //cannot drop the equipped guns ammo while the gun is reloading
            if (selectedGun != null
                && gameObject.GetComponent<Item_Ammo>() != null
                && selectedGun.GetComponent<Item_Gun>().caseType.ToString() == gameObject.GetComponent<Item_Ammo>().caseType.ToString()
                && selectedGun.GetComponent<Item_Gun>().isReloading)
            {
                Debug.Log("Error: Can't drop " + gameObject.GetComponent<Env_Item>().str_ItemName + " while " + selectedGun.GetComponent<Env_Item>().str_ItemName + " is reloading!");
                canContinue = false;
            }
            else if (selectedGun == null
                     || gameObject.GetComponent<Item_Ammo>() == null
                     || selectedGun.GetComponent<Item_Gun>().caseType.ToString() != gameObject.GetComponent<Item_Ammo>().caseType.ToString()
                     || !selectedGun.GetComponent<Item_Gun>().isReloading)
            {
                canContinue = true;
            }

            if (!isInRepairMenu)
            {
                par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
            }
        }

        if (canContinue)
        {
            if (int_itemCount == 1)
            {
                if (gameObject.GetComponent<Item_Gun>() != null)
                {
                    UnequipAndUnloadGun();
                }

                theItem = gameObject;

                PlayerInventoryScript.inventory.Remove(gameObject);
                PlayerInventoryScript.invSpace += int_ItemWeight;

                par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(str_ItemName);

                //get a random direction (360°) in radians
                float angle = Random.Range(0.0f, Mathf.PI * 2);
                //create a vector with length 1.0
                Vector3 dropPos = new(Mathf.Sin(angle), 0, Mathf.Cos(angle));
                //set item drop position
                gameObject.transform.position = thePlayer.transform.position + dropPos;

                droppedObject = true;
                time = 0;

                foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
                {
                    if (!cell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                    {
                        cell.GetComponent<Manager_CurrentCell>().items.Add(gameObject);
                        break;
                    }
                }

                gameObject.transform.parent = par_DroppedItems.transform;

                isInPlayerInventory = false;

                par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                RemoveListeners();
                par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();

                par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";

                foundDuplicate = false;
                duplicate = null;

                PlayerInventoryScript.UpdatePlayerInventoryStats();

                if (gameObject.GetComponent<Item_Ammo>() != null)
                {
                    RemoveAmmotypeFromAllGuns();
                }

                //Debug.Log("Dropped one " + str_ItemName + "! Added " + int_ItemWeight.ToString() + " space back to players inventory.");

                ActivateItem();
            }
            else if (int_itemCount > 1)
            {
                isDroppingMultipleItems = true;

                par_Managers.GetComponent<Manager_UIReuse>().par_ItemCount.SetActive(true);
                par_Managers.GetComponent<Manager_UIReuse>().itemCountSlider.maxValue = int_itemCount;

                par_Managers.GetComponent<Manager_UIReuse>().txt_CountInfo.text = "Dropping " + str_ItemName + "(s) from players inventory...";
                par_Managers.GetComponent<Manager_UIReuse>().itemCountSlider.onValueChanged.AddListener(SliderValue);
                par_Managers.GetComponent<Manager_UIReuse>().txt_CountValue.text = "Selected count: 1/" + int_itemCount;
                par_Managers.GetComponent<Manager_UIReuse>().txt_SliderInfo.text = "Total weight added: " + int_ItemWeight;
                par_Managers.GetComponent<Manager_UIReuse>().btn_ConfirmCount.onClick.AddListener(ConfirmCount);
                par_Managers.GetComponent<Manager_UIReuse>().btn_CancelCount.onClick.AddListener(CancelCount);

                foundDuplicate = false;
                duplicate = null;
            }
        }
        canContinue = true;

        if (!isInRepairMenu)
        {
            par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
        }
    }

    //destroying from own inventory
    public void Destroy()
    {
        //cannot destroy the equipped gun while it is reloading
        if (gameObject.GetComponent<Item_Gun>() != null
            && PlayerInventoryScript.equippedGun == gameObject
            && gameObject.GetComponent<Item_Gun>().isReloading)
        {
            Debug.Log("Error: Can't destroy " + str_ItemName + " while it is reloading!");
            canContinue = false;
        }
        else if (gameObject.GetComponent<Item_Gun>() == null
                 || PlayerInventoryScript.equippedGun != gameObject
                 || !gameObject.GetComponent<Item_Gun>().isReloading)
        {
            canContinue = true;
        }

        if (canContinue)
        {
            //finds the equipped gun
            foreach (GameObject item in PlayerInventoryScript.inventory)
            {
                if (item.GetComponent<Item_Gun>() != null
                    && PlayerInventoryScript.equippedGun == item)
                {
                    selectedGun = item;
                    break;
                }
            }

            //cannot destroy the equipped guns ammo while the gun is reloading
            if (selectedGun != null
                && gameObject.GetComponent<Item_Ammo>() != null
                && selectedGun.GetComponent<Item_Gun>().caseType.ToString() == gameObject.GetComponent<Item_Ammo>().caseType.ToString()
                && selectedGun.GetComponent<Item_Gun>().isReloading)
            {
                Debug.Log("Error: Can't destroy " + gameObject.GetComponent<Env_Item>().str_ItemName + " while " + selectedGun.GetComponent<Env_Item>().str_ItemName + " is reloading!");
                canContinue = false;
            }
            else if (selectedGun == null
                     || gameObject.GetComponent<Item_Ammo>() == null
                     || selectedGun.GetComponent<Item_Gun>().caseType.ToString() != gameObject.GetComponent<Item_Ammo>().caseType.ToString()
                     || !selectedGun.GetComponent<Item_Gun>().isReloading)
            {
                canContinue = true;
            }
        }

        if (canContinue)
        {
            if (int_itemCount == 1)
            {
                if (gameObject.GetComponent<Item_Gun>() != null)
                {
                    UnequipAndUnloadGun();
                }

                PlayerInventoryScript.inventory.Remove(gameObject);
                PlayerInventoryScript.invSpace += int_ItemWeight;

                par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(str_ItemName);

                isInPlayerInventory = false;

                par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                RemoveListeners();
                par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();

                par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";

                PlayerInventoryScript.UpdatePlayerInventoryStats();

                if (gameObject.GetComponent<Item_Ammo>() != null)
                {
                    RemoveAmmotypeFromAllGuns();
                }

                //Debug.Log("Destroyed one " + str_ItemName + "! Added " + int_ItemWeight.ToString() + " space back to players inventory.");

                Destroy(gameObject);
            }
            else if (int_itemCount > 1)
            {
                isDestroyingMultipleItems = true;

                par_Managers.GetComponent<Manager_UIReuse>().par_ItemCount.SetActive(true);
                par_Managers.GetComponent<Manager_UIReuse>().itemCountSlider.maxValue = int_itemCount;

                par_Managers.GetComponent<Manager_UIReuse>().txt_CountInfo.text = "Destroying " + str_ItemName + "(s) from players inventory...";
                par_Managers.GetComponent<Manager_UIReuse>().itemCountSlider.onValueChanged.AddListener(SliderValue);
                par_Managers.GetComponent<Manager_UIReuse>().txt_CountValue.text = "Selected count: 1/" + int_itemCount;
                par_Managers.GetComponent<Manager_UIReuse>().txt_SliderInfo.text = "Total weight added: " + int_ItemWeight;
                par_Managers.GetComponent<Manager_UIReuse>().btn_ConfirmCount.onClick.AddListener(ConfirmCount);
                par_Managers.GetComponent<Manager_UIReuse>().btn_CancelCount.onClick.AddListener(CancelCount);
            }
        }
        canContinue = true;

        if (!isInRepairMenu)
        {
            par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
        }
    }

    public void SliderValue(float value)
    {
        int_selectedCount = Mathf.FloorToInt(value);
        par_Managers.GetComponent<Manager_UIReuse>().txt_CountValue.text = "Selected count: " + int_selectedCount.ToString() + "/" + int_itemCount;

        if (isPickingUpMultipleItems
            || isTakingMultipleItems
            || isPlacingMultipleItems
            || isDroppingMultipleItems
            || isDestroyingMultipleItems)
        {
            int_totalSpace = int_ItemWeight * int_selectedCount;
        }
        if (isPickingUpMultipleItems
            || isTakingMultipleItems
            || isBuyingMultipleItems)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_SliderInfo.text = "Total weight removed: " + int_totalSpace;
        }
        if (isPlacingMultipleItems
            || isDroppingMultipleItems
            || isDestroyingMultipleItems
            || isSellingMultipleItems)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_SliderInfo.text = "Total weight added: " + int_totalSpace;
        }
        if (isBuyingMultipleItems)
        {
            int_totalSpace = int_ItemWeight * int_selectedCount;
            int_finalPrice = int_ItemValue * int_selectedCount;
            par_Managers.GetComponent<Manager_UIReuse>().txt_SliderInfo.text = par_Managers.GetComponent<Manager_UIReuse>().txt_SliderInfo.text + " Total money taken: " + int_finalPrice;
        }
        if (isSellingMultipleItems)
        {
            int_totalSpace = int_ItemWeight * int_selectedCount;
            int_singlePrice = int_finalPrice * 2;
            int_finalPrice = int_singlePrice * int_selectedCount;
            par_Managers.GetComponent<Manager_UIReuse>().txt_SliderInfo.text = par_Managers.GetComponent<Manager_UIReuse>().txt_SliderInfo.text + " Total money added: " + int_finalPrice;
        }
        //Debug.Log(int_selectedCount);

        if (isPickingUpMultipleItems || isTakingMultipleItems)
        {
            if (int_selectedCount * int_ItemWeight <= PlayerInventoryScript.invSpace)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_CountValue.color = Color.white;
                par_Managers.GetComponent<Manager_UIReuse>().btn_ConfirmCount.interactable = true;
            }
            else if (int_selectedCount * int_ItemWeight > PlayerInventoryScript.invSpace)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_CountValue.color = Color.red;
                par_Managers.GetComponent<Manager_UIReuse>().btn_ConfirmCount.interactable = false;
                //Debug.Log("Slider not interactable...");
            }
        }
        else if (isBuyingMultipleItems)
        {
            if (int_selectedCount * int_ItemValue <= PlayerInventoryScript.money)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_CountValue.color = Color.white;
                par_Managers.GetComponent<Manager_UIReuse>().btn_ConfirmCount.interactable = true;
            }
            else if (int_selectedCount * int_ItemValue > PlayerInventoryScript.money)
            {
                par_Managers.GetComponent<Manager_UIReuse>().txt_CountValue.color = Color.red;
                par_Managers.GetComponent<Manager_UIReuse>().btn_ConfirmCount.interactable = false;
                //Debug.Log("Slider not interactable...");
            }
        }
        else
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_CountValue.color = Color.white;
            par_Managers.GetComponent<Manager_UIReuse>().btn_ConfirmCount.interactable = true;
        }
    }

    public void ConfirmCount()
    {
        int_confirmedCount = Mathf.FloorToInt(par_Managers.GetComponent<Manager_UIReuse>().itemCountSlider.value);
        //Debug.Log("Selected " + int_confirmedCount + " " + str_ItemName + ".");

        if (isBuyingMultipleItems)
        {
            int_finalSpace = PlayerInventoryScript.invSpace - (int_ItemWeight * int_confirmedCount);
            if (int_finalSpace >= 0 && PlayerInventoryScript.money >= (int_ItemValue * int_confirmedCount))
            {
                if (foundDuplicate)
                {
                    duplicate.GetComponent<Env_Item>().int_itemCount += int_confirmedCount;
                    PlayerInventoryScript.invSpace -= int_totalSpace;

                    if (int_confirmedCount < int_itemCount)
                    {
                        int_itemCount -= int_confirmedCount;
                        if (gameObject.GetComponent<Item_Ammo>() != null)
                        {
                            UpdateGunsAndAmmo();
                        }
                    }
                    else if (int_confirmedCount == int_itemCount || int_confirmedCount - int_itemCount == 0)
                    {
                        PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().inventory.Remove(gameObject);

                        par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                        par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                        RemoveListeners();
                        par_Managers.GetComponent<Manager_UIReuse>().RebuildShopInventory();
                        par_Managers.GetComponent<Manager_UIReuse>().ClearCountSliderUI();

                        par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.onClick.AddListener(PlayerInventoryScript.CloseShop);

                        par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().str_ShopName;

                        par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(str_ItemName);

                        PlayerInventoryScript.UpdatePlayerInventoryStats();

                        if (gameObject.GetComponent<Item_Ammo>() != null)
                        {
                            UpdateGunsAndAmmo();
                        }

                        Destroy(gameObject);
                    }
                }
                else if (!foundDuplicate)
                {
                    if (int_confirmedCount == int_itemCount)
                    {
                        gameObject.transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;

                        PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().inventory.Remove(gameObject);
                        PlayerInventoryScript.inventory.Add(gameObject);
                        PlayerInventoryScript.invSpace -= int_ItemWeight * int_confirmedCount;

                        gameObject.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);

                        par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(str_ItemName);

                        if (gameObject.GetComponent<Item_Ammo>() != null)
                        {
                            UpdateGunsAndAmmo();
                        }

                        isInPlayerInventory = true;
                        isInTraderShop = false;
                    }
                    else if (int_confirmedCount < int_itemCount)
                    {
                        Vector3 playerInvPos = PlayerInventoryScript.par_PlayerItems.transform.position;
                        GameObject newDuplicate = Instantiate(gameObject, 
                                                              playerInvPos, 
                                                              Quaternion.identity, 
                                                              PlayerInventoryScript.par_PlayerItems.transform);

                        PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().inventory.Remove(newDuplicate);
                        PlayerInventoryScript.inventory.Add(newDuplicate);

                        PlayerInventoryScript.invSpace -= (int_ItemWeight * int_confirmedCount);
                        int_itemCount -= int_confirmedCount;

                        newDuplicate.name = newDuplicate.GetComponent<Env_Item>().str_ItemName;
                        newDuplicate.GetComponent<Env_Item>().int_itemCount = int_confirmedCount;
                        newDuplicate.GetComponent<Env_Item>().isInPlayerInventory = true;
                        newDuplicate.GetComponent<Env_Item>().isInTraderShop = false;

                        if (gameObject.GetComponent<Item_Ammo>() != null)
                        {
                            UpdateGunsAndAmmo();
                        }
                    }
                }

                par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
                par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                RemoveListeners();
                par_Managers.GetComponent<Manager_UIReuse>().RebuildShopInventory();
                par_Managers.GetComponent<Manager_UIReuse>().ClearCountSliderUI();

                int_finalPrice = int_ItemValue * int_confirmedCount;
                str_traderName = PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName;
                int_totalSpace = int_ItemWeight * int_confirmedCount;
                PlayerInventoryScript.money -= int_finalPrice;
                PlayerInventoryScript.invSpace -= int_totalSpace;

                par_Managers.GetComponent<Manager_UIReuse>().btn_SellToTrader.onClick.AddListener(PlayerInventoryScript.CloseShop);

                par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().str_ShopName;

                PlayerInventoryScript.UpdatePlayerInventoryStats();

                //Debug.Log("Bought " + int_confirmedCount + " " + str_ItemName + "(s) from " + str_traderName + " for " + int_ItemValue.ToString() + " each (" + int_finalPrice + " total)! Removed " + int_totalSpace.ToString() + " space from players inventory.");
            }
            else if (int_finalSpace < 0)
            {
                Debug.Log("Error: Not enough inventory space to buy " + str_ItemName + "!");
                CancelCount();
            }
            else if (PlayerInventoryScript.money < (int_ItemValue * int_confirmedCount))
            {
                Debug.Log("Error: Not enough money to buy " + str_ItemName + "!");
                CancelCount();
            }

            if (!isInRepairMenu)
            {
                par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
            }

            foundDuplicate = false;
            duplicate = null;
        }

        else if (isSellingMultipleItems)
        {
            if (foundDuplicate)
            {
                duplicate.GetComponent<Env_Item>().int_itemCount += int_confirmedCount;
                PlayerInventoryScript.invSpace += int_ItemWeight * int_confirmedCount;

                if (int_confirmedCount < int_itemCount)
                {
                    int_itemCount -= int_confirmedCount;
                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        UpdateGunsAndAmmo();
                    }
                }
                else if (int_confirmedCount == int_itemCount || int_confirmedCount - int_itemCount == 0)
                {
                    PlayerInventoryScript.inventory.Remove(gameObject);
                    par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(str_ItemName);

                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    RemoveListeners();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearCountSliderUI();

                    par_Managers.GetComponent<Manager_UIReuse>().btn_TakeFromContainer.onClick.AddListener(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().OpenShopUI);

                    par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        UpdateGunsAndAmmo();
                    }

                    PlayerInventoryScript.UpdatePlayerInventoryStats();

                    Destroy(gameObject);
                }
            }
            else if (!foundDuplicate)
            {
                if (int_confirmedCount == int_itemCount)
                {
                    gameObject.transform.position = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().par_TraderItems.transform.position;

                    PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().inventory.Add(gameObject);
                    PlayerInventoryScript.inventory.Remove(gameObject);
                    PlayerInventoryScript.invSpace += int_ItemWeight * int_confirmedCount;
                    gameObject.transform.SetParent(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().par_TraderItems.transform);

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        RemoveAmmotypeFromAllGuns();
                    }

                    isInPlayerInventory = false;
                    isInTraderShop = true;

                    par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(str_ItemName);
                }
                else if (int_confirmedCount < int_itemCount)
                {
                    Vector3 pos_container = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().par_TraderItems.transform.position;
                    GameObject newDuplicate = Instantiate(gameObject, 
                                                         pos_container, 
                                                         Quaternion.identity,
                                                         PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().par_TraderItems.transform);

                    newDuplicate.transform.position = pos_container;

                    PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().inventory.Add(newDuplicate);
                    PlayerInventoryScript.inventory.Remove(newDuplicate);
                    int_itemCount -= int_confirmedCount;
                    PlayerInventoryScript.invSpace += (int_ItemWeight * int_confirmedCount);

                    newDuplicate.name = newDuplicate.GetComponent<Env_Item>().str_ItemName;
                    newDuplicate.GetComponent<Env_Item>().int_itemCount = int_confirmedCount;
                    newDuplicate.GetComponent<Env_Item>().isInPlayerInventory = false;
                    newDuplicate.GetComponent<Env_Item>().isInTraderShop = true;

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        UpdateGunsAndAmmo();
                    }
                }
            }

            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            RemoveListeners();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();
            par_Managers.GetComponent<Manager_UIReuse>().ClearCountSliderUI();

            int_singlePrice = int_finalPrice * 2;
            int_finalPrice = int_singlePrice * int_confirmedCount;
            str_traderName = PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName;
            int_totalSpace = int_ItemWeight * int_confirmedCount;
            PlayerInventoryScript.money += int_finalPrice;

            par_Managers.GetComponent<Manager_UIReuse>().btn_BuyFromTrader.onClick.AddListener(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().OpenShopUI);

            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";

            foundDuplicate = false;
            duplicate = null;

            PlayerInventoryScript.UpdatePlayerInventoryStats();
            if (!isInRepairMenu)
            {
                par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
            }

            //Debug.Log("Sold " + int_confirmedCount + " " + str_ItemName + "(s) to " + str_traderName + " for " + int_singlePrice.ToString() + " each(" + int_finalPrice + " total)! Added " + int_totalSpace.ToString() + " space back to players inventory.");
        }

        else if (isPickingUpMultipleItems)
        {
            int_finalSpace = PlayerInventoryScript.invSpace - (int_ItemWeight * int_confirmedCount);
            if (int_finalSpace >= 0)
            {
                droppedObject = false;
                time = 0;
                if (foundDuplicate)
                {
                    duplicate.GetComponent<Env_Item>().int_itemCount += int_confirmedCount;
                    PlayerInventoryScript.invSpace -= int_ItemWeight * int_confirmedCount;

                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    RemoveListeners();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearCountSliderUI();

                    par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(str_ItemName);

                    //this is only called if queststage was assigned
                    //aka when this item is used in a quest
                    if (QuestStage != null)
                    {
                        QuestStage.PickedUpItem();
                    }

                    PlayerInventoryScript.UpdatePlayerInventoryStats();

                    if (int_confirmedCount < int_itemCount)
                    {
                        int_itemCount -= int_confirmedCount;
                        if (gameObject.GetComponent<Item_Ammo>() != null)
                        {
                            UpdateGunsAndAmmo();
                        }
                    }
                    else if (int_confirmedCount == int_itemCount || int_confirmedCount - int_itemCount == 0)
                    {
                        if (gameObject.GetComponent<Item_Ammo>() != null)
                        {
                            UpdateGunsAndAmmo();
                        }

                        foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
                        {
                            if (cell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                            {
                                cell.GetComponent<Manager_CurrentCell>().items.Remove(gameObject);
                                break;
                            }
                        }

                        par_Managers.GetComponent<Manager_UIReuse>().InteractUIDisabled();
                        Destroy(gameObject);
                    }
                }
                else if (!foundDuplicate)
                {
                    if (int_confirmedCount == int_itemCount)
                    {
                        theItem = gameObject;

                        PlayerInventoryScript.inventory.Add(gameObject);
                        PlayerInventoryScript.invSpace -= (int_ItemWeight * int_confirmedCount);

                        gameObject.GetComponent<Env_ObjectPickup>().isHolding = false;
                        gameObject.transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;
                        gameObject.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);

                        foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
                        {
                            if (cell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                            {
                                cell.GetComponent<Manager_CurrentCell>().items.Remove(gameObject);
                                break;
                            }
                        }

                        if (gameObject.GetComponent<Item_Ammo>() != null)
                        {
                            UpdateGunsAndAmmo();
                        }

                        //this is only called if queststage was assigned
                        //aka when this item is used in a quest
                        if (QuestStage != null)
                        {
                            QuestStage.PickedUpItem();
                        }

                        isInPlayerInventory = true;

                        DeactivateItem();
                    }
                    else if (int_confirmedCount < int_itemCount)
                    {
                        GameObject newDuplicate = Instantiate(gameObject, 
                                                              PlayerInventoryScript.par_PlayerItems.transform.position, 
                                                              Quaternion.identity,
                                                              PlayerInventoryScript.par_PlayerItems.transform);

                        theItem = newDuplicate;

                        PlayerInventoryScript.inventory.Add(newDuplicate);
                        PlayerInventoryScript.invSpace -= (int_ItemWeight * int_confirmedCount);

                        int_itemCount -= int_confirmedCount;
                        newDuplicate.name = newDuplicate.GetComponent<Env_Item>().str_ItemName;
                        newDuplicate.GetComponent<Env_Item>().int_itemCount = int_confirmedCount;
                        newDuplicate.GetComponent<Env_Item>().isInPlayerInventory = true;

                        foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
                        {
                            if (cell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                            {
                                cell.GetComponent<Manager_CurrentCell>().items.Remove(gameObject);
                                break;
                            }
                        }

                        if (gameObject.GetComponent<Item_Ammo>() != null)
                        {
                            UpdateGunsAndAmmo();
                        }

                        theItem.GetComponent<Env_Item>().DeactivateItem();
                    }

                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    RemoveListeners();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearCountSliderUI();

                    par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(str_ItemName);

                    foundDuplicate = false;
                    duplicate = null;

                    PlayerInventoryScript.UpdatePlayerInventoryStats();

                    int_totalSpace = int_ItemWeight * int_confirmedCount;
                    //Debug.Log("Picked up " + int_confirmedCount + " " + str_ItemName + "(s)! Removed " + int_totalSpace.ToString() + " space from players inventory.");
                }
            }
            else
            {
                foundDuplicate = false;
                duplicate = null;

                Debug.Log("Error: Not enough space to pick up " + str_ItemName + "!");
                CancelCount();
            }

            if (par_Managers.GetComponent<UI_PauseMenu>().isGamePaused)
            {
                par_Managers.GetComponent<UI_PauseMenu>().UnpauseGame();
            }

            if (!isInRepairMenu)
            {
                par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
            }
        }

        else if (isTakingMultipleItems)
        {
            int_finalSpace = PlayerInventoryScript.maxInvSpace - (int_ItemWeight * int_confirmedCount);
            if (int_finalSpace >= 0)
            {
                if (foundDuplicate)
                {
                    duplicate.GetComponent<Env_Item>().int_itemCount += int_confirmedCount;
                    PlayerInventoryScript.invSpace -= int_ItemWeight * int_confirmedCount;

                    if (int_confirmedCount < int_itemCount)
                    {
                        int_itemCount -= int_confirmedCount;
                        if (gameObject.GetComponent<Item_Ammo>() != null)
                        {
                            UpdateGunsAndAmmo();
                        }
                    }
                    else if (int_confirmedCount == int_itemCount || int_confirmedCount - int_itemCount == 0)
                    {
                        PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory.Remove(gameObject);

                        par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                        par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                        RemoveListeners();
                        par_Managers.GetComponent<Manager_UIReuse>().RebuildContainerInventory();
                        par_Managers.GetComponent<Manager_UIReuse>().ClearCountSliderUI();

                        par_Managers.GetComponent<Manager_UIReuse>().btn_PlaceIntoContainer.onClick.AddListener(PlayerInventoryScript.CloseContainer);

                        par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";

                        par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(str_ItemName);

                        PlayerInventoryScript.UpdatePlayerInventoryStats();

                        if (gameObject.GetComponent<Item_Ammo>() != null)
                        {
                            UpdateGunsAndAmmo();
                        }

                        Destroy(gameObject);
                    }
                }
                else if (!foundDuplicate)
                {
                    if (int_confirmedCount == int_itemCount)
                    {
                        gameObject.transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;

                        PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory.Remove(gameObject);
                        PlayerInventoryScript.inventory.Add(gameObject);
                        PlayerInventoryScript.invSpace -= int_ItemWeight * int_confirmedCount;

                        gameObject.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);

                        par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(str_ItemName);

                        if (gameObject.GetComponent<Item_Ammo>() != null)
                        {
                            UpdateGunsAndAmmo();
                        }

                        isInPlayerInventory = true;
                        isInContainer = false;
                    }
                    else if (int_confirmedCount < int_itemCount)
                    {
                        Vector3 playerInvPos = PlayerInventoryScript.par_PlayerItems.transform.position;
                        GameObject newDuplicate = Instantiate(gameObject, 
                                                              playerInvPos, 
                                                              Quaternion.identity, 
                                                              PlayerInventoryScript.par_PlayerItems.transform);

                        PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory.Remove(newDuplicate);
                        PlayerInventoryScript.inventory.Add(newDuplicate);

                        PlayerInventoryScript.invSpace -= (int_ItemWeight * int_confirmedCount);
                        int_itemCount -= int_confirmedCount;

                        newDuplicate.name = newDuplicate.GetComponent<Env_Item>().str_ItemName;
                        newDuplicate.GetComponent<Env_Item>().int_itemCount = int_confirmedCount;
                        newDuplicate.GetComponent<Env_Item>().isInPlayerInventory = true;
                        newDuplicate.GetComponent<Env_Item>().isInContainer = false;

                        if (gameObject.GetComponent<Item_Ammo>() != null)
                        {
                            UpdateGunsAndAmmo();
                        }
                    }
                }

                par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                RemoveListeners();
                par_Managers.GetComponent<Manager_UIReuse>().RebuildContainerInventory();
                par_Managers.GetComponent<Manager_UIReuse>().ClearCountSliderUI();

                par_Managers.GetComponent<Manager_UIReuse>().btn_PlaceIntoContainer.onClick.AddListener(PlayerInventoryScript.CloseContainer);

                par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";

                foundDuplicate = false;
                duplicate = null;

                PlayerInventoryScript.UpdatePlayerInventoryStats();

                str_containerName = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName;
                int_totalSpace = int_ItemWeight * int_confirmedCount;
                //Debug.Log("Took " + int_confirmedCount + " " + str_ItemName + "(s) from " + str_containerName + "! Removed " + int_totalSpace.ToString() + " space from players inventory.");
            }
            else
            {
                foundDuplicate = false;
                duplicate = null;

                str_containerName = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName;
                Debug.Log("Error: Not enough space to take " + str_ItemName + " from " + str_containerName + "!");
                CancelCount();
            }

            if (!isInRepairMenu)
            {
                par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
            }
        }

        else if (isPlacingMultipleItems)
        {
            if (foundDuplicate)
            {
                duplicate.GetComponent<Env_Item>().int_itemCount += int_confirmedCount;
                PlayerInventoryScript.invSpace += int_ItemWeight * int_confirmedCount;

                if (int_confirmedCount < int_itemCount)
                {
                    int_itemCount -= int_confirmedCount;
                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        UpdateGunsAndAmmo();
                    }
                }
                else if (int_confirmedCount == int_itemCount || int_confirmedCount - int_itemCount == 0)
                {
                    PlayerInventoryScript.inventory.Remove(gameObject);
                    par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(str_ItemName);

                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    RemoveListeners();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearCountSliderUI();

                    par_Managers.GetComponent<Manager_UIReuse>().btn_TakeFromContainer.onClick.AddListener(PlayerInventoryScript.Container.GetComponent<Inv_Container>().CheckIfLocked);

                    par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        RemoveAmmotypeFromAllGuns();
                    }

                    PlayerInventoryScript.UpdatePlayerInventoryStats();

                    Destroy(gameObject);
                }
            }
            else if (!foundDuplicate)
            {
                if (int_confirmedCount == int_itemCount)
                {
                    Vector3 pos_container = PlayerInventoryScript.Container.GetComponent<Inv_Container>().par_ContainerItems.transform.position;
                    gameObject.transform.position = pos_container;

                    PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory.Add(gameObject);
                    PlayerInventoryScript.inventory.Remove(gameObject);
                    PlayerInventoryScript.invSpace += int_ItemWeight * int_confirmedCount;
                    gameObject.transform.SetParent(PlayerInventoryScript.Container.GetComponent<Inv_Container>().par_ContainerItems.transform);

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        RemoveAmmotypeFromAllGuns();
                    }

                    isInPlayerInventory = false;
                    isInContainer = true;

                    par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(str_ItemName);
                }
                else if (int_confirmedCount < int_itemCount)
                {
                    Vector3 pos_container = PlayerInventoryScript.Container.GetComponent<Inv_Container>().par_ContainerItems.transform.position;
                    GameObject newDuplicate = Instantiate(gameObject, 
                                                          pos_container, 
                                                          Quaternion.identity, 
                                                          PlayerInventoryScript.Container.GetComponent<Inv_Container>().par_ContainerItems.transform);

                    newDuplicate.transform.position = pos_container;

                    PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory.Add(newDuplicate);
                    PlayerInventoryScript.inventory.Remove(newDuplicate);
                    int_itemCount -= int_confirmedCount;
                    PlayerInventoryScript.invSpace += (int_ItemWeight * int_confirmedCount);

                    newDuplicate.name = newDuplicate.GetComponent<Env_Item>().str_ItemName;
                    newDuplicate.GetComponent<Env_Item>().int_itemCount = int_confirmedCount;
                    newDuplicate.GetComponent<Env_Item>().isInPlayerInventory = false;
                    newDuplicate.GetComponent<Env_Item>().isInContainer = true;

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        UpdateGunsAndAmmo();
                    }
                }
            }

            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            RemoveListeners();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();
            par_Managers.GetComponent<Manager_UIReuse>().ClearCountSliderUI();

            par_Managers.GetComponent<Manager_UIReuse>().btn_TakeFromContainer.onClick.AddListener(PlayerInventoryScript.Container.GetComponent<Inv_Container>().CheckIfLocked);

            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";

            foundDuplicate = false;
            duplicate = null;

            PlayerInventoryScript.UpdatePlayerInventoryStats();
            if (!isInRepairMenu)
            {
                par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
            }

            str_containerName = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName;
            int_totalSpace = int_ItemWeight * int_confirmedCount;
            //Debug.Log("Placed " + int_confirmedCount + " " + str_ItemName + "(s) into " + str_containerName + "! Added " + int_totalSpace.ToString() + " space back to players inventory.");
        }

        else if (isDroppingMultipleItems)
        {
            if (int_confirmedCount == int_itemCount)
            {
                theItem = gameObject;

                PlayerInventoryScript.inventory.Remove(gameObject);
                PlayerInventoryScript.invSpace += int_ItemWeight * int_confirmedCount;

                if (gameObject.GetComponent<Item_Ammo>() != null)
                {
                    RemoveAmmotypeFromAllGuns();
                }

                par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(str_ItemName);

                //get a random direction (360°) in radians
                float angle = Random.Range(0.0f, Mathf.PI * 2);
                //create a vector with length 1.0
                Vector3 dropPos = new(Mathf.Sin(angle), 0, Mathf.Cos(angle));
                //set item drop position
                gameObject.transform.position = thePlayer.transform.position + dropPos;

                droppedObject = true;
                time = 0;

                foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
                {
                    if (!cell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                    {
                        cell.GetComponent<Manager_CurrentCell>().items.Add(gameObject);
                        break;
                    }
                }

                gameObject.transform.parent = par_DroppedItems.transform;

                isInPlayerInventory = false;

                ActivateItem();
            }

            else if (int_confirmedCount < int_itemCount)
            {
                GameObject duplicate = Instantiate(gameObject, 
                                                   pos_HoldItem.position, 
                                                   Quaternion.identity,
                                                   par_DroppedItems.transform);

                theItem = duplicate;

                PlayerInventoryScript.invSpace += (int_ItemWeight * int_confirmedCount);
                int_itemCount -= int_confirmedCount;

                //get a random direction (360°) in radians
                float angle = Random.Range(0.0f, Mathf.PI * 2);
                //create a vector with length 1.0
                Vector3 dropPos = new(Mathf.Sin(angle), 0, Mathf.Cos(angle));
                //set item drop position
                theItem.transform.position = thePlayer.transform.position + dropPos;

                droppedObject = true;
                time = 0;

                foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
                {
                    if (!cell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                    {
                        cell.GetComponent<Manager_CurrentCell>().items.Add(gameObject);
                        break;
                    }
                }

                theItem.name = theItem.GetComponent<Env_Item>().str_ItemName;
                theItem.GetComponent<Env_Item>().int_itemCount = int_confirmedCount;
                theItem.GetComponent<Env_Item>().isInPlayerInventory = false;

                if (gameObject.GetComponent<Item_Ammo>() != null)
                {
                    UpdateGunsAndAmmo();
                }

                theItem.GetComponent<Env_Item>().ActivateItem();
            }

            droppedObject = true;

            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            RemoveListeners();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();
            par_Managers.GetComponent<Manager_UIReuse>().ClearCountSliderUI();

            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";

            foundDuplicate = false;
            duplicate = null;

            PlayerInventoryScript.UpdatePlayerInventoryStats();
            if (!isInRepairMenu)
            {
                par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
            }

            int_totalSpace = int_ItemWeight * int_confirmedCount;
            //Debug.Log("Dropped " + int_confirmedCount + " " + str_ItemName + "(s)! Added " + int_totalSpace.ToString() + " space back to players inventory.");
        }

        else if (isDestroyingMultipleItems)
        {
            if (int_confirmedCount == int_itemCount)
            {
                PlayerInventoryScript.inventory.Remove(gameObject);
                PlayerInventoryScript.invSpace += int_ItemWeight * int_confirmedCount;
                isInPlayerInventory = false;

                par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                RemoveListeners();
                par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();
                par_Managers.GetComponent<Manager_UIReuse>().ClearCountSliderUI();

                par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";

                if (gameObject.GetComponent<Item_Ammo>() != null)
                {
                    RemoveAmmotypeFromAllGuns();
                }

                par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(str_ItemName);

                int_totalSpace = int_ItemWeight * int_confirmedCount;
                //Debug.Log("Destroyed " + int_confirmedCount + " " + str_ItemName + "(s)! Added " + int_totalSpace.ToString() + " space back to players inventory.");

                Destroy(gameObject);
            }

            else if (int_confirmedCount < int_itemCount)
            {
                PlayerInventoryScript.invSpace += int_ItemWeight * int_confirmedCount;
                int_itemCount -= int_confirmedCount;

                par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                RemoveListeners();
                par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();
                par_Managers.GetComponent<Manager_UIReuse>().ClearCountSliderUI();

                par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";

                if (gameObject.GetComponent<Item_Ammo>() != null)
                {
                    UpdateGunsAndAmmo();
                }

                foundDuplicate = false;
                duplicate = null;

                int_totalSpace = int_ItemWeight * int_confirmedCount;
                //Debug.Log("Destroyed " + int_confirmedCount + " " + str_ItemName + "(s)! Added " + int_totalSpace.ToString() + " space back to players inventory.");
            }

            PlayerInventoryScript.UpdatePlayerInventoryStats();
            if (!isInRepairMenu)
            {
                par_Managers.GetComponent<Manager_UIReuse>().EnableInventorySortButtons();
            }
        }
    }

    private void UpdateGunsAndAmmo()
    {
        RemoveAmmotypeFromAllGuns();

        //if the player currently has a gun equipped
        if (PlayerInventoryScript.equippedGun != null
            && PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>() != null)
        {
            foreach (GameObject item in PlayerInventoryScript.inventory)
            {
                //fill the currently equipped guns ammo with the correct ammo type
                if (item.GetComponent<Item_Ammo>() != null
                    && PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().caseType.ToString()
                    == item.GetComponent<Item_Ammo>().caseType.ToString())
                {
                    PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().ammoClip = item;
                    PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().AssignAmmoType();
                    break;
                }
            }
        }
    }
    private void RemoveAmmotypeFromAllGuns()
    {
        foreach (GameObject gun in PlayerInventoryScript.inventory)
        {
            //remove current ammotype from all player inventory guns with same ammo type as this gameobject
            if (gun.GetComponent<Item_Gun>() != null
                && gun.GetComponent<Item_Gun>().caseType.ToString()
                == gameObject.GetComponent<Item_Ammo>().caseType.ToString())
            {
                gun.GetComponent<Item_Gun>().ammoClip = null;
            }
        }

        par_Managers.GetComponent<Manager_UIReuse>().txt_ammoForGun.text = "0";
    }
    private void UnequipAndUnloadGun()
    {
        par_Managers.GetComponent<Manager_UIReuse>().ClearWeaponUI();
        //unequips this item if it is a gun
        if (gameObject.GetComponent<Item_Gun>().hasEquippedGun)
        {
            gameObject.GetComponent<Item_Gun>().UnequipGun();
            //Debug.Log("Unequipped this gun because player removed it from their inventory.");
        }
        //if the player is removing a gun from their inventory with a clip that isnt empty
        if (gameObject.GetComponent<Item_Gun>().currentClipSize > 0)
        {
            int removedAmmo = gameObject.GetComponent<Item_Gun>().currentClipSize;
            correctAmmo = null;

            //looks for the same ammo from the players inventory as this gun
            foreach (GameObject item in PlayerInventoryScript.inventory)
            {
                if (gameObject.GetComponent<Item_Gun>().caseType.ToString()
                    == item.GetComponent<Item_Ammo>().caseType.ToString())
                {
                    correctAmmo = item;
                    break;
                }
            }

            //finds the guns ammo type and assigns the guns ammo to the ammotype
            if (correctAmmo != null)
            {
                foreach (GameObject item in PlayerInventoryScript.inventory)
                {
                    if (gameObject.GetComponent<Item_Gun>().caseType.ToString()
                        == item.GetComponent<Item_Ammo>().caseType.ToString())
                    {
                        item.GetComponent<Env_Item>().int_itemCount += removedAmmo;
                        gameObject.GetComponent<Item_Gun>().currentClipSize = 0;
                        break;
                    }
                }
                //Debug.Log("Unloaded this gun and added " + removedAmmo + " ammo to existing ammo clip in players inventory.");
            }
            //if no ammo clip for this gun was found in the players inventory then a new clip is created
            else if (correctAmmo == null)
            {
                GameObject ammo = Instantiate(par_Managers.GetComponent<Manager_Console>().ammoTemplate, 
                                              PlayerInventoryScript.transform.position, 
                                              Quaternion.identity);

                ammo.GetComponent<Env_Item>().int_itemCount = removedAmmo;
                ammo.name = ammo.GetComponent<Env_Item>().str_ItemName;
                PlayerInventoryScript.inventory.Add(ammo);
                par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(ammo.GetComponent<Env_Item>().str_ItemName);

                if (PlayerInventoryScript.isPlayerInventoryOpen)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();
                    par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";
                }
                else if (PlayerInventoryScript.Container != null && PlayerInventoryScript.Container.GetComponent<Inv_Container>().isContainerInventoryOpen)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildContainerInventory();
                    par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";
                }
                else if (PlayerInventoryScript.Trader != null && PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().isShopOpen)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildShopInventory();
                    par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().str_ShopName;
                }
                else if (PlayerInventoryScript.Trader != null && PlayerInventoryScript.Trader.GetComponent<UI_RepairContent>().isNPCRepairUIOpen)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildRepairMenu();
                    par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.GetComponent<UI_AIContent>().str_NPCName + "'s repair shop";
                }
                else if (PlayerInventoryScript.Workbench != null)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
                    par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                    par_Managers.GetComponent<Manager_UIReuse>().RebuildRepairMenu();
                    par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.GetComponent<Env_Workbench>().str_workbenchName;
                }
                //Debug.Log("Unloaded this gun and added " + removedAmmo + " ammo to new ammo clip in players inventory.");
            }
        }
    }

    public void ActivateItem()
    {
        if (gameObject.GetComponent<MeshRenderer>() != null)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = true;
        }
        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
        gameObject.SetActive(true);
    }
    public void DeactivateItem()
    {
        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
        gameObject.SetActive(false);
    }

    public void DestroyObject()
    {
        //if the found cell doesnt contain the item then it looks through all the other cells items
        //and tries to destroy it if it finds the same gameobject
        foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
        {
            if (cell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
            {
                cell.GetComponent<Manager_CurrentCell>().items.Remove(gameObject);
                break;
            }
        }
        par_Managers.GetComponent<Manager_UIReuse>().InteractUIDisabled();
        Destroy(gameObject);
    }

    public void CancelCount()
    {
        //for some reason isInContainer is activated when picking up ammo
        if (!PlayerInventoryScript.isPlayerAndContainerOpen)
        {
            isInContainer = false;
        }
        //unpauses the game properly if the player cancels item pickup
        //if the player is not in player inv, container inv and trader shop
        if (!isInPlayerInventory && !isInContainer && !isInTraderShop)
        {
            par_Managers.GetComponent<UI_PauseMenu>().UnpauseGame();
        }

        foundDuplicate = false;
        duplicate = null;

        int_selectedCount = 0;
        int_confirmedCount = 0;
        isPickingUpMultipleItems = false;
        isBuyingMultipleItems = false;
        isSellingMultipleItems = false;
        isDroppingMultipleItems = false;
        isDestroyingMultipleItems = false;
        isTakingMultipleItems = false;
        isPlacingMultipleItems = false;

        par_Managers.GetComponent<Manager_UIReuse>().ClearCountSliderUI();
    }

    public void RemoveListeners()
    {
        par_Managers.GetComponent<Manager_UIReuse>().btn_BuyItem.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_SellItem.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_Take.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_Place.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_Drop.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_Destroy.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_ConfirmCount.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_CancelCount.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_Equip.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_Unequip.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_Consume.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_AddBattery.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_RemoveBattery.onClick.RemoveAllListeners();
    }

    //if this item collided with world border
    //then look for closest discovered cell spawn point
    //and teleport this item there
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("WorldBlocker"))
        {
            foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
            {
                if (cell.GetComponent<Manager_CurrentCell>().discoveredCell)
                {
                    float distance = Vector3.Distance(gameObject.transform.position, cell.GetComponent<Manager_CurrentCell>().currentCellSpawnpoint.position);

                    if (cell == par_Managers.GetComponent<Manager_Console>().allCells[0])
                    {
                        closestDistance = distance;
                    }
                    else
                    {
                        if (distance < closestDistance)
                        {
                            cellName = cell.GetComponent<Manager_CurrentCell>().str_CellName;
                        }
                    }
                }
            }

            foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
            {
                if (cell.GetComponent<Manager_CurrentCell>().discoveredCell
                    && cell.GetComponent<Manager_CurrentCell>().str_CellName
                    == cellName)
                {
                    gameObject.transform.position = cell.GetComponent<Manager_CurrentCell>().currentCellSpawnpoint.position + new Vector3(0, 0.2f, 0);
                    break;
                }
            }
        }
    }
}