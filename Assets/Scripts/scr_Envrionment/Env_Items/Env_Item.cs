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
    [Tooltip("Items with this tag will be randomized. Note: Items dropped by dead bodies are always randomized.")]
    [SerializeField] private bool randomizeCount;
    public string str_ItemName;
    public string str_ItemDescription;
    [Tooltip("How rare is this item? Note: Higher rarity items cost more and have better overall stats.")]
    public Itemrarity itemRarity;
    public enum Itemrarity
    {
        unassigned,
        Trash,
        Common,
        Rare,
        Legendary
    }

    [Tooltip("Which faction does this AI belong to?")]
    public Faction faction;
    public enum Faction
    {
        unassigned,
        Scientists,
        Geifers,
        Annies,
        Verbannte,
        Raiders,
        Military,
        Verteidiger,
        Others
    }
    [Tooltip("How much is this item worth in the in-game currency?")]
    public int int_ItemValue;
    [Tooltip("How much does this item weigh?")]
    public int int_ItemWeight;
    [Tooltip("How many of this item exist in this single item slot. Note: Randomized items or dead items dropped from enemies ignore this value.")]
    public int int_itemCount = 1;

    [Header("Assignables")]
    [SerializeField] private Transform pos_HoldItem;
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_DroppedItems;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private UI_PauseMenu PauseMenuScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;
    [SerializeField] private Manager_Console ConsoleScript;

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

    [Header("Exoskeleton assignables")]
    [SerializeField] private UI_PlayerMenu PlayerMenuScript;
    [SerializeField] private UI_AbilityAssignManager AbilityAssignManagerScript;

    //public but hidden variables
    [HideInInspector] public bool itemActivated;
    [HideInInspector] public bool isInPlayerInventory;
    [HideInInspector] public bool isInContainer;
    [HideInInspector] public bool isInTraderShop;
    [HideInInspector] public bool isInRepairMenu;
    [HideInInspector] public bool isBuying;
    [HideInInspector] public bool isTaking;
    [HideInInspector] public bool hasUnderscore;
    [HideInInspector] public bool droppedObject;
    [HideInInspector] public bool toBeDeleted;
    [HideInInspector] public float time;
    [HideInInspector] public GameObject theItem;

    //private variables
    private bool foundDuplicate;
    private bool canContinue;

    //multiple checkers
    private bool isPickingUpMultipleItems;
    private bool isBuyingMultipleItems;
    private bool isSellingMultipleItems;
    private bool isDroppingMultipleItems;
    private bool isDestroyingMultipleItems;
    private bool isTakingMultipleItems;
    private bool isPlacingMultipleItems;

    private int int_selectedCount;
    private int int_confirmedCount;
    private GameObject selectedGun;
    private GameObject correctAmmo;
    private GameObject duplicate;
    private GameObject cell;
    //buy, sell, space
    private int int_finalSpace;
    private int int_totalSpace;
    private int int_quarterPrice;
    private int int_singlePrice;
    private int int_finalPrice;
    private string str_itemName;
    private string str_containerName;
    private string str_traderName;

    //collision check
    private bool checkForCollision;
    private bool finishedCollisionCheck;
    private bool isColliding;
    private float collisionTimer;
    private LayerMask layer;

    private void Start()
    {
        if (gameObject.name != str_ItemName)
        {
            gameObject.name = str_ItemName;
        }

        string layerName = "Player";
        layer = LayerMask.NameToLayer(layerName);

        if (randomizeCount)
        {
            //GameObject spawnableParent = GameObject.Find("par_consoleSpawnables");
            if (gameObject.transform.parent.name != "par_consoleSpawnables"
                && gameObject.transform.parent.name != "DeadAILoot")
            {
                //randomize ammo count
                if (gameObject.GetComponent<Item_Ammo>() != null)
                {
                    int_itemCount = Random.Range(15, 45);
                }
                //randomize health kit amount
                if (gameObject.GetComponent<Item_Consumable>() != null
                    && gameObject.GetComponent<Item_Consumable>().consumableType
                    == Item_Consumable.ConsumableType.Healthkit)
                {
                    int_itemCount = Random.Range(1, 3);
                }
                //randomize money amount
                if (name == "money")
                {
                    int_itemCount = Random.Range(15, 200);
                }
            }
        }
    }

    private void Update()
    {
        //resets the stackable item bools
        if (!isInPlayerInventory && !isInContainer && !isInTraderShop && itemActivated)
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
        if (droppedObject && !PauseMenuScript.isGamePaused)
        {
            time += Time.deltaTime;

            if (time > 120)
            {
                if (!isProtected)
                {
                    Debug.Log("Perf: " + name + " was destroyed because it had been dropped for too long.");
                    DestroyObject();
                }
                /*
                else if (isProtected)
                {
                    Vector3 playerPos = thePlayer.transform.position;
                    Vector3 playerDirection = thePlayer.transform.forward;

                    Vector3 placePosition = playerPos + playerDirection * 2;
                    gameObject.transform.position = placePosition;

                    Debug.Log("Perf: " + name + " cannot be deleted because it is protected! Teleported it to player position.");

                    time = 0;
                    droppedObject = false;
                }
                */
            }
            else if (Vector3.Distance(transform.position, thePlayer.transform.transform.position) > 25)
            {
                if (!isProtected)
                {
                    Debug.Log("Perf: " + name + " was destroyed after player went too far from it.");
                    DestroyObject();
                }
                /*
                else if (isProtected)
                {
                    Vector3 playerPos = thePlayer.transform.position;
                    Vector3 playerDirection = thePlayer.transform.forward;

                    Vector3 placePosition = playerPos + playerDirection * 2;
                    gameObject.transform.position = placePosition;

                    Debug.Log("Perf: " + name + " cannot be deleted because it is protected! Teleported it to player position.");

                    droppedObject = false;
                }
                */
            }
        }
    }

    private void FixedUpdate()
    {
        if (checkForCollision)
        {
            //if anything collides with the ray then inventory items cant be dropped and holdable items cant be held
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out _, 1, ~layer, QueryTriggerInteraction.Ignore))
            {
                isColliding = true;
                //Debug.Log("Hit " + hit.transform.name + "! Can't drop inventory items and can't hold holdable items!");
            }

            collisionTimer += Time.deltaTime;
            if (collisionTimer >= 0.2f)
            {
                finishedCollisionCheck = true;
                checkForCollision = false;
            }
        }
        else if (checkForCollision && isColliding)
        {
            collisionTimer = 0;
            isColliding = false;
        }
    }

    //allows to use certain buttons depending on which inventory
    //the player is in currently
    public void ShowStats()
    {
        RemoveListeners();
        UIReuseScript.ClearAllInventories();
        UIReuseScript.ClearInventoryUI();
        UIReuseScript.ClearStatsUI();

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
        UIReuseScript.ClearCountSliderUI();

        UIReuseScript.txt_ItemCount.text = "";
        UIReuseScript.txt_protected.gameObject.SetActive(false);
        UIReuseScript.txt_notStackable.gameObject.SetActive(false);
        UIReuseScript.txt_tooHeavy.gameObject.SetActive(false);
        UIReuseScript.txt_tooExpensive.gameObject.SetActive(false);

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
            UIReuseScript.txt_ItemName.text = str_fakeName;
        }
        else if (!hasUnderscore)
        {
            UIReuseScript.txt_ItemName.text = str_ItemName;
        }
        //item description
        UIReuseScript.txt_ItemDescription.text = str_ItemDescription;
        UIReuseScript.txt_ItemRarity.text = itemRarity.ToString();

        // ### ITEM TYPE START ###

        //item type is weapon if this item can be used as a gun or melee weapon
        if (gameObject.GetComponent<Item_Gun>() != null)
        {
            UIReuseScript.txt_ItemType.text = "Weapon";
        }
        //item type is grenade if this item can be used as a grenade
        else if (gameObject.GetComponent<Item_Grenade>() != null)
        {
            UIReuseScript.txt_ItemType.text = "Grenade";
        }
        //item type is healing if this item can be consumed
        else if (gameObject.GetComponent<Item_Consumable>() != null)
        {
            UIReuseScript.txt_ItemType.text = "Healing";
        }
        //item type is ammo if this item can be used as ammo for any guns
        else if (gameObject.GetComponent<Item_Ammo>() != null)
        {
            UIReuseScript.txt_ItemType.text = "Ammo";
        }
        //item type is key if this item can be used as key for any doors or containers
        else if (gameObject.GetComponent<Item_Lockpick>() != null
                 && gameObject.GetComponent<Item_Lockpick>().itemType
                 == Item_Lockpick.ItemType.key)
        {
            UIReuseScript.txt_ItemType.text = "Key";
        }
        //item type is misc if no special item script was found
        else if (gameObject.GetComponent<Item_Gun>() == null
            && gameObject.GetComponent<Item_Consumable>() == null
            && gameObject.GetComponent<Item_Ammo>() == null)
        {
            UIReuseScript.txt_ItemType.text = "Misc";
        }

        // ### ITEM TYPE END ###

        //item clan
        UIReuseScript.txt_ItemClan.text = faction.ToString();
        //single or multiple item value and weight
        if (int_itemCount == 1)
        {
            UIReuseScript.txt_ItemValue.text = int_ItemValue.ToString();
            UIReuseScript.txt_ItemWeight.text = int_ItemWeight.ToString();
        }
        else if (int_itemCount > 1)
        {
            UIReuseScript.txt_ItemValue.text = int_ItemValue.ToString() + " (" + int_ItemValue * int_itemCount + ")";
            UIReuseScript.txt_ItemWeight.text = int_ItemWeight.ToString() + " (" + int_ItemWeight * int_itemCount + ")";
        }
        //gun ammo count
        if (gameObject.GetComponent<Item_Gun>() != null)
        {
            UIReuseScript.txt_ItemCount.text = "Ammo: " + gameObject.GetComponent<Item_Gun>().currentClipSize.ToString();
        }

        if (!isInRepairMenu)
        {
            UIReuseScript.EnableInventorySortButtons();
        }

        //if is in own inventory
        if (isInPlayerInventory
            && !PlayerInventoryScript.isPlayerAndContainerOpen
            && !PlayerInventoryScript.isPlayerAndTraderOpen
            && !PlayerInventoryScript.isPlayerAndRepairOpen)
        {
            UIReuseScript.txt_InventoryName.text = "Player inventory";
            UIReuseScript.RebuildPlayerInventory();

            if (gameObject.GetComponent<Item_Gun>() == null
                && gameObject.GetComponent<Item_Consumable>() == null
                && gameObject.GetComponent<Item_Ammo>() != null)
            {
                UIReuseScript.txt_ItemRemainder.text = "Ammo: " + int_itemCount;
            }
            else if (gameObject.GetComponent<Item_Consumable>() != null
                     && gameObject.GetComponent<Item_Gun>() == null)
            {
                if (gameObject.GetComponent<Item_Consumable>().consumableType == Item_Consumable.ConsumableType.Repairkit)
                {
                    float finalPercentage = gameObject.GetComponent<Item_Consumable>().currentConsumableAmount
                                            / gameObject.GetComponent<Item_Consumable>().maxConsumableAmount * 100;
                    UIReuseScript.txt_ItemRemainder.text = "Remainder: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";
                }
                else if (gameObject.GetComponent<Item_Consumable>().consumableType == Item_Consumable.ConsumableType.Healthkit
                         || gameObject.GetComponent<Item_Consumable>().consumableType == Item_Consumable.ConsumableType.Food)
                {
                    float finalPercentage = gameObject.GetComponent<Item_Consumable>().currentConsumableAmount
                                            / gameObject.GetComponent<Item_Consumable>().maxConsumableAmount * 100;
                    UIReuseScript.txt_ItemRemainder.text = "Remainder: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";

                    UIReuseScript.btn_Consume.gameObject.SetActive(true);
                    if (PlayerHealthScript.health < PlayerHealthScript.maxHealth)
                    {
                        UIReuseScript.btn_Consume.interactable = true;
                    }
                    else if (PlayerHealthScript.health == PlayerHealthScript.maxHealth)
                    {
                        UIReuseScript.btn_Consume.interactable = false;
                    }
                    UIReuseScript.btn_Consume.onClick.AddListener(gameObject.GetComponent<Item_Consumable>().Consume);
                }
            }
            else if (gameObject.GetComponent<Item_Lockpick>() != null
                     && gameObject.GetComponent<Item_Lockpick>().itemType
                     == Item_Lockpick.ItemType.lockpick)
            {
                float finalPercentage = gameObject.GetComponent<Item_Lockpick>().lockpickDurability
                                        / gameObject.GetComponent<Item_Lockpick>().maxLockpickDurability * 100;
                UIReuseScript.txt_ItemRemainder.text = "Remainder: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }
            else if (gameObject.GetComponent<Item_Melee>() != null
                     && gameObject.GetComponent<Item_Consumable>() == null)
            {
                if (!gameObject.GetComponent<Item_Melee>().hasEquippedMeleeWeapon)
                {
                    UIReuseScript.btn_Equip.gameObject.SetActive(true);
                    UIReuseScript.btn_Equip.onClick.AddListener(gameObject.GetComponent<Item_Melee>().EquipMeleeWeapon);
                }
                else if (gameObject.GetComponent<Item_Melee>().hasEquippedMeleeWeapon)
                {
                    UIReuseScript.btn_Unequip.gameObject.SetActive(true);
                    UIReuseScript.btn_Unequip.onClick.AddListener(gameObject.GetComponent<Item_Melee>().UnequipMeleeWeapon);
                }
            }
            else if (gameObject.GetComponent<Item_Gun>() != null
                     && gameObject.GetComponent<Item_Consumable>() == null)
            {
                float durability = gameObject.GetComponent<Item_Gun>().durability;
                float maxDurability = gameObject.GetComponent<Item_Gun>().maxDurability;

                float finalPercentage = durability / maxDurability * 100;
                UIReuseScript.txt_ItemDurability.text = "Durability: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";

                if (!gameObject.GetComponent<Item_Gun>().hasEquippedGun)
                {
                    UIReuseScript.btn_Equip.gameObject.SetActive(true);
                    UIReuseScript.btn_Equip.onClick.AddListener(gameObject.GetComponent<Item_Gun>().EquipGun);
                }
                else if (gameObject.GetComponent<Item_Gun>().hasEquippedGun)
                {
                    UIReuseScript.btn_Unequip.gameObject.SetActive(true);
                    UIReuseScript.btn_Unequip.onClick.AddListener(gameObject.GetComponent<Item_Gun>().UnequipGun);
                }
            }
            else if (gameObject.GetComponent<Item_Grenade>() != null
                     && gameObject.GetComponent<Item_Consumable>() == null)
            {
                if (!gameObject.GetComponent<Item_Grenade>().hasEquippedGrenade)
                {
                    UIReuseScript.btn_Equip.gameObject.SetActive(true);
                    UIReuseScript.btn_Equip.onClick.AddListener(gameObject.GetComponent<Item_Grenade>().EquipGrenade);
                }
                else if (gameObject.GetComponent<Item_Grenade>().hasEquippedGrenade)
                {
                    UIReuseScript.btn_Unequip.gameObject.SetActive(true);
                    UIReuseScript.btn_Unequip.onClick.AddListener(gameObject.GetComponent<Item_Grenade>().UnequipGrenade);
                }
            }

            if (!isProtected)
            {
                UIReuseScript.btn_Destroy.gameObject.SetActive(true);
                UIReuseScript.btn_Destroy.onClick.AddListener(Destroy);
                UIReuseScript.btn_Drop.gameObject.SetActive(true);
                UIReuseScript.btn_Drop.onClick.AddListener(Drop);
            }
            else if (isProtected)
            {
                UIReuseScript.txt_protected.gameObject.SetActive(true);
            }

            if (!isStackable)
            {
                UIReuseScript.txt_notStackable.gameObject.SetActive(true);
            }
        }

        //if is looting container
        else if (PlayerInventoryScript.isPlayerAndContainerOpen
                && isInContainer
                && isTaking)
        {
            UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";
            UIReuseScript.RebuildContainerInventory();

            UIReuseScript.btn_PlaceIntoContainer.onClick.AddListener(PlayerInventoryScript.CloseContainer);
            UIReuseScript.btn_Take.gameObject.SetActive(true);
            UIReuseScript.btn_Take.onClick.AddListener(Take);

            if (gameObject.GetComponent<Item_Gun>() == null
                && gameObject.GetComponent<Item_Consumable>() == null
                && gameObject.GetComponent<Item_Ammo>() != null)
            {
                UIReuseScript.txt_ItemRemainder.text = "Ammo: " + int_itemCount;
            }
            else if (gameObject.GetComponent<Item_Consumable>() != null
                && gameObject.GetComponent<Item_Gun>() == null)
            {
                if (gameObject.GetComponent<Item_Consumable>().consumableType == Item_Consumable.ConsumableType.Repairkit
                    || gameObject.GetComponent<Item_Consumable>().consumableType == Item_Consumable.ConsumableType.Healthkit)
                {
                    float finalPercentage = gameObject.GetComponent<Item_Consumable>().currentConsumableAmount
                                            / gameObject.GetComponent<Item_Consumable>().maxConsumableAmount * 100;
                    UIReuseScript.txt_ItemRemainder.text = "Remainder: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";
                }
            }
            else if (gameObject.GetComponent<Item_Lockpick>() != null
                    && gameObject.GetComponent<Item_Lockpick>().itemType
                    == Item_Lockpick.ItemType.lockpick)
            {
                float finalPercentage = gameObject.GetComponent<Item_Lockpick>().lockpickDurability
                                        / gameObject.GetComponent<Item_Lockpick>().maxLockpickDurability * 100;
                UIReuseScript.txt_ItemRemainder.text = "Remainder: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }
            else if (gameObject.GetComponent<Item_Gun>() != null
                     && gameObject.GetComponent<Item_Consumable>() == null)
            {
                float durability = gameObject.GetComponent<Item_Gun>().durability;
                float maxDurability = gameObject.GetComponent<Item_Gun>().maxDurability;

                float finalPercentage = durability / maxDurability * 100;
                UIReuseScript.txt_ItemDurability.text = "Durability: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }

            if (int_ItemWeight > PlayerInventoryScript.invSpace)
            {
                UIReuseScript.btn_Take.interactable = false;
                UIReuseScript.txt_tooHeavy.gameObject.SetActive(true);
            }
            else
            {
                UIReuseScript.btn_Take.interactable = true;
                UIReuseScript.txt_tooHeavy.gameObject.SetActive(false);
            }

            if (!isStackable)
            {
                UIReuseScript.txt_notStackable.gameObject.SetActive(true);
            }
            //Debug.Log("Looting from " + PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + "!");
        }
        //if is placing into container
        else if (PlayerInventoryScript.isPlayerAndContainerOpen
                && isInPlayerInventory
                && !isTaking)
        {
            UIReuseScript.txt_InventoryName.text = "Player inventory";
            UIReuseScript.RebuildPlayerInventory();

            UIReuseScript.btn_TakeFromContainer.onClick.AddListener(PlayerInventoryScript.Container.GetComponent<Inv_Container>().CheckIfLocked);

            if (gameObject.GetComponent<Item_Gun>() == null
                && gameObject.GetComponent<Item_Consumable>() == null
                && gameObject.GetComponent<Item_Ammo>() != null)
            {
                UIReuseScript.txt_ItemRemainder.text = "Ammo: " + int_itemCount;
            }
            else if (gameObject.GetComponent<Item_Consumable>() != null
                     && gameObject.GetComponent<Item_Gun>() == null)
            {
                if (gameObject.GetComponent<Item_Consumable>().consumableType == Item_Consumable.ConsumableType.Repairkit
                    || gameObject.GetComponent<Item_Consumable>().consumableType == Item_Consumable.ConsumableType.Healthkit)
                {
                    float finalPercentage = gameObject.GetComponent<Item_Consumable>().currentConsumableAmount
                                            / gameObject.GetComponent<Item_Consumable>().maxConsumableAmount * 100;
                    UIReuseScript.txt_ItemRemainder.text = "Remainder: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";
                }
            }
            else if (gameObject.GetComponent<Item_Lockpick>() != null
                     && gameObject.GetComponent<Item_Lockpick>().itemType
                     == Item_Lockpick.ItemType.lockpick)
            {
                float finalPercentage = gameObject.GetComponent<Item_Lockpick>().lockpickDurability
                                        / gameObject.GetComponent<Item_Lockpick>().maxLockpickDurability * 100;
                UIReuseScript.txt_ItemRemainder.text = "Remainder: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }
            else if (gameObject.GetComponent<Item_Gun>() != null
                     && gameObject.GetComponent<Item_Consumable>() == null)
            {
                float durability = gameObject.GetComponent<Item_Gun>().durability;
                float maxDurability = gameObject.GetComponent<Item_Gun>().maxDurability;

                float finalPercentage = durability / maxDurability * 100;
                UIReuseScript.txt_ItemDurability.text = "Durability: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }

            if (!isProtected)
            {
                UIReuseScript.btn_Place.gameObject.SetActive(true);
                UIReuseScript.btn_Place.interactable = true;
                UIReuseScript.btn_Place.onClick.AddListener(Place);
            }
            else if (isProtected)
            {
                UIReuseScript.txt_protected.gameObject.SetActive(true);
            }

            if (!isStackable)
            {
                UIReuseScript.txt_notStackable.gameObject.SetActive(true);
            }
            //Debug.Log("Placing into " + PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + "!");
        }
        //if is buying from npc
        else if (PlayerInventoryScript.isPlayerAndTraderOpen
                && isInTraderShop
                && isBuying)
        {
            UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().str_ShopName;
            UIReuseScript.RebuildShopInventory();

            UIReuseScript.btn_SellToTrader.onClick.AddListener(PlayerInventoryScript.CloseShop);

            if (gameObject.GetComponent<Item_Gun>() == null
                && gameObject.GetComponent<Item_Consumable>() == null
                && gameObject.GetComponent<Item_Ammo>() != null)
            {
                UIReuseScript.txt_ItemRemainder.text = "Ammo: " + int_itemCount;
            }
            else if (gameObject.GetComponent<Item_Consumable>() != null
                && gameObject.GetComponent<Item_Gun>() == null)
            {
                if (gameObject.GetComponent<Item_Consumable>().consumableType == Item_Consumable.ConsumableType.Repairkit
                    || gameObject.GetComponent<Item_Consumable>().consumableType == Item_Consumable.ConsumableType.Healthkit)
                {
                    float finalPercentage = gameObject.GetComponent<Item_Consumable>().currentConsumableAmount
                                            / gameObject.GetComponent<Item_Consumable>().maxConsumableAmount * 100;
                    UIReuseScript.txt_ItemRemainder.text = "Remainder: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";
                }
            }
            else if (gameObject.GetComponent<Item_Lockpick>() != null
                     && gameObject.GetComponent<Item_Lockpick>().itemType
                     == Item_Lockpick.ItemType.lockpick)
            {
                float finalPercentage = gameObject.GetComponent<Item_Lockpick>().lockpickDurability
                                        / gameObject.GetComponent<Item_Lockpick>().maxLockpickDurability * 100;
                UIReuseScript.txt_ItemRemainder.text = "Remainder: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }
            else if (gameObject.GetComponent<Item_Gun>() != null
                     && gameObject.GetComponent<Item_Consumable>() == null)
            {
                float durability = gameObject.GetComponent<Item_Gun>().durability;
                float maxDurability = gameObject.GetComponent<Item_Gun>().maxDurability;

                float finalPercentage = durability / maxDurability * 100;
                UIReuseScript.txt_ItemDurability.text = "Durability: "
                + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }

            if (PlayerInventoryScript.money >= int_ItemValue)
            {
                UIReuseScript.btn_BuyItem.gameObject.SetActive(true);
                UIReuseScript.btn_BuyItem.interactable = true;
                UIReuseScript.btn_BuyItem.onClick.AddListener(Buy);
            }
            else if (PlayerInventoryScript.money < int_ItemValue)
            {
                UIReuseScript.txt_tooExpensive.gameObject.SetActive(true);
            }

            if (!isStackable)
            {
                UIReuseScript.txt_notStackable.gameObject.SetActive(true);
            }
            //Debug.Log("Buying from str_traderName!");
        }
        //if is selling to npc
        else if (PlayerInventoryScript.isPlayerAndTraderOpen
                && isInPlayerInventory
                && !isBuying)
        {
            UIReuseScript.txt_InventoryName.text = "Player inventory";
            UIReuseScript.RebuildPlayerInventory();

            int_quarterPrice = Mathf.FloorToInt(int_ItemValue / 4);
            int_finalPrice = int_quarterPrice * 3;
            UIReuseScript.txt_ItemValue.text = int_finalPrice.ToString();

            UIReuseScript.btn_BuyFromTrader.onClick.AddListener(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().OpenShopUI);

            if (gameObject.GetComponent<Item_Gun>() == null
                && gameObject.GetComponent<Item_Consumable>() == null
                && gameObject.GetComponent<Item_Ammo>() != null)
            {
                UIReuseScript.txt_ItemRemainder.text = "Ammo: " + int_itemCount;
            }
            else if (gameObject.GetComponent<Item_Consumable>() != null
                     && gameObject.GetComponent<Item_Gun>() == null)
            {
                if (gameObject.GetComponent<Item_Consumable>().consumableType == Item_Consumable.ConsumableType.Repairkit
                    || gameObject.GetComponent<Item_Consumable>().consumableType == Item_Consumable.ConsumableType.Healthkit)
                {
                    float finalPercentage = gameObject.GetComponent<Item_Consumable>().currentConsumableAmount
                                            / gameObject.GetComponent<Item_Consumable>().maxConsumableAmount * 100;
                    UIReuseScript.txt_ItemRemainder.text = "Remainder: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";
                }
            }
            else if (gameObject.GetComponent<Item_Lockpick>() != null
                     && gameObject.GetComponent<Item_Lockpick>().itemType
                     == Item_Lockpick.ItemType.lockpick)
            {
                float finalPercentage = gameObject.GetComponent<Item_Lockpick>().lockpickDurability
                                        / gameObject.GetComponent<Item_Lockpick>().maxLockpickDurability * 100;
                UIReuseScript.txt_ItemRemainder.text = "Remainder: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }
            else if (gameObject.GetComponent<Item_Gun>() != null
                     && gameObject.GetComponent<Item_Consumable>() == null)
            {
                float durability = gameObject.GetComponent<Item_Gun>().durability;
                float maxDurability = gameObject.GetComponent<Item_Gun>().maxDurability;
                UIReuseScript.txt_ItemDurability.text = "Durability: "
                + Mathf.FloorToInt(durability / maxDurability * 100).ToString() + "%";
            }

            if (!isProtected)
            {
                UIReuseScript.btn_SellItem.gameObject.SetActive(true);
                UIReuseScript.btn_SellItem.interactable = true;
                UIReuseScript.btn_SellItem.onClick.AddListener(Sell);
            }
            else if (isProtected)
            {
                UIReuseScript.txt_protected.gameObject.SetActive(true);
            }

            if (!isStackable)
            {
                UIReuseScript.txt_notStackable.gameObject.SetActive(true);
            }
            //Debug.Log("Selling to str_traderName!");
        }
        //if is in repair menu
        if (PlayerInventoryScript.isPlayerAndRepairOpen
            && isInRepairMenu)
        {
            UIReuseScript.RebuildRepairMenu();

            //trader repairs with money
            if (PlayerInventoryScript.Trader != null
                && PlayerInventoryScript.Workbench == null)
            {
                float maxRepairPrice = gameObject.GetComponent<Item_Gun>().maxRepairPrice;
                float priceToRepairOnePercent = Mathf.FloorToInt(maxRepairPrice / 100);

                UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName + "(s) repair shop";

                if (gameObject.GetComponent<Item_Gun>() != null
                    && gameObject.GetComponent<Item_Gun>().durability
                    < gameObject.GetComponent<Item_Gun>().maxDurability)
                {
                    //looks for money in player inv and only enables repair button
                    //in trader repair menu if player actually
                    //has enough money to repair 1% or more of this item
                    if (PlayerInventoryScript.money >= priceToRepairOnePercent)
                    {
                        UIReuseScript.btn_Repair.gameObject.SetActive(true);
                        UIReuseScript.btn_Repair.interactable = true;
                        UIReuseScript.btn_Repair.onClick.AddListener(RepairWithMoney);
                    }
                    else
                    {
                        UIReuseScript.txt_tooExpensive.gameObject.SetActive(true);
                    }
                }
            }
            //workbench repairs with repair kits
            else if (PlayerInventoryScript.Trader == null
                     && PlayerInventoryScript.Workbench != null)
            {
                UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Workbench.GetComponent<Env_Workbench>().str_workbenchName;

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
                            UIReuseScript.btn_Repair.gameObject.SetActive(true);
                            UIReuseScript.btn_Repair.interactable = true;
                            item.GetComponent<Item_Consumable>().item = gameObject;
                            UIReuseScript.btn_Repair.onClick.AddListener(item.GetComponent<Item_Consumable>().Consume);
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
                UIReuseScript.txt_ItemDurability.text = "Durability: " + Mathf.FloorToInt(finalPercentage).ToString() + "%";
            }
            if (!isStackable)
            {
                UIReuseScript.txt_notStackable.gameObject.SetActive(true);
            }
            if (isProtected)
            {
                UIReuseScript.txt_protected.gameObject.SetActive(true);
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

            UIReuseScript.ClearStatsUI();
            UIReuseScript.ClearInventoryUI();
            RemoveListeners();
            UIReuseScript.RebuildRepairMenu();
            UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName + "(s) repair shop";

            PlayerInventoryScript.UpdatePlayerInventoryStats();
            UIReuseScript.durability = gameObject.GetComponent<Item_Gun>().maxDurability;
            UIReuseScript.maxDurability = gameObject.GetComponent<Item_Gun>().maxDurability;
            UIReuseScript.UpdateWeaponQuality();

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

            UIReuseScript.ClearStatsUI();
            UIReuseScript.ClearInventoryUI();
            RemoveListeners();
            UIReuseScript.RebuildRepairMenu();
            UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName + "(s) repair shop";

            PlayerInventoryScript.UpdatePlayerInventoryStats();
            UIReuseScript.durability = gameObject.GetComponent<Item_Gun>().durability;
            UIReuseScript.maxDurability = gameObject.GetComponent<Item_Gun>().maxDurability;
            UIReuseScript.UpdateWeaponQuality();

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

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    RemoveListeners();
                    UIReuseScript.RebuildPlayerInventory();

                    UIReuseScript.InteractUIDisabled();

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

                    UIReuseScript.InteractUIDisabled();

                    PlayerInventoryScript.inventory.Add(gameObject);
                    PlayerInventoryScript.invSpace -= int_ItemWeight;

                    ConsoleScript.playeritemnames.Add(str_ItemName);

                    if (ConsoleScript.currentCell != null
                        && ConsoleScript.currentCell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                    {
                        ConsoleScript.currentCell.GetComponent<Manager_CurrentCell>().items.Remove(gameObject);
                    }
                    else if (ConsoleScript.currentCell == null
                             && !ConsoleScript.lastCell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                    {
                        ConsoleScript.lastCell.GetComponent<Manager_CurrentCell>().items.Remove(gameObject);
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

            UIReuseScript.InteractUIDisabled();

            if (!PauseMenuScript.isGamePaused)
            {
                PauseMenuScript.PauseGame();
            }

            UIReuseScript.par_ItemCount.SetActive(true);
            UIReuseScript.itemCountSlider.maxValue = int_itemCount;

            UIReuseScript.txt_CountInfo.text = "Picking up " + str_ItemName + "(s) ...";
            UIReuseScript.itemCountSlider.onValueChanged.AddListener(SliderValue);
            UIReuseScript.txt_CountValue.text = "Selected count: 1/" + int_itemCount;
            UIReuseScript.txt_SliderInfo.text = "Total weight removed: " + int_ItemWeight;
            UIReuseScript.btn_ConfirmCount.onClick.AddListener(ConfirmCount);
            UIReuseScript.btn_CancelCount.onClick.AddListener(CancelCount);
        }
        else if (!isStackable)
        {
            if (int_finalSpace >= 0)
            {
                theItem = gameObject;

                UIReuseScript.InteractUIDisabled();

                PlayerInventoryScript.inventory.Add(gameObject);
                PlayerInventoryScript.invSpace -= int_ItemWeight;

                ConsoleScript.playeritemnames.Add(str_ItemName);

                if (ConsoleScript.currentCell != null
                    && ConsoleScript.currentCell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                {
                    ConsoleScript.currentCell.GetComponent<Manager_CurrentCell>().items.Remove(gameObject);
                }
                else if (ConsoleScript.currentCell == null
                         && !ConsoleScript.lastCell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                {
                    ConsoleScript.lastCell.GetComponent<Manager_CurrentCell>().items.Remove(gameObject);
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
                //this is only called if the gameobject is the Exoskeleton
                if (str_ItemName == "Exoskeleton")
                {
                    PlayerMenuScript.isExoskeletonEquipped = true;
                    AbilityAssignManagerScript.hasExosuit = true;
                    UIReuseScript.ShowExoskeletonUI();
                }

                PlayerInventoryScript.UpdatePlayerInventoryStats();

                UIReuseScript.InteractUIDisabled();
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

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    RemoveListeners();
                    UIReuseScript.RebuildShopInventory();

                    UIReuseScript.btn_SellToTrader.onClick.AddListener(PlayerInventoryScript.CloseShop);

                    UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().str_ShopName;

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

                    ConsoleScript.playeritemnames.Add(str_ItemName);

                    gameObject.transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;
                    GameObject pressedButton = EventSystem.current.currentSelectedGameObject;

                    PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().buttons.Remove(pressedButton);

                    pressedButton.SetActive(false);
                    isInPlayerInventory = true;
                    isInTraderShop = false;

                    gameObject.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    RemoveListeners();
                    UIReuseScript.RebuildShopInventory();

                    UIReuseScript.btn_SellToTrader.onClick.AddListener(PlayerInventoryScript.CloseShop);

                    UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().str_ShopName;

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

            UIReuseScript.par_ItemCount.SetActive(true);
            UIReuseScript.itemCountSlider.maxValue = int_itemCount;

            str_traderName = PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName;
            UIReuseScript.txt_CountInfo.text = "Buying " + str_ItemName + "(s) from " + str_traderName + "...";
            UIReuseScript.itemCountSlider.onValueChanged.AddListener(SliderValue);
            UIReuseScript.txt_CountValue.text = "Selected count: 1/" + int_itemCount;
            UIReuseScript.txt_SliderInfo.text = "Total weight removed: " + int_ItemWeight + " Total money removed: " + int_ItemValue;
            UIReuseScript.btn_ConfirmCount.onClick.AddListener(ConfirmCount);
            UIReuseScript.btn_CancelCount.onClick.AddListener(CancelCount);
        }
        else if (!isStackable)
        {
            PlayerInventoryScript.inventory.Add(gameObject);
            PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().inventory.Remove(gameObject);
            PlayerInventoryScript.invSpace -= int_ItemWeight;
            PlayerInventoryScript.money -= int_ItemValue;

            ConsoleScript.playeritemnames.Add(str_ItemName);

            gameObject.transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;
            GameObject pressedButton = EventSystem.current.currentSelectedGameObject;

            PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().buttons.Remove(pressedButton);

            pressedButton.SetActive(false);
            isInPlayerInventory = true;
            isInTraderShop = false;

            gameObject.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);

            UIReuseScript.ClearStatsUI();
            UIReuseScript.ClearInventoryUI();
            RemoveListeners();
            UIReuseScript.RebuildShopInventory();

            UIReuseScript.btn_SellToTrader.onClick.AddListener(PlayerInventoryScript.CloseShop);

            UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().str_ShopName;

            foundDuplicate = false;
            duplicate = null;

            PlayerInventoryScript.UpdatePlayerInventoryStats();

            str_traderName = PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName;
            //Debug.Log("Bought one non-stackable " + str_ItemName + " from " + str_traderName + " for " + int_ItemValue.ToString() + "! Removed " + int_ItemWeight.ToString() + " space from players inventory.");
        }

        if (!isInRepairMenu)
        {
            UIReuseScript.EnableInventorySortButtons();
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
                && selectedGun.GetComponent<Item_Gun>().ammoType.ToString() == gameObject.GetComponent<Item_Ammo>().ammoType.ToString()
                && selectedGun.GetComponent<Item_Gun>().isReloading)
            {
                Debug.Log("Error: Can't sell " + gameObject.GetComponent<Env_Item>().str_ItemName + " while " + selectedGun.GetComponent<Env_Item>().str_ItemName + " is reloading!");
                canContinue = false;
            }
            else if (selectedGun == null
                     || gameObject.GetComponent<Item_Ammo>() == null
                     || selectedGun.GetComponent<Item_Gun>().ammoType.ToString() != gameObject.GetComponent<Item_Ammo>().ammoType.ToString()
                     || !selectedGun.GetComponent<Item_Gun>().isReloading)
            {
                canContinue = true;
            }
        }

        if (canContinue)
        {
            int_quarterPrice = Mathf.FloorToInt(int_ItemValue / 4);
            int_singlePrice = int_quarterPrice * 3;
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

                    ConsoleScript.playeritemnames.Remove(str_ItemName);

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    RemoveListeners();
                    UIReuseScript.RebuildPlayerInventory();

                    UIReuseScript.btn_BuyFromTrader.onClick.AddListener(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().OpenShopUI);

                    UIReuseScript.txt_InventoryName.text = "Player inventory";

                    UIReuseScript.InteractUIDisabled();

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

                    ConsoleScript.playeritemnames.Remove(str_ItemName);

                    Vector3 pos_container = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().par_TraderItems.transform.position;
                    gameObject.transform.position = pos_container;
                    gameObject.transform.SetParent(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().par_TraderItems.transform);

                    isInPlayerInventory = false;
                    isInTraderShop = true;

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    RemoveListeners();
                    UIReuseScript.RebuildPlayerInventory();

                    UIReuseScript.btn_BuyFromTrader.onClick.AddListener(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().OpenShopUI);

                    UIReuseScript.txt_InventoryName.text = "Player inventory";

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

                UIReuseScript.par_ItemCount.SetActive(true);
                UIReuseScript.itemCountSlider.maxValue = int_itemCount;

                str_traderName = PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName;
                UIReuseScript.txt_CountInfo.text = "Selling " + str_ItemName + "(s) to " + str_traderName + "...";
                UIReuseScript.itemCountSlider.onValueChanged.AddListener(SliderValue);
                UIReuseScript.txt_CountValue.text = "Selected count: 1/" + int_itemCount;
                UIReuseScript.txt_SliderInfo.text = "Total weight added: " + int_ItemWeight + " Total money added: " + int_singlePrice;
                UIReuseScript.btn_ConfirmCount.onClick.AddListener(ConfirmCount);
                UIReuseScript.btn_CancelCount.onClick.AddListener(CancelCount);
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

                ConsoleScript.playeritemnames.Remove(str_ItemName);

                Vector3 pos_container = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().par_TraderItems.transform.position;
                gameObject.transform.position = pos_container;
                gameObject.transform.SetParent(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().par_TraderItems.transform);

                isInPlayerInventory = false;
                isInTraderShop = true;

                UIReuseScript.ClearStatsUI();
                UIReuseScript.ClearInventoryUI();
                RemoveListeners();
                UIReuseScript.RebuildPlayerInventory();

                UIReuseScript.btn_BuyFromTrader.onClick.AddListener(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().OpenShopUI);

                UIReuseScript.txt_InventoryName.text = "Player inventory";

                foundDuplicate = false;
                duplicate = null;

                PlayerInventoryScript.UpdatePlayerInventoryStats();

                //Debug.Log("Sold one non-stackable " + str_ItemName + " to " + str_traderName + " for " + int_singlePrice.ToString() + "! Added " + int_ItemWeight.ToString() + " space back to players inventory.");
            }
        }
        canContinue = true;

        if (!isInRepairMenu)
        {
            UIReuseScript.EnableInventorySortButtons();
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

            UIReuseScript.ClearStatsUI();
            UIReuseScript.ClearInventoryUI();
            RemoveListeners();
            UIReuseScript.RebuildContainerInventory();

            UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";

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

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    RemoveListeners();
                    UIReuseScript.RebuildContainerInventory();

                    UIReuseScript.btn_PlaceIntoContainer.onClick.AddListener(PlayerInventoryScript.CloseContainer);

                    UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";

                    UIReuseScript.InteractUIDisabled();

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

                    ConsoleScript.playeritemnames.Add(str_ItemName);

                    gameObject.transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;
                    GameObject pressedButton = EventSystem.current.currentSelectedGameObject;

                    PlayerInventoryScript.Container.GetComponent<Inv_Container>().buttons.Remove(pressedButton);

                    pressedButton.SetActive(false);
                    isInPlayerInventory = true;
                    isInContainer = false;

                    gameObject.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    RemoveListeners();
                    UIReuseScript.RebuildContainerInventory();

                    UIReuseScript.btn_PlaceIntoContainer.onClick.AddListener(PlayerInventoryScript.CloseContainer);

                    UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";

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

            UIReuseScript.par_ItemCount.SetActive(true);
            UIReuseScript.itemCountSlider.maxValue = int_itemCount;

            str_containerName = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName;
            UIReuseScript.txt_CountInfo.text = "Taking " + str_ItemName + "(s) from " + str_containerName + "...";
            UIReuseScript.itemCountSlider.onValueChanged.AddListener(SliderValue);
            UIReuseScript.txt_CountValue.text = "Selected count: 1/" + int_itemCount;
            UIReuseScript.txt_SliderInfo.text = "Total weight removed: " + int_ItemWeight;
            UIReuseScript.btn_ConfirmCount.onClick.AddListener(ConfirmCount);
            UIReuseScript.btn_CancelCount.onClick.AddListener(CancelCount);
        }
        else if (!isStackable)
        {
            PlayerInventoryScript.inventory.Add(gameObject);
            PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory.Remove(gameObject);
            PlayerInventoryScript.invSpace -= int_ItemWeight;

            ConsoleScript.playeritemnames.Add(str_ItemName);

            gameObject.transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;
            GameObject pressedButton = EventSystem.current.currentSelectedGameObject;

            PlayerInventoryScript.Container.GetComponent<Inv_Container>().buttons.Remove(pressedButton);

            pressedButton.SetActive(false);
            isInPlayerInventory = true;
            isInContainer = false;

            gameObject.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);

            UIReuseScript.ClearStatsUI();
            UIReuseScript.ClearInventoryUI();
            RemoveListeners();
            UIReuseScript.RebuildContainerInventory();

            UIReuseScript.btn_PlaceIntoContainer.onClick.AddListener(PlayerInventoryScript.CloseContainer);

            UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";

            foundDuplicate = false;
            duplicate = null;

            PlayerInventoryScript.UpdatePlayerInventoryStats();

            str_containerName = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName;
            //Debug.Log("Took one non-stackable " + str_ItemName + " from " + str_containerName + "! Removed " + int_ItemWeight.ToString() + " space from players inventory.");
        }

        if (!isInRepairMenu)
        {
            UIReuseScript.EnableInventorySortButtons();
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
                && selectedGun.GetComponent<Item_Gun>().ammoType.ToString() == gameObject.GetComponent<Item_Ammo>().ammoType.ToString()
                && selectedGun.GetComponent<Item_Gun>().isReloading)
            {
                Debug.Log("Error: Can't place " + gameObject.GetComponent<Env_Item>().str_ItemName + " to container while " + selectedGun.GetComponent<Env_Item>().str_ItemName + " is reloading!");
                canContinue = false;
            }
            else if (selectedGun == null
                     || gameObject.GetComponent<Item_Ammo>() == null
                     || selectedGun.GetComponent<Item_Gun>().ammoType.ToString() != gameObject.GetComponent<Item_Ammo>().ammoType.ToString()
                     || !selectedGun.GetComponent<Item_Gun>().isReloading)
            {
                canContinue = true;
            }

            if (!isInRepairMenu)
            {
                UIReuseScript.EnableInventorySortButtons();
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

                    ConsoleScript.playeritemnames.Remove(str_ItemName);

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    RemoveListeners();
                    UIReuseScript.RebuildPlayerInventory();

                    UIReuseScript.btn_TakeFromContainer.onClick.AddListener(PlayerInventoryScript.Container.GetComponent<Inv_Container>().CheckIfLocked);

                    UIReuseScript.txt_InventoryName.text = "Player inventory";

                    UIReuseScript.InteractUIDisabled();

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

                    ConsoleScript.playeritemnames.Remove(str_ItemName);

                    Vector3 pos_container = PlayerInventoryScript.Container.GetComponent<Inv_Container>().par_ContainerItems.transform.position;
                    gameObject.transform.position = pos_container;
                    gameObject.transform.SetParent(PlayerInventoryScript.Container.GetComponent<Inv_Container>().par_ContainerItems.transform);

                    isInPlayerInventory = false;
                    isInContainer = true;

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    RemoveListeners();
                    UIReuseScript.RebuildPlayerInventory();

                    UIReuseScript.btn_TakeFromContainer.onClick.AddListener(PlayerInventoryScript.Container.GetComponent<Inv_Container>().CheckIfLocked);

                    UIReuseScript.txt_InventoryName.text = "Player inventory";

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

                UIReuseScript.par_ItemCount.SetActive(true);
                UIReuseScript.itemCountSlider.maxValue = int_itemCount;

                str_containerName = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName;
                UIReuseScript.txt_CountInfo.text = "Placing " + str_ItemName + "(s) to " + str_containerName + "...";
                UIReuseScript.itemCountSlider.onValueChanged.AddListener(SliderValue);
                UIReuseScript.txt_CountValue.text = "Selected count: 1/" + int_itemCount;
                UIReuseScript.txt_SliderInfo.text = "Total weight added: " + int_ItemWeight;
                UIReuseScript.btn_ConfirmCount.onClick.AddListener(ConfirmCount);
                UIReuseScript.btn_CancelCount.onClick.AddListener(CancelCount);
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

                ConsoleScript.playeritemnames.Remove(str_ItemName);

                Vector3 pos_container = PlayerInventoryScript.Container.GetComponent<Inv_Container>().par_ContainerItems.transform.position;
                gameObject.transform.position = pos_container;
                gameObject.transform.SetParent(PlayerInventoryScript.Container.GetComponent<Inv_Container>().par_ContainerItems.transform);

                isInPlayerInventory = false;
                isInContainer = true;

                UIReuseScript.ClearStatsUI();
                UIReuseScript.ClearInventoryUI();
                RemoveListeners();
                UIReuseScript.RebuildPlayerInventory();

                UIReuseScript.btn_TakeFromContainer.onClick.AddListener(PlayerInventoryScript.Container.GetComponent<Inv_Container>().CheckIfLocked);

                UIReuseScript.txt_InventoryName.text = "Player inventory";

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
            UIReuseScript.EnableInventorySortButtons();
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
                && selectedGun.GetComponent<Item_Gun>().ammoType.ToString() == gameObject.GetComponent<Item_Ammo>().ammoType.ToString()
                && selectedGun.GetComponent<Item_Gun>().isReloading)
            {
                Debug.Log("Error: Can't drop " + gameObject.GetComponent<Env_Item>().str_ItemName + " while " + selectedGun.GetComponent<Env_Item>().str_ItemName + " is reloading!");
                canContinue = false;
            }
            else if (selectedGun == null
                     || gameObject.GetComponent<Item_Ammo>() == null
                     || selectedGun.GetComponent<Item_Gun>().ammoType.ToString() != gameObject.GetComponent<Item_Ammo>().ammoType.ToString()
                     || !selectedGun.GetComponent<Item_Gun>().isReloading)
            {
                canContinue = true;
            }

            if (!isInRepairMenu)
            {
                UIReuseScript.EnableInventorySortButtons();
            }
        }

        if (canContinue)
        {
            checkForCollision = true;

            if (finishedCollisionCheck)
            {
                if (!isColliding)
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

                        ConsoleScript.playeritemnames.Remove(str_ItemName);

                        gameObject.transform.position = pos_HoldItem.position;

                        droppedObject = true;
                        time = 0;

                        if (ConsoleScript.currentCell != null
                            && !ConsoleScript.currentCell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                        {
                            ConsoleScript.currentCell.GetComponent<Manager_CurrentCell>().items.Add(gameObject);
                        }
                        gameObject.transform.parent = par_DroppedItems.transform;

                        isInPlayerInventory = false;

                        UIReuseScript.ClearStatsUI();
                        UIReuseScript.ClearInventoryUI();
                        RemoveListeners();
                        UIReuseScript.RebuildPlayerInventory();

                        UIReuseScript.txt_InventoryName.text = "Player inventory";

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

                        UIReuseScript.par_ItemCount.SetActive(true);
                        UIReuseScript.itemCountSlider.maxValue = int_itemCount;

                        UIReuseScript.txt_CountInfo.text = "Dropping " + str_ItemName + "(s) from players inventory...";
                        UIReuseScript.itemCountSlider.onValueChanged.AddListener(SliderValue);
                        UIReuseScript.txt_CountValue.text = "Selected count: 1/" + int_itemCount;
                        UIReuseScript.txt_SliderInfo.text = "Total weight added: " + int_ItemWeight;
                        UIReuseScript.btn_ConfirmCount.onClick.AddListener(ConfirmCount);
                        UIReuseScript.btn_CancelCount.onClick.AddListener(CancelCount);

                        foundDuplicate = false;
                        duplicate = null;
                    }
                }
                else if (isColliding)
                {
                    foundDuplicate = false;
                    duplicate = null;

                    Debug.LogWarning("Error: Cannot drop " + str_ItemName + "... Something is in the way!");
                }

                finishedCollisionCheck = false;
            }
        }
        canContinue = true;

        if (!isInRepairMenu)
        {
            UIReuseScript.EnableInventorySortButtons();
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
                && selectedGun.GetComponent<Item_Gun>().ammoType.ToString() == gameObject.GetComponent<Item_Ammo>().ammoType.ToString()
                && selectedGun.GetComponent<Item_Gun>().isReloading)
            {
                Debug.Log("Error: Can't destroy " + gameObject.GetComponent<Env_Item>().str_ItemName + " while " + selectedGun.GetComponent<Env_Item>().str_ItemName + " is reloading!");
                canContinue = false;
            }
            else if (selectedGun == null
                     || gameObject.GetComponent<Item_Ammo>() == null
                     || selectedGun.GetComponent<Item_Gun>().ammoType.ToString() != gameObject.GetComponent<Item_Ammo>().ammoType.ToString()
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

                ConsoleScript.playeritemnames.Remove(str_ItemName);

                isInPlayerInventory = false;

                UIReuseScript.ClearStatsUI();
                UIReuseScript.ClearInventoryUI();
                RemoveListeners();
                UIReuseScript.RebuildPlayerInventory();

                UIReuseScript.txt_InventoryName.text = "Player inventory";

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

                UIReuseScript.par_ItemCount.SetActive(true);
                UIReuseScript.itemCountSlider.maxValue = int_itemCount;

                UIReuseScript.txt_CountInfo.text = "Destroying " + str_ItemName + "(s) from players inventory...";
                UIReuseScript.itemCountSlider.onValueChanged.AddListener(SliderValue);
                UIReuseScript.txt_CountValue.text = "Selected count: 1/" + int_itemCount;
                UIReuseScript.txt_SliderInfo.text = "Total weight added: " + int_ItemWeight;
                UIReuseScript.btn_ConfirmCount.onClick.AddListener(ConfirmCount);
                UIReuseScript.btn_CancelCount.onClick.AddListener(CancelCount);
            }
        }
        canContinue = true;

        if (!isInRepairMenu)
        {
            UIReuseScript.EnableInventorySortButtons();
        }
    }

    public void SliderValue(float value)
    {
        int_selectedCount = Mathf.FloorToInt(value);
        UIReuseScript.txt_CountValue.text = "Selected count: " + int_selectedCount.ToString() + "/" + int_itemCount;

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
            UIReuseScript.txt_SliderInfo.text = "Total weight removed: " + int_totalSpace;
        }
        if (isPlacingMultipleItems
            || isDroppingMultipleItems
            || isDestroyingMultipleItems
            || isSellingMultipleItems)
        {
            UIReuseScript.txt_SliderInfo.text = "Total weight added: " + int_totalSpace;
        }
        if (isBuyingMultipleItems)
        {
            int_totalSpace = int_ItemWeight * int_selectedCount;
            int_finalPrice = int_ItemValue * int_selectedCount;
            UIReuseScript.txt_SliderInfo.text = UIReuseScript.txt_SliderInfo.text + " Total money taken: " + int_finalPrice;
        }
        if (isSellingMultipleItems)
        {
            int_totalSpace = int_ItemWeight * int_selectedCount;
            int_quarterPrice = Mathf.FloorToInt(int_ItemValue / 4);
            int_singlePrice = int_quarterPrice * 3;
            int_finalPrice = int_singlePrice * int_selectedCount;
            UIReuseScript.txt_SliderInfo.text = UIReuseScript.txt_SliderInfo.text + " Total money added: " + int_finalPrice;
        }
        //Debug.Log(int_selectedCount);

        if (isPickingUpMultipleItems || isTakingMultipleItems)
        {
            if (int_selectedCount * int_ItemWeight <= PlayerInventoryScript.invSpace)
            {
                UIReuseScript.txt_CountValue.color = Color.white;
                UIReuseScript.btn_ConfirmCount.interactable = true;
            }
            else if (int_selectedCount * int_ItemWeight > PlayerInventoryScript.invSpace)
            {
                UIReuseScript.txt_CountValue.color = Color.red;
                UIReuseScript.btn_ConfirmCount.interactable = false;
                //Debug.Log("Slider not interactable...");
            }
        }
        else if (isBuyingMultipleItems)
        {
            if (int_selectedCount * int_ItemValue <= PlayerInventoryScript.money)
            {
                UIReuseScript.txt_CountValue.color = Color.white;
                UIReuseScript.btn_ConfirmCount.interactable = true;
            }
            else if (int_selectedCount * int_ItemValue > PlayerInventoryScript.money)
            {
                UIReuseScript.txt_CountValue.color = Color.red;
                UIReuseScript.btn_ConfirmCount.interactable = false;
                //Debug.Log("Slider not interactable...");
            }
        }
        else
        {
            UIReuseScript.txt_CountValue.color = Color.white;
            UIReuseScript.btn_ConfirmCount.interactable = true;
        }
    }

    public void ConfirmCount()
    {
        int_confirmedCount = Mathf.FloorToInt(UIReuseScript.itemCountSlider.value);
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

                        UIReuseScript.ClearStatsUI();
                        UIReuseScript.ClearInventoryUI();
                        RemoveListeners();
                        UIReuseScript.RebuildShopInventory();
                        UIReuseScript.ClearCountSliderUI();

                        UIReuseScript.btn_SellToTrader.onClick.AddListener(PlayerInventoryScript.CloseShop);

                        UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().str_ShopName;

                        ConsoleScript.playeritemnames.Add(str_ItemName);

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

                        ConsoleScript.playeritemnames.Add(str_ItemName);

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
                        GameObject newDuplicate = Instantiate(gameObject, playerInvPos, Quaternion.identity);

                        PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().inventory.Remove(newDuplicate);
                        PlayerInventoryScript.inventory.Add(newDuplicate);

                        PlayerInventoryScript.invSpace -= (int_ItemWeight * int_confirmedCount);
                        int_itemCount -= int_confirmedCount;

                        newDuplicate.name = newDuplicate.GetComponent<Env_Item>().str_ItemName;
                        newDuplicate.GetComponent<Env_Item>().int_itemCount = int_confirmedCount;
                        newDuplicate.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);
                        newDuplicate.GetComponent<Env_Item>().isInPlayerInventory = true;
                        newDuplicate.GetComponent<Env_Item>().isInTraderShop = false;

                        if (gameObject.GetComponent<Item_Ammo>() != null)
                        {
                            UpdateGunsAndAmmo();
                        }
                    }
                }

                UIReuseScript.ClearStatsUI();
                UIReuseScript.ClearAllInventories();
                UIReuseScript.ClearInventoryUI();
                RemoveListeners();
                UIReuseScript.RebuildShopInventory();
                UIReuseScript.ClearCountSliderUI();

                int_finalPrice = int_ItemValue * int_confirmedCount;
                str_traderName = PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName;
                int_totalSpace = int_ItemWeight * int_confirmedCount;
                PlayerInventoryScript.money -= int_finalPrice;
                PlayerInventoryScript.invSpace -= int_totalSpace;

                UIReuseScript.btn_SellToTrader.onClick.AddListener(PlayerInventoryScript.CloseShop);

                UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().str_ShopName;

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
                UIReuseScript.EnableInventorySortButtons();
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
                    ConsoleScript.playeritemnames.Remove(str_ItemName);

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    RemoveListeners();
                    UIReuseScript.RebuildPlayerInventory();
                    UIReuseScript.ClearCountSliderUI();

                    UIReuseScript.btn_TakeFromContainer.onClick.AddListener(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().OpenShopUI);

                    UIReuseScript.txt_InventoryName.text = "Player inventory";

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

                    ConsoleScript.playeritemnames.Remove(str_ItemName);
                }
                else if (int_confirmedCount < int_itemCount)
                {
                    Vector3 pos_container = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().par_TraderItems.transform.position;
                    GameObject newDuplicate = Instantiate(gameObject, pos_container, Quaternion.identity);
                    newDuplicate.transform.position = pos_container;

                    PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().inventory.Add(newDuplicate);
                    PlayerInventoryScript.inventory.Remove(newDuplicate);
                    int_itemCount -= int_confirmedCount;
                    PlayerInventoryScript.invSpace += (int_ItemWeight * int_confirmedCount);

                    newDuplicate.name = newDuplicate.GetComponent<Env_Item>().str_ItemName;
                    newDuplicate.GetComponent<Env_Item>().int_itemCount = int_confirmedCount;
                    newDuplicate.transform.SetParent(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().par_TraderItems.transform);
                    newDuplicate.GetComponent<Env_Item>().isInPlayerInventory = false;
                    newDuplicate.GetComponent<Env_Item>().isInTraderShop = true;

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        UpdateGunsAndAmmo();
                    }
                }
            }

            UIReuseScript.ClearStatsUI();
            UIReuseScript.ClearInventoryUI();
            RemoveListeners();
            UIReuseScript.RebuildPlayerInventory();
            UIReuseScript.ClearCountSliderUI();

            int_quarterPrice = Mathf.FloorToInt(int_ItemValue / 4);
            int_singlePrice = int_quarterPrice * 3;
            int_finalPrice = int_singlePrice * int_confirmedCount;
            str_traderName = PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName;
            int_totalSpace = int_ItemWeight * int_confirmedCount;
            PlayerInventoryScript.money += int_finalPrice;

            UIReuseScript.btn_BuyFromTrader.onClick.AddListener(PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().OpenShopUI);

            UIReuseScript.txt_InventoryName.text = "Player inventory";

            foundDuplicate = false;
            duplicate = null;

            PlayerInventoryScript.UpdatePlayerInventoryStats();
            if (!isInRepairMenu)
            {
                UIReuseScript.EnableInventorySortButtons();
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

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    RemoveListeners();
                    UIReuseScript.RebuildPlayerInventory();
                    UIReuseScript.ClearCountSliderUI();

                    ConsoleScript.playeritemnames.Add(str_ItemName);

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

                        if (ConsoleScript.currentCell != null
                            && ConsoleScript.currentCell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                        {
                            ConsoleScript.currentCell.GetComponent<Manager_CurrentCell>().items.Remove(gameObject);
                        }
                        else if (ConsoleScript.currentCell == null
                                && ConsoleScript.lastCell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                        {
                            ConsoleScript.lastCell.GetComponent<Manager_CurrentCell>().items.Remove(gameObject);
                        }

                        UIReuseScript.InteractUIDisabled();
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

                        if (ConsoleScript.currentCell != null
                            && ConsoleScript.currentCell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                        {
                            ConsoleScript.currentCell.GetComponent<Manager_CurrentCell>().items.Remove(gameObject);
                        }
                        else if (ConsoleScript.currentCell == null
                                && ConsoleScript.lastCell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                        {
                            ConsoleScript.lastCell.GetComponent<Manager_CurrentCell>().items.Remove(gameObject);
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
                        GameObject newDuplicate = Instantiate(gameObject, PlayerInventoryScript.par_PlayerItems.transform.position, Quaternion.identity);
                        theItem = newDuplicate;

                        PlayerInventoryScript.inventory.Add(newDuplicate);
                        PlayerInventoryScript.invSpace -= (int_ItemWeight * int_confirmedCount);

                        int_itemCount -= int_confirmedCount;
                        newDuplicate.name = newDuplicate.GetComponent<Env_Item>().str_ItemName;
                        newDuplicate.GetComponent<Env_Item>().int_itemCount = int_confirmedCount;
                        newDuplicate.GetComponent<Env_Item>().isInPlayerInventory = true;
                        newDuplicate.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);

                        if (ConsoleScript.currentCell != null
                            && ConsoleScript.currentCell.GetComponent<Manager_CurrentCell>().items.Contains(theItem))
                        {
                            ConsoleScript.currentCell.GetComponent<Manager_CurrentCell>().items.Remove(theItem);
                        }
                        else if (ConsoleScript.currentCell == null
                                 && ConsoleScript.lastCell.GetComponent<Manager_CurrentCell>().items.Contains(theItem))
                        {
                            ConsoleScript.lastCell.GetComponent<Manager_CurrentCell>().items.Remove(theItem);
                        }

                        if (gameObject.GetComponent<Item_Ammo>() != null)
                        {
                            UpdateGunsAndAmmo();
                        }

                        theItem.GetComponent<Env_Item>().DeactivateItem();
                    }

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    RemoveListeners();
                    UIReuseScript.RebuildPlayerInventory();
                    UIReuseScript.ClearCountSliderUI();

                    ConsoleScript.playeritemnames.Add(str_ItemName);

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

            if (PauseMenuScript.isGamePaused)
            {
                PauseMenuScript.UnpauseGame();
            }

            if (!isInRepairMenu)
            {
                UIReuseScript.EnableInventorySortButtons();
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

                        UIReuseScript.ClearStatsUI();
                        UIReuseScript.ClearInventoryUI();
                        RemoveListeners();
                        UIReuseScript.RebuildContainerInventory();
                        UIReuseScript.ClearCountSliderUI();

                        UIReuseScript.btn_PlaceIntoContainer.onClick.AddListener(PlayerInventoryScript.CloseContainer);

                        UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";

                        ConsoleScript.playeritemnames.Add(str_ItemName);

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

                        ConsoleScript.playeritemnames.Add(str_ItemName);

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
                        GameObject newDuplicate = Instantiate(gameObject, playerInvPos, Quaternion.identity);

                        PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory.Remove(newDuplicate);
                        PlayerInventoryScript.inventory.Add(newDuplicate);

                        PlayerInventoryScript.invSpace -= (int_ItemWeight * int_confirmedCount);
                        int_itemCount -= int_confirmedCount;

                        newDuplicate.name = newDuplicate.GetComponent<Env_Item>().str_ItemName;
                        newDuplicate.GetComponent<Env_Item>().int_itemCount = int_confirmedCount;
                        newDuplicate.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);
                        newDuplicate.GetComponent<Env_Item>().isInPlayerInventory = true;
                        newDuplicate.GetComponent<Env_Item>().isInContainer = false;

                        if (gameObject.GetComponent<Item_Ammo>() != null)
                        {
                            UpdateGunsAndAmmo();
                        }
                    }
                }

                UIReuseScript.ClearStatsUI();
                UIReuseScript.ClearInventoryUI();
                RemoveListeners();
                UIReuseScript.RebuildContainerInventory();
                UIReuseScript.ClearCountSliderUI();

                UIReuseScript.btn_PlaceIntoContainer.onClick.AddListener(PlayerInventoryScript.CloseContainer);

                UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";

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
                UIReuseScript.EnableInventorySortButtons();
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
                    ConsoleScript.playeritemnames.Remove(str_ItemName);

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    RemoveListeners();
                    UIReuseScript.RebuildPlayerInventory();
                    UIReuseScript.ClearCountSliderUI();

                    UIReuseScript.btn_TakeFromContainer.onClick.AddListener(PlayerInventoryScript.Container.GetComponent<Inv_Container>().CheckIfLocked);

                    UIReuseScript.txt_InventoryName.text = "Player inventory";

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

                    ConsoleScript.playeritemnames.Remove(str_ItemName);
                }
                else if (int_confirmedCount < int_itemCount)
                {
                    Vector3 pos_container = PlayerInventoryScript.Container.GetComponent<Inv_Container>().par_ContainerItems.transform.position;
                    GameObject newDuplicate = Instantiate(gameObject, pos_container, Quaternion.identity);
                    newDuplicate.transform.position = pos_container;

                    PlayerInventoryScript.Container.GetComponent<Inv_Container>().inventory.Add(newDuplicate);
                    PlayerInventoryScript.inventory.Remove(newDuplicate);
                    int_itemCount -= int_confirmedCount;
                    PlayerInventoryScript.invSpace += (int_ItemWeight * int_confirmedCount);

                    newDuplicate.name = newDuplicate.GetComponent<Env_Item>().str_ItemName;
                    newDuplicate.GetComponent<Env_Item>().int_itemCount = int_confirmedCount;
                    newDuplicate.transform.SetParent(PlayerInventoryScript.Container.GetComponent<Inv_Container>().par_ContainerItems.transform);
                    newDuplicate.GetComponent<Env_Item>().isInPlayerInventory = false;
                    newDuplicate.GetComponent<Env_Item>().isInContainer = true;

                    if (gameObject.GetComponent<Item_Ammo>() != null)
                    {
                        UpdateGunsAndAmmo();
                    }
                }
            }

            UIReuseScript.ClearStatsUI();
            UIReuseScript.ClearInventoryUI();
            RemoveListeners();
            UIReuseScript.RebuildPlayerInventory();
            UIReuseScript.ClearCountSliderUI();

            UIReuseScript.btn_TakeFromContainer.onClick.AddListener(PlayerInventoryScript.Container.GetComponent<Inv_Container>().CheckIfLocked);

            UIReuseScript.txt_InventoryName.text = "Player inventory";

            foundDuplicate = false;
            duplicate = null;

            PlayerInventoryScript.UpdatePlayerInventoryStats();
            if (!isInRepairMenu)
            {
                UIReuseScript.EnableInventorySortButtons();
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

                ConsoleScript.playeritemnames.Remove(str_ItemName);

                gameObject.transform.position = pos_HoldItem.position;

                droppedObject = true;
                time = 0;

                if (ConsoleScript.currentCell != null
                    && !ConsoleScript.currentCell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                {
                    ConsoleScript.currentCell.GetComponent<Manager_CurrentCell>().items.Add(gameObject);
                }
                gameObject.transform.parent = par_DroppedItems.transform;

                isInPlayerInventory = false;

                ActivateItem();
            }

            else if (int_confirmedCount < int_itemCount)
            {
                GameObject duplicate = Instantiate(gameObject, pos_HoldItem.position, Quaternion.identity);
                theItem = duplicate;

                PlayerInventoryScript.invSpace += (int_ItemWeight * int_confirmedCount);
                int_itemCount -= int_confirmedCount;

                theItem.transform.position = pos_HoldItem.position;

                droppedObject = true;
                time = 0;

                if (ConsoleScript.currentCell != null
                    && !ConsoleScript.currentCell.GetComponent<Manager_CurrentCell>().items.Contains(theItem))
                {
                    ConsoleScript.currentCell.GetComponent<Manager_CurrentCell>().items.Add(theItem);
                }
                theItem.transform.parent = par_DroppedItems.transform;

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

            UIReuseScript.ClearStatsUI();
            UIReuseScript.ClearInventoryUI();
            RemoveListeners();
            UIReuseScript.RebuildPlayerInventory();
            UIReuseScript.ClearCountSliderUI();

            UIReuseScript.txt_InventoryName.text = "Player inventory";

            foundDuplicate = false;
            duplicate = null;

            PlayerInventoryScript.UpdatePlayerInventoryStats();
            if (!isInRepairMenu)
            {
                UIReuseScript.EnableInventorySortButtons();
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

                UIReuseScript.ClearStatsUI();
                UIReuseScript.ClearInventoryUI();
                RemoveListeners();
                UIReuseScript.RebuildPlayerInventory();
                UIReuseScript.ClearCountSliderUI();

                UIReuseScript.txt_InventoryName.text = "Player inventory";

                if (gameObject.GetComponent<Item_Ammo>() != null)
                {
                    RemoveAmmotypeFromAllGuns();
                }

                ConsoleScript.playeritemnames.Remove(str_ItemName);

                int_totalSpace = int_ItemWeight * int_confirmedCount;
                //Debug.Log("Destroyed " + int_confirmedCount + " " + str_ItemName + "(s)! Added " + int_totalSpace.ToString() + " space back to players inventory.");

                Destroy(gameObject);
            }

            else if (int_confirmedCount < int_itemCount)
            {
                PlayerInventoryScript.invSpace += int_ItemWeight * int_confirmedCount;
                int_itemCount -= int_confirmedCount;

                UIReuseScript.ClearStatsUI();
                UIReuseScript.ClearInventoryUI();
                RemoveListeners();
                UIReuseScript.RebuildPlayerInventory();
                UIReuseScript.ClearCountSliderUI();

                UIReuseScript.txt_InventoryName.text = "Player inventory";

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
                UIReuseScript.EnableInventorySortButtons();
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
                    && PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().ammoType.ToString()
                    == item.GetComponent<Item_Ammo>().ammoType.ToString())
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
                && gun.GetComponent<Item_Gun>().ammoType.ToString()
                == gameObject.GetComponent<Item_Ammo>().ammoType.ToString())
            {
                gun.GetComponent<Item_Gun>().ammoClip = null;
            }
        }

        UIReuseScript.txt_ammoForGun.text = "0";
    }
    private void UnequipAndUnloadGun()
    {
        UIReuseScript.ClearWeaponUI();
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
                if (gameObject.GetComponent<Item_Gun>().ammoType.ToString()
                    == item.GetComponent<Item_Ammo>().ammoType.ToString())
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
                    if (gameObject.GetComponent<Item_Gun>().ammoType.ToString()
                        == item.GetComponent<Item_Ammo>().ammoType.ToString())
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
                GameObject ammo = Instantiate(ConsoleScript.ammoTemplate, PlayerInventoryScript.transform.position, Quaternion.identity);
                ammo.GetComponent<Env_Item>().int_itemCount = removedAmmo;
                ammo.name = ammo.GetComponent<Env_Item>().str_ItemName;
                PlayerInventoryScript.inventory.Add(ammo);
                ConsoleScript.playeritemnames.Add(ammo.GetComponent<Env_Item>().str_ItemName);

                if (PlayerInventoryScript.isPlayerInventoryOpen)
                {
                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    UIReuseScript.RebuildPlayerInventory();
                    UIReuseScript.txt_InventoryName.text = "Player inventory";
                }
                else if (PlayerInventoryScript.Container != null && PlayerInventoryScript.Container.GetComponent<Inv_Container>().isContainerInventoryOpen)
                {
                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    UIReuseScript.RebuildContainerInventory();
                    UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";
                }
                else if (PlayerInventoryScript.Trader != null && PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().isShopOpen)
                {
                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    UIReuseScript.RebuildShopInventory();
                    UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().str_ShopName;
                }
                else if (PlayerInventoryScript.Trader != null && PlayerInventoryScript.Trader.GetComponent<UI_RepairContent>().isNPCRepairUIOpen)
                {
                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    UIReuseScript.RebuildRepairMenu();
                    UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.GetComponent<UI_AIContent>().str_NPCName + "'s repair shop";
                }
                else if (PlayerInventoryScript.Workbench != null && PlayerInventoryScript.Workbench.GetComponent<Env_Workbench>().isActive)
                {
                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    UIReuseScript.RebuildRepairMenu();
                    UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.GetComponent<Env_Workbench>().str_workbenchName;
                }
                //Debug.Log("Unloaded this gun and added " + removedAmmo + " ammo to new ammo clip in players inventory.");
            }
        }
    }

    public void ActivateItem()
    {
        itemActivated = true;
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
        itemActivated = false;
        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
        gameObject.SetActive(false);
    }

    public void DestroyObject()
    {
        //if player is currently in a cell
        if (ConsoleScript.currentCell != null)
        {
            cell = ConsoleScript.currentCell;
        }
        //if player is not currently in a cell then it looks for last cell
        else if (ConsoleScript.currentCell == null)
        {
            cell = ConsoleScript.lastCell;
        }
        //if the found cell contains the destroyable item then its destroyed
        if (cell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
        {
            cell.GetComponent<Manager_CurrentCell>().items.Remove(gameObject);
        }
        //if the found cell doesnt contain the item then it looks through all the other cells items
        //and tries to destroy it if it finds the same gameobject
        else if (!cell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
        {
            foreach (GameObject gameCell in ConsoleScript.allCells)
            {
                if (gameCell.GetComponent<Manager_CurrentCell>().items.Contains(gameObject))
                {
                    gameCell.GetComponent<Manager_CurrentCell>().items.Remove(gameObject);
                    break;
                }
            }
        }
        UIReuseScript.InteractUIDisabled();
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
            PauseMenuScript.UnpauseGame();
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

        UIReuseScript.ClearCountSliderUI();
    }

    public void RemoveListeners()
    {
        UIReuseScript.btn_BuyItem.onClick.RemoveAllListeners();
        UIReuseScript.btn_SellItem.onClick.RemoveAllListeners();
        UIReuseScript.btn_Take.onClick.RemoveAllListeners();
        UIReuseScript.btn_Place.onClick.RemoveAllListeners();
        UIReuseScript.btn_Drop.onClick.RemoveAllListeners();
        UIReuseScript.btn_Destroy.onClick.RemoveAllListeners();
        UIReuseScript.btn_ConfirmCount.onClick.RemoveAllListeners();
        UIReuseScript.btn_CancelCount.onClick.RemoveAllListeners();
        UIReuseScript.btn_Equip.onClick.RemoveAllListeners();
        UIReuseScript.btn_Unequip.onClick.RemoveAllListeners();
        UIReuseScript.btn_Consume.onClick.RemoveAllListeners();
    }
}