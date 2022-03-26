using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Env_GunrangeScoreboard : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private TMP_Text txt_score;

    //public but hidden variables
    [HideInInspector] public int score;

    public void UpdateScoreboardValue()
    {
        if (score < 10)
        {
            txt_score.text = "0000000" + score.ToString();
        }
        else if (score < 100)
        {
            txt_score.text = "000000" + score.ToString();
        }
        else if (score < 1000)
        {
            txt_score.text = "00000" + score.ToString();
        }
        else if (score < 10000)
        {
            txt_score.text = "0000" + score.ToString();
        }
        else if (score < 100000)
        {
            txt_score.text = "000" +score.ToString();
        }
        else if (score < 1000000)
        {
            txt_score.text = "00" + score.ToString();
        }
        else if (score < 10000000)
        {
            txt_score.text = "0" + score.ToString();
        }
        else if (score < 100000000)
        {
            txt_score.text = score.ToString();
        }
    }
}