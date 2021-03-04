using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnSuccessHandler(WWW www);
public delegate void OnErrorHandeler(WWW www);

public class ServerController : MonoBehaviour
{

    #region Singlton
    private static ServerController instance;

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

    public static ServerController GetInstance
    {
        get
        {
            return instance;
        }
    }

    #endregion
    #region Public Delegates
    public static OnSuccessHandler onSuccessHandler;
    public static OnErrorHandeler onErrorHandeler;
    #endregion

    #region Private fields
    private string host = "https://vsnapp.pp.ua/"; //"http://185.184.247.167:5555/";//"http://localhost:5555/";//
    Dictionary<string, string> headers;
    #endregion

    #region Post Request
    void setHeaders(bool isHeader)
    {
        headers = new Dictionary<string, string>();
        if(isHeader)
        //headers.Add("Authorization", UserModel.user.data.token_type + " " + UserModel.user.data.access_token);
        headers.Add("accept", "application/json");
    }

    public static void PostREquest(string comand, WWWForm form, bool isheaders)
    {
        if (isNullreference) return;
        instance.StartCoroutine(instance.postRequest(comand, form, isheaders));
    }

    IEnumerator postRequest(string comand, WWWForm form, bool isheaders)
    {
        headers = null;
        setHeaders(isheaders);
        WWW www;
        if (form == null)
            www = new WWW(host + comand, null, headers);
        else
            www = new WWW(host + comand, form.data, headers);

        yield return www;
        if (string.IsNullOrEmpty(www.error))
        {
            if (onSuccessHandler != null)
                onSuccessHandler(www);
        }
        else
        {
            if(onErrorHandeler != null)
                onErrorHandeler(www);
            Debug.Log(www.text);
        }
    }
    #endregion
}
