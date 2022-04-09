using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Lift : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private float liftMoveSpeed;
    public int currentFloor;
    [SerializeField] private Transform liftFloor;
    [SerializeField] private Transform pos_Lift;
    [SerializeField] private Transform par_Lift;
    [SerializeField] private Env_Door LiftDoorScript;
    [SerializeField] private List<Transform> floors = new List<Transform>();

    //public but hidden variables
    [HideInInspector] public bool liftIsMoving;

    //private variables
    private int chosenFloor;
    private float distanceToChosenFloor;
    private List<GameObject> liftUsers = new List<GameObject>();
    private List<Transform> liftUserParents = new List<Transform>();

    private void OnTriggerEnter(Collider other)
    {
        if (!liftUsers.Contains(other.gameObject))
        {
            //add player, NPC and item onto lift floor
            if (other.CompareTag("Player")
                || other.CompareTag("NPC")
                || other.CompareTag("Item"))
            {
                liftUsers.Add(other.gameObject);
                liftUserParents.Add(other.transform.parent);

                other.transform.SetParent(liftFloor);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        for (int i = 0; i < liftUsers.Count; i++)
        {
            //remove target and target parent from lift lists
            if (other.gameObject == liftUsers[i])
            {
                Transform parent = liftUserParents[i];
                other.transform.SetParent(parent);

                liftUserParents.Remove(parent);
                liftUsers.Remove(other.gameObject);
                break;
            }
        }
    }

    private void Update()
    {
        if (liftIsMoving)
        {
            //get distance to chosen floor
            distanceToChosenFloor = Vector3.Distance(pos_Lift.position, floors[chosenFloor].position);

            //move lift towards chosen floor position
            float step = liftMoveSpeed * Time.deltaTime;
            par_Lift.transform.position = Vector3.MoveTowards(pos_Lift.position, floors[chosenFloor].position, step);

            //stop the lift once it reached its target floor
            if (distanceToChosenFloor <= 0.01f)
            {
                OpenLiftDoors();

                currentFloor = chosenFloor;
                liftIsMoving = false;
            }
        }
    }

    public void MoveToFloor(int floorNumber)
    {
        if (!LiftDoorScript.isClosed 
            && !LiftDoorScript.closeDoor)
        {
            CloseLiftDoors();
        }

        chosenFloor = floorNumber;
        liftIsMoving = true;
    }
    private void OpenLiftDoors()
    {
        if (LiftDoorScript.isClosed 
            && !LiftDoorScript.openDoor)
        {
            LiftDoorScript.openDoor = true;
        }
    }
    private void CloseLiftDoors()
    {
        if (!LiftDoorScript.isClosed 
            && !LiftDoorScript.closeDoor)
        {
            LiftDoorScript.forceCloseDoor = true;
        }
    }
}