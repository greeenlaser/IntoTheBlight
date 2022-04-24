using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_AbilityManager : MonoBehaviour
{
    [Header("Assignables")]
    public List<GameObject> allAbilities = new List<GameObject>();

    [Header("Scripts")]
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public int assigningToSlot;
    [HideInInspector] public float abilityAssignTimer;
    [HideInInspector] public List<GameObject> assignedAbilities = new List<GameObject>();
    [HideInInspector] public List<GameObject> unlockedAbilities = new List<GameObject>();

    //TODO: FILL SHOWABILITYTREE AND HIDEABILITYTREE FUNCTIONS

    //TODO: ADD ABILITY TIER LOGOS TO UI_ABILITY SCRIPTS
    //TODO: ADD ABILITY REUSE COOLDOWN VISUAL CONFIRMATION TO EACH ASSIGNED ABILITY SLOT

    //TODO: ADD ALL ABILITY UNLOCK/UPGRADE AND USE STATUS TO SAVE SYSTEM
    //TODO: ADD ALL ABILITY LENGTH AND REUSE COOLDOWNS TO SAVE SYSTEM

    private void Start()
    {
        assigningToSlot = -1;
    }

    private void Update()
    {
        if (abilityAssignTimer == 0)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                assigningToSlot = 0;
                abilityAssignTimer += Time.deltaTime;
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                assigningToSlot = 1;
                abilityAssignTimer += Time.deltaTime;
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                assigningToSlot = 2;
                abilityAssignTimer += Time.deltaTime;
            }
        }

        else if (abilityAssignTimer > 0
                 && abilityAssignTimer < 0.2f)
        {
            if (Input.GetKeyUp(KeyCode.Z))
            {
                UseAbility(0);
            }
            else if (Input.GetKeyUp(KeyCode.X))
            {
                UseAbility(1);
            }
            else if (Input.GetKeyUp(KeyCode.C))
            {
                UseAbility(2);
            }
        }
    }

    private void UseAbility(int slot)
    {
        assigningToSlot = -1;

        if (assignedAbilities.Count > 0
            && assignedAbilities[slot].GetComponent<UI_Ability>() != null
            && assignedAbilities[slot].GetComponent<UI_Ability>().assignedSlot == slot)
        {
            assignedAbilities[slot].GetComponent<UI_Ability>().UseAbility();
        }
    }

    public void ShowAbilityTree()
    {

    }
    private void HideAbilityTree()
    {

    }
}