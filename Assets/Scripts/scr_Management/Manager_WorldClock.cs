using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Manager_WorldClock : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private Manager_WorldCalendar CalendarScript;
    [SerializeField] private TMP_Text txt_time;

    //public but hidden variables
    [HideInInspector] public string time;
    [HideInInspector] public int hour;
    [HideInInspector] public int minute;
    [HideInInspector] public int second;

    private void Start()
    {
        hour = 12;
    }

    private void FixedUpdate()
    {
        second += Mathf.FloorToInt(120 * Time.deltaTime);
        if (second >= 60)
        {
            minute++;
            second = 0;
            if (minute >= 60)
            {
                hour++;
                minute = 0;
                if (hour >= 24)
                {
                    CalendarScript.UpdateDate();
                    hour = 0;
                }
            }
        }

        if (minute < 10)
        {
            time = hour + ":0" + minute;
        }
        else
        {
            time = hour + ":" + minute;
        }
        txt_time.text = time;
    }
    public void UpdateDate()
    {
        //keeps reducing hours by 1 day and advancing date by one day at a time until hours are less than 24
        while (hour > 24)
        {
            hour -= 24;
            CalendarScript.UpdateDate();
        }
    }
}