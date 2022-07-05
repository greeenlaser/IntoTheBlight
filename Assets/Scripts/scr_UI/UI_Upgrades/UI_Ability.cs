using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UI_Ability : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Assignables")]
    public string abilityName;
    [TextArea(5, 5)]
    [SerializeField] private string abilityDescription;
    [SerializeField] private float cooldownTime;
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

    [Header("Tier 1")]
    [Range(0, 10)]
    [SerializeField] private float boost1_1;
    [Range(0, 10)]
    [SerializeField] private float boost1_2;
    public int cost_unlock;
    public RawImage img_Tier1;

    [Header("Tier 2")]
    [Range(0, 10)]
    [SerializeField] private float boost2_1;
    [Range(0, 10)]
    [SerializeField] private float boost2_2;
    public int cost_tier2;
    public RawImage img_Tier2;

    [Header("Tier 3")]
    [Range(0, 10)]
    [SerializeField] private float boost3_1;
    [Range(0, 10)]
    [SerializeField] private float boost3_2;
    public int cost_tier3;
    public RawImage img_Tier3;

    [Header("Scripts")]
    [SerializeField] private UI_Tooltip TooltipScript;
    [SerializeField] private Player_Movement PlayerMovementScript;
    [SerializeField] private Player_Health PlayerHealthScript;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool isCooldownTimeRunning;
    [HideInInspector] public int upgradeStatus;
    [HideInInspector] public int assignStatus;
    [HideInInspector] public int selectedSlot;

    //private variables
    private bool usingAbility;
    private bool calledAbilityBoostOnce;
    private float remainder;
    private float boost_1;
    private float boost_2;
    private GameObject upgradeCell;
    private TMP_Text targetText;
    private UI_AbilityManager AbilityManagerScript;
    private Manager_UIReuse UIReuseScript;

    private void Start()
    {
        AbilityManagerScript = par_Managers.GetComponentInChildren<UI_AbilityManager>();
        UIReuseScript = par_Managers.GetComponent<Manager_UIReuse>();

        remainder = cooldownTime;
    }

    private void Update()
    {
        if (isCooldownTimeRunning
            && remainder > 0)
        {
            if (targetText == null)
            {
                if (assignStatus == 1)
                {
                    UIReuseScript.txt_cooldownTimer1.text = cooldownTime.ToString();
                    targetText = UIReuseScript.txt_cooldownTimer1;
                }
                else if (assignStatus == 2)
                {
                    UIReuseScript.txt_cooldownTimer2.text = cooldownTime.ToString();
                    targetText = UIReuseScript.txt_cooldownTimer2;
                }
                else if (assignStatus == 3)
                {
                    UIReuseScript.txt_cooldownTimer3.text = cooldownTime.ToString();
                    targetText = UIReuseScript.txt_cooldownTimer3;
                }
            }   

            remainder -= 1 * Time.deltaTime;
            targetText.text = Mathf.FloorToInt(remainder +1).ToString();

            if (!usingAbility)
            {
                usingAbility = true;
            }

            AssignBuffs();
        }
        else if (isCooldownTimeRunning
                 && remainder <= 0)
        {
            if (assignStatus == 1)
            {
                UIReuseScript.txt_cooldownTimer1.text = "0";
            }
            else if (assignStatus == 2)
            {
                UIReuseScript.txt_cooldownTimer2.text = "0";
            }
            else if (assignStatus == 3)
            {
                UIReuseScript.txt_cooldownTimer3.text = "0";
            }

            RemoveBuffs();
        }
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

    //assign the buffs from this ability to player
    private void AssignBuffs()
    {
        if (ability == Ability.jumpBoost
            && !calledAbilityBoostOnce)
        {
            PlayerMovementScript.jumpBuff += boost_1;
            PlayerHealthScript.fallProtection += boost_2;
            calledAbilityBoostOnce = true;
        }
        else if (ability == Ability.sprintBoost
                 && !calledAbilityBoostOnce)
        {
            PlayerMovementScript.sprintBuff += boost_1;
            calledAbilityBoostOnce = true;
        }
        else if (ability == Ability.healthRegenBoost)
        {
            if (PlayerHealthScript.health < PlayerHealthScript.maxHealth)
            {
                PlayerHealthScript.health += boost_1 * Time.deltaTime;
                UIReuseScript.health = PlayerHealthScript.health;
                UIReuseScript.maxHealth = PlayerHealthScript.maxHealth;
                UIReuseScript.UpdatePlayerHealth();
            }
        }
        else if (ability == Ability.staminaRegenBoost)
        {
            if (PlayerMovementScript.currentStamina < PlayerMovementScript.maxStamina)
            {
                PlayerMovementScript.currentStamina += boost_1 * Time.deltaTime;
                UIReuseScript.stamina = PlayerMovementScript.currentStamina;
                UIReuseScript.maxStamina = PlayerMovementScript.maxStamina;
                UIReuseScript.UpdatePlayerStamina();
            }
        }
        else if (ability == Ability.envProtectionBoost
                 && !calledAbilityBoostOnce)
        {
            PlayerHealthScript.psyProtection += boost_1;
            PlayerHealthScript.radProtection += boost_2;
        }
    }
    //remove the buffs of this ability from player
    private void RemoveBuffs()
    {
        if (ability == Ability.jumpBoost)
        {
            PlayerMovementScript.jumpBuff -= boost_1;
            PlayerHealthScript.fallProtection -= boost_2;
        }
        else if (ability == Ability.sprintBoost)
        {
            PlayerMovementScript.sprintBuff -= boost_1;
        }
        else if (ability == Ability.envProtectionBoost)
        {
            PlayerHealthScript.psyProtection -= boost_1;
            PlayerHealthScript.radProtection -= boost_2;
        }

        isCooldownTimeRunning = false;
        calledAbilityBoostOnce = false;
        usingAbility = false;

        remainder = cooldownTime;
    }
    //use this ability
    public void UseAbility()
    {
        Debug.Log("Using ability " + abilityName + " in slot " + assignStatus + ".");
        StartCoroutine(AbilityCooldown());
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
            boost_1 = boost1_1;
            boost_2 = boost1_2;

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
            boost_1 = boost2_1;
            boost_2 = boost2_2;

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
            boost_1 = boost3_1;
            boost_2 = boost3_2;

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

    private IEnumerator AbilityCooldown()
    {
        isCooldownTimeRunning = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldownTimeRunning = false;
    }
}