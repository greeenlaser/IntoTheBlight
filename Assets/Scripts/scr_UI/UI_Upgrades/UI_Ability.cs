using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Ability : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private float timeUntilAbilityUse;
    [SerializeField] private float abilityLength;
    [SerializeField] private float abilityReuseCooldown;
    public Ability ability;
    public enum Ability
    {
        jumpBoost,
        sprintBoost,
        healthRegen,
        staminaRegen,
        envProtection
    }

    [Header("Tier 1")]
    public float abilityBoostValue1_1;
    public float abilityBoostValue2_1;
    public float abilityBoostValue3_1;
    public int requiredUpgradeCells_1;

    [Header("Tier 2")]
    public float abilityBoostValue1_2;
    public float abilityBoostValue2_2;
    public float abilityBoostValue3_2;
    public int requiredUpgradeCells_2;

    [Header("Tier 3")]
    public float abilityBoostValue1_3;
    public float abilityBoostValue2_3;
    public float abilityBoostValue3_3;
    public int requiredUpgradeCells_3;

    [Header("Scripts")]
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private Player_Movement PlayerMovementScript;
    [SerializeField] private Player_Health PlayerHealthScript;
    [SerializeField] private UI_AbilityManager AbilityManagerScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool isAbilityInUse;
    [HideInInspector] public int abilityStatus;
    [HideInInspector] public int assignedSlot;

    //private variables
    private bool calledAbilityTreeShowOnce;
    private GameObject upgradeCell;
    private float abilityLengthTimer;
    private float abilityReuseCooldownTimer;

    private void Start()
    {
        if (AbilityManagerScript.assignedAbilities.Contains(gameObject))
        {
            assignedSlot = -1;
        }
    }

    private void Update()
    {
        if (!par_Managers.GetComponent<Manager_Console>().consoleOpen
            && PlayerHealthScript.isPlayerAlive)
        {
            //use ability
            if (isAbilityInUse
                && !par_Managers.GetComponent<UI_PauseMenu>().isGamePaused)
            {
                abilityLengthTimer += Time.deltaTime;

                if (assignedSlot == 0)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot1.text = Mathf.FloorToInt(abilityLengthTimer + 1).ToString();
                }
                else if (assignedSlot == 1)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot2.text = Mathf.FloorToInt(abilityLengthTimer + 1).ToString();
                }
                else if (assignedSlot == 2)
                {
                    par_Managers.GetComponent<Manager_UIReuse>().txt_timerSlot3.text = Mathf.FloorToInt(abilityLengthTimer + 1).ToString();
                }

                if (abilityLengthTimer >= timeUntilAbilityUse)
                {
                    UseAbility();
                }
                else if (abilityLengthTimer >= abilityLength)
                {
                    abilityReuseCooldownTimer += Time.deltaTime;

                    if (abilityReuseCooldownTimer >= abilityReuseCooldown)
                    {
                        isAbilityInUse = false;
                    }
                }
            }
            //show ability tree
            else if (!isAbilityInUse
                     && calledAbilityTreeShowOnce
                     && AbilityManagerScript.abilityAssignTimer >= 0.2f
                     && AbilityManagerScript.assigningToSlot > 0)
            {
                par_Managers.GetComponent<UI_PauseMenu>().PauseGame();

                par_Managers.GetComponent<Manager_UIReuse>().par_AbilityUI.SetActive(true);

                foreach (GameObject abilityButton in AbilityManagerScript.allAbilities)
                {
                    abilityButton.GetComponent<Button>().onClick.RemoveAllListeners();
                    abilityButton.GetComponent<Button>().interactable = false;
                }

                if (AbilityManagerScript.assigningToSlot == 0)
                {
                    foreach (GameObject abilityButton in AbilityManagerScript.unlockedAbilities)
                    {
                        abilityButton.GetComponent<Button>().onClick.AddListener(AssignToSlot1);

                        abilityButton.GetComponent<Button>().interactable = true;
                    }
                }
                else if (AbilityManagerScript.assigningToSlot == 1)
                {
                    foreach (GameObject abilityButton in AbilityManagerScript.unlockedAbilities)
                    {
                        abilityButton.GetComponent<Button>().onClick.AddListener(AssignToSlot2);

                        abilityButton.GetComponent<Button>().interactable = true;
                    }
                }
                else if (AbilityManagerScript.assigningToSlot == 2)
                {
                    foreach (GameObject abilityButton in AbilityManagerScript.unlockedAbilities)
                    {
                        abilityButton.GetComponent<Button>().onClick.AddListener(AssignToSlot3);

                        abilityButton.GetComponent<Button>().interactable = true;
                    }
                }

                calledAbilityTreeShowOnce = true;
            }
            //hide ability tree
            else if (calledAbilityTreeShowOnce
                     && Input.GetKeyUp(KeyCode.Z)
                     && Input.GetKeyUp(KeyCode.X)
                     && Input.GetKeyUp(KeyCode.C))
            {
                AbilityManagerScript.abilityAssignTimer = 0;
                AbilityManagerScript.assigningToSlot = -1;

                par_Managers.GetComponent<Manager_UIReuse>().par_AbilityUI.SetActive(false);

                par_Managers.GetComponent<UI_PauseMenu>().UnpauseGame();

                calledAbilityTreeShowOnce = false;
            }
        }
    }

    public void UnlockOrUpgradeAbility()
    {
        if (!isAbilityInUse)
        {
            //first time searching for upgrade cells
            if (upgradeCell == null)
            {
                foreach (GameObject item in PlayerInventoryScript.inventory)
                {
                    if (item.name == "Upgrade_cell")
                    {
                        upgradeCell = item;
                        break;
                    }
                }
            }

            //once upgrade cells have been found
            if (upgradeCell != null)
            {
                if (abilityStatus == 0
                    && upgradeCell.GetComponent<Env_Item>().int_itemCount >= requiredUpgradeCells_1)
                {
                    RemoveResources(requiredUpgradeCells_1);

                    abilityStatus = 1;
                    AbilityManagerScript.unlockedAbilities.Add(gameObject);

                    Debug.Log("Unlocked " + ability + ".");
                }
                else if (abilityStatus == 1
                         && upgradeCell.GetComponent<Env_Item>().int_itemCount >= requiredUpgradeCells_2)
                {
                    RemoveResources(requiredUpgradeCells_2);

                    abilityStatus = 2;
                    Debug.Log("Upgraded " + ability + " to tier 2.");
                }
                else if (abilityStatus == 2
                         && upgradeCell.GetComponent<Env_Item>().int_itemCount >= requiredUpgradeCells_3)
                {
                    RemoveResources(requiredUpgradeCells_3);

                    abilityStatus = 3;
                    Debug.Log("Upgraded " + ability + " to tier 3.");
                }

                else if (abilityStatus == 0
                         && upgradeCell.GetComponent<Env_Item>().int_itemCount < requiredUpgradeCells_1)
                {
                    Debug.LogWarning("Error: Not enough upgrade cells to unlock " + ability + "!");
                }
                else if (abilityStatus == 1
                         && upgradeCell.GetComponent<Env_Item>().int_itemCount < requiredUpgradeCells_2)
                {
                    Debug.LogWarning("Error: Not enough upgrade cells to upgrade " + ability + " to tier 2!");
                }
                else if (abilityStatus == 2
                         && upgradeCell.GetComponent<Env_Item>().int_itemCount < requiredUpgradeCells_3)
                {
                    Debug.LogWarning("Error: Not enough upgrade cells to upgrade " + ability + " to tier 3!");
                }
            }

            //player has no upgrade cells
            if (upgradeCell == null)
            {
                Debug.LogWarning("Error: Player has no upgrade cells in inventory!");
            }
        }
        else if (isAbilityInUse
                 && (abilityStatus == 1
                 || abilityStatus == 2))
        {
            Debug.LogWarning("Error: Cannot upgrade " + ability + " because it is in use!");
        }
    }

    private void RemoveResources(int upgradeCellCount)
    {
        if (upgradeCell.GetComponent<Env_Item>().int_itemCount > upgradeCellCount)
        {
            upgradeCell.GetComponent<Env_Item>().int_itemCount -= upgradeCellCount;
        }
        else if (upgradeCell.GetComponent<Env_Item>().int_itemCount == upgradeCellCount)
        {
            PlayerInventoryScript.inventory.Remove(upgradeCell);
            PlayerInventoryScript.invSpace += upgradeCell.GetComponent<Env_Item>().int_ItemWeight * upgradeCellCount;

            par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(upgradeCell.GetComponent<Env_Item>().str_ItemName);

            Destroy(upgradeCell);
            upgradeCell = null;
        }
    }

    public void UseAbility()
    {
        if (!isAbilityInUse)
        {
            abilityLengthTimer = 0;
            abilityReuseCooldownTimer = 0;

            if (ability == Ability.jumpBoost)
            {
                UseAbility_JumpBoost();
            }
            else if (ability == Ability.sprintBoost)
            {
                UseAbility_SprintBoost();
            }
            else if (ability == Ability.healthRegen)
            {
                UseAbility_HealthRegen();
            }
            else if (ability == Ability.staminaRegen)
            {
                UseAbility_StaminaRegen();
            }
            else if (ability == Ability.envProtection)
            {
                UseAbility_EnvProtection();
            }
        }
        else
        {
            if (abilityLengthTimer > 0
                && abilityReuseCooldownTimer == 0)
            {
                Debug.LogWarning("Error: Cannot use " + ability + " because it is already in use!");
            }
            else
            {
                Debug.LogWarning("Error: Cannot use " + ability + " because its not yet ready!");
            }
        }
    }

    public void AssignToSlot1()
    {
        if (assignedSlot != 0)
        {
            //if no abilities have yet been added
            if (AbilityManagerScript.assignedAbilities.Count == 0)
            {
                AbilityManagerScript.assignedAbilities.Add(gameObject);

                assignedSlot = 0;
                AbilityManagerScript.abilityAssignTimer = 0;
            }
            //if something has been added to slot 1
            else if (AbilityManagerScript.assignedAbilities.Count >= 1)
            {
                //placeholder or other not in use ability
                if (AbilityManagerScript.assignedAbilities[0] != null)
                {
                    if (AbilityManagerScript.assignedAbilities[0].GetComponent<UI_Ability>() == null)
                    {
                        GameObject placeholder = AbilityManagerScript.assignedAbilities[0];
                        AbilityManagerScript.assignedAbilities.Remove(placeholder);
                        Destroy(placeholder);

                        AbilityManagerScript.assignedAbilities.Insert(0, gameObject);

                        assignedSlot = 0;
                        AbilityManagerScript.abilityAssignTimer = 0;
                    }
                    else if (AbilityManagerScript.assignedAbilities[0].GetComponent<UI_Ability>() != null
                             && !AbilityManagerScript.assignedAbilities[0].GetComponent<UI_Ability>().isAbilityInUse)
                    {
                        GameObject otherAbility = AbilityManagerScript.assignedAbilities[0];
                        AbilityManagerScript.assignedAbilities.Remove(otherAbility);

                        AbilityManagerScript.assignedAbilities.Insert(0, gameObject);

                        assignedSlot = 0;
                        AbilityManagerScript.abilityAssignTimer = 0;
                    }
                }
                //other in use ability
                else if (AbilityManagerScript.assignedAbilities[0].GetComponent<UI_Ability>() != null
                         && AbilityManagerScript.assignedAbilities[0].GetComponent<UI_Ability>().isAbilityInUse)
                {
                    Debug.LogWarning("Error: Cannot assign " + ability + " to slot 1 because another already in use ability is in that slot!");
                }
            }
        }
        else
        {
            Debug.LogWarning("Error: Cannot assign " + ability + " to slot 1 because it is already assigned there!");
        }
    }
    public void AssignToSlot2()
    {
        if (assignedSlot != 1)
        {
            //if no abilities have yet been added
            if (AbilityManagerScript.assignedAbilities.Count == 0)
            {
                //add placeholder to slot 1
                GameObject placeholder1 = new GameObject();
                AbilityManagerScript.assignedAbilities.Add(placeholder1);
                //add actual ability to slot 2
                AbilityManagerScript.assignedAbilities.Add(gameObject);

                assignedSlot = 1;
                AbilityManagerScript.abilityAssignTimer = 0;
            }
            //if something has been added to slot 2
            else if (AbilityManagerScript.assignedAbilities.Count >= 2)
            {
                //placeholder or other not in use ability
                if (AbilityManagerScript.assignedAbilities[1] != null)
                {
                    if (AbilityManagerScript.assignedAbilities[1].GetComponent<UI_Ability>() == null)
                    {
                        GameObject placeholder = AbilityManagerScript.assignedAbilities[1];
                        AbilityManagerScript.assignedAbilities.Remove(placeholder);
                        Destroy(placeholder);

                        AbilityManagerScript.assignedAbilities.Insert(1, gameObject);

                        assignedSlot = 1;
                        AbilityManagerScript.abilityAssignTimer = 0;
                    }
                    else if (AbilityManagerScript.assignedAbilities[1].GetComponent<UI_Ability>() != null
                             && !AbilityManagerScript.assignedAbilities[1].GetComponent<UI_Ability>().isAbilityInUse)
                    {
                        GameObject otherAbility = AbilityManagerScript.assignedAbilities[1];
                        AbilityManagerScript.assignedAbilities.Remove(otherAbility);

                        AbilityManagerScript.assignedAbilities.Insert(1, gameObject);

                        assignedSlot = 1;
                        AbilityManagerScript.abilityAssignTimer = 0;
                    }
                }
                //other in use ability
                else if (AbilityManagerScript.assignedAbilities[1].GetComponent<UI_Ability>() != null
                         && AbilityManagerScript.assignedAbilities[1].GetComponent<UI_Ability>().isAbilityInUse)
                {
                    Debug.LogWarning("Error: Cannot assign " + ability + " to slot 2 because another already in use ability is in that slot!");
                }
            }
        }
        else
        {
            Debug.LogWarning("Error: Cannot assign " + ability + " to slot 2 because it is already assigned there!");
        }
    }
    public void AssignToSlot3()
    {
        if (assignedSlot != 2)
        {
            //if no abilities have yet been added
            if (AbilityManagerScript.assignedAbilities.Count == 0)
            {
                //add placeholder to slots 1 and 2
                GameObject placeholder1 = new GameObject();
                AbilityManagerScript.assignedAbilities.Add(placeholder1);
                GameObject placeholder2 = new GameObject();
                AbilityManagerScript.assignedAbilities.Add(placeholder2);
                //add actual ability to slot 3
                AbilityManagerScript.assignedAbilities.Add(gameObject);

                assignedSlot = 2;
                AbilityManagerScript.abilityAssignTimer = 0;
            }
            //if something has been added to slot 3
            else if (AbilityManagerScript.assignedAbilities.Count == 3)
            {
                //placeholder or other not in use ability
                if (AbilityManagerScript.assignedAbilities[2] != null)
                {
                    if (AbilityManagerScript.assignedAbilities[2].GetComponent<UI_Ability>() == null)
                    {
                        GameObject placeholder = AbilityManagerScript.assignedAbilities[2];
                        AbilityManagerScript.assignedAbilities.Remove(placeholder);
                        Destroy(placeholder);

                        AbilityManagerScript.assignedAbilities.Add(gameObject);

                        assignedSlot = 2;
                        AbilityManagerScript.abilityAssignTimer = 0;
                    }
                    else if (AbilityManagerScript.assignedAbilities[2].GetComponent<UI_Ability>() != null
                             && !AbilityManagerScript.assignedAbilities[2].GetComponent<UI_Ability>().isAbilityInUse)
                    {
                        GameObject otherAbility = AbilityManagerScript.assignedAbilities[2];
                        AbilityManagerScript.assignedAbilities.Remove(otherAbility);

                        AbilityManagerScript.assignedAbilities.Add(gameObject);

                        assignedSlot = 2;
                        AbilityManagerScript.abilityAssignTimer = 0;
                    }
                }
                //other in use ability
                else if (AbilityManagerScript.assignedAbilities[2].GetComponent<UI_Ability>() != null
                         && AbilityManagerScript.assignedAbilities[2].GetComponent<UI_Ability>().isAbilityInUse)
                {
                    Debug.LogWarning("Error: Cannot assign " + ability + " to slot 3 because another already in use ability is in that slot!");
                }
            }
        }
        else
        {
            Debug.LogWarning("Error: Cannot assign " + ability + " to slot 3 because it is already assigned there!");
        }
    }

    private void UseAbility_JumpBoost()
    {
        if (abilityStatus == 1)
        {
            PlayerMovementScript.jumpBuff = abilityBoostValue1_1;
        }
        else if (abilityStatus == 2)
        {
            PlayerMovementScript.jumpBuff = abilityBoostValue2_1;
        }
        else if (abilityStatus == 3)
        {
            PlayerMovementScript.jumpBuff = abilityBoostValue3_1;
        }

        isAbilityInUse = true;
    }
    private void UseAbility_SprintBoost()
    {
        if (abilityStatus == 1)
        {
            PlayerMovementScript.sprintBuff = abilityBoostValue1_1;
        }
        else if (abilityStatus == 2)
        {
            PlayerMovementScript.sprintBuff = abilityBoostValue2_1;
        }
        else if (abilityStatus == 3)
        {
            PlayerMovementScript.sprintBuff = abilityBoostValue3_1;
        }

        isAbilityInUse = true;
    }
    private void UseAbility_HealthRegen()
    {
        if (abilityStatus == 1)
        {
            PlayerHealthScript.health += abilityBoostValue1_1;
        }
        else if (abilityStatus == 2)
        {
            PlayerHealthScript.health += abilityBoostValue2_1;
        }
        else if (abilityStatus == 3)
        {
            PlayerHealthScript.health += abilityBoostValue3_1;
        }

        isAbilityInUse = true;
    }
    private void UseAbility_StaminaRegen()
    {
        if (abilityStatus == 1)
        {
            PlayerMovementScript.staminaRegenBuff = abilityBoostValue1_1;
        }
        else if (abilityStatus == 2)
        {
            PlayerMovementScript.staminaRegenBuff = abilityBoostValue2_1;
        }
        else if (abilityStatus == 3)
        {
            PlayerMovementScript.staminaRegenBuff = abilityBoostValue3_1;
        }

        isAbilityInUse = true;
    }
    private void UseAbility_EnvProtection()
    {
        if (abilityStatus == 1)
        {
            PlayerMovementScript.staminaRegenBuff = abilityBoostValue1_1;
        }
        else if (abilityStatus == 2)
        {
            PlayerMovementScript.staminaRegenBuff = abilityBoostValue2_1;
        }
        else if (abilityStatus == 3)
        {
            PlayerMovementScript.staminaRegenBuff = abilityBoostValue3_1;
        }

        isAbilityInUse = true;
    }
}