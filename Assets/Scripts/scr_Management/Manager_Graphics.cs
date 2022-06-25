using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Graphics : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer; 
    [SerializeField] private GameObject par_Managers;

    //private variables
    private string path;
    private Manager_UIReuse UIReuseScript;

    //default values                              //custom values
    private readonly float def_mouseSpeed = 50;   [HideInInspector] public float current_mouseSpeed;
    private readonly float def_fov = 90;          [HideInInspector] public float current_fov;

    private void Awake()
    {
        path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\LightsOff" + @"\GraphicsSettings.txt";
        UIReuseScript = par_Managers.GetComponent<Manager_UIReuse>();

        UIReuseScript.btn_SaveGraphicsSettings.onClick.AddListener(SaveData);
        UIReuseScript.btn_ResetGraphicsSettings.onClick.AddListener(ResetData);
    }

    public void SaveData()
    {
        //using a text editor to write text to the game graphics file
        using StreamWriter graphicsFile = File.CreateText(path);

        graphicsFile.WriteLine("Graphics settings file for Lights Off Version " + GetComponent<GameManager>().str_GameVersion);
        graphicsFile.WriteLine("Read more info about the game from https://greeenlaser.itch.io/lightsoff");
        graphicsFile.WriteLine("Download game versions from https://drive.google.com/drive/folders/12kvUT6EEndku0nDvZVrVd4QRPt50QV7g?usp=sharing");
        graphicsFile.WriteLine("");
        graphicsFile.WriteLine("WARNING: Invalid values will break the game - edit at your own risk!");

        graphicsFile.WriteLine("");

        graphicsFile.WriteLine("--- PLAYER VALUES ---");
        graphicsFile.WriteLine("pv_mouseSpeed: " + thePlayer.GetComponentInChildren<Player_Camera>().mouseSpeed.ToString());
        current_mouseSpeed = thePlayer.GetComponentInChildren<Player_Camera>().mouseSpeed;
        graphicsFile.WriteLine("pv_fov: " + thePlayer.GetComponentInChildren<Camera>().fieldOfView.ToString());
        current_fov = thePlayer.GetComponentInChildren<Camera>().fieldOfView;

        Debug.Log("Successfully saved graphics settings to GraphicsSettings.txt!");
    }
    public void LoadData()
    {
        if (File.Exists(path))
        {
            foreach (string line in File.ReadLines(path))
            {
                //get all separators in line
                char[] separators = new char[] { ':' };
                //remove unwanted separators and split line into separate strings
                string[] values = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                //load all player graphics values
                if (line.Contains("pv"))
                {
                    if (line.Contains("mouseSpeed"))
                    {
                        thePlayer.GetComponentInChildren<Player_Camera>().mouseSpeed = int.Parse(values[1]);
                        thePlayer.GetComponentInChildren<Player_Camera>().sensX = int.Parse(values[1]);
                        thePlayer.GetComponentInChildren<Player_Camera>().sensY = int.Parse(values[1]);
                        UIReuseScript.txt_mouseSpeedValue.text = values[1];
                        UIReuseScript.slider_mouseSpeed.value = int.Parse(values[1]);
                        current_mouseSpeed = thePlayer.GetComponentInChildren<Player_Camera>().mouseSpeed;
                    }
                    else if (line.Contains("fov"))
                    {
                        thePlayer.GetComponentInChildren<Camera>().fieldOfView = int.Parse(values[1]);
                        UIReuseScript.txt_fovValue.text = values[1];
                        UIReuseScript.slider_fovValue.value = int.Parse(values[1]);
                        current_fov = thePlayer.GetComponentInChildren<Camera>().fieldOfView;
                    }
                }
            }

            Debug.Log("Successfully loaded graphics settings from GraphicsSettings.txt!");
        }
        else
        {
            ResetData();
        }
    }
    public void ResetData()
    {
        Debug.Log("Resetting graphics settings to default values.");

        //reset mouse speed
        thePlayer.GetComponentInChildren<Player_Camera>().mouseSpeed = def_mouseSpeed;
        thePlayer.GetComponentInChildren<Player_Camera>().sensX = def_mouseSpeed;
        thePlayer.GetComponentInChildren<Player_Camera>().sensY = def_mouseSpeed;
        UIReuseScript.slider_mouseSpeed.value = def_mouseSpeed;
        UIReuseScript.txt_mouseSpeedValue.text = def_mouseSpeed.ToString();
        current_mouseSpeed = thePlayer.GetComponentInChildren<Player_Camera>().mouseSpeed;

        //reset fov
        thePlayer.GetComponentInChildren<Camera>().fieldOfView = def_fov;
        UIReuseScript.slider_fovValue.value = def_fov;
        UIReuseScript.txt_fovValue.text = def_fov.ToString();
        current_fov = thePlayer.GetComponentInChildren<Camera>().fieldOfView;
    }
}