using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_MeleeRangeTargets : MonoBehaviour
{
    //public but hidden variables
    public List<GameObject> targets;

    //private variables
    private bool startedTargetListUpdater;

    private void Update()
    {
        if (targets.Count > 0
            && !startedTargetListUpdater)
        {
            UpdateTargetsList();
        }
        else if (startedTargetListUpdater
                 && targets.Count == 0)
        {
            startedTargetListUpdater = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!targets.Contains(other.gameObject))
        {
            if (other.GetComponent<AI_Health>() != null
                && other.GetComponent<AI_Health>().isAlive
                && other.GetComponent<AI_Health>().isKillable)
            {
                targets.Add(other.gameObject);
            }
            else if (other.GetComponentInParent<Env_DestroyableCrate>() != null)
            {
                targets.Add(other.gameObject);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (targets.Contains(other.gameObject))
        {
            //Debug.Log(other.GetComponent<UI_AIContent>().str_NPCName + " is no longer in melee range.");
            targets.Remove(other.gameObject);
        }
    }

    //remove unwanted targets from targets list that died or became unkillable
    private void UpdateTargetsList()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].gameObject == null)
            {
                targets.Remove(targets[i]);
            }
            else if (targets[i].GetComponent<AI_Health>() != null
                && (!targets[i].GetComponent<AI_Health>().isKillable
                || !targets[i].GetComponent<AI_Health>().isAlive))
            {
                targets.Remove(targets[i]);
            }
        }
    }
}