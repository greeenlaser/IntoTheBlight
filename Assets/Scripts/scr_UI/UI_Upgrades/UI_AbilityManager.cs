using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_AbilityManager : MonoBehaviour
{
    [Header("Abilities")]
    [SerializeField] private Transform pos_ButtonHide;
    public List<GameObject> abilities;
    [SerializeField] private GameObject assignedAbilitiesPlaceholder1;
    [SerializeField] private GameObject assignedAbilitiesPlaceholder2;
    [SerializeField] private GameObject assignedAbilitiesPlaceholder3;

    [Header("Scripts")]
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private Player_Health PlayerHealthScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool hasExoskeleton;
    [HideInInspector] public int upgradeCellCount;
    [HideInInspector] public List<GameObject> slots;

    //private variables
    private bool startAssignTimer;
    private bool calledAbilityAssignUIOnce;
    private int selectedSlot;
    private float assignTimer;
    private GameObject upgradeCell;
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        UIReuseScript = par_Managers.GetComponent<Manager_UIReuse>();

        HideButtons();
        FillEmptyAbilitySlots();
    }

    private void Update()
    {
        if (hasExoskeleton
            && !par_Managers.GetComponent<Manager_Console>().consoleOpen
            && !par_Managers.GetComponent<Manager_GameSaving>().isLoading
            && PlayerHealthScript.isPlayerAlive)
        {
            //if ability counter hasnt started
            if (!startAssignTimer
                && !par_Managers.GetComponent<UI_PauseMenu>().isGamePaused)
            {
                if (Input.GetKey(KeyCode.Z))
                {
                    selectedSlot = 1;

                    startAssignTimer = true;
                }
                else if (Input.GetKey(KeyCode.X))
                {
                    selectedSlot = 2;

                    startAssignTimer = true;
                }
                else if (Input.GetKey(KeyCode.C))
                {
                    selectedSlot = 3;

                    startAssignTimer = true;
                }
            }
            //if ability counter has started
            else if (startAssignTimer)
            {
                assignTimer += Time.deltaTime;

                //if assign key was held
                if (assignTimer > 0.2f
                    && !calledAbilityAssignUIOnce)
                {
                    LoadUI();
                    ShowAssignButtonPositions(selectedSlot);
                }
                //if assign key was released
                else if ((Input.GetKeyUp(KeyCode.Z)
                         && selectedSlot == 1)
                         || (Input.GetKeyUp(KeyCode.X)
                         && selectedSlot == 2)
                         || (Input.GetKeyUp(KeyCode.C)
                         && selectedSlot == 3))
                {
                    //use assigned slot ability
                    if (assignTimer < 0.2f)
                    {
                        UseAbility(selectedSlot);
                    }
                    //hide ability assign ui
                    else if (assignTimer >= 0.2f)
                    {
                        HideButtons();

                        UIReuseScript.txt_assigningToSlot.text = "";

                        par_Managers.GetComponent<UI_PauseMenu>().UnlockCamera();
                    }
                }
            }
        }
    }

    //uses the ability in assigned slot
    private void UseAbility(int slot)
    {
        startAssignTimer = false;
        calledAbilityAssignUIOnce = false;
        assignTimer = 0;
        UIReuseScript.par_AbilityUI.SetActive(false);

        //if a valid ability is assigned and it is not in use
        if (slots[slot - 1].GetComponent<UI_Ability>() != null
            && !slots[slot - 1].GetComponent<UI_Ability>().isCooldownTimeRunning)
        {
            UI_Ability ability = slots[slot - 1].GetComponent<UI_Ability>();

            ability.UseAbility();
        }
    }
    //assigns an ability to the slot
    public void AssignToSlot(int slot, int newAbilityIndex)
    {
        UI_Ability newAbility = abilities[newAbilityIndex].GetComponent<UI_Ability>();

        //if nothing is assigned to the slot
        //or a placeholder is assigned to the slot
        if (slots[slot - 1] == null
            || (slots[slot - 1] != null
            && slots[slot - 1].GetComponent<UI_Ability>() == null))
        {
            newAbility.assignStatus = slot -1;

            int abilitySlot = slots.IndexOf(slots[slot -1]);
            if (abilitySlot == 0)
            {
                UIReuseScript.txt_cooldownTimer1.text = "0";
            }
            else if (abilitySlot == 1)
            {
                UIReuseScript.txt_cooldownTimer2.text = "0";
            }
            else if (abilitySlot == 2)
            {
                UIReuseScript.txt_cooldownTimer3.text = "0";
            }

            Debug.Log("Added " + newAbility.abilityName + " to slot " + slots.IndexOf(slots[slot -1]) + "!");

            FillEmptyAbilitySlots();
        }
        //if an ability is assigned to the slot
        else if (slots[slot - 1] != null
                 && slots[slot - 1].GetComponent<UI_Ability>() != null)
        {
            UI_Ability oldAbility = slots[slot - 1].GetComponent<UI_Ability>();

            //if old ability is still in use
            if (oldAbility.isCooldownTimeRunning)
            {
                string oldAbilityName = oldAbility.abilityName;
                string newAbilityName = newAbility.abilityName;
                Debug.LogWarning("Error: Cannot switch old ability " + oldAbilityName + " with new ability " + newAbilityName + " because the old ability is still in use!");
            }
            //if old ability is no longer in use
            else
            {
                oldAbility.assignStatus = -1;
                newAbility.assignStatus = slot;

                int oldAbilitySlot = slots.IndexOf(slots[slot -1]);
                if (oldAbilitySlot == 0)
                {
                    UIReuseScript.txt_cooldownTimer1.text = "";
                }
                else if (oldAbilitySlot == 1)
                {
                    UIReuseScript.txt_cooldownTimer2.text = "";
                }
                else if (oldAbilitySlot == 2)
                {
                    UIReuseScript.txt_cooldownTimer3.text = "";
                }

                int abilitySlot = slots.IndexOf(slots[slot -1]);
                if (abilitySlot == 0)
                {
                    UIReuseScript.txt_cooldownTimer1.text = "0";
                }
                else if (abilitySlot == 1)
                {
                    UIReuseScript.txt_cooldownTimer2.text = "0";
                }
                else if (abilitySlot == 2)
                {
                    UIReuseScript.txt_cooldownTimer3.text = "0";
                }

                Debug.Log("Moved " + oldAbility.abilityName + " from slot " + slots.IndexOf(slots[oldAbilitySlot - 1]) + " to new slot " + slots.IndexOf(slots[slot - 1]) + "!");

                //Debug.Log("Assigned " + newAbility.abilityName + " to slot " + slot + ".");

                FillEmptyAbilitySlots();
            }
        }
    }
    //fills empty ability slots if no abilities are assigned in one or more slots
    public void FillEmptyAbilitySlots()
    {
        //clears slot list
        slots.Clear();

        GameObject ability1 = null;
        GameObject ability2 = null;
        GameObject ability3 = null;

        //finds all assigned abilities
        foreach (GameObject ability in abilities)
        {
            UI_Ability abilityScript = ability.GetComponent<UI_Ability>();

            if (abilityScript.assignStatus == 1)
            {
                ability1 = ability;
            }
            else if (abilityScript.assignStatus == 2)
            {
                ability2 = ability;
            }
            else if (abilityScript.assignStatus == 3)
            {
                ability3 = ability;
            }
        }

        //assigns abilities to list
        if (ability1 != null)
        {
            slots.Add(ability1);
        }
        else
        {
            slots.Add(assignedAbilitiesPlaceholder1);
        }
        if (ability2 != null)
        {
            slots.Add(ability2);
        }
        else
        {
            slots.Add(assignedAbilitiesPlaceholder2);
        }
        if (ability3 != null)
        {
            slots.Add(ability3);
        }
        else
        {
            slots.Add(assignedAbilitiesPlaceholder3);
        }
    }

    //initial ui load for both upgrade and ability assign ui
    public void LoadUI()
    {
        upgradeCell = null;
        foreach (GameObject item in PlayerInventoryScript.inventory)
        {
            if (item != null
                && item.name == "Upgrade_cell")
            {
                upgradeCell = item;
                break;
            }
        }

        if (upgradeCell != null)
        {
            upgradeCellCount = upgradeCell.GetComponent<Env_Item>().int_itemCount;
            UIReuseScript.txt_UpgradeCellCount.text = upgradeCellCount.ToString();
        }
        else
        {
            upgradeCellCount = 0;
            UIReuseScript.txt_UpgradeCellCount.text = "0";
        }

        foreach (GameObject ability in abilities)
        {
            UI_Ability abilityScript = ability.GetComponent<UI_Ability>();

            abilityScript.img_Tier1.gameObject.SetActive(false);
            abilityScript.img_Tier2.gameObject.SetActive(false);
            abilityScript.img_Tier3.gameObject.SetActive(false);

            abilityScript.GetComponent<Button>().interactable = false;
            abilityScript.GetComponent<Button>().onClick.RemoveAllListeners();

            if (abilityScript.upgradeStatus == 1)
            {
                abilityScript.img_Tier1.gameObject.SetActive(true);
            }
            else if (abilityScript.upgradeStatus == 2)
            {
                abilityScript.img_Tier1.gameObject.SetActive(true);
                abilityScript.img_Tier2.gameObject.SetActive(true);
            }
            else if (abilityScript.upgradeStatus == 3)
            {
                abilityScript.img_Tier1.gameObject.SetActive(true);
                abilityScript.img_Tier2.gameObject.SetActive(true);
                abilityScript.img_Tier3.gameObject.SetActive(true);
            }
        }
    }
    //displays the upgrade ui
    public void ShowUpgradeButtonPositions()
    {
        foreach (GameObject button in abilities)
        {
            UI_Ability abilityScript = button.GetComponent<UI_Ability>();

            button.transform.SetParent(abilityScript.pos_Upgrade, false);
            button.transform.position = abilityScript.pos_Upgrade.position;

            if (upgradeCell != null
                && abilityScript.upgradeStatus == 0
                && upgradeCellCount >= abilityScript.cost_unlock)
            {
                abilityScript.GetComponent<Button>().interactable = true;

                button.GetComponent<Button>().onClick.AddListener(abilityScript.UnlockOrUpgradeAbility);
            }
            else if (upgradeCell != null
                     && abilityScript.upgradeStatus == 1
                     && upgradeCellCount >= abilityScript.cost_tier2
                     && !abilityScript.isCooldownTimeRunning)
            {
                abilityScript.GetComponent<Button>().interactable = true;

                button.GetComponent<Button>().onClick.AddListener(abilityScript.UnlockOrUpgradeAbility);
            }
            else if (upgradeCell != null
                     && abilityScript.upgradeStatus == 2
                     && upgradeCellCount >= abilityScript.cost_tier3
                     && !abilityScript.isCooldownTimeRunning)
            {
                abilityScript.GetComponent<Button>().interactable = true;

                button.GetComponent<Button>().onClick.AddListener(abilityScript.UnlockOrUpgradeAbility);
            }
        }
    }
    //displays the assign ui
    public void ShowAssignButtonPositions(int slot)
    {
        foreach (GameObject button in abilities)
        {
            UI_Ability abilityScript = button.GetComponent<UI_Ability>();

            button.transform.SetParent(abilityScript.pos_Assign, false);
            button.transform.position = abilityScript.pos_Assign.position;

            abilityScript.selectedSlot = slot;

            if (abilityScript.upgradeStatus >= 1)
            {
                abilityScript.GetComponent<Button>().interactable = true;
            }
        }

        UIReuseScript.par_AbilityUI.SetActive(true);
        UIReuseScript.txt_assigningToSlot.text = "Assigning to slot " + slot.ToString();

        calledAbilityAssignUIOnce = true;

        par_Managers.GetComponent<UI_PauseMenu>().LockCamera();
    }
    //hides upgrade and assign ui
    public void HideButtons()
    {
        foreach (GameObject button in abilities)
        {
            button.transform.SetParent(pos_ButtonHide, false);
            button.transform.position = pos_ButtonHide.position;
        }

        startAssignTimer = false;
        calledAbilityAssignUIOnce = false;
        assignTimer = 0;
        UIReuseScript.par_AbilityUI.SetActive(false);
    }
}