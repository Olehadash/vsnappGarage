using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Скрипт, синглтон, отвечает за настройку и предоставление главной камеры в сцене игры
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{ // используется вместо Camera.main
    #region Singleton
    private static CameraManager single;
    private void Awake()
    {
        if (single == null)
            single = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion

    #region Camera reference

    private Camera _gameCamera;
    private Camera gameCamera
    {
        get
        {
            if(_gameCamera == null)
                _gameCamera = GetComponent<Camera>();
            return _gameCamera;
        }
    }
    #endregion

    #region Setup Manager
    private void Start()
    {
        gameCamera.eventMask = 0;
    }
    #endregion

    #region Getters
    public static Camera GetMainCameraInstance()
    {
        return single == null ? null : single.gameCamera;
    }
    #endregion

    #region Return OnMouse Events
    public static void ReturnEventMask()
    {
        single.gameCamera.eventMask = -1;
    }
    #endregion
}
