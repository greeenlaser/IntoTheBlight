using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_AcceptedQuests : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private Manager_UIReuse UIReuseScript;
    [SerializeField] private UI_PauseMenu PauseMenuScript;
    [SerializeField] private Manager_Console ConsoleScript;
    [SerializeField] private UI_PlayerMenu PlayerMenuScript;

    //public but hidden variables
    [HideInInspector] public bool isQuestsListOpen;
    [HideInInspector] public List<GameObject> buttons = new List<GameObject>();
    [HideInInspector] public List<GameObject> acceptedQuests = new List<GameObject>();
    [HideInInspector] public List<GameObject> finishedQuests = new List<GameObject>();

    public void OpenQuests()
    {
        UIReuseScript.par_RealQuestUI.SetActive(true);
        ShowAcceptedQuests();
    }

    public void ShowAcceptedQuests()
    {
        UIReuseScript.ClearQuestUI();
        UIReuseScript.RebuildAcceptedQuestsList();
        UIReuseScript.btn_AcceptedQuests.interactable = false;
        UIReuseScript.btn_FinishedQuests.interactable = true;
    }
    public void ShowCompletedQuests()
    {
        UIReuseScript.ClearQuestUI();
        UIReuseScript.RebuildCompletedQuestsList();
        UIReuseScript.btn_AcceptedQuests.interactable = true;
        UIReuseScript.btn_FinishedQuests.interactable = false;
    }

    public void CloseQuests()
    {
        UIReuseScript.par_RealQuestUI.SetActive(false);
    }
}