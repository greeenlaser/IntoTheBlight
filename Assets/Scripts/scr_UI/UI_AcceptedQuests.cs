using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_AcceptedQuests : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool isQuestsListOpen;
    [HideInInspector] public List<GameObject> buttons = new List<GameObject>();
    [HideInInspector] public List<GameObject> acceptedQuests = new List<GameObject>();
    [HideInInspector] public List<GameObject> finishedQuests = new List<GameObject>();

    public void OpenQuests()
    {
        par_Managers.GetComponent<Manager_UIReuse>().par_RealQuestUI.SetActive(true);
        ShowAcceptedQuests();
    }

    public void ShowAcceptedQuests()
    {
        par_Managers.GetComponent<Manager_UIReuse>().ClearQuestUI();
        par_Managers.GetComponent<Manager_UIReuse>().RebuildAcceptedQuestsList();
        par_Managers.GetComponent<Manager_UIReuse>().btn_AcceptedQuests.interactable = false;
        par_Managers.GetComponent<Manager_UIReuse>().btn_FinishedQuests.interactable = true;
    }
    public void ShowCompletedQuests()
    {
        par_Managers.GetComponent<Manager_UIReuse>().ClearQuestUI();
        par_Managers.GetComponent<Manager_UIReuse>().RebuildCompletedQuestsList();
        par_Managers.GetComponent<Manager_UIReuse>().btn_AcceptedQuests.interactable = true;
        par_Managers.GetComponent<Manager_UIReuse>().btn_FinishedQuests.interactable = false;
    }

    public void CloseQuests()
    {
        par_Managers.GetComponent<Manager_UIReuse>().par_RealQuestUI.SetActive(false);
    }
}