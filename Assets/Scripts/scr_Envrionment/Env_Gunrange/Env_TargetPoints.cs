using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_TargetPoints : MonoBehaviour
{
    [Header("Assignables")]
    [Tooltip("Highest possible points.")]
    [SerializeField] private int points;
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private Env_GunrangeScoreboard ScoreboardScript;

    //private variables
    private int finalPoints;

    public void HitTarget()
    {
        float distance = Vector3.Distance(thePlayer.transform.position, transform.position);
        finalPoints = Mathf.RoundToInt(distance * points);
        ScoreboardScript.score += finalPoints;
        ScoreboardScript.UpdateScoreboardValue();
    }
}