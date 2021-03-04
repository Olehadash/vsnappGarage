using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO;
using UnityEngine.SceneManagement;

public delegate void AftergetTrueSocketMessage();

public struct CallMessage
{
    public string from;
    public string to;
    public string comand;
}
public class WEbSocketController : MonoBehaviour
{
    #region Singlton
    private static WEbSocketController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private static bool isNullreference
    {
        get
        {
            return instance == null;
        }
    }

    public static WEbSocketController GetInstance
    {
        get
        {
            return instance;
        }
    }

    #endregion

    SocketManager Manager;
    private string address = "https://vsnapp.pp.ua/socket.io/";//"http://185.184.247.167:5555/socket.io/";//
    public bool isConnect = false;
    //public CallPopupController callPopup;

    Dictionary<string, object> data;
    AftergetTrueSocketMessage afterMessage;

    // Start is called before the first frame update
    void Start()
    {
        var options = new SocketOptions();
        //options.ConnectWith = BestHTTP.SocketIO.Transports.TransportTypes.Polling;
        Manager = new SocketManager(new System.Uri(address), options);
        if (!isConnect)
        {
            Connect();
        }
    }

    public void Connect()
    {
        //webSocket.Open();
        Manager.Socket.On(SocketIOEventTypes.Connect, (s, p, a) =>
        {
            //Debug.Log("socketio Connteced");
            SendMessage("");
            isConnect = true;
        });

        Manager.Socket.On(SocketIOEventTypes.Error, (socket, packet, args) =>
        {
            Debug.Log(string.Format("Error: {0}", args[0].ToString()));
        });

        Manager.Socket.On("message", OnNewMessage);

        Manager.Socket.On(SocketIOEventTypes.Disconnect, (s, p, a) =>
        {
            Debug.Log("socketio DisConnteceted");
        });


    }
    public void Disconnect()
    {
        Manager.Socket.Disconnect();
    }

    void OnNewMessage(Socket socket, Packet packet, params object[] args)
    {
        //addChatMessage();
        //data = args[0] as Dictionary<string, object>;
        var msg = args[0] as string;
        Debug.Log(msg);
        if (string.IsNullOrEmpty(msg)) return;

        CallMessage cmsg = JsonUtility.FromJson<CallMessage>(msg);
        GlobalParameters.gcmsg = new CallMessage();
        GlobalParameters.gcmsg.from = Models.user.user;
        GlobalParameters.gcmsg.to = cmsg.from;
        if (cmsg.from.Equals(Models.user.user))
        {
            if (cmsg.comand.Equals("login"))
            {
                if (!SystemInfo.deviceUniqueIdentifier.Equals(cmsg.to))
                {
                    cmsg.comand = "outlog";
                    SendMessage(JsonUtility.ToJson(cmsg));
                }
            }
            if (cmsg.comand.Equals("outlog"))
            {
                if (SystemInfo.deviceUniqueIdentifier.Equals(cmsg.to))
                {
                    GlobalParameters.isLogined = false;
                    PlayerPrefs.DeleteKey("user");
                    GlobalParameters.IsUserLogined = true;
                    SceneManager.LoadScene(0);
                }
            }
        }

        if (cmsg.to.Equals(Models.user.user))
        {
            
            if (cmsg.comand.Equals("call"))
            {
                if (GlobalParameters.IsBusy)
                {
                    GlobalParameters.gcmsg.comand = "busy";
                    string data = JsonUtility.ToJson(GlobalParameters.gcmsg);
                    SendMessage(data);
                    return;
                }
                else
                {
                    CallPopupController.GetInstance.StartCall();
                    return;
                }
            }
            if(cmsg.comand.Equals("stop"))
            {
                if (GlobalParameters.IsBusy) return;
                if (SceneManager.GetActiveScene().name.Equals("VideoChatGArage"))
                {
                    TestHome.GetInstance.onLeaveButtonClicked();
                    GlobalParameters.IsBusy = false;
                    SceneManager.LoadScene(0);
                }
                else {
                    CallPopupController.GetInstance.StopCall();
                }
                return;
            }
            
        }
    }

    public void SendMessage(string message)
    {
        //Debug.Log("socketio = " + message);
        Manager.Socket.Emit("message", message);
    }
    private void OnApplicationQuit()
    {
        Disconnect();
    }
    private void OnApplicationPause(bool pause)
    {
        if (Manager == null) return;
        if (!pause)
            Connect();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (Manager == null) return;
        if (focus)
            Connect();
    }
}
