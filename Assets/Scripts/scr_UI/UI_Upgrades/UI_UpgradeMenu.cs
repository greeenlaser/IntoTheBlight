using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_UpgradeMenu : MonoBehaviour, IScrollHandler
{
    [Header("Assignables")]
    [Range(25f, 100f)]
    [SerializeField] private float scrollSpeed = 50f;
    [Range(0.1f, 1f)]
    [SerializeField] private float minScrollSpeedDetect = 0.5f;
    [Range(500f, 1000f)]
    [SerializeField] private float maxHeight = 600f;
    [Range(-1000f, -500f)]
    [SerializeField] private float minHeight = -600f;
    [SerializeField] private RectTransform par_UpgradeUI;

    public void OnScroll(PointerEventData eventData)
    {
        //can only scroll if upgrade ui is in correct height range
        if (par_UpgradeUI.localPosition.y < maxHeight + 0.1f
            && par_UpgradeUI.localPosition.y > minHeight - 0.1f)
        {
            //if scrolling down
            if (Input.mouseScrollDelta.y > minScrollSpeedDetect)
            {
                par_UpgradeUI.localPosition -= new Vector3(0, scrollSpeed, 0);
            }
            //if scrolling up
            else if (Input.mouseScrollDelta.y < minScrollSpeedDetect)
            {
                par_UpgradeUI.localPosition += new Vector3(0, scrollSpeed, 0);
            }
        }
    }

    private void Update()
    {
        //error correction to prevent the upgrade ui from going out of range
        if (par_UpgradeUI.localPosition.y >= maxHeight + 0.1f)
        {
            par_UpgradeUI.localPosition = new Vector3(0, maxHeight, 0);
        }
        else if (par_UpgradeUI.localPosition.y <= minHeight - 0.1f)
        {
            par_UpgradeUI.localPosition = new Vector3(0, minHeight, 0);
        }

        //Debug.Log(par_UpgradeUI.localPosition.y);
    }
}