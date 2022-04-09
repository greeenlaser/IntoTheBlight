using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_LiftButton : MonoBehaviour
{
    [SerializeField] private bool isLiftCallerButton;
    [SerializeField] private int targetFloor;
    [SerializeField] private Env_Lift LiftScript;
    
    public void GoToTargetFloor()
    {
        //if the lift isnt moving
        //and if the button calls the lift to current floor
        //or if the lift sends the lift to another floor
        //and the target floor isnt the current floor
        if (!LiftScript.liftIsMoving
            && (isLiftCallerButton
            || (!isLiftCallerButton
            && targetFloor != LiftScript.currentFloor)))
        {
            LiftScript.MoveToFloor(targetFloor);
        }
    }
}