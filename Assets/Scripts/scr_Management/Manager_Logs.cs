using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Manager_Logs : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private Manager_Console ConsoleScript;

    //public but hidden variables
    [HideInInspector] public string lastOutput;
    [HideInInspector] public string output;
    [HideInInspector] public string stack;

    //private variables
    private bool startedWait;

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        output = logString;
        stack = stackTrace;

        if (ConsoleScript.displayUnityLogs && !startedWait)
        {
            if (lastOutput == output)
            {
                startedWait = true;
                StartCoroutine(Wait());
            }

            ConsoleScript.NewUnitylogMessage();
        }
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.5f);
        startedWait = false;
    }
}