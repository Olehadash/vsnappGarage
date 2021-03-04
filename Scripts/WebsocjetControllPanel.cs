using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebsocjetControllPanel : MonoBehaviour
{
    
    public void ConnectClicked()
    {
        //WEbSocketController.GetInstance.Connect();
    }

    public void DisconnectClicked()
    {
        WEbSocketController.GetInstance.Disconnect();
    }

    public void SendMessage()
    {
        WEbSocketController.GetInstance.SendMessage("Message");
    }
}
