using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Exoskeleton : MonoBehaviour
{
    [Header("Jump boost ability")]
    [Range(0f, 120f)]
    public float maxCooldown_jumpBoost;
    [Range(50f, 100f)]
    public float abilityBuff_jumpBoost;

    //upgrades
    [Range(75f, 125f)]
    [SerializeField] private float upgrade1_jumpBoost;
    [Range(75f, 125f)]
    [SerializeField] private float upgrade2_jumpBoost;

    [Header("Sprint boost ability")]
    [Range(0f, 120f)]
    public float maxCooldown_sprintBoost;
    [Range(0f, 5f)]
    public float abilityBuff_sprintBoost;

    //upgrades
    [Range(2f, 10f)]
    [SerializeField] private float upgrade1_sprintBoost;
    [Range(2f, 10f)]
    [SerializeField] private float upgrade2_sprintBoost;

    [Header("Health regen ability")]
    [Range(0f, 120f)]
    public float maxCooldown_healthRegen;
    [Range(0f, 5f)]
    public float abilityBuff_healthRegen;

    //upgrades
    [Range(2f, 10f)]
    [SerializeField] private float upgrade1_healthRegen;
    [Range(2f, 10f)]
    [SerializeField] private float upgrade2_healthRegen;

    [Header("Stamina regen ability")]
    [Range(0f, 120f)]
    public float maxCooldown_staminaRegen;
    [Range(0f, 5f)]
    public float abilityBuff_staminaRegen;

    //upgrades
    [Range(1f, 15f)]
    [SerializeField] private float upgrade1_staminaRegen;
    [Range(1f, 15f)]
    [SerializeField] private float upgrade2_staminaRegen;

    [Header("Env protection ability")]
    [Range(0f, 120f)]
    public float maxCooldown_envProtection;
    [Range(0f, 25f)]
    public float abilityBuff_generalResistance;
    [Range(0f, 25f)]
    public float abilityBuff_mentalResistance;
    [Range(0f, 100f)]
    public float abilityBuff_radResistance;

    //upgrades
    [Range(5f, 40f)]
    [SerializeField] private float upgrade1_generalResistance;
    [Range(5f, 40f)]
    [SerializeField] private float upgrade2_generalResistance;
    [Range(5f, 40f)]
    [SerializeField] private float upgrade1_mentalResistance;
    [Range(5f, 40f)]
    [SerializeField] private float upgrade2_mentalResistance;
    [Range(25f, 500f)]
    [SerializeField] private float upgrade1_radResistance;
    [Range(25f, 500f)]
    [SerializeField] private float upgrade2_radResistance;

    [Header("Assignables")]
    public float maxRadCellPower;
    //----
    [SerializeField] private UI_AbilitySlot1 Slot1Script;
    [SerializeField] private UI_AbilitySlot2 Slot2Script;
    [SerializeField] private UI_AbilitySlot3 Slot3Script;
    [SerializeField] private Player_Movement PlayerMovementScript;
    [SerializeField] private Player_Health PlayerHealthScript;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private GameObject par_Managers;
    public List<GameObject> abilities = new List<GameObject>();

    //public but hidden variables
    [HideInInspector] public float remainingCooldown_jumpBoost;
    [HideInInspector] public float remainingCooldown_sprintBoost;
    [HideInInspector] public float remainingCooldown_healthRegen;
    [HideInInspector] public float remainingCooldown_staminaRegen;
    [HideInInspector] public float remainingCooldown_envProtection;
    [HideInInspector] public List<float> cooldowns;
    [HideInInspector] public List<float> maxCooldowns;

    [HideInInspector] public List<string> abilityNames = new List<string>();

    [HideInInspector] public bool usingAbility_jumpBoost;
    [HideInInspector] public bool usingAbility_sprintBoost;
    [HideInInspector] public bool usingAbility_healthRegen;
    [HideInInspector] public bool usingAbility_staminaRegen;
    [HideInInspector] public bool usingAbility_envProtection;

    [HideInInspector] public bool unlockedAbility_jumpBoost;
    [HideInInspector] public bool unlockedAbility_sprintBoost;
    [HideInInspector] public bool unlockedAbility_healthRegen;
    [HideInInspector] public bool unlockedAbility_staminaRegen;
    [HideInInspector] public bool unlockedAbility_envProtection;
    //----
    [HideInInspector] public float playerStat_healthProtection;
    [HideInInspector] public float playerStat_healthRegen;
    [HideInInspector] public float playerStat_staminaRegen;
    [HideInInspector] public float playerStat_mentalProtection;
    [HideInInspector] public float playerStat_radiationProtection;
    //----
    [HideInInspector] public float remainingRadCellPower;
    [HideInInspector] public int remainingUpgradeCells;

    //private variables
    private GameObject upgradeCell;

    private void Update()
    {
        if (!par_Managers.GetComponent<UI_PauseMenu>().isGamePaused
            && !par_Managers.GetComponent<Manager_Console>().consoleOpen
            && PlayerHealthScript.health > 0)
        {
            if (usingAbility_jumpBoost)
            {
                remainingCooldown_jumpBoost -= Time.deltaTime;

                if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.jumpBoost)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot1.text = Mathf.FloorToInt(remainingCooldown_jumpBoost + 1).ToString();
                }
                else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.jumpBoost)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot2.text = Mathf.FloorToInt(remainingCooldown_jumpBoost + 1).ToString();
                }
                else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.jumpBoost)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot3.text = Mathf.FloorToInt(remainingCooldown_jumpBoost + 1).ToString();
                }

                if (remainingCooldown_jumpBoost <= 0)
                {
                    EnablejumpBoostSlotButton();

                    PlayerMovementScript.jumpBuff = 0;

                    if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.jumpBoost)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot1.text = "0";
                    }
                    else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.jumpBoost)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot2.text = "0";
                    }
                    else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.jumpBoost)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot3.text = "0";
                    }

                    usingAbility_jumpBoost = false;
                }
            }

            if (usingAbility_sprintBoost)
            {
                remainingCooldown_sprintBoost -= Time.deltaTime;

                if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.sprintBoost)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot1.text = Mathf.FloorToInt(remainingCooldown_sprintBoost + 1).ToString();
                }
                else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.sprintBoost)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot2.text = Mathf.FloorToInt(remainingCooldown_sprintBoost + 1).ToString();
                }
                else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.sprintBoost)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot3.text = Mathf.FloorToInt(remainingCooldown_sprintBoost + 1).ToString();
                }

                if (remainingCooldown_sprintBoost <= 0)
                {
                    EnablesprintBoostSlotButton();

                    PlayerMovementScript.sprintBuff = 0;

                    if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.sprintBoost)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot1.text = "0";
                    }
                    else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.sprintBoost)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot2.text = "0";
                    }
                    else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.sprintBoost)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot3.text = "0";
                    }

                    usingAbility_sprintBoost = false;
                }
            }

            if (usingAbility_healthRegen)
            {
                remainingCooldown_healthRegen -= Time.deltaTime;

                if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.healthRegen)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot1.text = Mathf.FloorToInt(remainingCooldown_healthRegen + 1).ToString();
                }
                else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.healthRegen)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot2.text = Mathf.FloorToInt(remainingCooldown_healthRegen + 1).ToString();
                }
                else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.healthRegen)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot3.text = Mathf.FloorToInt(remainingCooldown_healthRegen + 1).ToString();
                }

                //healing ability only works if players health is below max health
                if (PlayerHealthScript.health + abilityBuff_healthRegen 
                    <= PlayerHealthScript.maxHealth + abilityBuff_healthRegen)
                {
                    PlayerHealthScript.health += abilityBuff_healthRegen * Time.deltaTime;
                }

                par_Managers.GetComponent<Manager_UIReuse>().txt_playerStatsHealthRegen.text = "+ " + abilityBuff_healthRegen +"/s";

                if (remainingCooldown_healthRegen <= 0)
                {
                    EnablehealthRegenSlotButton();

                    if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.healthRegen)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot1.text = "0";
                    }
                    else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.healthRegen)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot2.text = "0";
                    }
                    else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.healthRegen)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot3.text = "0";
                    }

                    par_Managers.GetComponent<Manager_UIReuse>().txt_playerStatsHealthRegen.text = "+ 0/s";

                    usingAbility_healthRegen = false;
                }
            }

            if (usingAbility_staminaRegen)
            {
                remainingCooldown_staminaRegen -= Time.deltaTime;

                if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.staminaRegen)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot1.text = Mathf.FloorToInt(remainingCooldown_staminaRegen + 1).ToString();
                }
                else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.staminaRegen)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot2.text = Mathf.FloorToInt(remainingCooldown_staminaRegen + 1).ToString();
                }
                else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.staminaRegen)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot3.text = Mathf.FloorToInt(remainingCooldown_staminaRegen + 1).ToString();
                }

                float result = (PlayerMovementScript.staminaRecharge + abilityBuff_staminaRegen) * Time.deltaTime;
                par_Managers.GetComponent<Manager_UIReuse>().txt_playerStatsStaminaRegen.text = "+ " + Mathf.Round(result * 100.0f) / 100.0f + "/s";

                if (remainingCooldown_staminaRegen <= 0)
                {
                    EnablestaminaRegenSlotButton();

                    PlayerMovementScript.staminaRegenBuff = 0;

                    if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.staminaRegen)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot1.text = "0";
                    }
                    else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.staminaRegen)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot2.text = "0";
                    }
                    else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.staminaRegen)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot3.text = "0";
                    }

                    par_Managers.GetComponent<Manager_UIReuse>().txt_playerStatsStaminaRegen.text = "+ 0/s";

                    usingAbility_staminaRegen = false;
                }
            }

            if (usingAbility_envProtection)
            {
                remainingCooldown_envProtection -= Time.deltaTime;

                if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.envProtection)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot1.text = Mathf.FloorToInt(remainingCooldown_envProtection + 1).ToString();
                }
                else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.envProtection)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot2.text = Mathf.FloorToInt(remainingCooldown_envProtection + 1).ToString();
                }
                else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.envProtection)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot3.text = Mathf.FloorToInt(remainingCooldown_envProtection + 1).ToString();
                }

                par_Managers.GetComponent<Manager_UIReuse>().txt_playerStatsHealthProtection.text = "+" + abilityBuff_generalResistance.ToString();
                par_Managers.GetComponent<Manager_UIReuse>().txt_playerStatsMentalProtection.text = "-" + abilityBuff_mentalResistance.ToString();
                par_Managers.GetComponent<Manager_UIReuse>().txt_playerStatsRadiationProtection.text = "-" + abilityBuff_radResistance.ToString();

                if (remainingCooldown_envProtection <= 0)
                {
                    EnableenvProtectionSlotButton();

                    if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.envProtection)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot1.text = "0";
                    }
                    else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.envProtection)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot2.text = "0";
                    }
                    else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.envProtection)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot3.text = "0";
                    }

                    par_Managers.GetComponent<Manager_UIReuse>().txt_playerStatsHealthProtection.text = "+0";
                    par_Managers.GetComponent<Manager_UIReuse>().txt_playerStatsMentalProtection.text = "-0";
                    par_Managers.GetComponent<Manager_UIReuse>().txt_playerStatsRadiationProtection.text = "-0";

                    usingAbility_envProtection = false;
                }
            }
        }
    }

    public void AddValues()
    {
        cooldowns.Clear();
        cooldowns.Add(remainingCooldown_jumpBoost);
        cooldowns.Add(remainingCooldown_sprintBoost);
        cooldowns.Add(remainingCooldown_healthRegen);
        cooldowns.Add(remainingCooldown_staminaRegen);
        cooldowns.Add(remainingCooldown_envProtection);

        maxCooldowns.Clear();
        maxCooldowns.Add(maxCooldown_jumpBoost);
        maxCooldowns.Add(maxCooldown_sprintBoost);
        maxCooldowns.Add(maxCooldown_healthRegen);
        maxCooldowns.Add(maxCooldown_staminaRegen);
        maxCooldowns.Add(maxCooldown_envProtection);

        abilityNames.Clear();
        abilityNames.Add("jumpBoost");
        abilityNames.Add("sprintBoost");
        abilityNames.Add("healthRegen");
        abilityNames.Add("staminaRegen");
        abilityNames.Add("envProtection");
    }

    public void UpdateCellValues()
    {
        //if no upgrade cell has been assigned yet
        if (upgradeCell == null)
        {
            //looks for upgrade cells from player inventory
            foreach (GameObject item in PlayerInventoryScript.inventory)
            {
                if (item.name == "Upgrade_cell")
                {
                    remainingUpgradeCells = item.GetComponent<Env_Item>().int_itemCount;
                    par_Managers.GetComponent<Manager_UIReuse>().txt_UpgradeCellCount.text = remainingUpgradeCells.ToString();

                    upgradeCell = item;
                }
            }
        }

        //if upgrade cell has already been assigned
        if (upgradeCell != null)
        {
            //if player ran out of upgrade cells
            if (remainingUpgradeCells == 0)
            {
                for (int i = 0; i < PlayerInventoryScript.inventory.Count; i++)
                {
                    if (PlayerInventoryScript.inventory[i] == upgradeCell)
                    {
                        GameObject destroyable = PlayerInventoryScript.inventory[i];
                        destroyable.GetComponent<Env_Item>().Destroy();
                        upgradeCell = null;
                    }
                }
            }
            //if player still has enough upgrade cells
            else if (remainingUpgradeCells > 0)
            {
                //looks for upgrade cells from player inventory
                foreach (GameObject item in PlayerInventoryScript.inventory)
                {
                    if (item == upgradeCell)
                    {
                        item.GetComponent<Env_Item>().int_itemCount = remainingUpgradeCells;
                    }
                }
            }
        }

        //if no upgrade cell was still found then remaining upgrade cell count is reset to 0
        if (upgradeCell == null)
        {
            remainingUpgradeCells = 0;
        }

        par_Managers.GetComponent<Manager_UIReuse>().txt_UpgradeCellCount.text = remainingUpgradeCells.ToString();
        par_Managers.GetComponent<Manager_UIReuse>().txt_RadCellPowerValue.text = Mathf.FloorToInt(remainingRadCellPower).ToString();
    }

    // --------
    // ### JUMP BOOST START ###

    public void UseAbility_jumpBoost()
    {
        if (!usingAbility_jumpBoost)
        {
            remainingCooldown_jumpBoost = 0;
            remainingCooldown_jumpBoost = maxCooldown_jumpBoost;

            PlayerMovementScript.jumpBuff = abilityBuff_jumpBoost;

            DisablejumpBoostSlotButton();

            usingAbility_jumpBoost = true;
        }
    }

    private void DisablejumpBoostSlotButton()
    {
        if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.jumpBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.interactable = false;
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.jumpBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.interactable = false;
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.jumpBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.interactable = false;
        }
    }
    private void EnablejumpBoostSlotButton()
    {
        if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.jumpBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.interactable = true;
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.jumpBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.interactable = true;
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.jumpBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.interactable = true;
        }
    }

    public void UnlockAbility_jumpBoost()
    {
        par_Managers.GetComponent<Manager_UIReuse>().img_UpgradeTier1_JumpBoost.gameObject.SetActive(true);
        unlockedAbility_jumpBoost = true;
    }
    public void UpgradeAbility_jumpBoost_Tier2()
    {
        if (!usingAbility_jumpBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().img_UpgradeTier2_JumpBoost.gameObject.SetActive(true);

            abilityBuff_jumpBoost = upgrade1_jumpBoost;
        }
        else if (usingAbility_jumpBoost)
        {
            Debug.LogWarning("Error: Cannot upgrade this ability while it is in use!");
        }
    }
    public void UpgradeAbility_jumpBoost_Tier3()
    {
        if (!usingAbility_jumpBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().img_UpgradeTier3_JumpBoost.gameObject.SetActive(true);

            abilityBuff_jumpBoost = upgrade2_jumpBoost;
        }
        else if (usingAbility_jumpBoost)
        {
            Debug.LogWarning("Error: Cannot upgrade this ability while it is in use!");
        }
    }

    // ### JUMP BOOST END ###
    // --------
    // ### SPRINT BOOST START ###

    public void UseAbility_sprintBoost()
    {
        if (!usingAbility_sprintBoost)
        {
            remainingCooldown_sprintBoost = 0;
            remainingCooldown_sprintBoost = maxCooldown_sprintBoost;

            PlayerMovementScript.sprintBuff = abilityBuff_sprintBoost;

            DisablesprintBoostSlotButton();

            usingAbility_sprintBoost = true;
        }
    }

    private void DisablesprintBoostSlotButton()
    {
        if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.sprintBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.interactable = false;
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.sprintBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.interactable = false;
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.sprintBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.interactable = false;
        }
    }
    private void EnablesprintBoostSlotButton()
    {
        if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.sprintBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.interactable = true;
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.sprintBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.interactable = true;
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.sprintBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.interactable = true;
        }
    }

    public void UnlockAbility_sprintBoost()
    {
        par_Managers.GetComponent<Manager_UIReuse>().img_UpgradeTier1_SprintBoost.gameObject.SetActive(true);
        unlockedAbility_sprintBoost = true;
    }
    public void UpgradeAbility_sprintBoost_Tier2()
    {
        if (!usingAbility_sprintBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().img_UpgradeTier2_SprintBoost.gameObject.SetActive(true);

            abilityBuff_sprintBoost = upgrade1_sprintBoost;
        }
        else if (usingAbility_sprintBoost)
        {
            Debug.LogWarning("Error: Cannot upgrade this ability while it is in use!");
        }
    }
    public void UpgradeAbility_sprintBoost_Tier3()
    {
        if (!usingAbility_sprintBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().img_UpgradeTier3_SprintBoost.gameObject.SetActive(true);

            abilityBuff_sprintBoost = upgrade2_sprintBoost;
        }
        else if (usingAbility_sprintBoost)
        {
            Debug.LogWarning("Error: Cannot upgrade this ability while it is in use!");
        }
    }

    // ### SPRINT BOOST END ###
    // --------
    // ### HEALTH REGEN START ###

    public void UseAbility_healthRegen()
    {
        if (!usingAbility_healthRegen)
        {
            remainingCooldown_healthRegen = 0;
            remainingCooldown_healthRegen = maxCooldown_healthRegen;

            DisablehealthRegenSlotButton();

            usingAbility_healthRegen = true;
        }
    }

    private void DisablehealthRegenSlotButton()
    {
        if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.healthRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.interactable = false;
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.healthRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.interactable = false;
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.healthRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.interactable = false;
        }
    }
    private void EnablehealthRegenSlotButton()
    {
        if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.healthRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.interactable = true;
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.healthRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.interactable = true;
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.healthRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.interactable = true;
        }
    }

    public void UnlockAbility_healthRegen()
    {
        par_Managers.GetComponent<Manager_UIReuse>().img_UpgradeTier1_HealthRegen.gameObject.SetActive(true);
        unlockedAbility_healthRegen = true;
    }
    public void UpgradeAbility_healthRegen_Tier2()
    {
        if (!usingAbility_healthRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().img_UpgradeTier2_HealthRegen.gameObject.SetActive(true);

            abilityBuff_healthRegen = upgrade1_healthRegen;
        }
        else if (usingAbility_healthRegen)
        {
            Debug.LogWarning("Error: Cannot upgrade this ability while it is in use!");
        }
    }
    public void UpgradeAbility_healthRegen_Tier3()
    {
        if (!usingAbility_healthRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().img_UpgradeTier3_HealthRegen.gameObject.SetActive(true);

            abilityBuff_healthRegen = upgrade2_healthRegen;
        }
        else if (usingAbility_healthRegen)
        {
            Debug.LogWarning("Error: Cannot upgrade this ability while it is in use!");
        }
    }

    // ### HEALTH REGEN END ###
    // --------
    // ### STAMINA REGEN START ###

    public void UseAbility_staminaRegen()
    {
        if (!usingAbility_staminaRegen)
        {
            remainingCooldown_staminaRegen = 0;
            remainingCooldown_staminaRegen = maxCooldown_staminaRegen;

            PlayerMovementScript.staminaRegenBuff = abilityBuff_staminaRegen;

            DisablestaminaRegenSlotButton();

            usingAbility_staminaRegen = true;
        }
    }

    private void DisablestaminaRegenSlotButton()
    {
        if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.staminaRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.interactable = false;
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.staminaRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.interactable = false;
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.staminaRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.interactable = false;
        }
    }
    private void EnablestaminaRegenSlotButton()
    {
        if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.staminaRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.interactable = true;
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.staminaRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.interactable = true;
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.staminaRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.interactable = true;
        }
    }

    public void UnlockAbility_staminaRegen()
    {
        par_Managers.GetComponent<Manager_UIReuse>().img_UpgradeTier1_StaminaRegen.gameObject.SetActive(true);
        unlockedAbility_staminaRegen = true;
    }
    public void UpgradeAbility_staminaRegen_Tier2()
    {
        if (!usingAbility_staminaRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().img_UpgradeTier2_StaminaRegen.gameObject.SetActive(true);

            abilityBuff_staminaRegen = upgrade1_staminaRegen;
        }
        else if (usingAbility_staminaRegen)
        {
            Debug.LogWarning("Error: Cannot upgrade this ability while it is in use!");
        }
    }
    public void UpgradeAbility_staminaRegen_Tier3()
    {
        if (!usingAbility_staminaRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().img_UpgradeTier3_StaminaRegen.gameObject.SetActive(true);

            abilityBuff_staminaRegen = upgrade2_staminaRegen;
        }
        else if (usingAbility_staminaRegen)
        {
            Debug.LogWarning("Error: Cannot upgrade this ability while it is in use!");
        }
    }

    // ### STAMINA REGEN END ###
    // --------
    // ### ENV PROTECTION START ###

    public void UseAbility_envProtection()
    {
        if (!usingAbility_envProtection)
        {
            remainingCooldown_envProtection = 0;
            remainingCooldown_envProtection = maxCooldown_envProtection;

            DisableenvProtectionSlotButton();

            usingAbility_envProtection = true;
        }
    }

    private void DisableenvProtectionSlotButton()
    {
        if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.envProtection)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.interactable = false;
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.envProtection)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.interactable = false;
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.envProtection)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.interactable = false;
        }
    }
    private void EnableenvProtectionSlotButton()
    {
        if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.envProtection)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.interactable = true;
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.envProtection)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.interactable = true;
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.envProtection)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.interactable = true;
        }
    }

    public void UnlockAbility_envProtection()
    {
        par_Managers.GetComponent<Manager_UIReuse>().img_UpgradeTier1_EnvProtection.gameObject.SetActive(true);
        unlockedAbility_envProtection = true;
    }
    public void UpgradeAbility_envProtection_Tier2()
    {
        if (!usingAbility_envProtection)
        {
            par_Managers.GetComponent<Manager_UIReuse>().img_UpgradeTier2_EnvProtection.gameObject.SetActive(true);

            abilityBuff_generalResistance = upgrade1_generalResistance;
            abilityBuff_mentalResistance = upgrade1_mentalResistance;
            abilityBuff_radResistance = upgrade1_radResistance;
        }
        else if (usingAbility_envProtection)
        {
            Debug.LogWarning("Error: Cannot upgrade this ability while it is in use!");
        }
    }
    public void UpgradeAbility_envProtection_Tier3()
    {
        if (!usingAbility_envProtection)
        {
            par_Managers.GetComponent<Manager_UIReuse>().img_UpgradeTier3_EnvProtection.gameObject.SetActive(true);

            abilityBuff_generalResistance = upgrade2_generalResistance;
            abilityBuff_mentalResistance = upgrade2_mentalResistance;
            abilityBuff_radResistance = upgrade2_radResistance;
        }
        else if (usingAbility_envProtection)
        {
            Debug.LogWarning("Error: Cannot upgrade this ability while it is in use!");
        }
    }

    // ### ENV PROTECTION END ###
    // --------
}