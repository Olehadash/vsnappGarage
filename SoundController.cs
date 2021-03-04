using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    #region Singlton
    private static SoundController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    

    private  bool isNullreference
    {
        get
        {
            return instance == null;
        }
    }

    public static SoundController GetInstance
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
    private AudioSource click;

    public void PlayClick()
    {
        if (isNullreference) return;
        click.Play();
    }
}
