using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlockController : MonoBehaviour
{
    [SerializeField]
    private InputField paswordText;
    [SerializeField]
    public GameObject popup;
    [SerializeField]
    public GameObject wrongPassword;
    [SerializeField]
    public MenuController menu;


    public void ShowBlock()
    {
        popup.SetActive(true);
    }

    public void ExitLog()
    {
        if (paswordText.text.Equals("0525349005"))
        {
            Debug.Log("tututututidkljklgjsd");
            menu.ExitApp();
        }
        else
        {
            wrongPassword.SetActive(false);
            StopCoroutine("WorngPass");
            StartCoroutine("WorngPass");
        }
    }

    IEnumerator WorngPass()
    {
        wrongPassword.SetActive(true);
        yield return new WaitForSeconds(1);
        wrongPassword.SetActive(false);
    }

    public void HideBlock()
    {
        popup.SetActive(false);
    }
}
