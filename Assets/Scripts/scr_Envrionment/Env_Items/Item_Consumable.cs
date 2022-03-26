using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Consumable : MonoBehaviour
{
    [Header("Medicine, health kit, repair kit")]
    [Tooltip("How much can this item be used?")]
    public float maxConsumableAmount;
    public ConsumableType consumableType;
    public enum ConsumableType
    {
        unassigned,
        Repairkit,
        Food,
        Healthkit,
        Medicine
    }
    public RepairKitType repairKitType;
    public enum RepairKitType
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

    [Header("Assignables")]
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private Player_Health PlayerHealthScript;
    [SerializeField] private Manager_Console ConsoleScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;

    //public but hidden variables
    [HideInInspector] public float currentConsumableAmount;
    [HideInInspector] public GameObject item;

    private void Awake()
    {
        currentConsumableAmount = maxConsumableAmount;
    }

    public void Consume()
    {
        //if player consumes a health kit
        if (consumableType.ToString() == Item_Consumable.ConsumableType.Healthkit.ToString())
        {
            string str_healthKitName = gameObject.GetComponent<Env_Item>().str_ItemName;
            float playerHealth = PlayerHealthScript.health;
            float playerMaxHealth = PlayerHealthScript.maxHealth;

            //if player health + health kit remainder equals player max health
            if (playerHealth + currentConsumableAmount == playerMaxHealth)
            {
                PlayerHealthScript.health = PlayerHealthScript.maxHealth;

                PlayerInventoryScript.inventory.Remove(gameObject);
                PlayerInventoryScript.invSpace -= gameObject.GetComponent<Env_Item>().int_ItemWeight;
                ConsoleScript.playeritemnames.Remove(str_healthKitName);

                UIReuseScript.ClearStatsUI();
                UIReuseScript.ClearInventoryUI();
                gameObject.GetComponent<Env_Item>().RemoveListeners();
                UIReuseScript.RebuildPlayerInventory();
                UIReuseScript.txt_InventoryName.text = "Player inventory";

                PlayerInventoryScript.UpdatePlayerInventoryStats();
                UIReuseScript.health = PlayerHealthScript.health;
                UIReuseScript.maxHealth = PlayerHealthScript.maxHealth;
                UIReuseScript.UpdatePlayerHealth();

                Destroy(gameObject);
            }
            //if player health is less than or more than or equal to health kit remainder
            else if (playerHealth < currentConsumableAmount 
                     || playerHealth > currentConsumableAmount
                     || playerHealth == currentConsumableAmount)
            {
                if (playerHealth + currentConsumableAmount > playerMaxHealth)
                {
                    float remainderToMax = playerMaxHealth - playerHealth;
                    PlayerHealthScript.health = playerMaxHealth;
                    currentConsumableAmount -= remainderToMax;

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    gameObject.GetComponent<Env_Item>().RemoveListeners();
                    UIReuseScript.RebuildPlayerInventory();
                    UIReuseScript.txt_InventoryName.text = "Player inventory";

                    PlayerInventoryScript.UpdatePlayerInventoryStats();
                    UIReuseScript.health = PlayerHealthScript.health;
                    UIReuseScript.maxHealth = PlayerHealthScript.maxHealth;
                    UIReuseScript.UpdatePlayerHealth();
                }
                else if (playerHealth + currentConsumableAmount <= playerMaxHealth)
                {
                    PlayerHealthScript.health += currentConsumableAmount;

                    PlayerInventoryScript.inventory.Remove(gameObject);
                    PlayerInventoryScript.invSpace -= gameObject.GetComponent<Env_Item>().int_ItemWeight;
                    ConsoleScript.playeritemnames.Remove(str_healthKitName);

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    gameObject.GetComponent<Env_Item>().RemoveListeners();
                    UIReuseScript.RebuildPlayerInventory();
                    UIReuseScript.txt_InventoryName.text = "Player inventory";

                    PlayerInventoryScript.UpdatePlayerInventoryStats();
                    UIReuseScript.health += currentConsumableAmount;
                    UIReuseScript.maxHealth = maxConsumableAmount;
                    UIReuseScript.UpdatePlayerHealth();

                    Destroy(gameObject);
                }
            }
        }
        //if player consumes a repair kit
        else if (consumableType == Item_Consumable.ConsumableType.Repairkit)
        {
            string str_repairKitName = gameObject.GetComponent<Env_Item>().str_ItemName;
            float durability = item.GetComponent<Item_Gun>().durability;
            float maxDurability = item.GetComponent<Item_Gun>().maxDurability;

            //if repairable item durability + repairkit remainder equals repairable item max durability
            if (durability + currentConsumableAmount == maxDurability)
            {
                item.GetComponent<Item_Gun>().durability = maxDurability;

                PlayerInventoryScript.inventory.Remove(gameObject);
                PlayerInventoryScript.invSpace -= gameObject.GetComponent<Env_Item>().int_ItemWeight;
                ConsoleScript.playeritemnames.Remove(str_repairKitName);

                UIReuseScript.ClearStatsUI();
                UIReuseScript.ClearInventoryUI();
                gameObject.GetComponent<Env_Item>().RemoveListeners();
                UIReuseScript.RebuildRepairMenu();
                UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Workbench.GetComponent<Env_Workbench>().str_workbenchName;

                PlayerInventoryScript.UpdatePlayerInventoryStats();
                UIReuseScript.durability = item.GetComponent<Item_Gun>().maxDurability;
                UIReuseScript.maxDurability = item.GetComponent<Item_Gun>().maxDurability;
                UIReuseScript.UpdateWeaponQuality();

                Destroy(gameObject);
            }
            //if repairable item durability is less than or more than or equal to repairkit remainder
            else if (durability < currentConsumableAmount 
                     || durability > currentConsumableAmount
                     || durability == currentConsumableAmount)
            {
                if (durability + currentConsumableAmount > maxDurability)
                {
                    float remainderToMax = maxDurability - durability;
                    item.GetComponent<Item_Gun>().durability = maxDurability;
                    currentConsumableAmount -= remainderToMax;

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    gameObject.GetComponent<Env_Item>().RemoveListeners();
                    UIReuseScript.RebuildRepairMenu();
                    UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Workbench.GetComponent<Env_Workbench>().str_workbenchName;

                    PlayerInventoryScript.UpdatePlayerInventoryStats();
                    UIReuseScript.durability = item.GetComponent<Item_Gun>().maxDurability;
                    UIReuseScript.maxDurability = item.GetComponent<Item_Gun>().maxDurability;
                }
                else if (durability + currentConsumableAmount <= maxDurability)
                {
                    item.GetComponent<Item_Gun>().durability += currentConsumableAmount;

                    PlayerInventoryScript.inventory.Remove(gameObject);
                    PlayerInventoryScript.invSpace -= gameObject.GetComponent<Env_Item>().int_ItemWeight;
                    ConsoleScript.playeritemnames.Remove(str_repairKitName);

                    UIReuseScript.ClearStatsUI();
                    UIReuseScript.ClearInventoryUI();
                    gameObject.GetComponent<Env_Item>().RemoveListeners();
                    UIReuseScript.RebuildRepairMenu();
                    UIReuseScript.txt_InventoryName.text = PlayerInventoryScript.Workbench.GetComponent<Env_Workbench>().str_workbenchName;

                    PlayerInventoryScript.UpdatePlayerInventoryStats();
                    UIReuseScript.durability += currentConsumableAmount;
                    UIReuseScript.maxDurability = item.GetComponent<Item_Gun>().maxDurability;
                    UIReuseScript.UpdateWeaponQuality();

                    Destroy(gameObject);
                }
            }
        }
    }
}