using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_QuestContent : MonoBehaviour
{
    [SerializeField] private bool canReDoQuest;
    public string str_questTitle;
    [TextArea(5, 5)]
    public string str_questDescription;

    [Header("AI content")]
    [SerializeField] private bool assignedByNPC;
    [SerializeField] private UI_DialogueChoice dialogueParent;
    [SerializeField] private GameObject dialogue_acceptQuest;
    [SerializeField] private GameObject dialogue_turnInQuest;
    [SerializeField] private UI_AIContent AIScript;

    [Header("Quest give stage assignables")]
    [SerializeField] private QuestStage_General QuestStage_Give;

    [Header("Assignables")]
    public List<GameObject> questStages;
    [SerializeField] private List<GameObject> questRewards;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool startedQuest;
    [HideInInspector] public bool completedQuest;
    [HideInInspector] public bool turnedInQuest;
    [HideInInspector] public bool finishedSingleTimeQuestOnce;
    [HideInInspector] public bool failedQuest;
    [HideInInspector] public int questCurrentStage;

    //private variables
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        UIReuseScript = par_Managers.GetComponent<Manager_UIReuse>();

        if (assignedByNPC)
        {
            dialogueParent.dialogues.Remove(dialogue_turnInQuest);
        }
    }

    public void ShowStats()
    {
        UIReuseScript.txt_QuestName.text = str_questTitle;
        UIReuseScript.txt_QuestGiver.text = AIScript.str_NPCName;
        UIReuseScript.txt_QuestGiverClan.text = AIScript.faction.ToString();
        if (startedQuest)
        {
            UIReuseScript.txt_QuestStatus.text = "Started";
        }
        else if (completedQuest)
        {
            UIReuseScript.txt_QuestStatus.text = "Completed";
        }
        else if (turnedInQuest)
        {
            UIReuseScript.txt_QuestStatus.text = "Finished";
        }
        else if (failedQuest)
        {
            UIReuseScript.txt_QuestStatus.text = "Failed";
        }
        UIReuseScript.txt_QuestRewards.text = "None";
        UIReuseScript.txt_QuestDescription.text = str_questDescription;
    }

    public void AcceptedQuest()
    {
        startedQuest = true;
        UIReuseScript.questTitle = str_questTitle;

        UIReuseScript.StartCoroutine(UIReuseScript.StartedQuestUI());
        questCurrentStage = 1;
        Debug.Log("Accepted " + str_questTitle + "!");

        par_Managers.GetComponent<UI_AcceptedQuests>().acceptedQuests.Add(gameObject);

        //start first stage
        questStages[0].GetComponent<QuestStage_General>().StartStage();

        if (assignedByNPC)
        {
            dialogueParent.dialogues.Remove(dialogue_acceptQuest);
            dialogueParent.BuildDialogueTree();
        }
    }
    public void CompletedQuest()
    {
        //completed quest but need to turn it in first from the npc who gave the quest
        completedQuest = true;
        UIReuseScript.questTitle = str_questTitle;
        UIReuseScript.StartCoroutine(UIReuseScript.CompletedQuestUI());

        if (assignedByNPC)
        {
            dialogueParent.dialogues.Add(dialogue_turnInQuest);
            dialogueParent.BuildDialogueTree();
        }

        Debug.Log("Completed " + str_questTitle + "! Waiting until player turns it in...");
    }
    public void TurnedInQuest()
    {
        if (questRewards.Count > 0)
        {
            foreach (GameObject reward in questRewards)
            {
                if (reward.name.Contains("-"))
                {
                    string[] sides = reward.name.Split('-');
                    int count = int.Parse(sides[1]);

                    if (sides[0].Contains("Money"))
                    {
                        PlayerInventoryScript.money += count;

                        UIReuseScript.txt_PlayerMoney.text = PlayerInventoryScript.money.ToString();
                    }
                    else
                    {
                        foreach (GameObject spawnable in par_Managers.GetComponent<Manager_Console>().spawnables)
                        {
                            if (spawnable.name == sides[0])
                            {
                                int itemWeight = spawnable.GetComponent<Env_Item>().int_ItemWeight;
                                int spaceTaken = count * itemWeight;
                                int playerRemainingSpace = PlayerInventoryScript.maxInvSpace - PlayerInventoryScript.invSpace;

                                if (playerRemainingSpace - spaceTaken >= 0)
                                {
                                    GameObject foundItem = null;
                                    foreach (GameObject item in PlayerInventoryScript.inventory)
                                    {
                                        if (item.name == sides[0])
                                        {
                                            foundItem = item;
                                            break;
                                        }
                                    }

                                    if (foundItem != null)
                                    {
                                        foundItem.GetComponent<Env_Item>().int_itemCount += count;
                                    }
                                    else if (foundItem == null
                                             || PlayerInventoryScript.inventory.Count == 0)
                                    {
                                        GameObject questReward = Instantiate(spawnable,
                                                                             PlayerInventoryScript.par_PlayerItems.transform.position,
                                                                             Quaternion.identity);

                                        questReward.name = questReward.GetComponent<Env_Item>().str_ItemName;

                                        questReward.GetComponent<Env_Item>().int_itemCount = count;
                                        questReward.SetActive(false);
                                        par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(questReward.name);
                                        PlayerInventoryScript.inventory.Add(questReward);
                                        questReward.transform.SetParent(PlayerInventoryScript.par_PlayerItems.transform);
                                    }

                                    PlayerInventoryScript.invSpace -= spaceTaken;

                                    UIReuseScript.txt_PlayerInventorySpace.text = PlayerInventoryScript.invSpace.ToString();
                                }
                                else
                                {
                                    GameObject questReward = Instantiate(spawnable,
                                                                         dialogueParent.AI.transform.position,
                                                                         Quaternion.identity);

                                    questReward.name = questReward.GetComponent<Env_Item>().str_ItemName;

                                    questReward.GetComponent<Env_Item>().int_itemCount = count;

                                    Debug.LogWarning("Error: Failed to add " + sides[0].Replace("_", " ") + " to players " +
                                                     "inventory because player doesn't have enough inventory space for it!" +
                                                     " Item was dropped on the ground.");
                                }

                                break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Error: Failed to create " + reward.name + " because its name is invalid!");
                }
            }
        }

        par_Managers.GetComponent<UI_AcceptedQuests>().acceptedQuests.Remove(gameObject);
        if (!canReDoQuest)
        {
            par_Managers.GetComponent<UI_AcceptedQuests>().finishedQuests.Add(gameObject);
        }

        startedQuest = false;
        completedQuest = false;
        if (!canReDoQuest)
        {
            finishedSingleTimeQuestOnce = true;
            turnedInQuest = true;

            if (assignedByNPC)
            {
                dialogueParent.dialogues.Remove(dialogue_turnInQuest);
                dialogueParent.BuildDialogueTree();
            }
        }
        else if (canReDoQuest)
        {
            //resets all stages to allow this quest to be redone
            foreach (GameObject questStage in questStages)
            {
                questStage.GetComponent<QuestStage_General>().startedStage = false;
                questStage.GetComponent<QuestStage_General>().completedStage = false;
            }

            finishedSingleTimeQuestOnce = false;

            if (assignedByNPC)
            {
                dialogueParent.dialogues.Add(dialogue_acceptQuest);
                dialogueParent.dialogues.Remove(dialogue_turnInQuest);
                dialogueParent.BuildDialogueTree();
            }
        }

        if (QuestStage_Give != null)
        {
            GameObject givenItem = QuestStage_Give.pickupItem.gameObject;
            PlayerInventoryScript.inventory.Remove(givenItem);
            int givenItemWeight = givenItem.GetComponent<Env_Item>().int_ItemWeight;
            int givenItemCount = givenItem.GetComponent<Env_Item>().int_itemCount;
            PlayerInventoryScript.invSpace += givenItemWeight * givenItemCount;
            par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(givenItem.GetComponent<Env_Item>().str_ItemName);
            Destroy(givenItem);
            QuestStage_Give.pickupItem = null;
        }

        Debug.Log("Turned in " + str_questTitle + "!");
    }
    public void FailedQuest()
    {
        //this quest can never be redone
        par_Managers.GetComponent<UI_AcceptedQuests>().acceptedQuests.Remove(gameObject);
        failedQuest = true;

        if (assignedByNPC)
        {
            dialogueParent.dialogues.Remove(dialogue_acceptQuest);
            dialogueParent.dialogues.Remove(dialogue_turnInQuest);
            dialogueParent.BuildDialogueTree();
        }

        Debug.Log("Failed " + str_questTitle + "!");
    }
}