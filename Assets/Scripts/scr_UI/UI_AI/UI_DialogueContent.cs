using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_DialogueContent : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private UI_AIContent AIScript;
    [SerializeField] private UI_QuestContent QuestContentScript;
    [SerializeField] private UI_QuestContent QuestContentScript2;
    [SerializeField] private Manager_UIReuse UIReuseScript;

    //private variables
    private bool allowFirstDialogueOption;
    private bool allowSecondDialogueOption;
    private bool allowThirdDialogueOption;
    private int currentDialogueState;
    private string currentPlayerMessage1;
    private string currentPlayerMessage2;
    private string currentPlayerMessage3;

    [Header("Dialogue choices parent")]
    [SerializeField] private string str_FirstNPCResponse;

    [Space(10)]
    [SerializeField] private string str_Option1PlayerChoice;
    [SerializeField] private string str_Option1NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option2PlayerChoice;
    [SerializeField] private string str_Option2NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option3PlayerChoice;
    [SerializeField] private string str_Option3NPCResponse;

    [Header("Dialogue choice 1")]
    [SerializeField] private string str_Option1_1PlayerChoice;
    [SerializeField] private string str_Option1_1NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option1_2PlayerChoice;
    [SerializeField] private string str_Option1_2NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option1_3PlayerChoice;
    [SerializeField] private string str_Option1_3NPCResponse;

    [Space(10)]
    [SerializeField] private string str_Option1_1_1PlayerChoice;
    [SerializeField] private string str_Option1_1_1NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option1_1_2PlayerChoice;
    [SerializeField] private string str_Option1_1_2NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option1_1_3PlayerChoice;
    [SerializeField] private string str_Option1_1_3NPCResponse;

    [Space(10)]
    [SerializeField] private string str_Option1_2_1PlayerChoice;
    [SerializeField] private string str_Option1_2_1NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option1_2_2PlayerChoice;
    [SerializeField] private string str_Option1_2_2NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option1_2_3PlayerChoice;
    [SerializeField] private string str_Option1_2_3NPCResponse;

    [Space(10)]
    [SerializeField] private string str_Option1_3_1PlayerChoice;
    [SerializeField] private string str_Option1_3_1NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option1_3_2NPCResponse;
    [SerializeField] private string str_Option1_3_2PlayerChoice;
    [Space(3)]
    [SerializeField] private string str_Option1_3_3PlayerChoice;
    [SerializeField] private string str_Option1_3_3NPCResponse;

    [Header("Dialogue choice 2")]
    [SerializeField] private string str_Option2_1PlayerChoice;
    [SerializeField] private string str_Option2_1NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option2_2PlayerChoice;
    [SerializeField] private string str_Option2_2NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option2_3PlayerChoice;
    [SerializeField] private string str_Option2_3NPCResponse;

    [Space(10)]
    [SerializeField] private string str_Option2_1_1PlayerChoice;
    [SerializeField] private string str_Option2_1_1NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option2_1_2PlayerChoice;
    [SerializeField] private string str_Option2_1_2NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option2_1_3PlayerChoice;
    [SerializeField] private string str_Option2_1_3NPCResponse;

    [Space(10)]
    [SerializeField] private string str_Option2_2_1PlayerChoice;
    [SerializeField] private string str_Option2_2_1NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option2_2_2PlayerChoice;
    [SerializeField] private string str_Option2_2_2NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option2_2_3PlayerChoice;
    [SerializeField] private string str_Option2_2_3NPCResponse;

    [Space(10)]
    [SerializeField] private string str_Option2_3_1PlayerChoice;
    [SerializeField] private string str_Option2_3_1NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option2_3_2PlayerChoice;
    [SerializeField] private string str_Option2_3_2NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option2_3_3PlayerChoice;
    [SerializeField] private string str_Option2_3_3NPCResponse;

    [Header("Dialogue choice 3")]
    [SerializeField] private string str_Option3_1PlayerChoice;
    [SerializeField] private string str_Option3_1NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option3_2PlayerChoice;
    [SerializeField] private string str_Option3_2NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option3_3PlayerChoice;
    [SerializeField] private string str_Option3_3NPCResponse;

    [Space(10)]
    [SerializeField] private string str_Option3_1_1PlayerChoice;
    [SerializeField] private string str_Option3_1_1NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option3_1_2PlayerChoice;
    [SerializeField] private string str_Option3_1_2NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option3_1_3PlayerChoice;
    [SerializeField] private string str_Option3_1_3NPCResponse;

    [Space(10)]
    [SerializeField] private string str_Option3_2_1PlayerChoice;
    [SerializeField] private string str_Option3_2_1NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option3_2_2PlayerChoice;
    [SerializeField] private string str_Option3_2_2NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option3_2_3PlayerChoice;
    [SerializeField] private string str_Option3_2_3NPCResponse;

    [Space(10)]
    [SerializeField] private string str_Option3_3_1PlayerChoice;
    [SerializeField] private string str_Option3_3_1NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option3_3_2PlayerChoice;
    [SerializeField] private string str_Option3_3_2NPCResponse;
    [Space(3)]
    [SerializeField] private string str_Option3_3_3PlayerChoice;
    [SerializeField] private string str_Option3_3_3NPCResponse;

    public void ShowDialogue()
    {
        ClearDialogue();
        currentDialogueState = 0;
        CheckIfFinishedAllQuests();

        UIReuseScript.txt_NPCDialogue.text = str_FirstNPCResponse;
        currentPlayerMessage1 = str_Option1PlayerChoice;
        currentPlayerMessage2 = str_Option2PlayerChoice;
        currentPlayerMessage3 = str_Option3PlayerChoice;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ReturnToNPC);
        if (currentPlayerMessage1 != "")
        {
            UIReuseScript.btn_DiaButton1.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton1.onClick.AddListener(PickOption1);
            UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = str_Option1PlayerChoice;
        }
        if (currentPlayerMessage2 != "")
        {
            UIReuseScript.btn_DiaButton2.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton2.onClick.AddListener(PickOption2);
            UIReuseScript.btn_DiaButton2.GetComponentInChildren<TMP_Text>().text = str_Option2PlayerChoice;
        }
        if (currentPlayerMessage3 != "")
        {
            UIReuseScript.btn_DiaButton3.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton3.onClick.AddListener(PickOption3);
            UIReuseScript.btn_DiaButton3.GetComponentInChildren<TMP_Text>().text = str_Option3PlayerChoice;
        }
    }

    private void PickOption1()
    {
        ClearDialogue();
        currentDialogueState = 1;
        CheckIfFinishedAllQuests();

        UIReuseScript.txt_NPCDialogue.text = str_Option1NPCResponse;
        currentPlayerMessage1 = str_Option1_1PlayerChoice;
        currentPlayerMessage2 = str_Option1_2PlayerChoice;
        currentPlayerMessage3 = str_Option1_3PlayerChoice;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
        if (currentPlayerMessage1 != "" && allowFirstDialogueOption)
        {
            UIReuseScript.btn_DiaButton1.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton1.onClick.AddListener(PickOption1_1);
            UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = str_Option1_1PlayerChoice;
        }
        if (currentPlayerMessage2 != "" && allowSecondDialogueOption)
        {
            UIReuseScript.btn_DiaButton2.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton2.onClick.AddListener(PickOption1_2);
            UIReuseScript.btn_DiaButton2.GetComponentInChildren<TMP_Text>().text = str_Option1_2PlayerChoice;
        }
        if (currentPlayerMessage3 != "" && allowThirdDialogueOption)
        {
            UIReuseScript.btn_DiaButton3.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton3.onClick.AddListener(PickOption1_3);
            UIReuseScript.btn_DiaButton3.GetComponentInChildren<TMP_Text>().text = str_Option1_3PlayerChoice;
        }
    }
    private void PickOption1_1()
    {
        ClearDialogue();
        currentDialogueState = 11;
        CheckIfFinishedAllQuests();

        UIReuseScript.txt_NPCDialogue.text = str_Option1_1NPCResponse;
        currentPlayerMessage1 = str_Option1_1_1PlayerChoice;
        currentPlayerMessage2 = str_Option1_1_2PlayerChoice;
        currentPlayerMessage3 = str_Option1_1_3PlayerChoice;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
        if (currentPlayerMessage1 != "" && allowFirstDialogueOption)
        {
            UIReuseScript.btn_DiaButton1.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton1.onClick.AddListener(PickOption1_1_1);
            UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage1;
        }
        if (currentPlayerMessage2 != "" && allowSecondDialogueOption)
        {
            UIReuseScript.btn_DiaButton2.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton2.onClick.AddListener(PickOption1_1_2);
            UIReuseScript.btn_DiaButton2.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage2;
        }
        if (currentPlayerMessage3 != "" && allowThirdDialogueOption)
        {
            UIReuseScript.btn_DiaButton3.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton3.onClick.AddListener(PickOption1_1_3);
            UIReuseScript.btn_DiaButton3.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage3;
        }
    }
    private void PickOption1_1_1()
    {
        ClearDialogue();
        currentDialogueState = 111;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option1_1_1NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption1_1_2()
    {
        ClearDialogue();
        currentDialogueState = 112;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option1_1_2NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption1_1_3()
    {
        ClearDialogue();
        currentDialogueState = 113;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option1_1_3NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption1_2()
    {
        ClearDialogue();
        currentDialogueState = 12;
        CheckIfFinishedAllQuests();

        UIReuseScript.txt_NPCDialogue.text = str_Option1_2NPCResponse;
        currentPlayerMessage1 = str_Option1_2_1PlayerChoice;
        currentPlayerMessage2 = str_Option1_2_2PlayerChoice;
        currentPlayerMessage3 = str_Option1_2_3PlayerChoice;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
        if (currentPlayerMessage1 != "" && allowFirstDialogueOption)
        {
            UIReuseScript.btn_DiaButton1.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton1.onClick.AddListener(PickOption1_2_1);
            UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage1;
        }
        if (currentPlayerMessage2 != "" && allowSecondDialogueOption)
        {
            UIReuseScript.btn_DiaButton2.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton2.onClick.AddListener(PickOption1_2_2);
            UIReuseScript.btn_DiaButton2.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage2;
        }
        if (currentPlayerMessage3 != "" && allowThirdDialogueOption)
        {
            UIReuseScript.btn_DiaButton3.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton3.onClick.AddListener(PickOption1_2_3);
            UIReuseScript.btn_DiaButton3.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage3;
        }
    }
    private void PickOption1_2_1()
    {
        ClearDialogue();
        currentDialogueState = 121;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option1_2_1NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption1_2_2()
    {
        ClearDialogue();
        currentDialogueState = 122;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option1_2_2NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption1_2_3()
    {
        ClearDialogue();
        currentDialogueState = 123;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option1_2_3NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption1_3()
    {
        ClearDialogue();
        currentDialogueState = 13;
        CheckIfFinishedAllQuests();

        UIReuseScript.txt_NPCDialogue.text = str_Option1_3NPCResponse;
        currentPlayerMessage1 = str_Option1_3_1PlayerChoice;
        currentPlayerMessage2 = str_Option1_3_2PlayerChoice;
        currentPlayerMessage3 = str_Option1_3_3PlayerChoice;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
        if (currentPlayerMessage1 != "" && allowFirstDialogueOption)
        {
            UIReuseScript.btn_DiaButton1.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton1.onClick.AddListener(PickOption1_3_1);
            UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage1;
        }
        if (currentPlayerMessage2 != "" && allowSecondDialogueOption)
        {
            UIReuseScript.btn_DiaButton2.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton2.onClick.AddListener(PickOption1_3_2);
            UIReuseScript.btn_DiaButton2.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage2;
        }
        if (currentPlayerMessage3 != "" && allowThirdDialogueOption)
        {
            UIReuseScript.btn_DiaButton3.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton3.onClick.AddListener(PickOption1_3_3);
            UIReuseScript.btn_DiaButton3.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage3;
        }
    }
    private void PickOption1_3_1()
    {
        ClearDialogue();
        currentDialogueState = 131;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option1_3_1NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption1_3_2()
    {
        ClearDialogue();
        currentDialogueState = 132;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option1_3_2NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption1_3_3()
    {
        ClearDialogue();
        currentDialogueState = 133;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option1_3_3NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }

    private void PickOption2()
    {
        ClearDialogue();
        currentDialogueState = 2;
        CheckIfFinishedAllQuests();

        UIReuseScript.txt_NPCDialogue.text = str_Option2NPCResponse;
        currentPlayerMessage1 = str_Option2_1PlayerChoice;
        currentPlayerMessage2 = str_Option2_2PlayerChoice;
        currentPlayerMessage3 = str_Option2_3PlayerChoice;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
        if (currentPlayerMessage1 != "" && allowFirstDialogueOption)
        {
            UIReuseScript.btn_DiaButton1.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton1.onClick.AddListener(PickOption2_1);
            UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage1;
        }
        if (currentPlayerMessage2 != "" && allowSecondDialogueOption)
        {
            UIReuseScript.btn_DiaButton2.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton2.onClick.AddListener(PickOption2_2);
            UIReuseScript.btn_DiaButton2.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage2;
        }
        if (currentPlayerMessage3 != "" && allowThirdDialogueOption)
        {
            UIReuseScript.btn_DiaButton3.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton3.onClick.AddListener(PickOption2_3);
            UIReuseScript.btn_DiaButton3.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage3;
        }
    }
    private void PickOption2_1()
    {
        ClearDialogue();
        currentDialogueState = 21;
        CheckIfFinishedAllQuests(); ;

        UIReuseScript.txt_NPCDialogue.text = str_Option2_1NPCResponse;
        currentPlayerMessage1 = str_Option2_1_1PlayerChoice;
        currentPlayerMessage2 = str_Option2_1_2PlayerChoice;
        currentPlayerMessage3 = str_Option2_1_3PlayerChoice;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
        if (currentPlayerMessage1 != "" && allowFirstDialogueOption)
        {
            UIReuseScript.btn_DiaButton1.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton1.onClick.AddListener(PickOption2_1_1);
            UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage1;
        }
        if (currentPlayerMessage2 != "" && allowSecondDialogueOption)
        {
            UIReuseScript.btn_DiaButton2.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton2.onClick.AddListener(PickOption2_1_2);
            UIReuseScript.btn_DiaButton2.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage2;
        }
        if (currentPlayerMessage3 != "" && allowThirdDialogueOption)
        {
            UIReuseScript.btn_DiaButton3.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton3.onClick.AddListener(PickOption2_1_3);
            UIReuseScript.btn_DiaButton3.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage3;
        }
    }
    private void PickOption2_1_1()
    {
        ClearDialogue();
        currentDialogueState = 211;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option2_1_1NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption2_1_2()
    {
        ClearDialogue();
        currentDialogueState = 212;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option2_1_2NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption2_1_3()
    {
        ClearDialogue();
        currentDialogueState = 213;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option2_1_3NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption2_2()
    {
        ClearDialogue();
        currentDialogueState = 22;
        CheckIfFinishedAllQuests();

        UIReuseScript.txt_NPCDialogue.text = str_Option2_2NPCResponse;
        currentPlayerMessage1 = str_Option2_2_1PlayerChoice;
        currentPlayerMessage2 = str_Option2_2_2PlayerChoice;
        currentPlayerMessage3 = str_Option2_2_3PlayerChoice;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
        if (currentPlayerMessage1 != "" && allowFirstDialogueOption)
        {
            UIReuseScript.btn_DiaButton1.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton1.onClick.AddListener(PickOption2_2_1);
            UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage1;
        }
        if (currentPlayerMessage2 != "" && allowSecondDialogueOption)
        {
            UIReuseScript.btn_DiaButton2.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton2.onClick.AddListener(PickOption2_2_2);
            UIReuseScript.btn_DiaButton2.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage2;
        }
        if (currentPlayerMessage3 != "" && allowThirdDialogueOption)
        {
            UIReuseScript.btn_DiaButton3.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton3.onClick.AddListener(PickOption2_2_3);
            UIReuseScript.btn_DiaButton3.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage3;
        }
    }
    private void PickOption2_2_1()
    {
        ClearDialogue();
        currentDialogueState = 221;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option2_2_1NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption2_2_2()
    {
        ClearDialogue();
        currentDialogueState = 222;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option2_2_2NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption2_2_3()
    {
        ClearDialogue();
        currentDialogueState = 223;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option2_2_3NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption2_3()
    {
        ClearDialogue();
        currentDialogueState = 23;
        CheckIfFinishedAllQuests();

        UIReuseScript.txt_NPCDialogue.text = str_Option2_3NPCResponse;
        currentPlayerMessage1 = str_Option2_3_1PlayerChoice;
        currentPlayerMessage2 = str_Option2_3_2PlayerChoice;
        currentPlayerMessage3 = str_Option2_3_3PlayerChoice;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
        if (currentPlayerMessage1 != "" && allowFirstDialogueOption)
        {
            UIReuseScript.btn_DiaButton1.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton1.onClick.AddListener(PickOption2_3_1);
            UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage1;
        }
        if (currentPlayerMessage2 != "" && allowSecondDialogueOption)
        {
            UIReuseScript.btn_DiaButton2.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton2.onClick.AddListener(PickOption2_3_2);
            UIReuseScript.btn_DiaButton2.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage2;
        }
        if (currentPlayerMessage3 != "" && allowThirdDialogueOption)
        {
            UIReuseScript.btn_DiaButton3.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton3.onClick.AddListener(PickOption2_3_3);
            UIReuseScript.btn_DiaButton3.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage3;
        }
    }
    private void PickOption2_3_1()
    {
        ClearDialogue();
        currentDialogueState = 231;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option2_3_1NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption2_3_2()
    {
        ClearDialogue();
        currentDialogueState = 232;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option2_3_2NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption2_3_3()
    {
        ClearDialogue();
        currentDialogueState = 233;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option2_3_3NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }

    private void PickOption3()
    {
        ClearDialogue();
        currentDialogueState = 3;
        CheckIfFinishedAllQuests();

        UIReuseScript.txt_NPCDialogue.text = str_Option3NPCResponse;
        currentPlayerMessage1 = str_Option3_1PlayerChoice;
        currentPlayerMessage2 = str_Option3_2PlayerChoice;
        currentPlayerMessage3 = str_Option3_3NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
        if (currentPlayerMessage1 != "" && allowFirstDialogueOption)
        {
            UIReuseScript.btn_DiaButton1.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton1.onClick.AddListener(PickOption3_1);
            UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage1;
        }
        if (currentPlayerMessage2 != "" && allowSecondDialogueOption)
        {
            UIReuseScript.btn_DiaButton2.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton2.onClick.AddListener(PickOption3_2);
            UIReuseScript.btn_DiaButton2.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage2;
        }
        if (currentPlayerMessage3 != "" && allowThirdDialogueOption)
        {
            UIReuseScript.btn_DiaButton3.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton3.onClick.AddListener(PickOption3_3);
            UIReuseScript.btn_DiaButton3.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage3;
        }
    }
    private void PickOption3_1()
    {
        ClearDialogue();
        currentDialogueState = 31;
        CheckIfFinishedAllQuests();

        UIReuseScript.txt_NPCDialogue.text = str_Option3_1NPCResponse;
        currentPlayerMessage1 = str_Option3_1_1PlayerChoice;
        currentPlayerMessage2 = str_Option3_1_2PlayerChoice;
        currentPlayerMessage3 = str_Option3_1_3PlayerChoice;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
        if (currentPlayerMessage1 != "" && allowFirstDialogueOption)
        {
            UIReuseScript.btn_DiaButton1.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton1.onClick.AddListener(PickOption3_1_1);
            UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage1;
        }
        if (currentPlayerMessage2 != "" && allowSecondDialogueOption)
        {
            UIReuseScript.btn_DiaButton2.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton2.onClick.AddListener(PickOption3_1_2);
            UIReuseScript.btn_DiaButton2.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage2;
        }
        if (currentPlayerMessage3 != "" && allowThirdDialogueOption)
        {
            UIReuseScript.btn_DiaButton3.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton3.onClick.AddListener(PickOption3_1_3);
            UIReuseScript.btn_DiaButton3.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage3;
        }
    }
    private void PickOption3_1_1()
    {
        ClearDialogue();
        currentDialogueState = 311;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option3_1_1NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption3_1_2()
    {
        ClearDialogue();
        currentDialogueState = 312;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option3_1_2NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption3_1_3()
    {
        ClearDialogue();
        currentDialogueState = 313;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option3_1_3NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption3_2()
    {
        ClearDialogue();
        currentDialogueState = 32;
        CheckIfFinishedAllQuests();

        UIReuseScript.txt_NPCDialogue.text = str_Option3_2NPCResponse;
        currentPlayerMessage1 = str_Option3_2_1PlayerChoice;
        currentPlayerMessage2 = str_Option3_2_2PlayerChoice;
        currentPlayerMessage3 = str_Option3_2_3PlayerChoice;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
        if (currentPlayerMessage1 != "" && allowFirstDialogueOption)
        {
            UIReuseScript.btn_DiaButton1.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton1.onClick.AddListener(PickOption3_2_1);
            UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage1;
        }
        if (currentPlayerMessage2 != "" && allowSecondDialogueOption)
        {
            UIReuseScript.btn_DiaButton2.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton2.onClick.AddListener(PickOption3_2_2);
            UIReuseScript.btn_DiaButton2.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage2;
        }
        if (currentPlayerMessage3 != "" && allowThirdDialogueOption)
        {
            UIReuseScript.btn_DiaButton3.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton3.onClick.AddListener(PickOption3_2_3);
            UIReuseScript.btn_DiaButton3.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage3;
        }
    }
    private void PickOption3_2_1()
    {
        ClearDialogue();
        currentDialogueState = 321;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option3_2_1NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption3_2_2()
    {
        ClearDialogue();
        currentDialogueState = 322;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option3_2_2NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption3_2_3()
    {
        ClearDialogue();
        currentDialogueState = 323;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option3_2_3NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption3_3()
    {
        ClearDialogue();
        currentDialogueState = 33;
        CheckIfFinishedAllQuests();

        UIReuseScript.txt_NPCDialogue.text = str_Option3_3NPCResponse;
        currentPlayerMessage1 = str_Option3_3_1PlayerChoice;
        currentPlayerMessage2 = str_Option3_3_2PlayerChoice;
        currentPlayerMessage3 = str_Option3_3_3PlayerChoice;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
        if (currentPlayerMessage1 != "" && allowFirstDialogueOption)
        {
            UIReuseScript.btn_DiaButton1.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton1.onClick.AddListener(PickOption3_3_1);
            UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage1;
        }
        if (currentPlayerMessage2 != "" && allowSecondDialogueOption)
        {
            UIReuseScript.btn_DiaButton2.gameObject.SetActive(true);

            UIReuseScript.btn_DiaButton2.onClick.AddListener(PickOption3_3_2);
            UIReuseScript.btn_DiaButton2.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage2;
        }
        if (currentPlayerMessage3 != "" && allowThirdDialogueOption)
        {
            UIReuseScript.btn_DiaButton3.gameObject.SetActive(true);
            UIReuseScript.btn_DiaButton3.onClick.AddListener(PickOption3_3_3);
            UIReuseScript.btn_DiaButton3.GetComponentInChildren<TMP_Text>().text = currentPlayerMessage3;
        }
    }
    private void PickOption3_3_1()
    {
        ClearDialogue();
        currentDialogueState = 331;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option3_3_1NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption3_3_2()
    {
        ClearDialogue();
        currentDialogueState = 332;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option3_3_2NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }
    private void PickOption3_3_3()
    {
        ClearDialogue();
        currentDialogueState = 333;
        CheckIfQuestStarted();
        UIReuseScript.txt_NPCDialogue.text = str_Option3_3_3NPCResponse;
        UIReuseScript.btn_DialogueReturn.onClick.AddListener(ShowDialogue);
    }

    private void CheckIfFinishedAllQuests()
    {
        allowFirstDialogueOption = true;
        allowSecondDialogueOption = true;
        allowThirdDialogueOption = true;

        if (AIScript.hasQuests)
        {
            if (QuestContentScript.turnedInQuest
                && QuestContentScript.finishedSingleTimeQuestOnce 
                && currentDialogueState != 0)
            {
                string currentDialogueStateInString = currentDialogueState.ToString();
                string questInfoPointInString = QuestContentScript.questInfoPoint.ToString();

                if (currentDialogueStateInString + 1 == questInfoPointInString)
                {
                    allowFirstDialogueOption = false;
                }
                else if (currentDialogueStateInString + 2 == questInfoPointInString)
                {
                    allowSecondDialogueOption = false;
                }
                else if (currentDialogueStateInString + 3 == questInfoPointInString)
                {
                    allowThirdDialogueOption = false;
                }
            }
            else if (QuestContentScript2.turnedInQuest
                && QuestContentScript2.finishedSingleTimeQuestOnce
                && currentDialogueState != 0)
            {
                string currentDialogueStateInString = currentDialogueState.ToString();
                string questInfoPoint2InString = QuestContentScript2.questInfoPoint.ToString();

                //Debug.Log(QuestContentScript2.turnedInQuest + " " + QuestContentScript2.finishedSingleTimeQuestOnce + " " + currentDialogueState + " " + QuestContentScript2.questInfoPoint);

                if (currentDialogueStateInString + 1 == questInfoPoint2InString)
                {
                    allowFirstDialogueOption = false;
                }
                else if (currentDialogueStateInString + 2 == questInfoPoint2InString)
                {
                    allowSecondDialogueOption = false;
                }
                else if (currentDialogueStateInString + 3 == questInfoPoint2InString)
                {
                    allowThirdDialogueOption = false;
                }
            }
            else
            {
                if (currentDialogueState != 0)
                {
                    CheckForFinishedSingleTimeQuest();
                }
                CheckIfQuestStarted();
            }
        }
        //if ai has no quests
        else if (!AIScript.hasQuests)
        {
            allowFirstDialogueOption = false;
            allowSecondDialogueOption = false;
            allowThirdDialogueOption = false;

            UIReuseScript.txt_NPCDialogue.text = "I don't have anything I need help with currently.";
        }
    }

    private void CheckForFinishedSingleTimeQuest()
    {
        string currentDialogueStateInString = currentDialogueState.ToString();
        string quest1InjectPoint = QuestContentScript.injectPoint.ToString();
        string quest2InjectPoint = QuestContentScript2.injectPoint.ToString();

        //check for first quest
        if (currentDialogueStateInString == "0")
        {
            //first dialogue option sends to finished one time quest
            if (quest1InjectPoint == "1"
                && QuestContentScript.turnedInQuest
                && QuestContentScript.finishedSingleTimeQuestOnce)
            {
                allowFirstDialogueOption = false;
            }
            //second dialogue option sends to finished one time quest
            else if (quest1InjectPoint == "2"
                && QuestContentScript.turnedInQuest
                && QuestContentScript.finishedSingleTimeQuestOnce)
            {
                allowSecondDialogueOption = false;
            }
            //third dialogue option sends to finished one time quest
            else if (quest1InjectPoint == "3"
                && QuestContentScript.turnedInQuest
                && QuestContentScript.finishedSingleTimeQuestOnce)
            {
                allowThirdDialogueOption = false;
            }
        }
        else if (currentDialogueStateInString != "0")
        {
            //first dialogue option sends to finished one time quest
            if (currentDialogueStateInString + "1" == quest1InjectPoint
                && QuestContentScript.turnedInQuest
                && QuestContentScript.finishedSingleTimeQuestOnce)
            {
                allowFirstDialogueOption = false;
            }
            //second dialogue option sends to finished one time quest
            else if (currentDialogueStateInString + "2" == quest1InjectPoint
                && QuestContentScript.turnedInQuest
                && QuestContentScript.finishedSingleTimeQuestOnce)
            {
                allowSecondDialogueOption = false;
            }
            //third dialogue option sends to finished one time quest
            else if (currentDialogueStateInString + "3" == quest1InjectPoint
                && QuestContentScript.turnedInQuest
                && QuestContentScript.finishedSingleTimeQuestOnce)
            {
                allowThirdDialogueOption = false;
            }
        }

        //check for second quest
        if (currentDialogueStateInString == "0")
        {
            //first dialogue option sends to finished one time quest
            if (quest1InjectPoint == "1"
                && QuestContentScript2.turnedInQuest
                && QuestContentScript2.finishedSingleTimeQuestOnce)
            {
                allowFirstDialogueOption = false;
            }
            //second dialogue option sends to finished one time quest
            else if (quest1InjectPoint == "2"
                && QuestContentScript2.turnedInQuest
                && QuestContentScript2.finishedSingleTimeQuestOnce)
            {
                allowSecondDialogueOption = false;
            }
            //third dialogue option sends to finished one time quest
            else if (quest1InjectPoint == "3"
                && QuestContentScript2.turnedInQuest
                && QuestContentScript2.finishedSingleTimeQuestOnce)
            {
                allowThirdDialogueOption = false;
            }
        }
        else if (currentDialogueStateInString != "0")
        {
            //first dialogue option sends to finished one time quest
            if (currentDialogueStateInString + "1" == quest2InjectPoint
            && QuestContentScript2.turnedInQuest
            && QuestContentScript2.finishedSingleTimeQuestOnce)
            {
                allowFirstDialogueOption = false;
            }
            //second dialogue option sends to finished one time quest
            if (currentDialogueStateInString + "2" == quest2InjectPoint
                && QuestContentScript2.turnedInQuest
                && QuestContentScript2.finishedSingleTimeQuestOnce)
            {
                allowSecondDialogueOption = false;
            }
            //third dialogue option sends to finished one time quest
            if (currentDialogueStateInString + "3" == quest2InjectPoint
                && QuestContentScript2.turnedInQuest
                && QuestContentScript2.finishedSingleTimeQuestOnce)
            {
                allowThirdDialogueOption = false;
            }
        }
    }

    private void CheckIfQuestStarted()
    {
        string str_currentDialogueState = currentDialogueState.ToString();
        //Debug.Log(str_currentDialogueState + 1 + " " + QuestContentScript.questInfoPoint);
        if ((str_currentDialogueState + 1 == QuestContentScript.questInfoPoint.ToString()
            && QuestContentScript.startedQuest)
            || (currentDialogueState + 1 == QuestContentScript2.questInfoPoint
            && QuestContentScript2.startedQuest))
        {
            allowFirstDialogueOption = false;
        }
        if ((str_currentDialogueState + 2 == QuestContentScript.questInfoPoint.ToString()
            && QuestContentScript.startedQuest)
            || (str_currentDialogueState + 2 == QuestContentScript2.questInfoPoint.ToString()
            && QuestContentScript2.startedQuest))
        {
            allowSecondDialogueOption = false;
        }
        if ((str_currentDialogueState + 3 == QuestContentScript.questInfoPoint.ToString()
            && QuestContentScript.startedQuest)
            || (str_currentDialogueState + 3 == QuestContentScript2.questInfoPoint.ToString()
            && QuestContentScript2.startedQuest))
        {
            allowThirdDialogueOption = false;
        }

        //start the quest if the currently selected dialogue option leads to a quest
        if (currentDialogueState == QuestContentScript.injectPoint 
            && !QuestContentScript.startedQuest 
            && !QuestContentScript.finishedSingleTimeQuestOnce)
        {
            QuestContentScript.AcceptedQuest();
        }
        if (currentDialogueState == QuestContentScript2.injectPoint
            && !QuestContentScript2.startedQuest
            && !QuestContentScript2.finishedSingleTimeQuestOnce)
        {
            QuestContentScript2.AcceptedQuest();
        }
    }

    private void ClearDialogue()
    {
        UIReuseScript.txt_NPCDialogue.text = "";

        UIReuseScript.btn_DiaButton1.onClick.RemoveAllListeners();
        UIReuseScript.btn_DiaButton1.GetComponentInChildren<TMP_Text>().text = "";
        UIReuseScript.btn_DiaButton1.gameObject.SetActive(false);
        UIReuseScript.btn_DiaButton2.onClick.RemoveAllListeners();
        UIReuseScript.btn_DiaButton2.GetComponentInChildren<TMP_Text>().text = "";
        UIReuseScript.btn_DiaButton2.gameObject.SetActive(false);
        UIReuseScript.btn_DiaButton3.onClick.RemoveAllListeners();
        UIReuseScript.btn_DiaButton3.GetComponentInChildren<TMP_Text>().text = "";
        UIReuseScript.btn_DiaButton3.gameObject.SetActive(false);
        UIReuseScript.btn_DialogueReturn.onClick.RemoveAllListeners();
    }

    private void ReturnToNPC()
    {
        ClearDialogue();
        AIScript.CheckIfAnyQuestIsCompleted();
    }
}