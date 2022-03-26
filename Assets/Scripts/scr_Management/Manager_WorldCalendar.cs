using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Manager_WorldCalendar : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private TMP_Text txt_date;

    //public but hidden variables
    [HideInInspector] public int dayNumber;
    [HideInInspector] public int monthNumber;
    [HideInInspector] public int realYearNumber;
    [HideInInspector] public int fakeYearNumber;

    //private variables
    private int monthLastDayNumber;
    private List<string> monthEndDayNumbers = new List<string>();

    private void Start()
    {
        //2056
        string jan1_1 = "31";
        monthEndDayNumbers.Add(jan1_1);
        string feb1_2 = "29";
        monthEndDayNumbers.Add(feb1_2);
        string mar1_3 = "31";
        monthEndDayNumbers.Add(mar1_3);
        string apr1_4 = "30";
        monthEndDayNumbers.Add(apr1_4);
        string may1_5 = "31";
        monthEndDayNumbers.Add(may1_5);
        string jun1_6 = "30";
        monthEndDayNumbers.Add(jun1_6);
        string jul1_7 = "31";
        monthEndDayNumbers.Add(jul1_7);
        string aug1_8 = "31";
        monthEndDayNumbers.Add(aug1_8);
        string sep1_9 = "30";
        monthEndDayNumbers.Add(sep1_9);
        string oct1_10 = "31";
        monthEndDayNumbers.Add(oct1_10);
        string nov1_11 = "30";
        monthEndDayNumbers.Add(nov1_11);
        string dec1_12 = "31";
        monthEndDayNumbers.Add(dec1_12);
        //2057
        string jan2_1 = "31";
        monthEndDayNumbers.Add(jan2_1);
        string feb2_2 = "28";
        monthEndDayNumbers.Add(feb2_2);
        string mar2_3 = "31";
        monthEndDayNumbers.Add(mar2_3);
        string apr2_4 = "30";
        monthEndDayNumbers.Add(apr2_4);
        string may2_5 = "31";
        monthEndDayNumbers.Add(may2_5);
        string jun2_6 = "30";
        monthEndDayNumbers.Add(jun2_6);
        string jul2_7 = "31";
        monthEndDayNumbers.Add(jul2_7);
        string aug2_8 = "31";
        monthEndDayNumbers.Add(aug2_8);
        string sep2_9 = "30";
        monthEndDayNumbers.Add(sep2_9);
        string oct2_10 = "31";
        monthEndDayNumbers.Add(oct2_10);
        string nov2_11 = "30";
        monthEndDayNumbers.Add(nov2_11);
        string dec2_12 = "31";
        monthEndDayNumbers.Add(dec2_12);

        dayNumber = 4;
        monthNumber = 9;
        realYearNumber = 2056;
        fakeYearNumber = 2056;
        SetDate();
    }


    public void UpdateDate()
    {
        dayNumber++;
        CheckCurrentDate();
        if (dayNumber > monthLastDayNumber)
        {
            monthNumber++;
            dayNumber = 1;
            if (monthNumber > 12 && fakeYearNumber < 2058)
            {
                realYearNumber++;
                fakeYearNumber++;
                monthNumber = 1;
            }
            else if (monthNumber > 12 && fakeYearNumber >= 2058)
            {
                realYearNumber = 2056;
                fakeYearNumber++;
                monthNumber = 1;
            }
        }

        SetDate();
    }
    private void SetDate()
    {
        string year = fakeYearNumber.ToString();
        if (dayNumber < 10 && monthNumber >= 10)
        {
            txt_date.text = "0" + dayNumber + "." + monthNumber + "." + year[2] + year[3];
        }
        else if (dayNumber >= 10 && monthNumber < 10)
        {
            txt_date.text = dayNumber + ".0" + monthNumber + "." + year[2] + year[3];
        }
        else if (dayNumber < 10 && monthNumber < 10)
        {
            txt_date.text = "0" + dayNumber + ".0" + monthNumber + "." + year[2] + year[3];
        }
        else
        {
            txt_date.text = dayNumber + "." + monthNumber + "." + year[2] + year[3];
        }
    }
    private void CheckCurrentDate()
    {
        if (monthNumber == 1 && realYearNumber == 2056)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[0]);
        }
        if (monthNumber == 2 && realYearNumber == 2056)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[1]);
        }
        if (monthNumber == 3 && realYearNumber == 2056)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[2]);
        }
        if (monthNumber == 4 && realYearNumber == 2056)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[3]);
        }
        if (monthNumber == 5 && realYearNumber == 2056)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[4]);
        }
        if (monthNumber == 6 && realYearNumber == 2056)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[5]);
        }
        if (monthNumber == 7 && realYearNumber == 2056)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[6]);
        }
        if (monthNumber == 8 && realYearNumber == 2056)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[7]);
        }
        if (monthNumber == 9 && realYearNumber == 2056)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[8]);
        }
        if (monthNumber == 10 && realYearNumber == 2056)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[9]);
        }
        if (monthNumber == 11 && realYearNumber == 2056)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[10]);
        }
        if (monthNumber == 12 && realYearNumber == 2056)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[11]);
        }
        if (monthNumber == 1 && realYearNumber == 2057)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[0]);
        }
        if (monthNumber == 2 && realYearNumber == 2057)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[1]);
        }
        if (monthNumber == 3 && realYearNumber == 2057)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[2]);
        }
        if (monthNumber == 4 && realYearNumber == 2057)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[3]);
        }
        if (monthNumber == 5 && realYearNumber == 2057)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[4]);
        }
        if (monthNumber == 6 && realYearNumber == 2057)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[5]);
        }
        if (monthNumber == 7 && realYearNumber == 2057)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[6]);
        }
        if (monthNumber == 8 && realYearNumber == 2057)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[7]);
        }
        if (monthNumber == 9 && realYearNumber == 2057)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[8]);
        }
        if (monthNumber == 10 && realYearNumber == 2057)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[9]);
        }
        if (monthNumber == 11 && realYearNumber == 2057)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[10]);
        }
        if (monthNumber == 12 && realYearNumber == 2057)
        {
            monthLastDayNumber = int.Parse(monthEndDayNumbers[11]);
        }
    }
}