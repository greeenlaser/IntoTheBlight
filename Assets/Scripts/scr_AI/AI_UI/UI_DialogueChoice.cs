using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_DialogueChoice : MonoBehaviour
{
    [Header("Assignables")]
    public DialogueChoice dialogueChoice;
    public enum DialogueChoice
    {
        dialogue,
        quest,
        repair,
        shop,
        closeDialogue
    }
    [TextArea(2, 5)]
    public string parentButtonText;
    [TextArea(2, 5)]
    [SerializeField] private string returnText;
    [TextArea(5, 5)]
    [SerializeField] private string npcResponse;
    public List<GameObject> dialogues;

    [Header("Scripts")]
    public GameObject AI;
    public UI_QuestContent QuestMenu;
    [SerializeField] private UI_RepairContent RepairMenu;
    [SerializeField] private UI_ShopContent ShopMenu;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public int buttonIndex;

    //private variables
    private Manager_UIReuse UIReuseScript;

    private void Start()
    {
        UIReuseScript = par_Managers.GetComponent<Manager_UIReuse>();
    }

    public void BuildDialogueTree()
    {
        UIReuseScript.txt_NPCName.text = AI.GetComponent<UI_AIContent>().str_NPCName;
        UIReuseScript.txt_NPCDialogue.text = npcResponse;

        if (UIReuseScript.buttons.Count > 0)
        {
            UIReuseScript.CloseDialogueUI();
        }

        bool hasCloseDialogueButton = false;

        if (dialogues.Count > 0)
        {
            //create buttons for each dialogue option
            foreach (GameObject dialogue in dialogues)
            {
                Button btn_new = Instantiate(UIReuseScript.btn_dialogueTemplate);
                btn_new.transform.SetParent(UIReuseScript.par_DialoguePanel.transform, false);

                UIReuseScript.buttons.Add(btn_new);

                //dialogue
                if (dialogue.GetComponent<UI_DialogueChoice>().dialogueChoice
                    == UI_DialogueChoice.DialogueChoice.dialogue)
                {
                    btn_new.onClick.AddListener(delegate { dialogue.GetComponent<UI_DialogueChoice>().ButtonFunction("dialogue"); });
                }
                //repair shop
                else if (dialogue.GetComponent<UI_DialogueChoice>().dialogueChoice
                         == UI_DialogueChoice.DialogueChoice.repair)
                {
                    btn_new.onClick.AddListener(delegate { dialogue.GetComponent<UI_DialogueChoice>().ButtonFunction("repair"); });
                }
                //shop
                else if (dialogue.GetComponent<UI_DialogueChoice>().dialogueChoice
                         == UI_DialogueChoice.DialogueChoice.shop)
                {
                    btn_new.onClick.AddListener(delegate { dialogue.GetComponent<UI_DialogueChoice>().ButtonFunction("shop"); });
                }
                //quest
                else if (dialogue.GetComponent<UI_DialogueChoice>().dialogueChoice
                         == UI_DialogueChoice.DialogueChoice.quest
                         && dialogue.GetComponent<UI_DialogueChoice>().QuestMenu != null
                         && !dialogue.GetComponent<UI_DialogueChoice>().QuestMenu.turnedInQuest
                         && !dialogue.GetComponent<UI_DialogueChoice>().QuestMenu.failedQuest)
                {
                    btn_new.onClick.AddListener(delegate { dialogue.GetComponent<UI_DialogueChoice>().ButtonFunction("quest"); });
                }
                //close dialogue
                else if (dialogue.GetComponent<UI_DialogueChoice>().dialogueChoice
                         == UI_DialogueChoice.DialogueChoice.closeDialogue)
                {
                    btn_new.onClick.AddListener(delegate { dialogue.GetComponent<UI_DialogueChoice>().ButtonFunction("close"); });
                    hasCloseDialogueButton = true;
                }

                dialogue.GetComponent<UI_DialogueChoice>().buttonIndex
                    = UIReuseScript.buttons.IndexOf(btn_new);

                //add button text to each dialogue option
                int dialogueIndex = dialogues.IndexOf(dialogue);
                string buttonText = dialogues[dialogueIndex].GetComponent<UI_DialogueChoice>().parentButtonText;
                btn_new.GetComponentInChildren<TMP_Text>().text = buttonText;

                btn_new.interactable = true;
            }
        }

        if (!hasCloseDialogueButton)
        {
            //the final button which returns to main npc dialogue
            Button btn_return = Instantiate(UIReuseScript.btn_dialogueTemplate);
            btn_return.transform.SetParent(UIReuseScript.par_DialoguePanel.transform, false);

            UIReuseScript.buttons.Add(btn_return);

            btn_return.onClick.AddListener(AI.GetComponent<UI_DialogueChoice>().BuildDialogueTree);

            btn_return.GetComponentInChildren<TMP_Text>().text = returnText;

            btn_return.interactable = true;
        }
    }

    public void ButtonFunction(string function)
    {
        if (function == "dialogue")
        {
            BuildDialogueTree();
        }
        else if (function == "repair")
        {
            RepairMenu.OpenRepairUI();
        }
        else if (function == "shop")
        {
            ShopMenu.OpenShopUI();
        }
        else if (function == "quest")
        {
            if (!QuestMenu.startedQuest
                && !QuestMenu.completedQuest
                && !QuestMenu.turnedInQuest
                && !QuestMenu.failedQuest)
            {
                QuestMenu.AcceptedQuest();
            }
            else if (QuestMenu.startedQuest
                     && QuestMenu.completedQuest
                     && !QuestMenu.turnedInQuest
                     && !QuestMenu.failedQuest)
            {
                QuestMenu.TurnedInQuest();
            }
        }
        else if (function == "close")
        {
            AI.GetComponent<UI_AIContent>().CloseNPCDialogue();
        }
    }
}