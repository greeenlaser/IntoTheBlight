using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_Movement : MonoBehaviour
{
    [SerializeField] private float waypointWaitTime;
    [SerializeField] private float targetSearchWaitTime;
    [SerializeField] private List<Transform> wayPoints = new List<Transform>();

    [Header("Assignables")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private UI_AIContent AIContentScript;
    [SerializeField] private Manager_Console ConsoleScript;

    //these two bools are only used when the AI has combat mechanics, otherwise theyre always false by default
    [HideInInspector] public bool canMove;
    [HideInInspector] public bool isStunned;
    [HideInInspector] public bool goingTowardsTarget;
    [HideInInspector] public bool calledTargetLostOnce;
    [HideInInspector] public GameObject target;
    [HideInInspector] public GameObject currentCell;
    [HideInInspector] public GameObject lastCell;

    //private variables
    private bool calledWaitOnce;
    private bool targetInSight;
    private int destinationPoint;
    private float realWaitTime;
    private float stunRemaining;
    //these are only used for AI with combat mechanics

    private Vector3 lastKnownTargetPosition;

    private void Start()
    {
        canMove = true;
        realWaitTime = waypointWaitTime;
        waypointWaitTime = 0;
        agent.autoBraking = false;
        GoToNextPoint();
    }

    private void Update()
    {
        if (canMove && !isStunned)
        {
            //resets stun timer
            if (stunRemaining < 3)
            {
                stunRemaining = 3;
            }

            if (!goingTowardsTarget)
            {
                if (Vector3.Distance(transform.position, thePlayer.transform.position) < 3f)
                {
                    //can talk to this AI
                    if (AIContentScript.AIActivated
                        && !AIContentScript.isAIUIOpen
                        && AIContentScript.hasDialogue
                        && ConsoleScript.toggleAIDetection)
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
                else if (Vector3.Distance(transform.position, thePlayer.transform.position) > 3f)
                {
                    agent.isStopped = false;
                    GoToNextPoint();
                }
            }
            //chases the target and deals melee damage when in range
            if (goingTowardsTarget)
            {
                if (target == null)
                {
                    //getting the target gameobject
                    target = gameObject.GetComponent<AI_Combat>().confirmedTarget;
                }

                Vector3 targetDir = (target.transform.position - gameObject.transform.position);
                if (Physics.Raycast(transform.position, targetDir, out RaycastHit hit, 50))
                {
                    if (hit.transform.gameObject == target)
                    {
                        Debug.DrawRay(transform.position, targetDir, Color.green);
                        //if (target.CompareTag("Player"))
                        //{
                        //Debug.Log("Hit correct target - player.");
                        //}
                        //else if (target.CompareTag("NPC"))
                        //{
                        //Debug.Log("Hit correct target - " + target.GetComponent<UI_AIContent>().str_NPCName + ".");
                        //}

                        if (!targetInSight)
                        {
                            targetInSight = true;
                        }
                    }
                    else
                    {
                        Debug.DrawRay(transform.position, targetDir, Color.red);
                        //Debug.Log("Hit incorrect target - " + hit.transform.name + ".");

                        if (targetInSight)
                        {
                            targetInSight = false;
                        }
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
        else
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
            }
        }

        if (isStunned)
        {
            agent.isStopped = true;
            stunRemaining -= 1 * Time.deltaTime;
            //Debug.Log(gameObject.GetComponent<UI_AIContent>().str_NPCName + " is stunned.");
            //Debug.Log(gameObject.GetComponent<UI_AIContent>().str_NPCName + " is stunned for " + Mathf.FloorToInt(stunRemaining) + " more seconds.");

            if (stunRemaining <= 0)
            {
                isStunned = false;
            }
        }
        if (!isStunned
            && gameObject.GetComponent<Rigidbody>().velocity.x < 1
            && gameObject.GetComponent<Rigidbody>().velocity.y < 1)
        {
            agent.isStopped = false;
            GoToNextPoint();
        }
    }

    //regular waypoint movement
    private void GoToNextPoint()
    {
        if (wayPoints.Count == 0)
        {
            Debug.Log("No waypoints were set up for " + gameObject.GetComponent<UI_AIContent>().str_NPCName + ".");
            return;
        }

        if (agent.remainingDistance < 1f && !agent.pathPending && !calledWaitOnce)
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
            waypointWaitTime = realWaitTime;
            agent.isStopped = false;
            //update to random point in array
            destinationPoint = Random.Range(0, wayPoints.Count);
            //set next destination for AI as targetPos
            Transform targetPos = wayPoints[destinationPoint];
            agent.destination = targetPos.position;
            calledWaitOnce = false;
            GoToNextPoint();
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("WorldBlocker"))
        {
            if (currentCell != null)
            {
                transform.position = currentCell.GetComponent<Manager_CurrentCell>().currentCellSpawnpoint.position;
            }
            else if (currentCell == null && lastCell != null)
            {
                transform.position = lastCell.GetComponent<Manager_CurrentCell>().currentCellSpawnpoint.position;
            }
        }
    }
}