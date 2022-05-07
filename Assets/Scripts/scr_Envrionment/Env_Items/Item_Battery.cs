using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Battery : MonoBehaviour
{
    [Header("Assignables")]
    public float maxBattery;

    //public but hidden variables
    [HideInInspector] public bool isInUse;
    [HideInInspector] public bool isBeingAssigned;
    [HideInInspector] public float currentBattery;
    [HideInInspector] public GameObject target;

    private void Start()
    {
        currentBattery = maxBattery;
    }

    public void LoadValues()
    {
        //update battery value based off of current remainder
        if (currentBattery < maxBattery / 100 * 75)
        {
            //get battery max value
            int itemValue = gameObject.GetComponent<Env_Item>().int_maxItemValue;
            //get battery current remainder percentage from max remainder
            float remainderPercentage = (currentBattery / maxBattery) * 100;
            //calculate new battery value according to battery remainder percentage
            itemValue = Mathf.FloorToInt(itemValue / 100 * remainderPercentage);
            //assign battery value
            gameObject.GetComponent<Env_Item>().int_ItemValue = itemValue;
        }
        else
        {
            gameObject.GetComponent<Env_Item>().int_ItemValue = gameObject.GetComponent<Env_Item>().int_maxItemValue;
        }
    }
}