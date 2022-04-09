using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_DetectType : MonoBehaviour
{
    [Header("Vision cone assignables")]
    public Vector3 defaultValues;
    public MeshCollider visionCone;

    [Header("Assignables")]
    [SerializeField] private Transform target;
    [SerializeField] private AI_Combat AICombatScript;
    [SerializeField] private AI_Health AIHealthScript;
    [SerializeField] private GameObject par_Managers;

    //private variables
    private bool finishedWaiting;

    private void Start()
    {
        if (!finishedWaiting && visionCone != null)
        {
            StartCoroutine(Wait());
        }

        if (visionCone != null && visionCone.transform.localScale != defaultValues)
        {
            visionCone.transform.localScale = defaultValues;
        }
    }
    private void Update()
    {
        if (AIHealthScript.currentHealth > 0)
        {
            if (visionCone != null)
            {
                Vector3 targetRotation = target.transform.eulerAngles;
                gameObject.transform.eulerAngles = new Vector3(0, targetRotation.y + 90, 0);
            }

            gameObject.transform.position = target.position;
        }
        else if (AIHealthScript.currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (finishedWaiting && AIHealthScript.isAlive && AIHealthScript.canBeHostile)
        {
            if (other.CompareTag("Player")
                && par_Managers.GetComponent<Manager_Console>().toggleAIDetection
                && other.GetComponent<Player_Health>().health > 0
                && !AICombatScript.collidingObjects.Contains(other.gameObject))
            {
                AICombatScript.collidingObjects.Add(other.gameObject);
                //Debug.Log("Added " + other.name + " to the AI spotted list.");
            }
            /*
            if (other.CompareTag("NPC")
                && target != other.gameObject
                && !AICombatScript.collidingObjects.Contains(other.gameObject))
            {
                AICombatScript.collidingObjects.Add(other.gameObject);
                //Debug.Log("Added " + other.GetComponent<UI_AIContent>().str_NPCName + " to the AI spotted list.");
            }
            if (other.CompareTag("Item")
                && !AICombatScript.collidingObjects.Contains(other.gameObject))
            {
                AICombatScript.collidingObjects.Add(other.gameObject);
                //Debug.Log("Added " + other.name + " to the AI spotted list.");
            }
            */
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (AIHealthScript.isAlive)
        {
            if (other.CompareTag("Player")
                && AICombatScript.collidingObjects.Contains(other.gameObject))
            {
                AICombatScript.collidingObjects.Remove(other.gameObject);
                //Debug.Log("Removed " + other.name + " from the AI spotted list.");
            }
            /*
            if (other.CompareTag("NPC")
                && AICombatScript.collidingObjects.Contains(other.gameObject))
            {
                AICombatScript.collidingObjects.Remove(other.gameObject);
                //Debug.Log("Removed " + other.GetComponent<UI_AIContent>().str_NPCName + " from the AI spotted list.");
            }
            if (other.CompareTag("Item")
                && AICombatScript.collidingObjects.Contains(other.gameObject))
            {
                AICombatScript.collidingObjects.Remove(other.gameObject);
                //Debug.Log("Removed " + other.name + " from the AI spotted list.");
            }
            */
        }
    }

    //waits 0.1 seconds before allowing AI to detect stuff
    //so that incorrect visionCone rotation will be fixed
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.1f);
        finishedWaiting = true;
    }
}