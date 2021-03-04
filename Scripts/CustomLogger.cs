using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CustomLogger : MonoBehaviour
{
    #region Singleton
    private static CustomLogger instance;
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
                LogErrorMessage(scriptName + " instance not found at line " + lineNumber + " !");
#else
                LogErrorMessage("CustomLogger instance not found!");
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
            LogErrorMessage("There is two CustomLogger's in scene!");
            Destroy(gameObject);
            return;
        }

        stringBuilder = new System.Text.StringBuilder(64);
        Application.logMessageReceived += LogUnityMessage;

        fileLogger = new FileLogger();

        LogMessage("CustomLogger is started!");
    }
    #endregion

    #region Serializable Fields
    [SerializeField]
    private Text logText;
    [SerializeField]
    private GameObject logBackgroundObject;
    #endregion

    #region Private Fields
    private Image logImage;

    private Button showConsoleButton;

    private FileLogger fileLogger;

    private System.Text.StringBuilder stringBuilder;

    private const float loggerVersion = 1.9f;

    private const int textWindowSize = 10000;
    #endregion

    #region Setup
    private void Start()
    {
        stringBuilder = new System.Text.StringBuilder(64);

        showConsoleButton = GetComponent<Button>();
        if(showConsoleButton == null)
        {
            LogErrorMessage("Logger showConsoleButton is null!");
            return;
        }
        showConsoleButton.onClick.AddListener(SetVisibleLogConsole);

        if (logBackgroundObject == null)
        {
            LogErrorMessage("Logger logBackgroundObject is null!");
            return;
        }

        logImage = GetComponent<Image>();
        if (logImage == null)
        {
            LogErrorMessage("Logger logImage is null!");
            return;
        }
    }

    public static void ConnectRemoteFileLogger(IRemoteFileLogger remoteFileLogger)
    {
        if (isNullInstance)
            return;

        instance.fileLogger.ConnectRemoteFileLogger(remoteFileLogger);
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= LogUnityMessage;
    }
    #endregion

    #region Visible Logging Part
    public static void LogMessage(string message)
    {
        if (isNullInstance)
            return;

#if UNITY_EDITOR
        Debug.Log(message);
        return;
#endif

        if (instance.stringBuilder.Length > 0)
            instance.stringBuilder.Clear();

        instance.stringBuilder.Append("Log: ").Append(message).Append("\n");

        message = instance.stringBuilder.ToString();
        instance.AppendMessageToDebugWindow(true, Color.yellow);

        if (instance.fileLogger != null)
            instance.fileLogger.WriteNewLine(message, false);

        if (instance.stringBuilder.Length > 0)
            instance.stringBuilder.Clear();
    }

    public static void LogErrorMessage(string message)
    {
        if (isNullInstance)
            return;

#if UNITY_EDITOR
        Debug.LogError(message);
        return;
#endif

        if (instance.stringBuilder.Length > 0)
            instance.stringBuilder.Clear();

        instance.logImage.color = Color.red;

        instance.stringBuilder.Append("Err: ").Append(message).Append("\n");

        message = instance.stringBuilder.ToString();
        instance.AppendMessageToDebugWindow(true, Color.red);

        if (instance.fileLogger != null)
            instance.fileLogger.WriteNewLine(message, true);

        if (instance.stringBuilder.Length > 0)
            instance.stringBuilder.Clear();
    }

    public static void LogUnityMessage(string message, string stackTrace, LogType type)
    {
        if (isNullInstance)
            return;

        bool isError = type == LogType.Error || type == LogType.Exception;
        if(isError)
            instance.logImage.color = Color.red;

        if (instance.stringBuilder.Length > 0)
            instance.stringBuilder.Clear();

        instance.stringBuilder.Append(isError ? "UErr: " : "ULog: ").
            Append(message).Append("\n");

        message = instance.stringBuilder.ToString();
        instance.AppendMessageToDebugWindow(false, isError ? Color.red : Color.blue);

        if (instance.fileLogger != null)
            instance.fileLogger.WriteNewLine(message, isError);

        instance.stringBuilder.Clear();
    }
    #endregion

    #region Visualize Part
    private void SetVisibleLogConsole()
    {
        logBackgroundObject.SetActive(!logBackgroundObject.activeSelf);
    }

    private void AppendMessageToDebugWindow(bool isCustom, Color color)
    {
        if (instance.logText != null)
        {
            if (instance.logText.text.Length >= textWindowSize)
                instance.logText.text = string.Empty;

            string hexColor = ColorUtility.ToHtmlStringRGB(color);
            if (isCustom)
                instance.stringBuilder.Insert(3, "</color>");
            else
                instance.stringBuilder.Insert(4, "</color>");
            instance.stringBuilder.Insert(0, "<color=#" + hexColor + ">");
            instance.logText.text += instance.stringBuilder.ToString();
        }
    }
    #endregion

    #region Getters
    public static string GetLogFileName()
    {
        if (isNullInstance)
            return string.Empty;

        return instance.fileLogger.GetLogFileName();
    }
    #endregion

    private class FileLogger
    {
        #region Private Fields
        private IRemoteFileLogger remoteFileLogger;

        private System.Text.StringBuilder fileLoggerStringBuilder;

        private const string defaultLogSeparator = "--------------------------------------------------------------------------------";

        private const string LOG_FILE_DIR = "LocalLogFile";
        private const string LOG_FILENAME = "Log.txt";

        private readonly string localLogFileFolderPath;
        private readonly string localLogFilePath;
        private string remoteLogFilePath;
        #endregion

        public FileLogger()
        {
            fileLoggerStringBuilder = new System.Text.StringBuilder();

            localLogFileFolderPath = Path.Combine(Application.persistentDataPath, LOG_FILE_DIR);
            if (!Directory.Exists(localLogFileFolderPath))
                Directory.CreateDirectory(localLogFileFolderPath);

            localLogFilePath = Path.Combine(localLogFileFolderPath, LOG_FILENAME);

            CreateOrAddSession(!IsLocalLogFileExist());
        }

        #region Remote Logger
        public void ConnectRemoteFileLogger(IRemoteFileLogger remoteFileLogger)
        {
            this.remoteFileLogger = remoteFileLogger;
            this.remoteFileLogger.AddDownloadRemoteLogFileSuccessEventListiner(DownloadLogFileResult);
            this.remoteFileLogger.AddUploadRemoteLogFileSuccessEventListiner(UploadLogFileResult);
        }

        private void DownloadLogFileResult(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                LogMessage("Remote log file does not exist, upload local file: \n" +
                    localLogFilePath);
                remoteFileLogger.UploadLogFile(localLogFilePath);
                return;
            }

            remoteLogFilePath = filePath;
            CompareLocalLogFileWithRemote();
        }

        private void UploadLogFileResult(bool result)
        {
            if (!result || remoteFileLogger.IsBackupModeEnabled())
                return;

            if (string.IsNullOrEmpty(remoteLogFilePath))
            {
                LogMessage("remoteLogFilePath in LogFileUpdatedSuccess was not loaded, attempt to upload remote log file...");
                remoteFileLogger.DownloadLogFile(GetLogFileName());
                return;
            }

            string localLogFileRawData = File.ReadAllText(localLogFilePath, System.Text.Encoding.UTF8);
            if (string.IsNullOrEmpty(localLogFileRawData))
            {
                LogMessage("localLogFileRawData in LogFileUpdatedSuccess is empty!");
                return;
            }

            int removeIndex = 0;
            while (true)
            {
                int index = localLogFileRawData.IndexOf("~", removeIndex) + 1;
                if (index == 0)
                    break;
                removeIndex = index;
            }

            localLogFileRawData = localLogFileRawData.Remove(0, removeIndex);
            localLogFileRawData = localLogFileRawData.Trim("\r\n".ToCharArray());

            File.WriteAllText(localLogFilePath, localLogFileRawData);
        }
        #endregion

        #region Local Log File Part
        private void CreateOrAddSession(bool isCreate)
        {
            if (!isCreate)
                fileLoggerStringBuilder.Append(Environment.NewLine).Append("~").
                    Append(Environment.NewLine).Append(Environment.NewLine).Append(Environment.NewLine);

            fileLoggerStringBuilder.Append("Session Number: ").Append(Guid.NewGuid()).Append("\n");
            fileLoggerStringBuilder.Append("Session Date: ").
                Append(DateTime.Now.Day).Append(".").
                Append(DateTime.Now.Month).Append(".").
                Append(DateTime.Now.Year).
                Append(" ").
                Append(DateTime.Now.Hour).Append(":").
                Append(DateTime.Now.Minute).Append(":").
                Append(DateTime.Now.Second).
                Append("\n");
            fileLoggerStringBuilder.Append("CustomLogger Version: ").Append(loggerVersion).Append("\n");
            fileLoggerStringBuilder.Append("@").Append("\n");
#if !UNITY_EDITOR
            AndroidJavaObject javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject unityContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
            string packageName = unityContext.Call<string>("getPackageName");

            AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            AndroidJavaObject packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);

            int apiLevel = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
            string deviceName = new AndroidJavaClass("android.os.Build").GetStatic<string>("MODEL");

            fileLoggerStringBuilder.Append("Build Version: ").Append(Application.version).Append("\n");
            fileLoggerStringBuilder.Append("Bundle Version Code: ").Append(packageInfo.Get<string>("versionName")).Append("\n");
            fileLoggerStringBuilder.Append("Package Name: ").Append(packageName).Append("\n");
            fileLoggerStringBuilder.Append("Android Version: ").Append(apiLevel).Append("\n");
            fileLoggerStringBuilder.Append("Device Name: ").Append(deviceName).Append("\n");
            fileLoggerStringBuilder.Append("@").Append("\n");
#endif
            File.AppendAllText(localLogFilePath, fileLoggerStringBuilder.ToString());
            fileLoggerStringBuilder.Clear();
            LogMessage("Setup log file success!");
        }

        public void WriteNewLine(string message, bool isNeedToUpload)
        {
            fileLoggerStringBuilder.Append("_").
                Append(DateTime.Now.Day).Append("_").
                Append(DateTime.Now.Hour).Append("-").
                Append(DateTime.Now.Minute).Append("-").
                Append(DateTime.Now.Second);
            message = message.Insert(message.IndexOf(':', 0), fileLoggerStringBuilder.ToString()) + 
                defaultLogSeparator + "\n";

            File.AppendAllText(localLogFilePath, message);
            fileLoggerStringBuilder.Clear();

            if (isNeedToUpload &&
                !string.IsNullOrEmpty(remoteLogFilePath) && remoteFileLogger.IsAvailable())
                CompareLocalLogFileWithRemote();
        }
        private bool IsLocalLogFileExist()
        {
            if (!Directory.Exists(localLogFileFolderPath))
                return false;

            if(!File.Exists(localLogFilePath))
                return false;

            return true;
        }

        private void CompareLocalLogFileWithRemote()
        {
            if (string.IsNullOrEmpty(remoteLogFilePath))
            {
                LogErrorMessage("RemoteLogFile was not loaded...");
                return;
            }

            string localLogFileRawData = File.ReadAllText(localLogFilePath, System.Text.Encoding.UTF8);
            if(string.IsNullOrEmpty(localLogFileRawData))
            {
                LogMessage("localLogFileRawData in CompareLocalLogFileWithRemote is empty!");
                return;
            }
            string remoteLogFileRawData = File.ReadAllText(remoteLogFilePath, System.Text.Encoding.UTF8);
            if (string.IsNullOrEmpty(remoteLogFileRawData))
            {
                LogMessage("remoteLogFileRawData in CompareLocalLogFileWithRemote is empty!");
                return;
            }

            string[] localSessionsData = localLogFileRawData.Split('~');
            if (localSessionsData.Length == 0)
            {
                LogMessage("localSessionsData in CompareLocalLogFileWithRemote is empty!");
                return;
            }
            string[] remoteSessionsData = remoteLogFileRawData.Split('~');
            if (remoteSessionsData.Length == 0)
            {
                LogMessage("remoteSessionsData in CompareLocalLogFileWithRemote is empty!");
                return;
            }

            List<string> localSessionNumbers = new List<string>();
            List<string> localSession = new List<string>();

            char[] trimChars = "\r\n".ToCharArray();
            for (int i = 0; i < localSessionsData.Length; ++i)
            {
                localSessionsData[i] = localSessionsData[i].Trim(trimChars);

                string[] logSectors = localSessionsData[i].Split('@');
                logSectors[0] = logSectors[0].Trim(trimChars);

                string[] localLogRows = logSectors[0].Split('\n');
                localLogRows[0] = localLogRows[0].Trim(trimChars);

                localSessionNumbers.Add(localLogRows[0]);
                localSession.Add(localSessionsData[i]);
            }

            int startIndex = 0;
            for (int i = 0; i < remoteSessionsData.Length; ++i)
            {
                remoteSessionsData[i] = remoteSessionsData[i].Trim(trimChars);

                string[] playFabSectors = remoteSessionsData[i].Split('@');
                playFabSectors[0] = playFabSectors[0].Trim(trimChars);

                string[] playFabLogRows = playFabSectors[0].Split('\n');
                playFabLogRows[0] = playFabLogRows[0].Trim(trimChars);

                for (int j = 0; j < localSessionNumbers.Count; ++j)
                {
                    if(localSessionNumbers[j] == playFabLogRows[0])
                    {
                        int startLogSessionIndex =
                            remoteLogFileRawData.IndexOf(playFabLogRows[0], startIndex);
                        int endLogSessionIndex = 
                            remoteLogFileRawData.IndexOf("~", startLogSessionIndex);
                        if (endLogSessionIndex == -1)
                            endLogSessionIndex = remoteLogFileRawData.Length;
                        else
                            startIndex = endLogSessionIndex;

                        remoteLogFileRawData =
                            remoteLogFileRawData.Remove(startLogSessionIndex, endLogSessionIndex - startLogSessionIndex);
                        remoteLogFileRawData =
                            remoteLogFileRawData.Insert(startLogSessionIndex, localSession[j]);

                        localSession.RemoveAt(j);
                        localSessionNumbers.RemoveAt(j);
                    }
                }
            }

            for (int i = 0; i < localSession.Count; ++i)
            {
                remoteLogFileRawData += 
                    Environment.NewLine + "~" +
                    Environment.NewLine +
                    Environment.NewLine +
                    localSession[i];
            }

            File.WriteAllText(remoteLogFilePath, remoteLogFileRawData);
            remoteFileLogger.UploadLogFile(remoteLogFilePath);
        }
        #endregion

        #region Getters
        public string GetLogFileName()
        {
            return LOG_FILENAME;
        }
        #endregion
    }
}

public interface IRemoteFileLogger
{
    void AddDownloadRemoteLogFileSuccessEventListiner(System.Action<string> eventListiner);
    void AddUploadRemoteLogFileSuccessEventListiner(System.Action<bool> eventListiner);

    void DownloadLogFile(string fileName);
    void UploadLogFile(string filePath);

    void SetBackupMode(bool state);
    bool IsBackupModeEnabled();
    bool IsAvailable();
}
