using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_ResetPos : MonoBehaviour
{
    [SerializeField] private Manager_Console ConsoleScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (ConsoleScript.currentCell != null)
            {
                GameObject currentCell = ConsoleScript.currentCell;
                other.transform.position = currentCell.GetComponent<Manager_CurrentCell>().currentCellSpawnpoint.position;
            }

            else if (ConsoleScript.currentCell == null && ConsoleScript.lastCell != null)
            {
                GameObject lastCell = ConsoleScript.lastCell;
                other.transform.position = lastCell.GetComponent<Manager_CurrentCell>().currentCellSpawnpoint.position;
            }
        }

        else if (other.CompareTag("Item"))
        {
            if (ConsoleScript.currentCell != null)
            {
                GameObject currentCell = ConsoleScript.currentCell;
                other.transform.position = currentCell.GetComponent<Manager_CurrentCell>().currentCellSpawnpoint.position;
                other.GetComponent<Env_Item>().itemActivated = true;
                other.GetComponent<Rigidbody>().isKinematic = true;
                other.GetComponent<Rigidbody>().isKinematic = false;
            }

            else if (ConsoleScript.currentCell == null && ConsoleScript.lastCell != null)
            {
                GameObject lastCell = ConsoleScript.lastCell;
                other.transform.position = lastCell.GetComponent<Manager_CurrentCell>().currentCellSpawnpoint.position;
                other.GetComponent<Env_Item>().itemActivated = true;
                other.GetComponent<Rigidbody>().isKinematic = true;
                other.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }
}