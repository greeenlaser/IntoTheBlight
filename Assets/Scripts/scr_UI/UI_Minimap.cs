using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Minimap : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private UI_PauseMenu PauseMenuScript;
    [SerializeField] private Manager_Console ConsoleScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;

    //public but hidden variables
    [HideInInspector] public Vector3 playerRotation;
    [HideInInspector] public Vector3 playerPosition;
    //[HideInInspector] public List<RawImage> friendlyNPCs = new List<RawImage>();

    //private variables
    private float playerYRot;
    private Vector3 playerPos;

    private void Update()
    {
        if (!PauseMenuScript.isGamePaused)
        {
            //minimap updates with player rotation and position
            //get player y global rotation
            playerYRot = thePlayer.transform.eulerAngles.y;

            //get player global x and z positions
            playerPos = thePlayer.transform.position;

            //Debug.Log(playerYRot + " (" + MinimapPlayerPos.eulerAngles + ")");

            //get player position transform on minimap
            Transform MinimapPlayerPos = UIReuseScript.MinimapPlayerPosition.transform;
            //update player z rotation on minimap to be the same as players own y rotation in game
            MinimapPlayerPos.eulerAngles = new Vector3(MinimapPlayerPos.eulerAngles.x, MinimapPlayerPos.eulerAngles.y, -playerYRot);
            //save player minimap rotation as new vector to use with main map player rotation
            playerRotation = MinimapPlayerPos.eulerAngles;

            //update minimap position according to the opposite of player position
            UIReuseScript.Minimap.transform.position = new Vector3(-playerPos.x + 2605, -playerPos.z -920, 0);
            //save player position as new vector to use with main map player position
            playerPosition = MinimapPlayerPos.transform.position;
        }
    }
}