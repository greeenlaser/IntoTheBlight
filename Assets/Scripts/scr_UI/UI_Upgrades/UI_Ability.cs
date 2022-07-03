using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Ability : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Assignables")]
    public string abilityName;
    [TextArea(10, 10)]
    [SerializeField] private string abilityDescription;
    public int cost_unlock;
    public int cost_tier2;
    public int cost_tier3;
    [SerializeField] private float cooldownTime;
    [SerializeField] private float reuseTime;
    public Ability ability;
    public enum Ability
    {
        jumpBoost,
        sprintBoost,
        healthRegenBoost,
        staminaRegenBoost,
        envProtectionBoost
    }
    public Transform pos_Assign;
    public Transform pos_Upgrade;
    public RawImage img_Tier1;
    public RawImage img_Tier2;
    public RawImage img_Tier3;

    [Header("Scripts")]
    [SerializeField] private UI_Tooltip TooltipScript;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool isCooldownTimeRunning;
    [HideInInspector] public bool isReuseTimeRunning;
    [HideInInspector] public int upgradeStatus;
    [HideInInspector] public int assignStatus;
    [HideInInspector] public int selectedSlot;

    //private variables
    private GameObject upgradeCell;
    private UI_AbilityManager AbilityManagerScript;

    private void Start()
    {
        AbilityManagerScript = par_Managers.GetComponentInChildren<UI_AbilityManager>();
    }

    //assigns or unlocks/upgrades this ability when clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameObject.GetComponent<Button>().interactable == true)
        {
            //unlock or upgrade this ability in upgrade ui
            if (par_Managers.GetComponent<UI_PlayerMenu>().openedUpgradeUI
                && ((upgradeStatus == 0
                && AbilityManagerScript.upgradeCellCount >= cost_unlock)
                || (upgradeStatus == 1
                && AbilityManagerScript.upgradeCellCount >= cost_tier2)
                || (upgradeStatus == 2
                && AbilityManagerScript.upgradeCellCount >= cost_tier3)))
            {
                TooltipScript.showTooltipUI = false;
            }
            //assign this ability to selected slot in assign ui
            else
            {
                int newAbility = AbilityManagerScript.abilities.IndexOf(gameObject);
                AbilityManagerScript.AssignToSlot(selectedSlot, newAbility);

                TooltipScript.showTooltipUI = false;
            }
        }
    }
    //loads tooltip when hovering over this ability
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltipText();
    }
    //closes tooltip when no longer hovering over this ability
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipScript.showTooltipUI = false;
    }

    //use this ability
    public void UseAbility()
    {
        Debug.Log("Using ability " + abilityName + " in slot " + assignStatus + ".");
    }
    //unlock or upgrade this ability
    public void UnlockOrUpgradeAbility()
    {
        upgradeStatus++;

        foreach (GameObject item in PlayerInventoryScript.inventory)
        {
            if (item.name == "Upgrade_cell")
            {
                upgradeCell = item;
                break;
            }
        }

        int itemWeight = upgradeCell.GetComponent<Env_Item>().int_ItemWeight * upgradeCell.GetComponent<Env_Item>().int_itemCount;

        if (upgradeStatus == 1)
        {
            if (AbilityManagerScript.upgradeCellCount > cost_unlock)
            {
                upgradeCell.GetComponent<Env_Item>().int_itemCount -= cost_unlock;
            }
            else
            {
                PlayerInventoryScript.inventory.Remove(upgradeCell);
                PlayerInventoryScript.invSpace += itemWeight;

                par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(upgradeCell.name);

                Destroy(upgradeCell);
                upgradeCell = null;
            }
        }
        else if (upgradeStatus == 2)
        {
            if (AbilityManagerScript.upgradeCellCount > cost_tier2)
            {
                upgradeCell.GetComponent<Env_Item>().int_itemCount -= cost_tier2;
            }
            else
            {
                PlayerInventoryScript.inventory.Remove(upgradeCell);
                PlayerInventoryScript.invSpace += itemWeight;

                par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(upgradeCell.name);

                Destroy(upgradeCell);
                upgradeCell = null;
            }
        }
        else if (upgradeStatus == 3)
        {
            if (AbilityManagerScript.upgradeCellCount > cost_tier3)
            {
                upgradeCell.GetComponent<Env_Item>().int_itemCount -= cost_tier3;
            }
            else
            {
                PlayerInventoryScript.inventory.Remove(upgradeCell);
                PlayerInventoryScript.invSpace += itemWeight;

                par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(upgradeCell.name);

                Destroy(upgradeCell);
                upgradeCell = null;
            }
        }

        AbilityManagerScript.LoadUI();
        AbilityManagerScript.ShowUpgradeButtonPositions();
    }

    //shows the actual tooltip ui
    private void ShowTooltipText()
    {
        TooltipScript.showTooltipUI = true;

        string tooltipText = abilityName;

        tooltipText += "\n\n" + abilityDescription;

        //shows cost only in upgrade ui
        if (par_Managers.GetComponent<UI_PlayerMenu>().openedUpgradeUI)
        {
            string abilityCost = "";
            int cost = 0;
            int upgradeCellCount = AbilityManagerScript.upgradeCellCount;

            if (upgradeStatus == 0)
            {
                cost = cost_unlock;
            }
            else if (upgradeStatus == 1)
            {
                cost = cost_tier2;
            }
            else if (upgradeStatus == 2)
            {
                cost = cost_tier3;
            }
            else if (upgradeStatus == 3)
            {
                cost = 0;
            }

            if (cost != 0)
            {
                if (cost > upgradeCellCount)
                {
                    abilityCost += "<color=red>" + cost.ToString() + "</color>";
                }
                else if (cost <= upgradeCellCount)
                {
                    abilityCost += "<color=green>" + cost.ToString() + "</color>";
                }
            }
            else
            {
                abilityCost += "<color=green>Fully upgraded</color>";
            }

            tooltipText += "\n\n" + "Cost: " + abilityCost;
        }
        //shows assigned slot only in assign ui
        else
        {
            tooltipText += "\n\n" + "Slot: " + assignStatus;
        }

        tooltipText += "\n" + "Tier: " + upgradeStatus;
        TooltipScript.SetText(tooltipText);
    }
}