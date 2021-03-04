using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCamManager : MonoBehaviour
{
    #region Singleton
    private static DisplayCamManager instance;
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
                Debug.LogError("DisplayCamManager instance not found!");
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

    public static DisplayCamManager GetInstance
    {
        get
        {
            return instance;
        }
    }
    #endregion
    #region Serializable Fields
    [SerializeField]
    private GameObject remoteDisplay;
    [SerializeField]
    private GameObject localDisplay;
    [SerializeField]
    private RawImage displayCamRawImg;
    
    #endregion
    #region Private Fields

    private Vector2 _standartSizeDelta;
    private Vector2 standartSizeDelta
    {
        get
        {
            if (_standartSizeDelta == Vector2.zero)
            {
                _standartSizeDelta = GameResolution.GetCurrentMainCanvasResolution();
                if (_standartSizeDelta.x < _standartSizeDelta.y)
                    _standartSizeDelta = new Vector2(_standartSizeDelta.y, _standartSizeDelta.x);
            }

            return _standartSizeDelta;
        }
    }
    #endregion
    #region Private Properties
    private RectTransform _displayCamImgRect;
    private RectTransform displayCamImgRect
    {
        get
        {
            if (_displayCamImgRect == null)
                _displayCamImgRect = displayCamRawImg.GetComponent<RectTransform>();

            return _displayCamImgRect;
        }
    }
    #endregion
    #region Getters
    public GameObject GetRemote
    {
        get { return this.remoteDisplay; }
    }

    public GameObject GetLocal
    {
        get { return this.localDisplay; }
    }

    #endregion

    #region Setup
    public static void SetupDisplaySize(DeviceOrientation currentDeviceOrientation)
    {
        instance.displayCamImgRect.anchorMin = new Vector2(0.5f, 0.5f);
        instance.displayCamImgRect.anchorMax = new Vector2(0.5f, 0.5f);

#if UNITY_STANDALONE
        instance.displayCamImgRect.sizeDelta = new Vector2(instance.standartSizeDelta.x / 2f, instance.standartSizeDelta.y);
#else
        if (currentDeviceOrientation == DeviceOrientation.Portrait || currentDeviceOrientation == DeviceOrientation.PortraitUpsideDown)
            instance.displayCamImgRect.sizeDelta = new Vector2(instance.standartSizeDelta.y, instance.standartSizeDelta.x);
        else
            instance.displayCamImgRect.sizeDelta = instance.standartSizeDelta;
#endif
    }

    #endregion

    #region Display Camera Orientation
    public static void SetCameraDisplayOrientation(DeviceOrientation oponentDeviceOrientation)
    {
        if (isNullInstance)
            return;

#if UNITY_STANDALONE || UNITY_EDITOR
        instance.SetStandaloneCameraDisplayOrientation(oponentDeviceOrientation);
#else
        instance.SetMobileCameraDisplayOrientation(oponentDeviceOrientation);
#endif
    }

    private void SetStandaloneCameraDisplayOrientation(DeviceOrientation oponentDeviceOrientation)
    {
        if (oponentDeviceOrientation == DeviceOrientation.Portrait)
        {
            displayCamImgRect.sizeDelta = new Vector2(instance.standartSizeDelta.x / 2f, instance.standartSizeDelta.y);
        }
        else
        {
            displayCamImgRect.sizeDelta = new Vector2(instance.standartSizeDelta.x, instance.standartSizeDelta.y);
        }
    }

    private void SetMobileCameraDisplayOrientation(DeviceOrientation oponentDeviceOrientation)
    { // метод для переворота RawImage дисплей камеры в соответствении с ориентацией опонента, при учёте нынешней ориентации
        if (Input.deviceOrientation == DeviceOrientation.Portrait)
        {
            Debug.Log("Current device orientation is Portret");
            switch (oponentDeviceOrientation)
            {
                case DeviceOrientation.LandscapeLeft:
                    {
                        Debug.Log("Oponent device orientation is HorizontalLeft");
                        displayCamImgRect.sizeDelta =
                            new Vector2(instance.standartSizeDelta.x, instance.standartSizeDelta.y);
                        displayCamImgRect.localEulerAngles =
                            new Vector3(instance.displayCamImgRect.localEulerAngles.x, instance.displayCamImgRect.localEulerAngles.y, -90f);
                    }
                    break;

                case DeviceOrientation.LandscapeRight:
                    {
                        Debug.Log("Oponent device orientation is HorizontalRight");
                        displayCamImgRect.sizeDelta =
                            new Vector2(instance.standartSizeDelta.x, instance.standartSizeDelta.y);
                        displayCamImgRect.localEulerAngles =
                            new Vector3(instance.displayCamImgRect.localEulerAngles.x, instance.displayCamImgRect.localEulerAngles.y, 90f);
                    }
                    break;

                default:
                    {
                        Debug.Log("Oponent device orientation is Vertical");
                        displayCamImgRect.sizeDelta =
                            new Vector2(instance.standartSizeDelta.y, instance.standartSizeDelta.x);
                        displayCamImgRect.localEulerAngles =
                            new Vector3(instance.displayCamImgRect.localEulerAngles.x, instance.displayCamImgRect.localEulerAngles.y, 180f);
                    }
                    break;
            }
        }
        else if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft)
        {
            Debug.Log("Current device orientation is LandscapeLeft");
            switch (oponentDeviceOrientation)
            {
                case DeviceOrientation.LandscapeLeft:
                    {
                        Debug.Log("Oponent device orientation is HorizontalLeft");
                        displayCamImgRect.sizeDelta =
                            new Vector2(instance.standartSizeDelta.x, instance.standartSizeDelta.y);
                        displayCamImgRect.localEulerAngles =
                            new Vector3(instance.displayCamImgRect.localEulerAngles.x, instance.displayCamImgRect.localEulerAngles.y, 180f);
                    }
                    break;

                case DeviceOrientation.LandscapeRight:
                    {
                        Debug.Log("Oponent device orientation is HorizontalRight");
                        displayCamImgRect.sizeDelta =
                            new Vector2(instance.standartSizeDelta.x, instance.standartSizeDelta.y);
                        displayCamImgRect.localEulerAngles =
                            new Vector3(instance.displayCamImgRect.localEulerAngles.x, instance.displayCamImgRect.localEulerAngles.y, 0f);
                    }
                    break;

                default:
                    {
                        Debug.Log("Oponent device orientation is Vertical");
                        displayCamImgRect.sizeDelta =
                            new Vector2(instance.standartSizeDelta.y, instance.standartSizeDelta.x);
                        displayCamImgRect.localEulerAngles =
                            new Vector3(instance.displayCamImgRect.localEulerAngles.x, instance.displayCamImgRect.localEulerAngles.y, 90f);
                    }
                    break;
            }
        }
        else if (Input.deviceOrientation == DeviceOrientation.LandscapeRight)
        {
            Debug.Log("Current device orientation is LandscapeRight");
            switch (oponentDeviceOrientation)
            {
                case DeviceOrientation.LandscapeLeft:
                    {
                        Debug.Log("Oponent device orientation is HorizontalLeft");
                        displayCamImgRect.sizeDelta =
                            new Vector2(instance.standartSizeDelta.x, instance.standartSizeDelta.y);
                        displayCamImgRect.localEulerAngles =
                            new Vector3(instance.displayCamImgRect.localEulerAngles.x, instance.displayCamImgRect.localEulerAngles.y, 0f);
                    }
                    break;

                case DeviceOrientation.LandscapeRight:
                    {
                        Debug.Log("Oponent device orientation is HorizontalRight");
                        displayCamImgRect.sizeDelta =
                            new Vector2(instance.standartSizeDelta.x, instance.standartSizeDelta.y);
                        displayCamImgRect.localEulerAngles =
                            new Vector3(instance.displayCamImgRect.localEulerAngles.x, instance.displayCamImgRect.localEulerAngles.y, 180f);
                    }
                    break;

                default:
                    {
                        Debug.Log("Oponent device orientation is Vertical");
                        displayCamImgRect.sizeDelta =
                            new Vector2(instance.standartSizeDelta.y, instance.standartSizeDelta.x);
                        displayCamImgRect.localEulerAngles =
                            new Vector3(instance.displayCamImgRect.localEulerAngles.x, instance.displayCamImgRect.localEulerAngles.y, -90f);
                    }
                    break;
            }
        }
    }
    #endregion

    public void RemoteActive(bool active)
    {
        this.remoteDisplay.SetActive(active);
    }

    #region Agora VideoSurface Part
    public static void EnableDisplayCam()
    {
        Debug.Log("DisplayCam Enabled");
        agora_gaming_rtc.VideoSurface videoSurface =
            instance.remoteDisplay.GetComponent<agora_gaming_rtc.VideoSurface>();
        if (videoSurface != null)
        {
            videoSurface.SetEnable(true);
            videoSurface.SetForUser(0);
        }
        else
        {
            Debug.LogError("VideoSurface on DisplayCam is not present!");
        }
    }

    private void DisableDisplayCam()
    {
        Debug.Log("ANOTHER USER DISCONNECTED");
        agora_gaming_rtc.VideoSurface videoSurface =
            remoteDisplay.GetComponent<agora_gaming_rtc.VideoSurface>();
    }

    public static void ManualDisalbeDisplayCam()
    {
        Debug.Log("MANUAL DISABLE DISPLAYCAM");
        instance.DisableDisplayCam();
    }
    #endregion
}
