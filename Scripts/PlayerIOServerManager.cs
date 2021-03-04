using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIOClient;
using System.Security.Cryptography;
using System;
using System.Text;

public class PlayerIOServerManager : MonoBehaviour
{
    #region Singleton
    private static PlayerIOServerManager instance;
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
                Debug.LogError("PlayerIOServerManager instance not found!");
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

    #region Private Properties
    private bool isNullConnection
    {
        get
        {
            if (currentServerConnection == null)
            {
                Debug.Log("Client is not connected to Server!");
                return true;
            }
            return false;
        }
    }
    #endregion

    #region Private Fields
    private Client currentClient;
    private Connection currentServerConnection;

    private System.Action ConnectionSuccessEvent;
    private System.Action UserJoinedEvent;
    private System.Action LostConnectionEvent;
    private System.Action ConnectionFailedEvent;

    private Coroutine connectionWaitingCoroutine;

    private const string gameId = "vsnaap-ibl9rv4skw1zsa1eofma";

    private const string FirstConnectionMessageType = "FirstConnection";

    private const string GetCardsNamesMessageType = "GetCardsNames";
    private const string GetCardsDataMessageType = "GetCardsData";
    private const string GetCardsStatusMessageType = "GetCardsStatus";

    private const string GetMissedCallsDataMessageType = "GetMissedCallsData";
    private const string SendReadyForCallMessageType = "ReadyForCall";

    private const string GetDocumentsDataMessageType = "GetDocumentsData";
    private const string DeleteGarageDocumentMessageType = "DeleteGarageDocument";

    private const string AdditionalToCardMessageType = "AdditionalToCard";
    private const string CheckCardDataMessageType = "CheckCardData";
    private const string CloseCardMessageType = "CloseCard";

    private const string CreateCardMessageType = "CreateCard";

    private const string SendSetPlayFabIDMessageType = "SetPlayFabID";
    private const string SendGetAppraiserPlayFabIDMessageType = "GetAppraiserPlayFabID";

    private const string GetAppraiserNamesMessageType = "GetAppraiserNames";
    private const string AddNewEmailMessageType = "AddNewEmail";
    private const string RemoveFromEmailsMessageType = "RemoveFromEmails";

    private const string CheckingConnectionMessageType = "CheckingConnection";

    private const string DebugMessageType = "Debug";

    private string lastGaragePassword;
    #endregion

    #region Add Event Listiners
    public static void AddConnectionSuccessEventListiner(System.Action eventListiner)
    {
        if (isNullInstance)
            return;

        instance.ConnectionSuccessEvent += eventListiner;
    }

    public static void AddUserJoinedEventListiner(System.Action eventListiner)
    {
        if (isNullInstance)
            return;

        instance.UserJoinedEvent += eventListiner;
    }

    public static void AddLostConnectionEventListiner(System.Action eventListiner)
    {
        if (isNullInstance)
            return;

        instance.LostConnectionEvent += eventListiner;
    }

    public static void AddConnectionFailedEventListiner(System.Action eventListiner)
    {
        if (isNullInstance)
            return;

        instance.ConnectionFailedEvent += eventListiner;
    }
    #endregion

    #region Connection
    private IEnumerator WaitForConnection(string garageLogin, string garagePassword)
    {
        yield return null;

        while (true)
        {
            ConnectToServer(garageLogin, garagePassword);

            yield return new WaitForSeconds(5f);

            Debug.Log("Can't connect to PlayerIOServer... Retrying...");
        }
    }

    public static void ConnectToServer(string garageLogin, string garagePassword)
    {
        if (isNullInstance)
            return;

        if (instance.connectionWaitingCoroutine == null)
        {
            Debug.Log(string.Format("ConnectToServer with login: {0}, password: {1}",
    garageLogin, garagePassword));

            instance.connectionWaitingCoroutine = 
                instance.StartCoroutine(instance.WaitForConnection(garageLogin, garagePassword));
            return;
        }
        else
        {
            Debug.Log(string.Format("Continue ConnectToServer with login: {0}, password: {1}",
    garageLogin, garagePassword));
        }

        if (instance.currentServerConnection != null)
        {
            Debug.LogError("Client is already connected to Server!");
            return;
        }
        Dictionary<string, string> authArgs = new Dictionary<string, string>();
        authArgs.Add("userId", garageLogin);
        instance.lastGaragePassword = garagePassword;

        PlayerIO.Authenticate(gameId, "public", authArgs, null,
            instance.ConnectionToGameSuccess, 
            instance.ConnectionFailed);
    }
    #endregion

    #region Connection To Game Handler
    private void ConnectionToGameSuccess(Client client)
    {
        if (instance.currentClient != null)
            return;

        instance.currentClient = client;

        Dictionary<string, string> joinData = new Dictionary<string, string>();
        joinData.Add("password", lastGaragePassword);
        joinData.Add("accountType", "garage");

        client.Multiplayer.CreateJoinRoom(client.ConnectUserId, "RoomGarage", false, null, joinData,
            ConnectionToServerRoomSuccess,
            ConnectionFailed);
    }

    private void ConnectionFailed(PlayerIOError error)
    {
        Debug.LogError("Server ConnectionFailed:\n" + error.Message);
    }
    #endregion

    #region Connection To Room Handler
    private void ConnectionToServerRoomSuccess(Connection connection)
    {
        Debug.Log("Connection to Room Success");
        currentServerConnection = connection;
        currentServerConnection.OnMessage += ServerMessageHandler;
        currentServerConnection.OnDisconnect += ServerDisconnectHandler;

        ConnectionSuccessEvent?.Invoke();

        if (connectionWaitingCoroutine != null)
        {
            StopCoroutine(connectionWaitingCoroutine);
            connectionWaitingCoroutine = null;
        }
    }
    #endregion

    #region Server Messages Part

    public string displayName = "";
    public static string DisplayName
    {
        get
        {
            if (isNullInstance) return "";
            return instance.displayName;
        }
    }
    private void ServerMessageHandler(object sender, Message e)
    {
        switch(e.Type)
        {
            #region First Data
            case FirstConnectionMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string rawMissedCallsData = e.GetString(1);
                        //HomeMissedCallListManager.SetupMissedCallsList(rawMissedCallsData);

                        string rawCardDatas = e.GetString(2);
                        //HomeCardListManager.SetupCardsList(rawCardDatas);

                        string rawDocumentsData = e.GetString(3);
                        //HomeDocumentsListManager.SetupDocumentsList(rawDocumentsData);

                        string rawEmailsData = e.GetString(4);
                       /** if (!string.IsNullOrEmpty(rawEmailsData))
                            EmailsListScreen.CreateEmailsList(rawEmailsData);*/

                        displayName = e.GetString(5);
                        //GameManager.SetUserDisplayName(displayName);


                        //if (checkConnectionCoroutine != null)
                        //{
                        //    Debug.Log("FirstConnectionMessageType warning: checkConnectionCoroutine is not null!");
                        //    StopCoroutine(checkConnectionCoroutine);
                        //    checkConnectionCoroutine = null;
                        //}
                        //checkConnectionCoroutine = StartCoroutine(CheckConnection());
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.Log("FirstConnection warning:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.Log("FirstConnection unknown warning...");
                        }
                        ConnectionFailedEvent?.Invoke();
                    }
                }
                break;
            #endregion

            #region Update Cards
            case GetCardsNamesMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        //HomeCardListManager.CheckUpdateCardsList(e.GetString(1).Split('_'));
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(GetCardsNamesMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(GetCardsNamesMessageType + " unknown error...");
                        }
                    }
                }
                break;

            case GetCardsDataMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string rawCardDatas = e.GetString(1);
                        //HomeCardListManager.UpdateCardsList(rawCardDatas.Split('_'));
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(GetCardsDataMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(GetCardsDataMessageType + " unknown error...");
                        }


                        if (e.Count < 2)
                        {
                            Debug.Log("GetBlocksData can't add 'problemCardNumber', cuz 'problemCardNumber' is null!");
                            return;
                        }

                        string problemCardNumber = e.GetString(2);
                        if (string.IsNullOrEmpty(problemCardNumber))
                        {
                            Debug.LogError("GetBlocksData 'problemBlockName' is null!");
                            return;
                        }
                        //HomeCardListManager.AddProblemCard(problemCardNumber);
                    }
                }
                break;
            #endregion

            #region Update Calls/Photos
            case GetMissedCallsDataMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        //HomeMissedCallListManager.CheckUpdateMissedCallsList(e.GetString(1).Split('_'));
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(GetMissedCallsDataMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(GetMissedCallsDataMessageType + " unknown error...");
                        }
                    }
                }
                break;

            case GetDocumentsDataMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        //HomeDocumentsListManager.CheckUpdateDocumentsList(e.GetString(1).Split('_'));
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(GetDocumentsDataMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(GetDocumentsDataMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Get Cards Status
            case GetCardsStatusMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string[] cardStatusData = e.GetString(1).Split('_');
                        //HomeCardListManager.UpdateStatusResult(cardStatusData);
                        //CardStatusScreen.UpdateStatusResult(cardStatusData);
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(GetCardsNamesMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(GetCardsNamesMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Create Card
            case CreateCardMessageType:
                {
                    bool result = e.GetBoolean(0);
                    //CardCreationScreen.CreationResult(result);
                    //CardCheckingScreen.CreationCardResult(result);
                    if (!result)
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError("Card creation error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError("Card creation unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Check Connection
            case CheckingConnectionMessageType:
                {
                    currentServerConnection.Send(CheckingConnectionMessageType, true);
                    SendGetMissedCallsDataMessage();
                    SendGetDocumentsDataMessage();

                    SendGetCardNamesMessage();

                    SendGetAppraiserNamesMessage();

                    //HomeCardListManager.GetUpdateStatus();

                    //connectionChecked = true;
                }
                break;
            #endregion

            #region Ready For Call
            case SendReadyForCallMessageType:
                {
                    bool result = e.GetBoolean(0);
                    string messageData = e.GetString(1);
                    if (!result)
                    {
                        if (!string.IsNullOrEmpty(messageData))
                        {
                            Debug.LogError(SendReadyForCallMessageType + " error:\n" +
                                messageData);
                        }
                        else
                        {
                            Debug.LogError(SendReadyForCallMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Get Appraiser PlayFabID

            case SendGetAppraiserPlayFabIDMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string appraiserPlayFabID = e.GetString(1);
                        //PlayFabManager.SendCallNotificationMessage(appraiserPlayFabID);
                    }
                    else
                    {
                        string messageData = e.GetString(1);
                        if (!string.IsNullOrEmpty(messageData))
                        {
                            Debug.LogError(SendReadyForCallMessageType + " error:\n" +
                                messageData);
                        }
                        else
                        {
                            Debug.LogError(SendReadyForCallMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Delete Garage Document
            case DeleteGarageDocumentMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string documentUrl = e.GetString(1);
                        //HomeDocumentsListManager.RemoveDocumentElementFromList(documentUrl);
                    }
                    else
                    {
                        //GarageDocumentsScreen.UnlockBusy();
                        string messageData = e.GetString(1);
                        if (!string.IsNullOrEmpty(messageData))
                        {
                            Debug.LogError(DeleteGarageDocumentMessageType + " error:\n" +
                                messageData);
                        }
                        else
                        {
                            Debug.LogError(DeleteGarageDocumentMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Check/Additional/Close Card 
            case CheckCardDataMessageType:
                {
                    bool result = e.GetBoolean(0);
                    //CardCheckingScreen.UnlockBusy();
                    if (result)
                    {
                        //CardCheckingScreen.CheckCardDataResult();
                    }
                    else
                    {
                        string messageData = e.GetString(1);
                        if (!string.IsNullOrEmpty(messageData))
                        {
                            Debug.LogError(CheckCardDataMessageType + " error:\n" +
                                messageData);
                        }
                        else
                        {
                            Debug.LogError(CheckCardDataMessageType + " unknown error...");
                        }
                    }
                }
                break;

            case AdditionalToCardMessageType:
                {
                    bool result = e.GetBoolean(0);
                    //CardAdditionalScreen.SendPhotoUrlsResult(result);
                    if (!result)
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(AdditionalToCardMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(AdditionalToCardMessageType + " unknown error...");
                        }
                    }
                }
                break;

            case CloseCardMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string cardNumber = e.GetString(1);
                        //HomeCardListManager.ChangeCardStatus(cardNumber);
                        //CardStatusScreen.CloseCardStatusResult(cardNumber, true);
                    }
                    else
                    {
                        //GarageDocumentsScreen.UnlockBusy();
                        string messageData = e.GetString(1);
                        if (!string.IsNullOrEmpty(messageData))
                        {
                            Debug.LogError(CloseCardMessageType + " error:\n" +
                                messageData);
                        }
                        else
                        {
                            Debug.LogError(CloseCardMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Update Appraiser List
            case GetAppraiserNamesMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string rawAppraiserNames = e.GetString(1);
                        //AppraiserListScreen.CheckUpdateAppraisersList(rawAppraiserNames);
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(GetAppraiserNamesMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(GetAppraiserNamesMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Add/Remove Email
            #region Add New Email
            case AddNewEmailMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (result)
                    {
                        string rawEmailData = e.GetString(1);
                        //EmailsListScreen.AddNewEmailSuccess(rawEmailData);
                    }
                    else
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(AddNewEmailMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(AddNewEmailMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion

            #region Remove Email
            case RemoveFromEmailsMessageType:
                {
                    bool result = e.GetBoolean(0);
                    if (!result)
                    {
                        string errorMessage = e.GetString(1);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.LogError(RemoveFromEmailsMessageType + " error:\n" +
                                errorMessage);
                        }
                        else
                        {
                            Debug.LogError(RemoveFromEmailsMessageType + " unknown error...");
                        }
                    }
                }
                break;
            #endregion
            #endregion

            #region Debug
            case DebugMessageType:
                {
                    string messageData = e.GetString(0);
                    if (!string.IsNullOrEmpty(messageData))
                    {
                        Debug.Log(DebugMessageType + " warning:\n" +
                            messageData);
                    }
                    else
                    {
                        Debug.Log(DebugMessageType + " unknown warning...");
                    }
                }
                break;
                #endregion
        }
    }

    #region Send Messages
    #region SetPlayFabID
    public static void SendSetPlayFabID(string playFabID)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(SendSetPlayFabIDMessageType, playFabID);
    }
    #endregion

    #region Add/Remove Email
    #region Add Email To List
    public static void SendAddNewEmailMessage(string rawEmailData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(AddNewEmailMessageType, rawEmailData);
    }
    #endregion

    #region Remove Email From List
    public static void SendRemoveFromEmailMessage(string rawEmailData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(RemoveFromEmailsMessageType, rawEmailData);
    }
    #endregion
    #endregion

    #region Update Appraiser List
    private static void SendGetAppraiserNamesMessage()
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(GetAppraiserNamesMessageType, true);
    }
    #endregion

    #region Get Cards Status
    public static void SendGetCardsStatusMessage(string rawCardNames)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(GetCardsStatusMessageType, rawCardNames);
    }
    #endregion

    #region Additional/Close/Check Card
    public static void SendCloseCard(string rawClosedCardData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(CloseCardMessageType, rawClosedCardData);
    }
    public static void SendAdditionalToCard(string rawAdditionalData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(AdditionalToCardMessageType, rawAdditionalData);
    }

    public static void SendCheckCardDataMessage(string cardData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(CheckCardDataMessageType, cardData);
    }
    #endregion

    #region Delete Garage Document
    public static void SendDeleteGarageDocumentMessage(string rawDocumentData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(DeleteGarageDocumentMessageType, rawDocumentData);
    }
    #endregion

    #region Ready For Call
    public static void SendReadyForCallMessage(string missedCallData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(SendReadyForCallMessageType, missedCallData);
    }
    #endregion

    #region Get Update Cards Status
    private static void SendGetCardNamesMessage()
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(GetCardsNamesMessageType, true);
    }
    #endregion

    #region Get Data
    public static void SendGetCardsDataMessage(string rawDataNames)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(GetCardsDataMessageType, rawDataNames);
    }
    public static void SendGetMissedCallsDataMessage()
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(GetMissedCallsDataMessageType, true);
    }

    public static void SendGetDocumentsDataMessage()
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(GetDocumentsDataMessageType, true);
    }
    #endregion

    #region Create Card Part
    public static void SendCreateCardMessage(string rawCardData)
    {
        if (isNullInstance || instance.isNullConnection)
            return;

        instance.currentServerConnection.Send(CreateCardMessageType, rawCardData);
    }
    #endregion

    #endregion

    #endregion

    #region Check Connection
    //private Coroutine checkConnectionCoroutine;
    //private bool connectionChecked;
    //private IEnumerator CheckConnection()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSecondsRealtime(10f);
    //        if (!connectionChecked)
    //            ManualDisconnect();
    //        connectionChecked = false;
    //    }
    //}
    #endregion

    #region Disconnect Part
    public static void StopConnectionToServer()
    {
        if (isNullInstance)
            return;

        Debug.Log("StopConnectionToServer");
        if (instance.connectionWaitingCoroutine != null)
        {
            Debug.Log("Stop connectionWaitingCoroutine...");
            instance.StopCoroutine(instance.connectionWaitingCoroutine);
            instance.connectionWaitingCoroutine = null;
        }
        else
        {
            instance.ManualDisconnect();
        }
    }
    private void ManualDisconnect()
    {
        if (isNullInstance)
            return;

        if (currentServerConnection != null)
        {
            Debug.Log("Server ManualDisconnect");

            instance.currentServerConnection.Disconnect();
            currentServerConnection = null;
            currentClient = null;
        }
    }
    private void ServerDisconnectHandler(object sender, string message)
    {
        Debug.Log(string.Format("ServerDisconnectHandler." +
            "Client was disconnected by {0} with reason:\n{1}", sender.ToString(), message));

        if (connectionWaitingCoroutine != null)
        {
            Debug.Log("Stop connectionWaitingCoroutine");

            StopCoroutine(connectionWaitingCoroutine);
            connectionWaitingCoroutine = null;
        }

        if (currentServerConnection != null)
        {
            currentClient = null;
            currentServerConnection = null;
            LostConnectionEvent?.Invoke();
        }

        //if (checkConnectionCoroutine != null)
        //{
        //    Debug.Log("Stop checkConnectionCoroutine");

        //    StopCoroutine(checkConnectionCoroutine);
        //    checkConnectionCoroutine = null;
        //}
    }

    private void OnApplicationQuit()
    {
        if (currentServerConnection != null)
        {
            currentServerConnection.Disconnect();
            currentServerConnection = null;
            currentClient = null;
        }
    }
    #endregion
}
