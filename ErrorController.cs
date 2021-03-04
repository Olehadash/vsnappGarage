using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

struct Messager
{
    public string msg;
}

public class ErrorController : MonoBehaviour
{
    [SerializeField]
    private GameObject parent;
    [SerializeField]
    TextMeshProUGUI message;
    [SerializeField]
    TextMeshProUGUI butLabel;

    private string errorMsg = "אין חיבור לאינטרנט. לבדוק את ההגדרות";
    private string errorLoginMsg = "משתמש שגוי";
    private string errorPassMsg = "סיסמה שגויה";
    private string errorBlockMsg = "משתמש חסום";
    private string userLoginError = "שם משתמש תפוס";

    private void Start()
    {
        ServerController.onErrorHandeler = null;
        ServerController.onErrorHandeler = onServerError;
        if(GlobalParameters.IsUserLogined)
        {
            UserLogError();
        }
    }

    public void UserLogError()
    {
        message.text = userLoginError;
        butLabel.text = "סגור";
        GlobalParameters.IsUserLogined = false;
        parent.SetActive(true);
    }

    public void onButtonHandler()
    {
        parent.SetActive(false);
        
    }

    void onServerError(WWW www)
    {
        if (string.IsNullOrEmpty(www.text))
        {
            message.text = errorMsg;
        }

        

        try
        {
            Messager msg = JsonUtility.FromJson<Messager>(www.text);
            if (msg.msg.Equals("Logon Error. "))
            {
                message.text = errorLoginMsg;
                butLabel.text = "שלח";
            }
            if (msg.msg.Equals("Password Error. "))
            {
                message.text = errorPassMsg;
                butLabel.text = "שלח";
            }

            if (msg.msg.Equals("User Blocked "))
            {
                message.text = errorBlockMsg;
                butLabel.text = "סגור";
            }
        }
        catch
        {
            message.text = errorMsg;
        }
        
        parent.SetActive(true);
    }
}
