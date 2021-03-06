using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrade_Gun : MonoBehaviour
{
    [Header("Upgrades")]
    [SerializeField] private string str_UpgradeName;
    [SerializeField] private string str_UpgradeDescription;

    [SerializeField] private List<string> upgradeRequirements;
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private enum UpgradeType
    {
        fireType,
        firerate,
        accuracy,
        bulletDrop,
        clipSize,
        ammoType
    }

    [Header("Tier 1")]
    public bool maxTier1;
    public string upgradeValue1;

    [Header("Tier 1")]
    public bool maxTier2;
    public string upgradeValue2;

    [Header("Tier 1")]
    public bool maxTier3;
    public string upgradeValue3;

    [Header("Scripts")]
    public UI_Tooltip TooltipScript;
    [SerializeField] private Manager_ItemUpgrades UpgradeManagerScript;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public int maxTier;
    [HideInInspector] public int upgradeTier;

    //private variables
    private string upgradeValue;

    private void Awake()
    {
        if (maxTier1)
        {
            maxTier = 1;
        }
        else if (maxTier2)
        {
            maxTier = 2;
        }
        else if (maxTier3)
        {
            maxTier = 3;
        }
    }

    public void UpgradeGun()
    {
        bool missingRequirements = false;
        List<GameObject> requiredItems = new();

        //check for requirements
        foreach (string requiredItem in upgradeRequirements)
        {
            //get all separators in line
            char[] separators = new char[] { 'x' };
            //remove unwanted separators and split line into separate strings
            string[] values = requiredItem.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            //assign required item name and quantity
            string requiredItemName = values[0];
            int requiredItemQuantity = int.Parse(values[1]);

            //look if player has enough of each of the required items in their inventory
            //break the loop and throw error in console if atleast one of the required items is missing
            GameObject theRequiredItem = null;
            foreach (GameObject item in PlayerInventoryScript.inventory)
            {
                if (item.name == requiredItemName
                    && item.GetComponent<Env_Item>().int_itemCount >= requiredItemQuantity)
                {
                    //create a new placeholder item
                    theRequiredItem = Instantiate(item, null);
                    //set count as required item count
                    theRequiredItem.GetComponent<Env_Item>().int_itemCount = int.Parse(values[1]);
                    //add to temporary list
                    requiredItems.Add(theRequiredItem);
                }
            }

            if (theRequiredItem == null)
            {
                foreach (GameObject item in requiredItems)
                {
                    Destroy(item);
                }
                requiredItems.Clear();

                missingRequirements = true;

                Debug.LogWarning("Error: Player is missing required resources to upgrade " + str_UpgradeName + "!");

                break;
            }
        }

        if (!missingRequirements)
        {
            UpdateUpgradeValue();

            Item_Gun gunScript = transform.parent.GetComponent<Item_Gun>();

            //upgrade gun fire type
            if (upgradeType == UpgradeType.fireType)
            {
                if (upgradeValue.Contains("Automatic"))
                {
                    gunScript.isSingleShot = false;
                }
            }
            //upgrade gun fire rate
            else if (upgradeType == UpgradeType.firerate)
            {
                gunScript.fireRate = int.Parse(upgradeValue);
            }
            //upgrade gun accuracy
            else if (upgradeType == UpgradeType.accuracy)
            {
                //not yet functional
            }
            //upgrade gun bullet drop
            else if (upgradeType == UpgradeType.bulletDrop)
            {
                //not yet functional
            }
            //upgrade gun clip size
            else if (upgradeType == UpgradeType.clipSize)
            {
                gunScript.maxClipSize = int.Parse(upgradeValue);

                GameObject ammo;
                if (gunScript.ammoClip != null)
                {
                    ammo = gunScript.ammoClip;

                    foreach (GameObject item in PlayerInventoryScript.inventory)
                    {
                        if (item == ammo)
                        {
                            int removedAmount = gunScript.currentClipSize;
                            gunScript.currentClipSize = 0;
                            item.GetComponent<Env_Item>().int_itemCount += removedAmount;

                            par_Managers.GetComponent<Manager_UIReuse>().ClearWeaponUI();
                            par_Managers.GetComponent<Manager_UIReuse>().durability = gunScript.durability;
                            par_Managers.GetComponent<Manager_UIReuse>().maxDurability = gunScript.maxDurability;
                            par_Managers.GetComponent<Manager_UIReuse>().UpdateWeaponQuality();

                            break;
                        }
                    }
                }
            }
            //upgrade gun ammo type
            else if (upgradeType == UpgradeType.ammoType)
            {
                _ = (Item_Gun.CaseType)Enum.Parse(typeof(Item_Gun.CaseType), upgradeValue);
                gunScript.AssignAmmoType();
            }

            //get each required item
            foreach (GameObject requiredItem in requiredItems)
            {
                //sort through each player inventory item
                for (int i = 0; i < PlayerInventoryScript.inventory.Count; i++)
                {
                    GameObject item = PlayerInventoryScript.inventory[i];

                    if (item == requiredItem)
                    {
                        //if there are more required items in player inventory
                        //than the amount this upgrade needs
                        if (item.GetComponent<Env_Item>().int_itemCount
                            > requiredItem.GetComponent<Env_Item>().int_itemCount)
                        {
                            //calculate how many items need to be removed
                            int remainder =
                                item.GetComponent<Env_Item>().int_itemCount
                                - requiredItem.GetComponent<Env_Item>().int_itemCount;

                            //update item count for required item in player inventory
                            item.GetComponent<Env_Item>().int_itemCount -= remainder;

                            //update player inventory inv space
                            PlayerInventoryScript.invSpace +=
                                item.GetComponent<Env_Item>().int_ItemWeight
                                * remainder;

                            PlayerInventoryScript.UpdatePlayerInventoryStats();
                        }
                        //if there are the same amount of required items in player inventory
                        //as the amount this upgrade needs
                        else
                        {
                            //remove all item weight of this item from player inventory
                            PlayerInventoryScript.invSpace +=
                                item.GetComponent<Env_Item>().int_ItemWeight
                                * item.GetComponent<Env_Item>().int_itemCount;

                            //remove item name from console player item names list
                            par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(item.name);

                            Destroy(item);
                        }
                    }
                }
            }

            //remove all null gameobjects from player inventory
            for (int i = 0; i < PlayerInventoryScript.inventory.Count; i++)
            {
                GameObject item = PlayerInventoryScript.inventory[i];

                if (item == null)
                {
                    PlayerInventoryScript.inventory.Remove(item);
                }
            }

            //update UI
            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();

            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";

            PlayerInventoryScript.UpdatePlayerInventoryStats();

            foreach (GameObject item in requiredItems)
            {
                Destroy(item);
            }
            requiredItems.Clear();

            upgradeTier++;

            if (upgradeTier == maxTier)
            {
                upgradeRequirements.Clear();
            }
            UpdateUpgradeValue();

            Debug.Log("Successfully upgraded " + transform.parent.name + "'s " + upgradeType.ToString() + " to " + upgradeValue + "!");
        }

        TooltipScript.showTooltipUI = false;

        UpgradeManagerScript.UpdateWeaponUpgradeButtons();
    }

    public void ShowTooltipText()
    {
        TooltipScript.showTooltipUI = true;

        string tooltipText = str_UpgradeName;

        tooltipText += "\n\n" + str_UpgradeDescription;

        tooltipText += "\n\nTier: " + upgradeTier;

        tooltipText += "\n\nRequirements:";

        if (upgradeRequirements.Count > 0)
        {
            foreach (string requirement in upgradeRequirements)
            {
                //get all separators in line
                char[] separators = new char[] { 'x' };
                //remove unwanted separators and split line into separate strings
                string[] values = requirement.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                //assign required item name and quantity
                string requiredItemName = values[0];
                int requiredItemQuantity = int.Parse(values[1]);

                GameObject theRequiredItem = null;
                foreach (GameObject item in PlayerInventoryScript.inventory)
                {
                    if (item.name == requiredItemName
                        && item.GetComponent<Env_Item>().int_itemCount >= requiredItemQuantity)
                    {
                        theRequiredItem = item;

                        tooltipText += "\n" + requiredItemName + " x <color=green>" + requiredItemQuantity + "</color>";

                        break;
                    }
                }

                if (theRequiredItem == null)
                {
                    tooltipText += "\n" + requiredItemName + " x <color=red>" + requiredItemQuantity + "</color>";
                }
            }
        }
        else
        {
            tooltipText += "none";
        }

        TooltipScript.SetText(tooltipText);
    }

    private void UpdateUpgradeValue()
    {
        if (upgradeTier == 0)
        {
            upgradeValue = upgradeValue1;
        }
        else if (upgradeTier == 1)
        {
            upgradeValue = upgradeValue2;
        }
        else if (upgradeTier == 2)
        {
            upgradeValue = upgradeValue3;
        }
        else
        {
            upgradeValue = "";
        }
    }
}