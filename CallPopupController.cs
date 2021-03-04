using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallPopupController : MonoBehaviour
{
    #region Singlton
    private static CallPopupController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private bool isNullreference
    {
        get
        {
            return instance == null;
        }
    }

    public static CallPopupController GetInstance
    {
        get
        {
            return instance;
        }
    }
    private void OnDestroy()
    {
        instance = null;
    }

    #endregion

    [SerializeField]
    private GameObject parent;
    [SerializeField]
    private AudioSource rington;

    public void StartCall()
    {
        if (isNullreference) return;
        parent.SetActive(true);
        rington.Play();
    }

    public void StopCall()
    {
        if (isNullreference) return;
        parent.SetActive(false);
        rington.Stop();
    }

    public void Answer()
    {
        GlobalParameters.gcmsg.comand = "accept";
        string data = JsonUtility.ToJson(GlobalParameters.gcmsg);
        WEbSocketController.GetInstance.SendMessage(data);
        TestHome.GetInstance.onJoinButtonClicked();
        rington.Stop();
    }

    public void Denay()
    {
        GlobalParameters.gcmsg.comand = "reject";
        string data = JsonUtility.ToJson(GlobalParameters.gcmsg);
        WEbSocketController.GetInstance.SendMessage(data);
        parent.SetActive(false);
        rington.Stop();
    }
}
