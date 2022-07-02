using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_AIContent : MonoBehaviour
{
    public bool hasDialogue;
    public string str_NPCName;
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
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool isAIUIOpen;

    public void OpenNPCDialogue()
    {
        isAIUIOpen = true;
        par_Managers.GetComponent<UI_PauseMenu>().isTalkingToAI = true;

        par_Managers.GetComponent<Manager_UIReuse>().par_Dialogue.SetActive(true);
        par_Managers.GetComponent<Manager_UIReuse>().txt_NPCName.text = str_NPCName;

        gameObject.GetComponent<UI_DialogueChoice>().BuildDialogueTree();

        if (!par_Managers.GetComponent<UI_PauseMenu>().isUIOpen)
        {
            par_Managers.GetComponent<UI_PauseMenu>().PauseGameAndCloseUIAndResetBools();
            par_Managers.GetComponent<UI_PauseMenu>().callPMCloseOnce = true;
        }

        thePlayer.GetComponent<Inv_Player>().canOpenPlayerInventory = false;
    }

    public void CloseNPCDialogue()
    {
        par_Managers.GetComponent<Manager_UIReuse>().CloseAllDialogueUI();

        par_Managers.GetComponent<UI_PauseMenu>().isTalkingToAI = false;
        par_Managers.GetComponent<UI_PauseMenu>().callPMCloseOnce = false;
        par_Managers.GetComponent<UI_PauseMenu>().UnpauseGame();

        thePlayer.GetComponent<Inv_Player>().canOpenPlayerInventory = true;

        isAIUIOpen = false;
    }
}