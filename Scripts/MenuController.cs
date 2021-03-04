using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    #region Singleton
    private static MenuController instance;
    private static bool isNullInstance
    {
        get
        {
            if (instance == null)
            {
#if UNITY_EDITOR
                System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackTrace(true).GetFrame(1);
                string scriptName = stackFrame.GetFileName();
                int lineNumber = stackFrame.GetFileLineNumber();
                Debug.LogError(scriptName + " instance not found at line " + lineNumber + " !");
#else
                Debug.LogError("MenuPanelManager instance not found!");
#endif
                return true;
            }
            return false;
        }
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion

    #region Serializable Fields
    [SerializeField]
    private RectTransform rectTransform;
    [SerializeField]
    private GameObject menuPanel;

    [Space(10)]
    [SerializeField]
    private GameObject homeScreenObj;

    [Space(10)]
    [SerializeField]
    private TextMeshProUGUI garageDisplayNameText;
    #endregion

    #region Private Fields
    private System.Action ExitFromSystemEvent;

    private List<TextMeshProUGUI> allTextElements = new List<TextMeshProUGUI>();

    private Vector2 showSize;
    private Vector2 hideSize;

    private const float movingSpeed = 5000f;
    private const float colorSpeed = 100f;

    private const int siblingIndex = 10;

    private bool isMoving;
    #endregion

    #region Setup
    private void Start()
    {
        //float kof = SceneManager.GetActiveScene().name.Equals("LoginScene") ? 1 : 2;
        showSize = new Vector2(GameResolution.GetCurrentResolution().x * 0.7f, rectTransform.sizeDelta.y);
        hideSize = Vector2.zero;
        rectTransform.sizeDelta = hideSize;

        GetAllTextChildElements(rectTransform);
    }

    private void Update()
    {
        garageDisplayNameText.text = Models.user.user;
    }

    private void GetAllTextChildElements(Transform targetTransform)
    {
        foreach (Transform child in targetTransform)
        {
            TextMeshProUGUI textElement = targetTransform.GetComponent<TextMeshProUGUI>();
            if (textElement != null)
                allTextElements.Add(textElement);

            if (child.childCount == 0)
            {
                TextMeshProUGUI childTextElement = child.GetComponent<TextMeshProUGUI>();
                if (childTextElement != null)
                    allTextElements.Add(childTextElement);
            }
            else
                GetAllTextChildElements(child);
        }
    }
    #endregion

    #region Add Event Listiners
    public static void AddExitFromSystemEventListiner(System.Action eventListiner)
    {
        if (isNullInstance)
            return;

        instance.ExitFromSystemEvent += eventListiner;
    }
    #endregion

    #region Show Panel Part
    public void ShowMenu()
    {
        if (isMoving)
            return;

        isMoving = true;
        StartCoroutine(ShowMenuCoroutine());
    }
    private IEnumerator ShowMenuCoroutine()
    {
        menuPanel.SetActive(true);
        while (rectTransform.sizeDelta.x != showSize.x)
        {
            rectTransform.sizeDelta = Vector2.MoveTowards(rectTransform.sizeDelta,
                showSize, movingSpeed * Time.deltaTime);
            yield return null;
        }
        yield return ShowMenuTextElements();
    }

    private IEnumerator ShowMenuTextElements()
    {
        while (allTextElements[0].color.a != 1f)
        {
            foreach (TextMeshProUGUI textElement in allTextElements)
            {
                textElement.color = new Color(0f, 0f, 0f,
                    Mathf.MoveTowards(textElement.color.a, 1f, colorSpeed * Time.deltaTime));
            }
            yield return null;
        }

        isMoving = false;
    }
    #endregion

    #region Hide Panel Part
    public void HideMenu()
    {
        if (isMoving)
            return;

        isMoving = true;
        StartCoroutine(HideMenuCoroutine());
    }
    private IEnumerator HideMenuCoroutine()
    {
        StartCoroutine(HideMenuTextElements());
        while (rectTransform.sizeDelta.x != hideSize.x)
        {
            rectTransform.sizeDelta = Vector2.MoveTowards(rectTransform.sizeDelta,
                hideSize, movingSpeed * Time.deltaTime);
            yield return null;
        }
        isMoving = false;

        menuPanel.SetActive(false);
    }

    private IEnumerator HideMenuTextElements()
    {
        while (allTextElements[0].color.a != 0f)
        {
            foreach (TextMeshProUGUI textElement in allTextElements)
            {
                textElement.color = new Color(0f, 0f, 0f,
                    Mathf.MoveTowards(textElement.color.a, 0f, colorSpeed * Time.deltaTime));
            }
            yield return null;
        }
    }
    #endregion
    public void ExitApp()
    {
        PlayerPrefs.DeleteKey("user");
        GlobalParameters.isLogined = false;
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
