using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_Movement : MonoBehaviour
{
    [SerializeField] private float waypointWaitTime;
    [SerializeField] private float targetSearchWaitTime;
    [SerializeField] private List<Transform> wayPoints;

    [Header("Assignables")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private UI_AIContent AIContentScript;
    [SerializeField] private GameObject par_Managers;

    //goingTowardsTarget and calledTargetLostOnce are only used when the
    //AI has combat mechanics, otherwise theyre always false by default
    [HideInInspector] public bool canMove;
    [HideInInspector] public bool isStunned;
    [HideInInspector] public bool goingTowardsTarget;
    [HideInInspector] public bool calledTargetLostOnce;
    [HideInInspector] public GameObject target;

    //private variables
    private bool calledWaitOnce;
    private bool targetInSight;
    private int destinationPoint;
    private float stunRemaining;

    //world border variables
    private float closestDistance;
    private string cellName;

    //these are only used for AI with combat mechanics
    private Vector3 lastKnownTargetPosition;

    private void Start()
    {
        canMove = true;
        agent.autoBraking = false;

        if (wayPoints.Count == 0)
        {
            Debug.LogWarning("Error: No waypoints have been set up for " + gameObject.GetComponent<UI_AIContent>().str_NPCName + ".");
        }
        else
        {
            //update to random point in array
            destinationPoint = Random.Range(0, wayPoints.Count);
            //set next destination for AI as targetPos
            Transform targetPos = wayPoints[destinationPoint];
            agent.destination = targetPos.position;

            calledWaitOnce = false;
        }
    }

    private void Update()
    {
        if (canMove
            && !par_Managers.GetComponent<Manager_GameSaving>().isLoading)
        {
            if (!isStunned)
            {
                //resets stun timer
                if (stunRemaining < 3)
                {
                    stunRemaining = 3;
                }

                if (!goingTowardsTarget)
                {
                    //can talk to this AI if this AI has dialogue
                    //and if AI detection is enabled
                    //and if player is close enough
                    if (Vector3.Distance(transform.position, thePlayer.transform.position) < 3f
                        && !AIContentScript.isAIUIOpen
                        && AIContentScript.hasDialogue
                        && par_Managers.GetComponent<Manager_Console>().toggleAIDetection)
                    {
                        agent.isStopped = true;

                        var lookPos = thePlayer.transform.position - transform.position;
                        lookPos.y = 0;
                        var rotation = Quaternion.LookRotation(lookPos);
                        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5);
                    }
                    else
                    {
                        agent.isStopped = false;
                        GoToNextPoint();
                    }
                }
                //chases the target and deals melee damage when in range
                else if (goingTowardsTarget)
                {
                    if (target == null)
                    {
                        //getting the target gameobject
                        target = gameObject.GetComponent<AI_Combat>().confirmedTarget;
                    }

                    Vector3 targetDir = (target.transform.position - gameObject.transform.position);

                    if (Physics.Raycast(transform.position,
                                        targetDir,
                                        out RaycastHit hit,
                                        50))
                    {
                        if (hit.transform.gameObject == target)
                        {
                            targetInSight = true;
                        }
                        else
                        {
                            targetInSight = false;
                        }
                    }

                    if (targetInSight)
                    {
                        //last known target position always updates if the player is in sight
                        lastKnownTargetPosition = gameObject.GetComponent<AI_Combat>().confirmedTarget.transform.position;
                        //sends the AI to the players position
                        agent.destination = gameObject.GetComponent<AI_Combat>().confirmedTarget.transform.position;

                        //how far is the AI from the target
                        float distance = Vector3.Distance(gameObject.transform.position, target.transform.position);
                        //if AI is in melee range to attack target
                        if (distance <= gameObject.GetComponent<AI_Combat>().attackRange
                            && !gameObject.GetComponent<AI_Combat>().attackConfirmedTarget
                            && !agent.isStopped)
                        {
                            agent.isStopped = true;
                            gameObject.GetComponent<AI_Combat>().attackConfirmedTarget = true;
                        }
                        //if AI is not in melee range to attack target
                        else if (distance > gameObject.GetComponent<AI_Combat>().attackRange
                                 && gameObject.GetComponent<AI_Combat>().attackConfirmedTarget)
                        {
                            agent.isStopped = false;
                            gameObject.GetComponent<AI_Combat>().dealtFirstDamage = false;
                            gameObject.GetComponent<AI_Combat>().attackConfirmedTarget = false;
                        }
                    }
                    else if (!targetInSight)
                    {
                        if (gameObject.GetComponent<AI_Combat>().finishedHostileSearch)
                        {
                            gameObject.GetComponent<AI_Combat>().finishedHostileSearch = false;

                            if (gameObject.GetComponent<AI_Combat>().foundPossibleHostiles)
                            {
                                gameObject.GetComponent<AI_Combat>().foundPossibleHostiles = false;
                            }
                        }

                        //AI goes to last known target position when target goes out of sight
                        agent.destination = lastKnownTargetPosition;

                        //AI waits at last known target position
                        if (agent.remainingDistance < 1f && !targetInSight)
                        {
                            if (!agent.isStopped)
                            {
                                agent.isStopped = true;
                            }
                            StartCoroutine(WaitAtLastKnownTargetPosition());
                        }
                    }
                }
            }
            else if (isStunned)
            {
                agent.isStopped = true;
                stunRemaining -= 1 * Time.deltaTime;

                //Debug.Log(gameObject.GetComponent<UI_AIContent>().str_NPCName + " is stunned for " + Mathf.FloorToInt(stunRemaining) + " more seconds.");

                if (stunRemaining <= 0)
                {
                    isStunned = false;
                }
            }
        }
        else
        {
            agent.isStopped = true;
        }
    }

    //regular waypoint movement
    private void GoToNextPoint()
    {
        if (wayPoints.Count == 0)
        {
            Debug.LogWarning("Error: No waypoints have been set up for " + gameObject.GetComponent<UI_AIContent>().str_NPCName + ".");
        }
        else if (wayPoints.Count > 0
                 && agent.remainingDistance < 1f 
                 && !agent.pathPending 
                 && !calledWaitOnce)
        {
            StartCoroutine(Wait());
        }
    }
    //waits at waypoint until moving to next waypoint
    private IEnumerator Wait()
    {
        if ((gameObject.GetComponent<AI_Combat>() != null
            && gameObject.GetComponent<AI_Combat>().confirmedTarget == null)
            || gameObject.GetComponent<AI_Combat>() == null)
        {
            calledWaitOnce = true;

            agent.isStopped = true;

            yield return new WaitForSeconds(waypointWaitTime);

            agent.isStopped = false;

            //update to random point in array
            destinationPoint = Random.Range(0, wayPoints.Count);
            //set next destination for AI as targetPos
            Transform targetPos = wayPoints[destinationPoint];
            agent.destination = targetPos.position;

            calledWaitOnce = false;
        }
        else
        {
            StopCoroutine(Wait());
        }
    }

    //if target was lost then AI will wait at last known target position for a short period
    private IEnumerator WaitAtLastKnownTargetPosition()
    {
        calledTargetLostOnce = false;
        if (!targetInSight && goingTowardsTarget)
        {
            yield return new WaitForSeconds(targetSearchWaitTime);

            if (!calledTargetLostOnce)
            {
                //AI loses interest in target if target is out of sight for more than 5 seconds
                LostTarget();
            }
        }
        else
        {
            Debug.Log(gameObject.GetComponent<UI_AIContent>().str_NPCName + " spotted the target again!");
            StopCoroutine(WaitAtLastKnownTargetPosition());
        }
    }

    //if target was lost and AI hasnt spotted target
    //then AI will return to original waypoints
    private void LostTarget()
    {
        gameObject.GetComponent<AI_Combat>().lostTarget = true;
        target = null;
        goingTowardsTarget = false;

        Debug.Log(gameObject.GetComponent<UI_AIContent>().str_NPCName + " has lost the target. Returning to original waypoints.");
        calledTargetLostOnce = true;
        StartCoroutine(Wait());
    }

    //if AI collided with world border
    //then look for closest cell spawn point
    //and teleport AI there
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("WorldBlocker"))
        {
            foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
            {
                float distance = Vector3.Distance(gameObject.transform.position, cell.GetComponent<Manager_CurrentCell>().currentCellSpawnpoint.position);

                if (cell == par_Managers.GetComponent<Manager_Console>().allCells[0])
                {
                    closestDistance = distance;
                }
                else
                {
                    if (distance < closestDistance)
                    {
                        cellName = cell.GetComponent<Manager_CurrentCell>().str_CellName;
                    }
                }
            }

            foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
            {
                if (cell.GetComponent<Manager_CurrentCell>().discoveredCell
                    && cell.GetComponent<Manager_CurrentCell>().str_CellName
                    == cellName)
                {
                    gameObject.transform.position = cell.GetComponent<Manager_CurrentCell>().currentCellSpawnpoint.position + new Vector3(0, 0.2f, 0);
                    break;
                }
            }
        }
    }
}