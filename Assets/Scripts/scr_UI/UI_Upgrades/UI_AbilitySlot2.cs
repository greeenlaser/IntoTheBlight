using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_AbilitySlot2 : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private Player_Exoskeleton ExoskeletonScript;

    //public but hidden variables
    [HideInInspector] public AssignedAbility assignedAbility;
    [HideInInspector]
    public enum AssignedAbility
    {
        unassigned,
        jumpBoost,
        sprintBoost,
        healthRegen,
        staminaRegen,
        envProtection
    }

    public void UseAbility()
    {
        if (assignedAbility == AssignedAbility.jumpBoost
            && ExoskeletonScript.unlockedAbility_jumpBoost
            && !ExoskeletonScript.usingAbility_jumpBoost)
        {
            ExoskeletonScript.UseAbility_jumpBoost();
            //Debug.Log("Used " + assignedAbility.ToString() + " in slot 2!");
        }
        else if (assignedAbility == AssignedAbility.sprintBoost
                 && ExoskeletonScript.unlockedAbility_sprintBoost
                 && !ExoskeletonScript.usingAbility_sprintBoost)
        {
            ExoskeletonScript.UseAbility_sprintBoost();
            //Debug.Log("Used " + assignedAbility.ToString() + " in slot 2!");
        }
        else if (assignedAbility == AssignedAbility.healthRegen
                 && ExoskeletonScript.unlockedAbility_healthRegen
                 && !ExoskeletonScript.usingAbility_healthRegen)
        {
            ExoskeletonScript.UseAbility_healthRegen();
            //Debug.Log("Used " + assignedAbility.ToString() + " in slot 2!");
        }
        else if (assignedAbility == AssignedAbility.staminaRegen
                 && ExoskeletonScript.unlockedAbility_staminaRegen
                 && !ExoskeletonScript.usingAbility_staminaRegen)
        {
            ExoskeletonScript.UseAbility_staminaRegen();
            //Debug.Log("Used " + assignedAbility.ToString() + " in slot 2!");
        }
        else if (assignedAbility == AssignedAbility.envProtection
                 && ExoskeletonScript.unlockedAbility_envProtection
                 && !ExoskeletonScript.usingAbility_envProtection)
        {
            ExoskeletonScript.UseAbility_envProtection();
            //Debug.Log("Used " + assignedAbility.ToString() + " in slot 2!");
        }
    }
}