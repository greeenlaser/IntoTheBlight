using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestStage_General : MonoBehaviour
{
    public StageType stageType;
    public enum StageType
    {
        unassigned,
        goTo,
        use,
        pickUp,
        place,
        take,
        equip,
        give,
        talkTo,
        kill
    }

    [Header("All general stage assignables")]
    [Tooltip("Which quest is completed if this quest with a single item-related state or more is done?")]
    [SerializeField] private UI_QuestContent Quest;

    [Header("Quest trigger assignables")]
    [SerializeField] private GameObject trigger;

    [Header("Quest item assignables")]
    public int desiredCount;
    [Tooltip("What item is picked up/placed/taken/given/used?")]
    public Env_Item pickupItem;
    [Tooltip("The stage where the pickupitem needs to be sent to.")]
    [SerializeField] private QuestStage_General QuestStageGive;

    [Header("Queststage give assignables")]
    [Tooltip("Who this picked up/taken item needs to be given to?")]
    [SerializeField] private UI_AIContent itemReciever;

    //public but hidden variables
    [HideInInspector] public bool requiresStartFromElsewhere;
    [HideInInspector] public bool startedStage;
    [HideInInspector] public bool completedStage;
    [HideInInspector] public bool failedStage;
    [HideInInspector] public int itemCount;

    //private variables
    private int remainingUntilDone;
    private int currentQuestStageIndex;
    private int finalQuestStageIndex;

    private void Start()
    {
        if (stageType == StageType.unassigned)
        {
            Debug.LogWarning("Error: Unassigned questStage type!");
        }
        if (stageType == StageType.goTo)
        {
            trigger.SetActive(false);
        }
        if (stageType == StageType.use
            || stageType == StageType.pickUp
            || stageType == StageType.place
            || stageType == StageType.take
            || stageType == StageType.equip)
        {
            pickupItem.gameObject.SetActive(false);
        }
    }

    public void StartStage()
    {
        if (!requiresStartFromElsewhere)
        {
            if (stageType == StageType.goTo)
            {
                startedStage = true;
                trigger.SetActive(true);
                //Debug.Log("The trigger was enabled. Started goto stage!");
            }
            else if (stageType == StageType.pickUp)
            {
                startedStage = true;
                pickupItem.gameObject.SetActive(true);
                //Debug.Log(pickupItem.str_ItemName + " was enabled. Started pickup stage!");
            }
            else if (stageType == StageType.give)
            {
                startedStage = true;
                //Debug.Log("Started give stage!");
            }
            else
            {
                Debug.LogWarning("Error: Incomplete stage start method or incorrectly assigned state!");
            }
        }
        else
        {
            Debug.LogWarning("Error: This quest can't be started automatically because it requires to be started from elsewhere!");
        }
    }

    //used for pickup and take stages
    public void PickedUpItem()
    {
        itemCount += pickupItem.int_itemCount;

        if (itemCount < desiredCount)
        {
            if (pickupItem.int_itemCount == 1)
            {
                //Debug.Log("Picked up one " + pickupItem.str_ItemName + ". Total count of this item is " + itemCount + ".");
            }
            else if (pickupItem.int_itemCount > 1)
            {
                //Debug.Log("Picked up " + pickupItem.int_itemCount + " of " + pickupItem.str_ItemName + "(s). Total count of this item is " + itemCount + ".");
            }
        }
        else if (itemCount >= desiredCount)
        {
            //Debug.Log("Picked up all " + pickupItem.str_ItemName + "es. Stage complete!");
            QuestStageGive.pickupItem = pickupItem;
            CheckQuestState();
        }
    }

    //check if we need to go to next stage or finish the quest
    public void CheckQuestState()
    {
        //start next quest stage if this is not the final one and does not require start from elsewhere
        if (Quest.questStages.Count == 1)
        {
            finalQuestStageIndex = 1;
        }
        else if (Quest.questStages.Count > 1)
        {
            finalQuestStageIndex = Quest.questStages.Count - 1;
        }

        if (stageType == StageType.goTo)
        {
            trigger.SetActive(false);
        }

        completedStage = true;

        for (int i = 0; i < finalQuestStageIndex; i++)
        {
            if (gameObject == Quest.questStages[i])
            {
                currentQuestStageIndex = i + 1;
                break;
            }
        }

        remainingUntilDone = finalQuestStageIndex - currentQuestStageIndex;
        //Debug.Log((finalQuestStageIndex + 1) + " " + (currentQuestStageIndex + 1) + " " + remainingUntilDone);

        if (!requiresStartFromElsewhere)
        {
            if (remainingUntilDone > 0)
            {
                for (int i = 0; i < finalQuestStageIndex; i++)
                {
                    if (gameObject == Quest.questStages[i])
                    {
                        Quest.questCurrentStage++;
                        //Debug.Log("Current quest stage is " + Quest.questCurrentStage + ".");
                        GameObject nextStage = Quest.questStages[i];
                        if (nextStage.GetComponent<QuestStage_General>() != null)
                        {
                            nextStage.GetComponent<QuestStage_General>().StartStage();
                        }
                        else
                        {
                            Debug.LogWarning("Error: This queststage (" + Quest.str_questTitle + " [" + nextStage + "]) does not have a start yet assigned!");
                        }
                        break;
                    }
                }
            }
            //complete quest if this stage is the last one in that quest
            else if (remainingUntilDone == 0)
            {
                //Debug.Log("Completed last stage for quest " + Quest.str_questTitle + "!");
                Quest.questCurrentStage = Quest.questStages.Count;
                //Debug.Log("Current quest stage is " + Quest.questCurrentStage + ".");
                Quest.CompletedQuest();
            }
        }
        else if (requiresStartFromElsewhere)
        {
            for (int i = 0; i < Quest.questStages.Count; i++)
            {
                if (gameObject == Quest.questStages[i])
                {
                    GameObject currentQuestStage = Quest.questStages[i];
                    Debug.LogError("This queststage (" + Quest.str_questTitle + " [" + currentQuestStage + "]) does not have a start yet assigned!");
                    break;
                }
            }
        }
    }
}