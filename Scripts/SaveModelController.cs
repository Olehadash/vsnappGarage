using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveModelController : MonoBehaviour
{
    #region Singlton
    private static SaveModelController instance;

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

    public static SaveModelController GetInstance
    {
        get
        {
            return instance;
        }
    }

    #endregion

    public static void SaweUserData()
    {
        string json = JsonUtility.ToJson(Models.user);
        PlayerPrefs.SetString("user", json);
    }

    public static bool LoadUserData()
    {
        if (PlayerPrefs.HasKey("user"))
        {
            string json = PlayerPrefs.GetString("user");
            Models.user = JsonUtility.FromJson<UserModel>(json);
            return true;
        }
        return false;
    }
}
