using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Env_Wait : MonoBehaviour
{
    public float maxDistance;

    [Header("Assignables")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private Manager_WorldClock ClockScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;
    [SerializeField] private UI_PauseMenu PauseMenuScript;

    //public but hidden variables
    [HideInInspector] public bool isActivated;
    [HideInInspector] public bool isTimeSliderOpen;
    [HideInInspector] public int int_selectedCount;

    public void OpenTimeSlider()
    {
        UIReuseScript.InteractUIDisabled();

        PauseMenuScript.PauseGameAndCloseUIAndResetBools();

        UIReuseScript.par_TimeSlider.SetActive(true);
        UIReuseScript.txt_CurrentTime.text = ClockScript.time;

        UIReuseScript.btn_Confirm.onClick.AddListener(Confirm);
        UIReuseScript.btn_Cancel.onClick.AddListener(Cancel);

        UIReuseScript.timeSlider.onValueChanged.AddListener(SliderValue);
        UIReuseScript.timeSlider.value = 1;
        UIReuseScript.txt_TimeToWait.text = "1";
        int_selectedCount = 1;
    }

    public void SliderValue(float value)
    {
        //rounding slider value to int and setting total value to selected count
        int_selectedCount = Mathf.FloorToInt(value);
        //setting count value text to selected count
        UIReuseScript.txt_TimeToWait.text = int_selectedCount.ToString();
    }
    public void Confirm()
    {
        PauseMenuScript.callPMCloseOnce = false;
        PauseMenuScript.UnpauseGame();

        ClockScript.hour += int_selectedCount;
        ClockScript.UpdateDate();
        UIReuseScript.ClearTimeSliderUI();
    }
    public void Cancel()
    {
        PauseMenuScript.callPMCloseOnce = false;
        PauseMenuScript.UnpauseGame();

        UIReuseScript.ClearTimeSliderUI();
    }
}