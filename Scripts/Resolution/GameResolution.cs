using UnityEngine;

/// <summary>
/// Скрипт, синглтон, единственны, кто работает с резолюцией в игре
/// </summary>
[RequireComponent(typeof(Canvas))]
public class GameResolution : MonoBehaviour
{ // определяет резолюцию при помощи любого Canvas'а в сцене, и хранить некоторые значения и методы связанные с ним.
    #region Singleton setup
    private static GameResolution instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            DestroyImmediate(this);
            return;
        }

        _defaultDevelopmentCanvasScale = canvasRect.localScale.x;
    }
    #endregion

    #region Resolution data
    private static Camera _cam;
    private static Camera mainCamera
    {
        get
        {
            if (_cam == null)
                _cam = CameraManager.GetMainCameraInstance();
            return _cam;
        }
    }
    private RectTransform _canvasRect;
    private AspectRatio _aspectRatio = AspectRatio.Undefined;
    private Vector2 _canvasResolution = Vector2.zero;
    private Vector2 _nativeScreenResolution = Vector2.zero;
    private float _modifier = float.NaN;
    private double _ratio = double.NaN;

    private float _defaultDevelopmentCanvasScale;

    #endregion

    #region Resolution data`s getters
    private RectTransform canvasRect
    {
        get
        {
            if (_canvasRect == null)
                _canvasRect = GetComponent<RectTransform>();
            return _canvasRect;
        }
    }    
    private Vector2 canvasResolution // разрешение Canvas'a отличается от реального разрешения устройства(за исключением того, в котором велась разработка)
    {
        get
        {
            if(_canvasResolution == Vector2.zero)
                _canvasResolution = new Vector2(canvasRect.sizeDelta.x, canvasRect.sizeDelta.y);
            return _canvasResolution;
        }
    }
    private Vector2 screenResolution // разрешение нынешнего устройства
    {
        get
        {
            if (_nativeScreenResolution == Vector2.zero)
                _nativeScreenResolution = canvasResolution / modifier;
            return _nativeScreenResolution;
        }
    }
    private float modifier // соотношение между скейлом канваса по умолчанию(0.025) скейлом нынешнего разрешения
    {
        get
        {
            if (float.IsNaN(_modifier))
                _modifier = _defaultDevelopmentCanvasScale / canvasRect.localScale.x;
            return _modifier;
        }
    } 

    private double ratio // является соотношением между сторонами экрана(не является AspectRatio)
    {
        get
        {
            if (double.IsNaN(_ratio))
                _ratio = screenResolution.x / screenResolution.y;
            return _ratio;
        }
    }

    private AspectRatio aspectRatio
    {
        get
        {
            if (_aspectRatio == AspectRatio.Undefined)
                _aspectRatio = AspectRatioTable.GetAspectRatioByRatio(ratio);
            return _aspectRatio;
        }
    }
    #endregion

    #region Getters

    public static AspectRatio GetCurrentAspectRatio()
    {// возвращает соотношение дисплея устройства
        return instance.aspectRatio;
    }
    public static Vector2 GetCurrentResolution() // возвращает разрешение дисплея устройства
    {
        return instance.screenResolution;
    }
    public static Vector2 GetCurrentMainCanvasResolution() // возвращает разрешение InviewUICanvas'а
    {
        return instance.canvasResolution;
    }
    public static double GetCurrentRatio()
    {
        return instance.ratio;
    }
    public static float GetCurrentModifier()
    {
        return instance.modifier;
    }
    public static Vector3 GetCanvasScale()
    {
        return instance.canvasRect.localScale;
    }
    public static Vector3 GetPointerPositionOnScreen(Vector2 pointerPosition)
    { // нажатие на дисплей(Input.mousePosition и Input.touches) расчитываются от левого нижнего угла,
      // эта функция возвращает точку нажатия в коорданатах InviewUICanvas'а, и началом отсчёта является центр экрана
        return mainCamera.ScreenToWorldPoint(pointerPosition) / GetCanvasScale().x;
    }
    #endregion

    #region Useful methods
    public static Vector2 ScreenPointToCanvasPoint(Vector2 screenPointPosition)
    { // нажатие на дисплей расчитываются в разрешении дисплея, а эта функция возвращает точку нажатия в координатах InviewUICanvas'а
        return screenPointPosition * instance.modifier;
    }
    #endregion

    #region Destroy Self
    public static void OnLevelWillChange()
    {
        if (instance != null)
        {
            Destroy(instance);
            instance = null;
        }
    }
    #endregion
}
