using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomePageControll : MonoBehaviour
{
    
    public void GoToVidoeCall()
    {
        GlobalParameters.IsBusy = true;
        SceneManager.LoadScene(1);
    }

    public void GoToHomaPage()
    {
        GlobalParameters.IsBusy = false;
        SceneManager.LoadScene(0);

    }
}
