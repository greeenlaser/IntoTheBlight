using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_AbilityStatus : MonoBehaviour
{
    [Header("Assignables")]
    //unlocking ability
    [SerializeField] private int uc_unlock;
    [SerializeField] private float rcp_unlock;
    //upgrading to tier 2
    [SerializeField] private int uc_upgrade_tier2;
    [SerializeField] private float rcp_upgrade_tier2;
    //upgrading to tier 3
    [SerializeField] private int uc_upgrade_tier3;
    [SerializeField] private float rcp_upgrade_tier3;
    //----
    public Ability ability;
    public enum Ability
    {
        unassigned,
        jumpBoost,
        sprintBoost,
        healthRegen,
        staminaRegen,
        envProtection
    }
    [SerializeField] private Player_Exoskeleton ExoskeletonScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;

    //public but hidden variables
    [HideInInspector] public bool isUnlocked;
    [HideInInspector] public bool isTier2;
    [HideInInspector] public bool isTier3;

    //private variables
    private bool requirementsMet;

    //check if player has the required resources to unlock/upgrade the chosen ability
    private void CheckRequirements()
    {
        //when unlocking ability
        if (!isUnlocked 
            && ExoskeletonScript.remainingRadCellPower >= rcp_unlock
            && ExoskeletonScript.remainingUpgradeCells >= uc_unlock)
        {
            ExoskeletonScript.remainingRadCellPower -= rcp_unlock;
            ExoskeletonScript.remainingUpgradeCells -= uc_unlock;

            ExoskeletonScript.UpdateCellValues();

            requirementsMet = true;
        }

        //when upgrading ability to tier 2
        else if (isUnlocked
                 && !isTier2
                 && ExoskeletonScript.remainingRadCellPower >= rcp_upgrade_tier2
                 && ExoskeletonScript.remainingUpgradeCells >= uc_upgrade_tier2)
        {
            ExoskeletonScript.remainingRadCellPower -= rcp_upgrade_tier2;
            ExoskeletonScript.remainingUpgradeCells -= uc_upgrade_tier2;

            ExoskeletonScript.UpdateCellValues();

            requirementsMet = true;
        }

        //when upgrading ability to tier 3
        else if (isUnlocked
                 && isTier2
                 && !isTier3
                 && ExoskeletonScript.remainingRadCellPower >= rcp_upgrade_tier3
                 && ExoskeletonScript.remainingUpgradeCells >= uc_upgrade_tier3)
        {
            ExoskeletonScript.remainingRadCellPower -= rcp_upgrade_tier3;
            ExoskeletonScript.remainingUpgradeCells -= uc_upgrade_tier3;

            ExoskeletonScript.UpdateCellValues();

            requirementsMet = true;
        }

        else
        {
            Debug.LogWarning("Error: Insufficient resources to unlock or upgrade " + ability.ToString() + "!");
        }
    }

    public void UnlockAbility()
    {
        //if ability is not yet unlocked
        if (!isUnlocked)
        {
            requirementsMet = false;

            //unlock jump boost
            if (ability == Ability.jumpBoost)
            {
                CheckRequirements();
                if (requirementsMet)
                {
                    ExoskeletonScript.UnlockAbility_jumpBoost();

                    //Debug.Log("Unlocked " + ability.ToString() + "!");
                    isUnlocked = true;
                }
            }

            //unlock sprint boost
            else if (ability == Ability.sprintBoost)
            {
                CheckRequirements();
                if (requirementsMet)
                {
                    ExoskeletonScript.UnlockAbility_sprintBoost();

                    //Debug.Log("Unlocked " + ability.ToString() + "!");
                    isUnlocked = true;
                }
            }

            //unlock health regen
            else if (ability == Ability.healthRegen)
            {
                CheckRequirements();
                if (requirementsMet)
                {
                    ExoskeletonScript.UnlockAbility_healthRegen();

                    //Debug.Log("Unlocked " + ability.ToString() + "!");
                    isUnlocked = true;
                }
            }

            //unlock stamina regen
            else if (ability == Ability.staminaRegen)
            {
                CheckRequirements();
                if (requirementsMet)
                {
                    ExoskeletonScript.UnlockAbility_staminaRegen();

                    //Debug.Log("Unlocked " + ability.ToString() + "!");
                    isUnlocked = true;
                }
            }

            //unlock env protection
            else if (ability == Ability.envProtection)
            {
                CheckRequirements();
                if (requirementsMet)
                {
                    ExoskeletonScript.UnlockAbility_envProtection();

                    //Debug.Log("Unlocked " + ability.ToString() + "!");
                    isUnlocked = true;
                }
            }
        }

        //if ability is already unlocked
        else if (isUnlocked)
        {
            UpgradeAbility();
        }
    }

    private void UpgradeAbility()
    {
        requirementsMet = false;

        //upgrade jump boost
        if (ability == Ability.jumpBoost && isUnlocked)
        {
            if (!isTier2)
            {
                CheckRequirements();
                if (requirementsMet)
                {
                    ExoskeletonScript.UpgradeAbility_jumpBoost_Tier2();

                    //Debug.Log("Upgraded " + ability.ToString() + " to tier 2!");
                    isTier2 = true;
                }
            }
            else if (isTier2 && !isTier3)
            {
                CheckRequirements();
                if (requirementsMet)
                {
                    ExoskeletonScript.UpgradeAbility_jumpBoost_Tier3();
                    UIReuseScript.btn_JumpBoost.interactable = false;

                    //Debug.Log("Upgraded " + ability.ToString() + " to tier 3!");
                    isTier3 = true;
                }
            }
        }

        //upgrade sprint boost
        else if (ability == Ability.sprintBoost && isUnlocked)
        {
            if (!isTier2)
            {
                CheckRequirements();
                if (requirementsMet)
                {
                    ExoskeletonScript.UpgradeAbility_sprintBoost_Tier2();

                    //Debug.Log("Upgraded " + ability.ToString() + " to tier 2!");
                    isTier2 = true;
                }
            }
            else if (isTier2 && !isTier3)
            {
                CheckRequirements();
                if (requirementsMet)
                {
                    ExoskeletonScript.UpgradeAbility_sprintBoost_Tier3();
                    UIReuseScript.btn_SprintBoost.interactable = false;

                    //Debug.Log("Upgraded " + ability.ToString() + " to tier 3!");
                    isTier3 = true;
                }
            }
        }

        //upgrade health regen
        else if (ability == Ability.healthRegen && isUnlocked)
        {
            if (!isTier2)
            {
                CheckRequirements();
                if (requirementsMet)
                {
                    ExoskeletonScript.UpgradeAbility_healthRegen_Tier2();

                    //Debug.Log("Upgraded " + ability.ToString() + " to tier 2!");
                    isTier2 = true;
                }
            }
            else if (isTier2 && !isTier3)
            {
                CheckRequirements();
                if (requirementsMet)
                {
                    ExoskeletonScript.UpgradeAbility_healthRegen_Tier3();
                    UIReuseScript.btn_HealthRegen.interactable = false;

                    //Debug.Log("Upgraded " + ability.ToString() + " to tier 3!");
                    isTier3 = true;
                }
            }
        }

        //upgrade stamina regen
        else if (ability == Ability.staminaRegen && isUnlocked)
        {
            if (!isTier2)
            {
                CheckRequirements();
                if (requirementsMet)
                {
                    ExoskeletonScript.UpgradeAbility_staminaRegen_Tier2();

                    //Debug.Log("Upgraded " + ability.ToString() + " to tier 2!");
                    isTier2 = true;
                }
            }
            else if (isTier2 && !isTier3)
            {
                CheckRequirements();
                if (requirementsMet)
                {
                    ExoskeletonScript.UpgradeAbility_staminaRegen_Tier3();
                    UIReuseScript.btn_StaminaRegen.interactable = false;

                    //Debug.Log("Upgraded " + ability.ToString() + " to tier 3!");
                    isTier3 = true;
                }
            }
        }

        //upgrade env protection
        else if (ability == Ability.envProtection && isUnlocked)
        {
            if (!isTier2)
            {
                CheckRequirements();
                if (requirementsMet)
                {
                    ExoskeletonScript.UpgradeAbility_envProtection_Tier2();

                    //Debug.Log("Upgraded " + ability.ToString() + " to tier 2!");
                    isTier2 = true;
                }
            }
            else if (isTier2 && !isTier3)
            {
                CheckRequirements();
                if (requirementsMet)
                {
                    ExoskeletonScript.UpgradeAbility_envProtection_Tier3();
                    UIReuseScript.btn_EnvProtection.interactable = false;

                    //Debug.Log("Upgraded " + ability.ToString() + " to tier 3!");
                    isTier3 = true;
                }
            }
        }
    }
}