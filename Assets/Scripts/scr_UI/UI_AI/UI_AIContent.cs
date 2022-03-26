using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_AIContent : MonoBehaviour
{
    public bool hasDialogue;
    public bool hasQuests;
    public bool canRepair;
    public bool hasShop;
    public string str_NPCName;
    [SerializeField] private string str_Dialogue;
    [SerializeField] private string str_Shop;
    [SerializeField] private string str_Repair;
    [SerializeField] private string str_startNPCDialogue;
    public float maxDistance;
    [Tooltip("Which faction does this AI belong to?")]
    public Faction faction;
    public enum Faction
    {
        unassigned,
        scientists,
        geifers,
        annies,
        verbannte,
        raiders,
        military,
        verteidiger,
        others
    }

    [Header("Assignables")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private UI_DialogueContent DialogueScript;
    [SerializeField] private UI_ShopContent ShopScript;
    [SerializeField] private UI_RepairContent RepairScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;
    [SerializeField] private Manager_FactionReputation FactionScript;
    [SerializeField] private UI_PauseMenu PauseMenuScript;
    [SerializeField] private Manager_Console ConsoleScript;
    [SerializeField] private GameManager GameManagerScript;

    [Header("Quest assignables")]
    [SerializeField] private UI_QuestContent QuestContentScript;
    [SerializeField] private UI_QuestContent QuestContentScript2;

    //public but hidden variables
    [HideInInspector] public bool AIActivated;
    [HideInInspector] public bool isAIUIOpen;

    private void Start()
    {
        AIActivated = true;
    }

    public void CheckIfAnyQuestIsCompleted()
    {
        isAIUIOpen = true;
        PauseMenuScript.isTalkingToAI = true;

        UIReuseScript.par_Dialogue.SetActive(true);
        UIReuseScript.txt_NPCName.text = str_NPCName;

        //if has no quests 
        if (!hasQuests)
        {
            LoadStartContent();
        }
        //if has some quests
        else if (hasQuests && (QuestContentScript != null || QuestContentScript2 != null))
        {
            //if has some quests but none are completed
            if (QuestContentScript != null && !QuestContentScript.completedQuest
                && QuestContentScript2 != null && !QuestContentScript2.completedQuest)
            {
                LoadStartContent();
            }
            //if has first quest and first quest is completed
            else if (QuestContentScript != null && QuestContentScript.completedQuest)
            {
                UIReuseScript.txt_NPCDialogue.text = QuestContentScript.str_questEndNPCResponse;

                UIReuseScript.btn_DiaButton1.gameObject.SetActive(true);
                UIReuseScript.btn_DiaButton1.onClick.AddListener(QuestContentScript.TurnedInQuest);
                UIReuseScript.btn_DiaButton1.onClick.AddListener(CloseStartContent);
                UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = QuestContentScript.str_questEndPlayerResponse;
            }
            //if has second quest and second quest is completed
            else if (QuestContentScript2 != null && QuestContentScript2.completedQuest)
            {
                UIReuseScript.txt_NPCDialogue.text = QuestContentScript2.str_questEndNPCResponse;

                UIReuseScript.btn_DiaButton1.gameObject.SetActive(true);
                UIReuseScript.btn_DiaButton1.onClick.AddListener(QuestContentScript2.TurnedInQuest);
                UIReuseScript.btn_DiaButton1.onClick.AddListener(CloseStartContent);
                UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = QuestContentScript2.str_questEndPlayerResponse;
            }
        }

        if (!PauseMenuScript.isUIOpen)
        {
            PauseMenuScript.PauseGameAndCloseUIAndResetBools();
            PauseMenuScript.callPMCloseOnce = true;
        }

        thePlayer.GetComponent<Inv_Player>().canOpenPlayerInventory = false;
    }
    private void LoadStartContent()
    {
        UIReuseScript.txt_NPCDialogue.text = str_startNPCDialogue;
        if (hasDialogue)
        {
            UIReuseScript.btn_DiaButton1.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton1.onClick.AddListener(DialogueScript.ShowDialogue);
            UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = str_Dialogue;
        }
        if (canRepair)
        {
            UIReuseScript.btn_DiaButton2.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton2.onClick.AddListener(RepairScript.OpenRepairUI);
            UIReuseScript.btn_DiaButton2.GetComponentInChildren<TMP_Text>().text = str_Repair;
        }
        if (hasShop)
        {
            UIReuseScript.btn_DiaButton3.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton3.onClick.AddListener(ShopScript.OpenShopUI);
            UIReuseScript.btn_DiaButton3.GetComponentInChildren<TMP_Text>().text = str_Shop;
        }

        UIReuseScript.btn_DialogueReturn.gameObject.SetActive(true);
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(CloseStartContent);
    }

    public void CloseStartContent()
    {
        UIReuseScript.CloseDialogueUI();

        PauseMenuScript.isTalkingToAI = false;
        PauseMenuScript.callPMCloseOnce = false;
        PauseMenuScript.UnpauseGame();

        thePlayer.GetComponent<Inv_Player>().canOpenPlayerInventory = true;

        isAIUIOpen = false;
    }
}