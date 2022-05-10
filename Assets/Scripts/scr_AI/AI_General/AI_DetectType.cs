using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_DetectType : MonoBehaviour
{
    [Header("Vision cone assignables")]
    public Vector3 defaultValues;

    [Header("Assignables")]
    [SerializeField] private Transform target;
    [SerializeField] private AI_Combat AICombatScript;
    [SerializeField] private AI_Health AIHealthScript;
    [SerializeField] private GameObject par_Managers;

    //private variables
    private bool finishedWaiting;
    private MeshCollider visionCone;

    private void Start()
    {
        if (gameObject.GetComponent<MeshCollider>() != null)
        {
            visionCone = gameObject.GetComponent<MeshCollider>();

            visionCone.transform.localScale = defaultValues;
            StartCoroutine(Wait());
        }
    }
    private void Update()
    {
        if (visionCone != null)
        {
            if (AIHealthScript.isAlive)
            {
                Vector3 targetRotation = target.transform.eulerAngles;
                gameObject.transform.eulerAngles = new Vector3(0, targetRotation.y + 90, 0);

                gameObject.transform.position = target.position;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (finishedWaiting 
            && AIHealthScript.isAlive 
            && AIHealthScript.canBeHostile)
        {
            if (other.CompareTag("Player")
                && other.GetComponent<Player_Health>().isPlayerAlive
                && other.GetComponent<Player_Health>().canTakeDamage
                && par_Managers.GetComponent<Manager_Console>().toggleAIDetection
                && !AICombatScript.collidingObjects.Contains(other.gameObject))
            {
                AICombatScript.collidingObjects.Add(other.gameObject);
            }
            else
            {
                Debug.Log(other.name);
            }
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