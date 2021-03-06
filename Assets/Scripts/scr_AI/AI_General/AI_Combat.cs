using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Combat : MonoBehaviour
{
    [Header("Assignables")]
    [Tooltip("How much melee damage does this AI deal to the confirmed target in range.")]
    public float meleeDamage;
    [Tooltip("How often can this AI deal melee damage?")]
    public float meleeAttackCooldown;
    [Tooltip("How far can this AI hit the target from?")]
    public float attackRange;
    //[Tooltip("How far can this AI detect the target from?")]
    //public float detectRange;
    //[SerializeField] private Manager_FactionReputation FactionScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool searchingForHostiles;
    [HideInInspector] public bool foundPossibleHostiles;
    [HideInInspector] public bool finishedHostileSearch;
    [HideInInspector] public bool attackConfirmedTarget;
    [HideInInspector] public bool dealtFirstDamage;
    [HideInInspector] public bool lostTarget;
    [HideInInspector] public GameObject confirmedTarget;
    [HideInInspector] public List<GameObject> hostileTargets;
    public List<GameObject> collidingObjects;

    //private variables
    private bool calledResetOnce;
    private float timer;

    private void OnDrawGizmos()
    {
        //red sphere for attack range
        Gizmos.color = new Color32(255, 0, 0, 255);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private void Update()
    {
        if (gameObject.GetComponent<AI_Movement>().canMove
            && gameObject.GetComponent<AI_Health>() != null 
            && gameObject.GetComponent<AI_Health>().isAlive
            && !par_Managers.GetComponent<Manager_GameSaving>().isLoading)
        {
            if (!lostTarget 
                && par_Managers.GetComponent<Manager_Console>().toggleAIDetection 
                && gameObject.GetComponent<AI_Health>().canBeHostile)
            {
                if (collidingObjects.Count > 0
                    && hostileTargets.Count == 0
                    && !searchingForHostiles
                    && !finishedHostileSearch)
                {
                    searchingForHostiles = true;
                }
                if (searchingForHostiles
                    && hostileTargets.Count == 0)
                {
                    CheckForHostileTargets();
                }
                else if (finishedHostileSearch
                         && foundPossibleHostiles
                         && hostileTargets.Count > 0
                         && confirmedTarget == null)
                {
                    confirmedTarget = hostileTargets[0];
                    gameObject.GetComponent<AI_Movement>().goingTowardsTarget = true;

                    if (confirmedTarget.CompareTag("Player"))
                    {
                        Debug.Log("Target was confirmed by " + gameObject.GetComponent<UI_AIContent>().str_NPCName + ". Target is the player.");
                    }
                }

                if (attackConfirmedTarget)
                {
                    //only currently attacking player
                    if (confirmedTarget.GetComponent<Player_Health>() != null 
                        && confirmedTarget.GetComponent<Player_Health>().canTakeDamage)
                    {
                        //keeps attacking player while players health is above 0
                        //and while this AI isnt stunned
                        if (confirmedTarget.GetComponent<Player_Health>().isPlayerAlive
                            && !gameObject.GetComponent<AI_Movement>().isStunned)
                        {
                            if (!dealtFirstDamage)
                            {
                                DealDamage();
                                timer = 0;
                                dealtFirstDamage = true;
                            }

                            timer += Time.deltaTime;
                            if (timer > meleeAttackCooldown)
                            {
                                DealDamage();
                                timer = 0;
                            }
                        }
                        //stops attacking player if players health gets to 0
                        else
                        {
                            Debug.Log(gameObject.GetComponent<UI_AIContent>().str_NPCName + " killed the player!");
                            hostileTargets.Remove(confirmedTarget);
                            timer = 0;

                            if (hostileTargets.Count == 0)
                            {
                                gameObject.GetComponent<AI_Movement>().goingTowardsTarget = false;
                            }

                            attackConfirmedTarget = false;
                        }
                    }
                }
            }
            else if ((lostTarget 
                     || !par_Managers.GetComponent<Manager_Console>().toggleAIDetection 
                     || !gameObject.GetComponent<AI_Health>().canBeHostile)
                     && !calledResetOnce)
            {
                Reset();
            }
        }
    }

    //only currently looking for player
    private void CheckForHostileTargets()
    {
        foundPossibleHostiles = false;
        finishedHostileSearch = false;

        int oldCount = collidingObjects.Count;

        for (int i = 0; i < collidingObjects.Count; i++)
        {
            if (collidingObjects[i] == null)
            {
                collidingObjects.Remove(collidingObjects[i]);
                oldCount = collidingObjects.Count;
            }
            else
            {
                Vector3 targetDir = (collidingObjects[i].transform.position - gameObject.transform.position);
                if (Physics.Raycast(transform.position, 
                                    targetDir, 
                                    out RaycastHit hit, 
                                    50))
                {
                    if (collidingObjects[i].CompareTag("Player")
                        && hit.transform.gameObject == collidingObjects[i])
                    {
                        hostileTargets.Add(collidingObjects[i]);
                        oldCount++;
                    }
                }
            }
        }

        if (collidingObjects.Count != oldCount)
        {
            foundPossibleHostiles = true;
        }
        finishedHostileSearch = true;

        if (!foundPossibleHostiles)
        {
            Reset();
        }
    }

    private void DealDamage()
    {
        if (confirmedTarget.GetComponent<Player_Health>() != null)
        {
            confirmedTarget.GetComponent<Player_Health>().DealDamage(name, "melee", meleeDamage);
        }
    }

    private void Reset()
    {
        if ((par_Managers.GetComponent<Manager_Console>().toggleAIDetection
            || lostTarget)
            && calledResetOnce)
        {
            calledResetOnce = false;
        }

        foundPossibleHostiles = false;
        finishedHostileSearch = false;
        attackConfirmedTarget = false;
        dealtFirstDamage = false;
        hostileTargets.Remove(gameObject.GetComponent<AI_Combat>().confirmedTarget);
        confirmedTarget = null;

        gameObject.GetComponent<AI_Movement>().target = null;
        gameObject.GetComponent<AI_Movement>().goingTowardsTarget = false;

        //Debug.Log("Resetting " + gameObject.GetComponent<UI_AIContent>().str_NPCName + "'s settings.");

        lostTarget = false;
    }
}