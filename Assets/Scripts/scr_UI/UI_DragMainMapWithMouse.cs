using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_DragMainMapWithMouse : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [Header("Assignables")]
    [SerializeField] private GameObject par_Managers;

    //private variables
    //map dragging
    private bool startedDrag;
    private bool calledOriginOnce;
    private Vector3 dragOrigin;

    private void Update()
    {
        if (startedDrag)
        {
            if (!calledOriginOnce)
            {
                //get the origin point of the mouse where the drag started
                dragOrigin = Input.mousePosition;
                calledOriginOnce = true;
            }

            //get the difference between mouse original and current position
            Vector3 difference = dragOrigin - Input.mousePosition;
            //move the map
            par_Managers.GetComponent<Manager_UIReuse>().Minimap.transform.position -= difference / 40;
            par_Managers.GetComponent<Manager_UIReuse>().MinimapPlayerPosition.transform.position -= difference / 40;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!startedDrag)
        {
            startedDrag = true;
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        startedDrag = false;
        calledOriginOnce = false;
    }
}