using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_AbilityAssignManager : MonoBehaviour
{
    [Header("Assignables")]
    [Range(0.1f, 1f)]
    [SerializeField] private float maxWait;
    [SerializeField] private UI_AbilitySlot1 Slot1Script;
    [SerializeField] private UI_AbilitySlot2 Slot2Script;
    [SerializeField] private UI_AbilitySlot3 Slot3Script;
    //----
    [SerializeField] private Player_Health PlayerHealthScript;
    [SerializeField] private Player_Exoskeleton ExoskeletonScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool hasExosuit;
    [HideInInspector] public bool assingingToSlot1;
    [HideInInspector] public bool assingingToSlot2;
    [HideInInspector] public bool assingingToSlot3;

    //private variables
    private bool calledAbilityAssignLoadOnce;
    private bool canIncreaseTimer;
    private float timer;

    private void Update()
    {
        if (PlayerHealthScript.health > 0
            && !par_Managers.GetComponent<UI_PauseMenu>().isGamePaused
            && !par_Managers.GetComponent<Manager_Console>().consoleOpen
            && hasExosuit)
        {
            //assinging ability to slot 1
            if (Input.GetKey(KeyCode.Z)
                && !assingingToSlot2
                && !assingingToSlot3
                && !calledAbilityAssignLoadOnce)
            {
                if (timer == 0)
                {
                    canIncreaseTimer = true;
                }
                else if (timer > maxWait)
                {
                    canIncreaseTimer = false;
                    par_Managers.GetComponent<UI_PauseMenu>().PauseGame();
                    AssignToSlot1();
                }
            }
            //assinging ability to slot 2
            else if (Input.GetKey(KeyCode.X)
                     && !assingingToSlot1
                     && !assingingToSlot3
                     && !calledAbilityAssignLoadOnce)
            {
                if (timer == 0)
                {
                    canIncreaseTimer = true;
                }
                else if (timer > maxWait)
                {
                    canIncreaseTimer = false;
                    par_Managers.GetComponent<UI_PauseMenu>().PauseGame();
                    AssignToSlot2();
                }
            }
            //assinging ability to slot 3
            else if (Input.GetKey(KeyCode.C)
                     && !assingingToSlot1
                     && !assingingToSlot2
                     && !calledAbilityAssignLoadOnce)
            {
                if (timer == 0)
                {
                    canIncreaseTimer = true;
                }
                else if (timer > maxWait)
                {
                    canIncreaseTimer = false;
                    par_Managers.GetComponent<UI_PauseMenu>().PauseGame();
                    AssignToSlot3();
                }
            }

            //using ability in first slot
            else if (Input.GetKeyUp(KeyCode.Z)
                     && timer > 0
                     && timer < maxWait
                     && Slot1Script.assignedAbility
                     != UI_AbilitySlot1.AssignedAbility.unassigned)
            {
                Slot1Script.UseAbility();
                timer = 0;
            }
            //using ability in second slot
            else if (Input.GetKeyUp(KeyCode.X)
                     && timer > 0
                     && timer < maxWait
                     && Slot2Script.assignedAbility
                     != UI_AbilitySlot2.AssignedAbility.unassigned)
            {
                Slot2Script.UseAbility();
                timer = 0;
            }
            //using ability in third slot
            else if (Input.GetKeyUp(KeyCode.C)
                     && timer > 0
                     && timer < maxWait
                     && Slot3Script.assignedAbility
                     != UI_AbilitySlot3.AssignedAbility.unassigned)
            {
                Slot3Script.UseAbility();
                timer = 0;
            }
            /*
            //custom error for not having any ability assigned to the pressed ability key
            else if (timer > 0
                     && timer < maxWait
                     && (Input.GetKeyUp(KeyCode.Z)
                     && Slot1Script.assignedAbility
                     == UI_AbilitySlot1.AssignedAbility.unassigned)
                     || (Input.GetKeyUp(KeyCode.X)
                     && Slot2Script.assignedAbility
                     == UI_AbilitySlot2.AssignedAbility.unassigned)
                     || (Input.GetKeyUp(KeyCode.C)
                     && Slot3Script.assignedAbility
                     == UI_AbilitySlot3.AssignedAbility.unassigned))
            {
                Debug.LogWarning("Error: No ability has been assigned to this key yet!");
            }
            */

            if (canIncreaseTimer)
            {
                timer += Time.deltaTime;
            }
            if (canIncreaseTimer
                && !Input.GetKey(KeyCode.Z)
                && !Input.GetKey(KeyCode.X)
                && !Input.GetKey(KeyCode.C))
            {
                timer = 0;
                canIncreaseTimer = false;
            }
            //Debug.Log(timer);
        }

        //closes UI if Z, X and C arent held down
        if (calledAbilityAssignLoadOnce
            && !Input.GetKey(KeyCode.Z)
            && !Input.GetKey(KeyCode.X)
            && !Input.GetKey(KeyCode.C))
        {
            CloseAbilityUI();
            par_Managers.GetComponent<UI_PauseMenu>().UnpauseGame();
        }
        //resets timer if ability keys were released
        //and timer was over 0 and no ability was actually assigned
        else if (!calledAbilityAssignLoadOnce
                 && !Input.GetKey(KeyCode.Z)
                 && !Input.GetKey(KeyCode.X)
                 && !Input.GetKey(KeyCode.C)
                 && timer > 0)
        {
            timer = 0;
        }
    }

    private void AssignToSlot1()
    {
        timer = 0;

        assingingToSlot1 = true;

        par_Managers.GetComponent<Manager_UIReuse>().par_AbilityUI.SetActive(true);

        if (ExoskeletonScript.unlockedAbility_jumpBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.onClick.AddListener(par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot1);
        }
        if (ExoskeletonScript.unlockedAbility_sprintBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.onClick.AddListener(par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot1);
        }
        if (ExoskeletonScript.unlockedAbility_healthRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.onClick.AddListener(par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot1);
        }
        if (ExoskeletonScript.unlockedAbility_staminaRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.onClick.AddListener(par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot1);
        }
        if (ExoskeletonScript.unlockedAbility_envProtection)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.onClick.AddListener(par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot1);
        }

        calledAbilityAssignLoadOnce = true;
    }

    private void AssignToSlot2()
    {
        timer = 0;

        assingingToSlot2 = true;

        par_Managers.GetComponent<Manager_UIReuse>().par_AbilityUI.SetActive(true);

        if (ExoskeletonScript.unlockedAbility_jumpBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.onClick.AddListener(par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot2);
        }
        if (ExoskeletonScript.unlockedAbility_sprintBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.onClick.AddListener(par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot2);
        }
        if (ExoskeletonScript.unlockedAbility_healthRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.onClick.AddListener(par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot2);
        }
        if (ExoskeletonScript.unlockedAbility_staminaRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.onClick.AddListener(par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot2);
        }
        if (ExoskeletonScript.unlockedAbility_envProtection)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.onClick.AddListener(par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot2);
        }

        calledAbilityAssignLoadOnce = true;
    }

    private void AssignToSlot3()
    {
        timer = 0;

        assingingToSlot3 = true;

        par_Managers.GetComponent<Manager_UIReuse>().par_AbilityUI.SetActive(true);

        if (ExoskeletonScript.unlockedAbility_jumpBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.onClick.AddListener(par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot3);
        }
        if (ExoskeletonScript.unlockedAbility_sprintBoost)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.onClick.AddListener(par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot3);
        }
        if (ExoskeletonScript.unlockedAbility_healthRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.onClick.AddListener(par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot3);
        }
        if (ExoskeletonScript.unlockedAbility_staminaRegen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.onClick.AddListener(par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot3);
        }
        if (ExoskeletonScript.unlockedAbility_envProtection)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.interactable = true;
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.onClick.RemoveAllListeners();
            par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.onClick.AddListener(par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot3);
        }

        calledAbilityAssignLoadOnce = true;
    }

    private void CloseAbilityUI()
    {
        timer = 0;
        canIncreaseTimer = false;

        par_Managers.GetComponent<Manager_UIReuse>().ClearAssignUI();

        assingingToSlot1 = false;
        assingingToSlot2 = false;
        assingingToSlot3 = false;

        calledAbilityAssignLoadOnce = false;
    }
}