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
    [SerializeField] private GameObject par_Managers;

    public void AssignToSlot1()
    {
        if (assignedAbility == AssignedAbility.jumpBoost
            && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.jumpBoost
            && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.jumpBoost)
        {
            Slot1Script.assignedAbility = UI_AbilitySlot1.AssignedAbility.jumpBoost;

            AssignKeysToAssignedAbilities();

            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 1!");
        }
        else if (assignedAbility == AssignedAbility.sprintBoost
                 && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.sprintBoost
                 && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.sprintBoost)
        {
            Slot1Script.assignedAbility = UI_AbilitySlot1.AssignedAbility.sprintBoost;

            AssignKeysToAssignedAbilities();

            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 1!");
        }
        else if (assignedAbility == AssignedAbility.healthRegen
                 && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.healthRegen
                 && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.healthRegen)
        {
            Slot1Script.assignedAbility = UI_AbilitySlot1.AssignedAbility.healthRegen;

            AssignKeysToAssignedAbilities();

            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 1!");
        }
        else if (assignedAbility == AssignedAbility.staminaRegen
                 && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.staminaRegen
                 && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.staminaRegen)
        {
            Slot1Script.assignedAbility = UI_AbilitySlot1.AssignedAbility.staminaRegen;

            AssignKeysToAssignedAbilities();

            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 1!");
        }
        else if (assignedAbility == AssignedAbility.envProtection
                 && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.envProtection
                 && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.envProtection)
        {
            Slot1Script.assignedAbility = UI_AbilitySlot1.AssignedAbility.envProtection;

            AssignKeysToAssignedAbilities();

            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot1.onClick.AddListener(Slot1Script.UseAbility);

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

            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 2!");
        }
        else if (assignedAbility == AssignedAbility.sprintBoost
                 && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.sprintBoost
                 && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.sprintBoost)
        {
            Slot2Script.assignedAbility = UI_AbilitySlot2.AssignedAbility.sprintBoost;

            AssignKeysToAssignedAbilities();

            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 2!");
        }
        else if (assignedAbility == AssignedAbility.healthRegen
                 && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.healthRegen
                 && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.healthRegen)
        {
            Slot2Script.assignedAbility = UI_AbilitySlot2.AssignedAbility.healthRegen;

            AssignKeysToAssignedAbilities();

            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 2");
        }
        else if (assignedAbility == AssignedAbility.staminaRegen
                 && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.staminaRegen
                 && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.staminaRegen)
        {
            Slot2Script.assignedAbility = UI_AbilitySlot2.AssignedAbility.staminaRegen;

            AssignKeysToAssignedAbilities();

            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 2!");
        }
        else if (assignedAbility == AssignedAbility.envProtection
                 && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.envProtection
                 && Slot3Script.assignedAbility != UI_AbilitySlot3.AssignedAbility.envProtection)
        {
            Slot2Script.assignedAbility = UI_AbilitySlot2.AssignedAbility.envProtection;

            AssignKeysToAssignedAbilities();

            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot2.onClick.AddListener(Slot1Script.UseAbility);

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

            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 3!");
        }
        else if (assignedAbility == AssignedAbility.sprintBoost
                 && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.sprintBoost
                 && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.sprintBoost)
        {
            Slot3Script.assignedAbility = UI_AbilitySlot3.AssignedAbility.sprintBoost;

            AssignKeysToAssignedAbilities();

            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 3!");
        }
        else if (assignedAbility == AssignedAbility.healthRegen
                 && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.healthRegen
                 && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.healthRegen)
        {
            Slot3Script.assignedAbility = UI_AbilitySlot3.AssignedAbility.healthRegen;

            AssignKeysToAssignedAbilities();

            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 3!");
        }
        else if (assignedAbility == AssignedAbility.staminaRegen
                 && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.staminaRegen
                 && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.staminaRegen)
        {
            Slot3Script.assignedAbility = UI_AbilitySlot3.AssignedAbility.staminaRegen;

            AssignKeysToAssignedAbilities();

            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 3!");
        }
        else if (assignedAbility == AssignedAbility.envProtection
                 && Slot1Script.assignedAbility != UI_AbilitySlot1.AssignedAbility.envProtection
                 && Slot2Script.assignedAbility != UI_AbilitySlot2.AssignedAbility.envProtection)
        {
            Slot3Script.assignedAbility = UI_AbilitySlot3.AssignedAbility.envProtection;

            AssignKeysToAssignedAbilities();

            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_AbilitySlot3.onClick.AddListener(Slot1Script.UseAbility);

            //Debug.Log("Assigned " + assignedAbility.ToString() + " to slot 3!");
        }
    }

    private void AssignKeysToAssignedAbilities()
    {
        //clear all keys
        par_Managers.GetComponent<Manager_UIReuse>().txt_abilityJumpBoostAssignedKey.text = "";
        par_Managers.GetComponent<Manager_UIReuse>().txt_abilitySprintBoostAssignedKey.text = "";
        par_Managers.GetComponent<Manager_UIReuse>().txt_abilityHealthRegenAssignedKey.text = "";
        par_Managers.GetComponent<Manager_UIReuse>().txt_abilityStaminaRegenAssignedKey.text = "";
        par_Managers.GetComponent<Manager_UIReuse>().txt_abilityEnvProtectionAssignedKey.text = "";

        //slot 1 key
        if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.jumpBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_abilityJumpBoostAssignedKey.text = "1";
        }
        else if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.sprintBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_abilitySprintBoostAssignedKey.text = "1";
        }
        else if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.healthRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_abilityHealthRegenAssignedKey.text = "1";
        }
        else if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.staminaRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_abilityStaminaRegenAssignedKey.text = "1";
        }
        else if (Slot1Script.assignedAbility == UI_AbilitySlot1.AssignedAbility.envProtection)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_abilityEnvProtectionAssignedKey.text = "1";
        }

        //slot 2 key
        if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.jumpBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_abilityJumpBoostAssignedKey.text = "2";
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.sprintBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_abilitySprintBoostAssignedKey.text = "2";
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.healthRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_abilityHealthRegenAssignedKey.text = "2";
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.staminaRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_abilityStaminaRegenAssignedKey.text = "2";
        }
        else if (Slot2Script.assignedAbility == UI_AbilitySlot2.AssignedAbility.envProtection)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_abilityEnvProtectionAssignedKey.text = "2";
        }

        //slot 3 key
        if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.jumpBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_abilityJumpBoostAssignedKey.text = "3";
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.sprintBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_abilitySprintBoostAssignedKey.text = "3";
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.healthRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_abilityHealthRegenAssignedKey.text = "3";
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.staminaRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_abilityStaminaRegenAssignedKey.text = "3";
        }
        else if (Slot3Script.assignedAbility == UI_AbilitySlot3.AssignedAbility.envProtection)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_abilityEnvProtectionAssignedKey.text = "3";
        }
    }
}