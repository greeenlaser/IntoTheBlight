using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Ladder : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_Ladder;
    [SerializeField] private AudioSource ladderSFX;
    [SerializeField] private Player_Movement PlayerMovementScript;

    //private variables
    private bool isColliding;

    private void Update()
    {
        if (isColliding)
        {
            if (par_Ladder.transform.eulerAngles.y == 0
                && thePlayer.transform.eulerAngles.y > 0
                && thePlayer.transform.eulerAngles.y < 180)
            {
                PlayerMovementScript.facingOtherLadderSide = true;
            }
            else if (par_Ladder.transform.eulerAngles.y == 90
                && thePlayer.transform.eulerAngles.y > 90
                && thePlayer.transform.eulerAngles.y < 270)
            {
                PlayerMovementScript.facingOtherLadderSide = true;
            }
            else if (par_Ladder.transform.eulerAngles.y == 180
                && thePlayer.transform.eulerAngles.y > 180)
            {
                PlayerMovementScript.facingOtherLadderSide = true;
            }
            else if (par_Ladder.transform.eulerAngles.y == 270
                && (thePlayer.transform.eulerAngles.y > 270
                || thePlayer.transform.eulerAngles.y < 90))
            {
                PlayerMovementScript.facingOtherLadderSide = true;
            }
            else
            {
                PlayerMovementScript.facingOtherLadderSide = false;
            }
            //Debug.Log(thePlayer.transform.eulerAngles.y);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovementScript.isClimbingLadder = true;
            isColliding = true;
            //Debug.Log("Player is climbing ladder.");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ladderSFX.Stop();
            PlayerMovementScript.isClimbingLadder = false;
            PlayerMovementScript.facingOtherLadderSide = false;
            isColliding = false;
            //Debug.Log("Player is no longer climbing ladder.");
        }
    }
}