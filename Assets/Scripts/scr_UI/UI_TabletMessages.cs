using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_TabletMessages : MonoBehaviour
{
    [Header("Assignables")]
    [Range(3, 10)]
    [SerializeField] private int maxMessageCount;
    [SerializeField] private Transform pos_messageSpawn;
    [SerializeField] private GameObject template_message;
    [SerializeField] private Transform par_SpawnedTabletMessages;

    //public but hidden variables
    [HideInInspector] public List<GameObject> messages = new List<GameObject>();

    public void SendMessage(GameObject logo, string messageTime, string senderName, string messageContent)
    {
        //delete first added message if messages count is over limit
        if (messages.Count +1 > maxMessageCount)
        {
            GameObject deletableMessage = messages[0];
            messages.Remove(deletableMessage);
            Destroy(deletableMessage);
        }
        //move previous sent messages up
        foreach (GameObject message in messages)
        {
            message.transform.position += new Vector3(0, 150, 0);
        }

        //spawn the message gameobject
        GameObject newMessage = Instantiate(template_message, 
                                            pos_messageSpawn.position, 
                                            Quaternion.identity,
                                            par_SpawnedTabletMessages);

        //add message to messages list
        messages.Add(newMessage);

        //get child content from new message
        RawImage logo_message = null;
        TMP_Text time_message = null;
        TMP_Text name_messageSender = null;
        TMP_Text content_message = null;

        foreach (Transform messageChild in newMessage.transform)
        {
            if (messageChild.name == "logo_message")
            {
                foreach (Transform child in messageChild)
                {
                    if (child.name == "image")
                    {
                        logo_message = child.GetComponent<RawImage>();
                    }
                }
            }
            else if (messageChild.name == "txt_messageTime")
            {
                time_message = messageChild.GetComponent<TMP_Text>();
            }
            else if (messageChild.name == "txt_messageSender")
            {
                name_messageSender = messageChild.GetComponent<TMP_Text>();
            }
            else if (messageChild.name == "txt_messageContent")
            {
                content_message = messageChild.GetComponent<TMP_Text>();
            }
        }

        //assign content to new message
        foreach (Transform child in logo.transform)
        {
            if (child.name == "image")
            {
                logo_message.texture = child.GetComponent<RawImage>().texture;
                break;
            }
        }
        time_message.text = messageTime;
        name_messageSender.text = senderName;
        content_message.text = messageContent;
    }
}