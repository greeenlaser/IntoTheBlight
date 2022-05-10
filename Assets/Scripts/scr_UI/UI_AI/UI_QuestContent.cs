using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_QuestContent : MonoBehaviour
{
    [SerializeField] private bool canReDoQuest;
    public string str_questTitle;
    [TextArea] public string str_questDescription;
    [Tooltip("Which section starts mentioning this quest?")]
    public int questInfoPoint;
    [Tooltip("Which section in an npc's dialogue will this quest inject itself into to start at?")]
    public int injectPoint;

    [Header("Quest end response")]
    public string str_questEndNPCResponse;
    public string str_questEndPlayerResponse;

    [Header("Assignables")]
    public List<GameObject> questStages = new();
    public List<GameObject> questRewards = new();
    [SerializeField] private UI_AIContent AIScript;
    [SerializeField] private GameObject par_Managers;

    [Header("Quest give stage assignables")]
    [SerializeField] private QuestStage_General QuestStage_Give;
    [SerializeField] private Inv_Player PlayerInventoryScript;

    //public but hidden variables
    [HideInInspector] public bool startedQuest;
    [HideInInspector] public bool completedQuest;
    [HideInInspector] public bool turnedInQuest;
    [HideInInspector] public bool finishedSingleTimeQuestOnce;
    [HideInInspector] public bool failedQuest;
    [HideInInspector] public int questCurrentStage;

    //private variables
    Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        UIReuseScript = par_Managers.GetComponent<Manager_UIReuse>();
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
    }
    public void CompletedQuest()
    {
        //completed quest but need to turn it in first from the npc who gave the quest
        completedQuest = true;
        UIReuseScript.questTitle = str_questTitle;
        UIReuseScript.StartCoroutine(UIReuseScript.CompletedQuestUI());
        Debug.Log("Completed " + str_questTitle + "! Waiting until player turns it in...");
    }
    public void TurnedInQuest()
    {
        //add all quest rewards to players inventory if player has enough inventory space

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
        Debug.Log("Failed " + str_questTitle + "!");
    }
}