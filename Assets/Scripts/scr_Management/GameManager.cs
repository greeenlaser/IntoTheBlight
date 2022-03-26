using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game version")]
    public string str_GameVersion;
    [SerializeField] private TMP_Text txt_GameVersion;

    [Header("Framerate")]
    [SerializeField] private int maxFPS;
    [Range(0.01f, 1f)]
    [SerializeField] private float fpsUpdateSpeed;
    [SerializeField] private TMP_Text txt_fpsValue;

    //private variables
    private float fps;
    private int scrCount;

    private void Awake()
    {
        txt_GameVersion.text = str_GameVersion;

        //EXCLUSIVE WINDOW AND WINDOWED MODE WILL BREAK THE GAME UI!
        //MAXIMIZED WINDOW IS NOT SUPPORTED IN WINDOWS!
        Screen.SetResolution(1920, 1080, true, maxFPS);

        InvokeRepeating(nameof(GetFPS), 1, fpsUpdateSpeed);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            Screenshot();
        }
    }

    private void GetFPS()
    {
        fps = Mathf.FloorToInt(1 / Time.deltaTime);
        txt_fpsValue.text = fps.ToString();
    }

    //press F12 for screenshot
    private void Screenshot()
    {
        scrCount++;
        ScreenCapture.CaptureScreenshot("GameScreenshot_" + scrCount + ".png");
        Debug.Log("Screenshot " + scrCount + " was taken!");
    }
}