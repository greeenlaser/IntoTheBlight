using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_MeleeRangeTargets : MonoBehaviour
{
    //public but hidden variables
    [HideInInspector] public List<GameObject> targets = new List<GameObject>();

    //private variables
    private bool startedTargetListUpdater;

    private void Update()
    {
        if (targets.Count > 0
            && !startedTargetListUpdater)
        {
            StartCoroutine(UpdateTargetsList());
        }
        else if (startedTargetListUpdater
                 && targets.Count == 0)
        {
            startedTargetListUpdater = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!targets.Contains(other.gameObject)
            && other.GetComponent<AI_Health>() != null
            && other.GetComponent<AI_Health>().isAlive
            && other.GetComponent<AI_Health>().isKillable)
        {
            Debug.Log(other.GetComponent<UI_AIContent>().str_NPCName + " is in melee range!");
            targets.Add(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (targets.Contains(other.gameObject))
        {
            Debug.Log(other.GetComponent<UI_AIContent>().str_NPCName + " is no longer in melee range.");
            targets.Remove(other.gameObject);
        }
    }

    //simple loop to remove unwanted targets
    //from targets list that died or became unkillable
    private IEnumerator UpdateTargetsList()
    {
        while (targets.Count > 0)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (!targets[i].GetComponent<AI_Health>().isKillable
                    || !targets[i].GetComponent<AI_Health>().isAlive)
                {
                    targets.Remove(targets[i]);
                }
            }

            foreach (GameObject target in targets)
            {
                if (!target.GetComponent<AI_Health>().isKillable
                    || !target.GetComponent<AI_Health>().isAlive)
                {
                    targets.Remove(target);
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}