using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginScreen : MonoBehaviour
{
    #region Serializable fields Fields
    [SerializeField]
    private GameObject radioButon;
    [SerializeField]
    private GameObject radioButon1;
    [SerializeField]
    private Button logInButton;
    [SerializeField]
    private Image logInButtonImg;
    [SerializeField]
    private TextMeshProUGUI logInButtonText;

    [Space(10)]
    [SerializeField]
    private InputField loginInputField;
    [SerializeField]
    private InputField passwordInputField;

    [Space(10)]
    [SerializeField]
    private GameObject LoginPage;
    #endregion

    private void Start()
    {
        if(GlobalParameters.isLogined)
        {
            LoginPage.SetActive(false);
        }
        else
        {
            if (SaveModelController.LoadUserData())
                LogIn(false);
        }
        
    }

    #region Private Fields
    private bool isSaveble = false, isSaveble1 = false;
    #endregion
    #region ChangeInput Field

    public void SetLogin()
    {
        Models.user.user = loginInputField.text;
    }

    public void SetPassword()
    {
         Models.user.password = passwordInputField.text;
    }

    #endregion
    #region LogIn
    public void LogIn(bool checkField)
    {
        if (checkField)
        {
            if (string.IsNullOrEmpty(loginInputField.text))
            {
                StartCoroutine(HighlighInputField(loginInputField.image));
                return;
            }

            /*if (string.IsNullOrEmpty(passwordInputField.text))
            {
                StartCoroutine(HighlighInputField(passwordInputField.image));
                return;
            }*/
        }
        ServerController.onSuccessHandler += SuccesLogin;
        WWWForm form = new WWWForm();
        form.AddField("login", Models.user.user);
        //##form.AddField("password", Models.user.password);
        //Debug.Log("login = " + Models.user.user + " password = " + Models.user.password);
        ServerController.PostREquest("login_app", form, false);

    }
    private void SuccesLogin(WWW www)
    {
        
        Models.user = JsonUtility.FromJson<UserModel>(www.text);
        LoginPage.SetActive(false);
        GlobalParameters.isLogined = true;
        
        if(isSaveble)
            SaveModelController.SaweUserData();
        ServerController.onSuccessHandler -= SuccesLogin;

        CallMessage cmsg = new CallMessage();
        cmsg.from = Models.user.user;
        cmsg.to = SystemInfo.deviceUniqueIdentifier;
        cmsg.comand = "login";

        WEbSocketController.GetInstance.SendMessage(JsonUtility.ToJson(cmsg));
    }
    #endregion
    #region Highlight InputField
    private const float alphaHighlighSpeed = 10f;
    private IEnumerator HighlighInputField(Image inputFieldImg)
    {
        while (inputFieldImg.color.a < 1f)
        {
            inputFieldImg.color = new Color(inputFieldImg.color.r,
                 inputFieldImg.color.g, inputFieldImg.color.b,
                 Mathf.MoveTowards(inputFieldImg.color.a, 1f, alphaHighlighSpeed * Time.deltaTime));
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);

        while (inputFieldImg.color.a > 0f)
        {
            inputFieldImg.color = new Color(inputFieldImg.color.r,
                 inputFieldImg.color.g, inputFieldImg.color.b,
                 Mathf.MoveTowards(inputFieldImg.color.a, 0f, alphaHighlighSpeed * Time.deltaTime));
            yield return null;
        }
    }
    #endregion

    #region Buttons Events
    private void SetLoginButtonActive()
    {
        if(isSaveble1)
        {
            logInButtonImg.color = GlobalParameters.selectedButtonColor;
            logInButtonText.color = GlobalParameters.selectedTextColor;
            logInButton.interactable = true;
        }
        else
        {
            logInButtonImg.color = GlobalParameters.unselectedButtonColor;
            logInButtonText.color = GlobalParameters.unselectedTextColor;
            logInButton.interactable = false;
        }
    }

    public void AutologinButton()
    {
        isSaveble = !isSaveble;
        radioButon.SetActive(isSaveble);
        //SetLoginButtonActive();
    }

    public void ACheckMarkButton()
    {
        isSaveble1 = !isSaveble1;
        radioButon1.SetActive(isSaveble1);
        SetLoginButtonActive();
    }
    #endregion
}
