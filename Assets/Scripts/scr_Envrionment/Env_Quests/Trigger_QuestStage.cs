using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_QuestStage : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private QuestStage_General QuestStage;
    [SerializeField] private UI_QuestContent Quest;

    private void OnTriggerEnter(Collider other)
    {
        if (QuestStage.startedStage 
            && !QuestStage.completedStage 
            && !QuestStage.failedStage 
            && other.CompareTag("Player"))
        {
            QuestStage.CheckQuestState();
            //Debug.Log("Player reached queststage target. Stage complete!");
        }
    }
}