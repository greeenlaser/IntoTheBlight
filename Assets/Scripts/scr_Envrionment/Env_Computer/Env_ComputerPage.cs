using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_ComputerPage : MonoBehaviour
{
    [Header("Assignables")]
    public string str_PageTitle;
    public string str_PageDescription;
    public List<GameObject> pages = new List<GameObject>();

    [Header("Scripts")]
    public Env_ComputerManager ComputerManagerScript;
    [SerializeField] private GameObject par_Managers;

    //private variables
    private Manager_UIReuse UIReuseManager;

    private void Awake()
    {
        UIReuseManager = par_Managers.GetComponent<Manager_UIReuse>();
    }

    public void LoadPageContent()
    {
        UIReuseManager.RebuildComputerPageList(gameObject);
    }
}