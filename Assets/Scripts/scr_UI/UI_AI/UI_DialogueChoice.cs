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
        shop
    }
    [TextArea(2, 5)]
    public string parentButtonText;
    [TextArea(2, 5)]
    [SerializeField] private string returnText;
    [TextArea(5, 5)]
    [SerializeField] private string npcResponse;
    public List<GameObject> dialogues = new();

    [Header("Scripts")]
    [SerializeField] private UI_DialogueParent DialogueParentScript;
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

    private void BuildDialogueTree()
    {
        UIReuseScript.txt_NPCName.text = DialogueParentScript.AIContentScript.str_NPCName;
        UIReuseScript.txt_NPCDialogue.text = npcResponse;

        if (UIReuseScript.buttons.Count > 0)
        {
            UIReuseScript.CloseDialogueUI();

        }

        if (dialogues.Count > 0)
        {
            //create buttons for each dialogue option
            foreach (GameObject dialogue in dialogues)
            {
                Button btn_new = Instantiate(UIReuseScript.btn_dialogueTemplate);
                btn_new.transform.SetParent(UIReuseScript.par_DialoguePanel.transform, false);

                UIReuseScript.buttons.Add(btn_new);

                //if this is a dialogue choice
                //or a repair shop
                //or a regular shop
                //or a quest that isnt yet turned in or failed
                if (dialogue.GetComponent<UI_DialogueChoice>().dialogueChoice
                    == UI_DialogueChoice.DialogueChoice.dialogue
                    || dialogue.GetComponent<UI_DialogueChoice>().dialogueChoice
                    == UI_DialogueChoice.DialogueChoice.repair
                    || dialogue.GetComponent<UI_DialogueChoice>().dialogueChoice
                    == UI_DialogueChoice.DialogueChoice.shop
                    || (dialogue.GetComponent<UI_DialogueChoice>().dialogueChoice
                    == UI_DialogueChoice.DialogueChoice.quest
                    && dialogue.GetComponent<UI_DialogueChoice>().QuestMenu != null
                    && !dialogue.GetComponent<UI_DialogueChoice>().QuestMenu.turnedInQuest
                    && !dialogue.GetComponent<UI_DialogueChoice>().QuestMenu.failedQuest))
                {
                    dialogue.GetComponent<UI_DialogueChoice>().buttonIndex
                        = UIReuseScript.buttons.IndexOf(btn_new);

                    //add button function
                    btn_new.onClick.AddListener(dialogue.GetComponent<UI_DialogueChoice>().ButtonFunction);

                    //add button text to each dialogue option
                    int dialogueIndex = dialogues.IndexOf(dialogue);
                    string buttonText = dialogues[dialogueIndex].GetComponent<UI_DialogueChoice>().parentButtonText;
                    btn_new.GetComponentInChildren<TMP_Text>().text = buttonText;

                    btn_new.interactable = true;
                }
            }
        }

        //the final button which returns to main npc dialogue
        Button btn_return = Instantiate(UIReuseScript.btn_dialogueTemplate);
        btn_return.transform.SetParent(UIReuseScript.par_DialoguePanel.transform, false);

        UIReuseScript.buttons.Add(btn_return);

        btn_return.onClick.AddListener(DialogueParentScript.BuildDialogueTree);

        btn_return.GetComponentInChildren<TMP_Text>().text = returnText;

        btn_return.interactable = true;
    }

    public void ButtonFunction()
    {
        if (dialogueChoice == DialogueChoice.dialogue)
        {
            BuildDialogueTree();
        }
        else if (dialogueChoice == DialogueChoice.quest)
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
        else if (dialogueChoice == DialogueChoice.repair)
        {
            RepairMenu.OpenRepairUI();
        }
        else if (dialogueChoice == DialogueChoice.shop)
        {
            ShopMenu.OpenShopUI();
        }
    }
}