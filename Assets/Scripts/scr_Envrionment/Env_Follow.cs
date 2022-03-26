using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Follow : MonoBehaviour
{
    [SerializeField] private GameObject target;

    private void Update()
    {
        gameObject.transform.position = target.transform.position;
    }
}