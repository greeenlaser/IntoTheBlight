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
    public List<GameObject> slots;

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

        //are there any slots filled
        if (slots.Count >= slot)
        {
            foreach (GameObject ability in slots)
            {
                if (ability.GetComponent<UI_Ability>().assignStatus == slot
                    && !ability.GetComponent<UI_Ability>().isCooldownTimeRunning)
                {
                    ability.GetComponent<UI_Ability>().UseAbility();
                    break;
                }
            }
        }
    }
    //assigns an ability to the slot
    public void AssignToSlot(int slot, int newAbilityIndex)
    {
        int abilitySlot = slot--;
        UI_Ability newAbility = abilities[newAbilityIndex].GetComponent<UI_Ability>();

        foreach (GameObject ability in abilities)
        {
            //if an ability is assigned to this slot
            if (ability.GetComponent<UI_Ability>().selectedSlot == abilitySlot)
            {
                //the assigned ability in the slot is not in use
                if (!ability.GetComponent<UI_Ability>().isCooldownTimeRunning)
                {
                    UI_Ability oldAbility = ability.GetComponent<UI_Ability>();
                    oldAbility.assignStatus = -1;
                    slots.Remove(oldAbility.gameObject);

                    newAbility.assignStatus = abilitySlot;
                    slots.Add(newAbility.gameObject);

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

                    Debug.Log("Added " + newAbility.abilityName + " to slot " + abilitySlot + "!");

                    break;
                }
                //the assigned ability in the slot is the same as this one
                else if (ability.GetComponent<UI_Ability>() == newAbility)
                {
                    Debug.LogWarning("Error: Cannot assign " + ability.GetComponent<UI_Ability>().abilityName +
                                     " to slot " + abilitySlot + " because it is already assigned there!");

                    break;
                }
                //the assigned ability in the slot is already in use
                else if (ability.GetComponent<UI_Ability>().isCooldownTimeRunning)
                {
                    Debug.LogWarning("Error: Cannot assign " + ability.GetComponent<UI_Ability>().abilityName +
                                     " to slot " + abilitySlot + " because the old ability in that slot is in use!");

                    break;
                }
            }
            //no ability assigned to this slot
            else
            {
                newAbility.assignStatus = abilitySlot;
                slots.Add(newAbility.gameObject);

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

                Debug.Log("Added " + newAbility.abilityName + " to slot " + abilitySlot + "!");

                break;
            }
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