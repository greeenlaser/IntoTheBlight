using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_GunrangeTargetMove : MonoBehaviour
{
    [Header("Assignables")]
    [Range(0.1f, 5f)]
    [SerializeField] private float speed;
    [SerializeField] private Transform leftEnd;
    [SerializeField] private Transform rightEnd;

    //private variables
    private bool leftSide;
    private bool checkSide;
    private Vector3 endPos;

    private void Start()
    {
        leftSide = Random.value > 0.5f;
        if (leftSide)
        {
            endPos = leftEnd.position;
        }
        else if (!leftSide)
        {
            endPos = rightEnd.position;
        }
    }

    private void Update()
    {
        if (!checkSide)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, endPos, step);

            float distanceToTarget = Vector3.Distance(endPos, transform.position);

            if (distanceToTarget < 0.1f)
            {
                checkSide = true;
            }
        }

        if (checkSide)
        {
            leftSide = !leftSide;
            if (leftSide)
            {
                endPos = leftEnd.position;
            }
            else if (!leftSide)
            {
                endPos = rightEnd.position;
            }

            checkSide = false;
        }
    }
}