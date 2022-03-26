using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_AssignThisAbilityToSlot : MonoBehaviour
{
    [Header("Assignables")]
    public AssignedAbility assignedAbility;
    public enum AssignedAbility
    {
        unassigned,
        jumpBoost,
        sprintBoost,
        healthRegen,
        staminaRegen,
        envProtection
    }
    [SerializeField] private UI_AbilitySlot1 Slot1Script;
    [SerializeField] private UI_AbilitySlot2 Slot2Script;
    [SerializeField] private UI_AbilitySlot3 Slot3Script;
    [SerializeField] private Manager_UIReuse UIReuseScript;

    public void AssignToSlot1()
    {
        if (assignedAbility == AssignedAbility.jumpBoost
            && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.jumpBoost
            && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.jumpBoost)
        {
            Slot1Script.assignedAbility = UI_AbilitySlot1.AssignedAbility.jumpBoost;

            AssignKeysToAssignedAbilities();

            UIReuseScript.btn_AbilitySlot1.onClick.RemoveAllListeners();
            UIReuseScript.btn_AbilitySlot1.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 1!");
        }
        else if (assignedAbility == AssignedAbility.sprintBoost
                 && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.sprintBoost
                 && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.sprintBoost)
        {
            Slot1Script.assignedAbility = UI_AbilitySlot1.AssignedAbility.sprintBoost;

            AssignKeysToAssignedAbilities();

            UIReuseScript.btn_AbilitySlot1.onClick.RemoveAllListeners();
            UIReuseScript.btn_AbilitySlot1.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 1!");
        }
        else if (assignedAbility == AssignedAbility.healthRegen
                 && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.healthRegen
                 && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.healthRegen)
        {
            Slot1Script.assignedAbility = UI_AbilitySlot1.AssignedAbility.healthRegen;

            AssignKeysToAssignedAbilities();

            UIReuseScript.btn_AbilitySlot1.onClick.RemoveAllListeners();
            UIReuseScript.btn_AbilitySlot1.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 1!");
        }
        else if (assignedAbility == AssignedAbility.staminaRegen
                 && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.staminaRegen
                 && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.staminaRegen)
        {
            Slot1Script.assignedAbility = UI_AbilitySlot1.AssignedAbility.staminaRegen;

            AssignKeysToAssignedAbilities();

            UIReuseScript.btn_AbilitySlot1.onClick.RemoveAllListeners();
            UIReuseScript.btn_AbilitySlot1.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 1!");
        }
        else if (assignedAbility == AssignedAbility.envProtection
                 && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.envProtection
                 && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.envProtection)
        {
            Slot1Script.assignedAbility = UI_AbilitySlot1.AssignedAbility.envProtection;

            AssignKeysToAssignedAbilities();

            UIReuseScript.btn_AbilitySlot1.onClick.RemoveAllListeners();
            UIReuseScript.btn_AbilitySlot1.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 1!");
        }
    }

    public void AssignToSlot2()
    {
        if (assignedAbility == AssignedAbility.jumpBoost
            && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.jumpBoost
            && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.jumpBoost)
        {
            Slot2Script.assignedAbility = UI_AbilitySlot2.AssignedAbility.jumpBoost;

            AssignKeysToAssignedAbilities();

            UIReuseScript.btn_AbilitySlot2.onClick.RemoveAllListeners();
            UIReuseScript.btn_AbilitySlot2.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 2!");
        }
        else if (assignedAbility == AssignedAbility.sprintBoost
                 && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.sprintBoost
                 && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.sprintBoost)
        {
            Slot2Script.assignedAbility = UI_AbilitySlot2.AssignedAbility.sprintBoost;

            AssignKeysToAssignedAbilities();

            UIReuseScript.btn_AbilitySlot2.onClick.RemoveAllListeners();
            UIReuseScript.btn_AbilitySlot2.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 2!");
        }
        else if (assignedAbility == AssignedAbility.healthRegen
                 && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.healthRegen
                 && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.healthRegen)
        {
            Slot2Script.assignedAbility = UI_AbilitySlot2.AssignedAbility.healthRegen;

            AssignKeysToAssignedAbilities();

            UIReuseScript.btn_AbilitySlot2.onClick.RemoveAllListeners();
            UIReuseScript.btn_AbilitySlot2.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 2");
        }
        else if (assignedAbility == AssignedAbility.staminaRegen
                 && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.staminaRegen
                 && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.staminaRegen)
        {
            Slot2Script.assignedAbility = UI_AbilitySlot2.AssignedAbility.staminaRegen;

            AssignKeysToAssignedAbilities();

            UIReuseScript.btn_AbilitySlot2.onClick.RemoveAllListeners();
            UIReuseScript.btn_AbilitySlot2.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 2!");
        }
        else if (assignedAbility == AssignedAbility.envProtection
                 && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.envProtection
                 && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.envProtection)
        {
            Slot2Script.assignedAbility = UI_AbilitySlot2.AssignedAbility.envProtection;

            AssignKeysToAssignedAbilities();

            UIReuseScript.btn_AbilitySlot2.onClick.RemoveAllListeners();
            UIReuseScript.btn_AbilitySlot2.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 2!");
        }
    }

    public void AssignToSlot3()
    {
        if (assignedAbility == AssignedAbility.jumpBoost
            && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.jumpBoost
            && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.jumpBoost)
        {
            Slot3Script.assignedAbility = UI_AbilitySlot3.AssignedAbility.jumpBoost;

            AssignKeysToAssignedAbilities();

            UIReuseScript.btn_AbilitySlot3.onClick.RemoveAllListeners();
            UIReuseScript.btn_AbilitySlot3.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 3!");
        }
        else if (assignedAbility == AssignedAbility.sprintBoost
                 && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.sprintBoost
                 && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.sprintBoost)
        {
            Slot3Script.assignedAbility = UI_AbilitySlot3.AssignedAbility.sprintBoost;

            AssignKeysToAssignedAbilities();

            UIReuseScript.btn_AbilitySlot3.onClick.RemoveAllListeners();
            UIReuseScript.btn_AbilitySlot3.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 3!");
        }
        else if (assignedAbility == AssignedAbility.healthRegen
                 && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.healthRegen
                 && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.healthRegen)
        {
            Slot3Script.assignedAbility = UI_AbilitySlot3.AssignedAbility.healthRegen;

            AssignKeysToAssignedAbilities();

            UIReuseScript.btn_AbilitySlot3.onClick.RemoveAllListeners();
            UIReuseScript.btn_AbilitySlot3.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 3!");
        }
        else if (assignedAbility == AssignedAbility.staminaRegen
                 && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.staminaRegen
                 && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.staminaRegen)
        {
            Slot3Script.assignedAbility = UI_AbilitySlot3.AssignedAbility.staminaRegen;

            AssignKeysToAssignedAbilities();

            UIReuseScript.btn_AbilitySlot3.onClick.RemoveAllListeners();
            UIReuseScript.btn_AbilitySlot3.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 3!");
        }
        else if (assignedAbility == AssignedAbility.envProtection
                 && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.envProtection
                 && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.envProtection)
        {
            Slot3Script.assignedAbility = UI_AbilitySlot3.AssignedAbility.envProtection;

            AssignKeysToAssignedAbilities();

            UIReuseScript.btn_AbilitySlot3.onClick.RemoveAllListeners();
            UIReuseScript.btn_AbilitySlot3.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 3!");
        }
    }

    private void AssignKeysToAssignedAbilities()
    {
        //clear all keys
        UIReuseScript.txt_abilityJumpBoostAssignedKey.text = "";
        UIReuseScript.txt_abilitySprintBoostAssignedKey.text = "";
        UIReuseScript.txt_abilityHealthRegenAssignedKey.text = "";
        UIReuseScript.txt_abilityStaminaRegenAssignedKey.text = "";
        UIReuseScript.txt_abilityEnvProtectionAssignedKey.text = "";

        //slot 1 key
        if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.jumpBoost)
        {
            UIReuseScript.txt_abilityJumpBoostAssignedKey.text = "1";
        }
        else if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.sprintBoost)
        {
            UIReuseScript.txt_abilitySprintBoostAssignedKey.text = "1";
        }
        else if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.healthRegen)
        {
            UIReuseScript.txt_abilityHealthRegenAssignedKey.text = "1";
        }
        else if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.staminaRegen)
        {
            UIReuseScript.txt_abilityStaminaRegenAssignedKey.text = "1";
        }
        else if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.envProtection)
        {
            UIReuseScript.txt_abilityEnvProtectionAssignedKey.text = "1";
        }

        //slot 2 key
        if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.jumpBoost)
        {
            UIReuseScript.txt_abilityJumpBoostAssignedKey.text = "2";
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.sprintBoost)
        {
            UIReuseScript.txt_abilitySprintBoostAssignedKey.text = "2";
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.healthRegen)
        {
            UIReuseScript.txt_abilityHealthRegenAssignedKey.text = "2";
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.staminaRegen)
        {
            UIReuseScript.txt_abilityStaminaRegenAssignedKey.text = "2";
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.envProtection)
        {
            UIReuseScript.txt_abilityEnvProtectionAssignedKey.text = "2";
        }

        //slot 3 key
        if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.jumpBoost)
        {
            UIReuseScript.txt_abilityJumpBoostAssignedKey.text = "3";
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.sprintBoost)
        {
            UIReuseScript.txt_abilitySprintBoostAssignedKey.text = "3";
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.healthRegen)
        {
            UIReuseScript.txt_abilityHealthRegenAssignedKey.text = "3";
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.staminaRegen)
        {
            UIReuseScript.txt_abilityStaminaRegenAssignedKey.text = "3";
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.envProtection)
        {
            UIReuseScript.txt_abilityEnvProtectionAssignedKey.text = "3";
        }
    }
}