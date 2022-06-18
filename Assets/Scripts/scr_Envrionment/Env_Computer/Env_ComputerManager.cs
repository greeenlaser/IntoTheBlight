using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Env_ComputerManager : MonoBehaviour
{
    [Header("General")]
    public bool isLocked;
    public string password;
    public string computerTitle;

    [Header("Scripts")]
    [SerializeField] private GameObject par_Managers;

    //private variables
    private Manager_UIReuse UIReuseManager;

    private void Awake()
    {
        UIReuseManager = par_Managers.GetComponent<Manager_UIReuse>();
    }

    private void Update()
    {
        if (isLocked
            && Input.GetKeyDown(KeyCode.Return))
        {
            CheckInput();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UIReuseManager.CloseComputerUI();
        }
    }

    public void CheckInput()
    {
        string input = UIReuseManager.Input_ComputerPassword.text;

        if (input.Length == 0
            || input.Length > 10
            || input != password)
        {
            Debug.LogWarning("Error: Invalid computer password!");
            UIReuseManager.Input_ComputerPassword.text = "";
        }
        else
        {
            isLocked = false;
            UIReuseManager.OpenPasswordUI(gameObject);
        }
    }
}