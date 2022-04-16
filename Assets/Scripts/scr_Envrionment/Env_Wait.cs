using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Wait : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] GameObject par_Managers;

    //private variables
    private int int_selectedCount;

    public void OpenTimeSlider()
    {
        par_Managers.GetComponent<UI_PauseMenu>().isWaitableUIOpen = true;
        par_Managers.GetComponent<Manager_UIReuse>().InteractUIDisabled();

        par_Managers.GetComponent<UI_PauseMenu>().PauseGameAndCloseUIAndResetBools();

        par_Managers.GetComponent<Manager_UIReuse>().par_TimeSlider.SetActive(true);
        par_Managers.GetComponent<Manager_UIReuse>().txt_CurrentTime.text = par_Managers.GetComponent<Manager_WorldClock>().time;

        par_Managers.GetComponent<Manager_UIReuse>().btn_Confirm.onClick.AddListener(Confirm);
        par_Managers.GetComponent<Manager_UIReuse>().btn_Cancel.onClick.AddListener(Cancel);

        par_Managers.GetComponent<Manager_UIReuse>().timeSlider.onValueChanged.AddListener(SliderValue);
        par_Managers.GetComponent<Manager_UIReuse>().timeSlider.value = 1;
        par_Managers.GetComponent<Manager_UIReuse>().txt_TimeToWait.text = "1";
        int_selectedCount = 1;
    }

    public void SliderValue(float value)
    {
        //rounding slider value to int and setting total value to selected count
        int_selectedCount = Mathf.FloorToInt(value);
        //setting count value text to selected count
        par_Managers.GetComponent<Manager_UIReuse>().txt_TimeToWait.text = int_selectedCount.ToString();
    }
    public void Confirm()
    {
        par_Managers.GetComponent<UI_PauseMenu>().callPMCloseOnce = false;
        par_Managers.GetComponent<UI_PauseMenu>().UnpauseGame();

        par_Managers.GetComponent<Manager_WorldClock>().hour += int_selectedCount;
        par_Managers.GetComponent<Manager_WorldClock>().hoursUntilCellReset -= int_selectedCount;

        if (par_Managers.GetComponent<Manager_WorldClock>().hoursUntilCellReset <= 0)
        {
            par_Managers.GetComponent<GameManager>().GlobalCellReset();

            //removes more hours from the next full 72 hours
            //if this waitable waited past the 0 hours until global cell restart
            int hoursToRemove = 0;
            for (int i = hoursToRemove; i < 1; i++)
            {
                hoursToRemove++;
            }

            //resets timer back to 72
            par_Managers.GetComponent<Manager_WorldClock>().hoursUntilCellReset = 72;

            if (hoursToRemove > 0)
            {
                par_Managers.GetComponent<Manager_WorldClock>().hoursUntilCellReset -= hoursToRemove;
            }
        }

        par_Managers.GetComponent<Manager_WorldClock>().UpdateDate();
        par_Managers.GetComponent<Manager_UIReuse>().ClearTimeSliderUI();

        par_Managers.GetComponent<UI_PauseMenu>().isWaitableUIOpen = false;
    }
    public void Cancel()
    {
        par_Managers.GetComponent<UI_PauseMenu>().callPMCloseOnce = false;
        par_Managers.GetComponent<UI_PauseMenu>().UnpauseGame();

        par_Managers.GetComponent<Manager_UIReuse>().ClearTimeSliderUI();

        par_Managers.GetComponent<UI_PauseMenu>().isWaitableUIOpen = false;
    }
}