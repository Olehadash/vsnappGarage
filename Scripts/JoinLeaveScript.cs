using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
#if (UNITY_2018_3_OR_NEWER)
using UnityEngine.Android;
#endif

public class JoinLeaveScript : MonoBehaviour
{

	#region Singleton
	private static JoinLeaveScript instance;
	private void Awake()
	{
		if (instance == null)
			instance = this;
	}

	public static JoinLeaveScript GetInstance
	{
		get
		{
			return instance;
		}
	}

	#endregion

	// Use this for initialization
	private ArrayList permissionList = new ArrayList();

	public GameObject msg;
	public GameObject waytforConnect;
	public GameObject recall;

	void Start()
	{
#if (UNITY_2018_3_OR_NEWER)
		permissionList.Add(Permission.Microphone);
		permissionList.Add(Permission.Camera);
#endif
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

	private void CheckPermission()
	{
#if (UNITY_2018_3_OR_NEWER)
		foreach (string permission in permissionList)
		{
			if (Permission.HasUserAuthorizedPermission(permission))
			{

			}
			else
			{
				Permission.RequestUserPermission(permission);
			}
		}
#endif
	}

	// Update is called once per frame
	void Update()
	{
#if (UNITY_2018_3_OR_NEWER)
		CheckPermission();
#endif
	}

	static TestHelloUnityVideo app = null;

	private void onJoinButtonClicked()
	{
		Debug.Log("agora_: onJoinButtonClicked");
		if (ReferenceEquals(app, null))
		{
			app = new TestHelloUnityVideo();
			app.loadEngine("58de5f787c6848feb866522f1998391e"); 
			Debug.Log("agora_: ApkLoaded");
		}
		app.join("test");
	}

	private void onLeaveButtonClicked()
	{
		GlobalParameters.IsBusy = true;
		if (!ReferenceEquals(app, null))
		{
			app.leave(); // leave channel
			app.unloadEngine(); // delete engine
			app = null; // delete app
		}
	}

	public void SwitchCamera()
	{
		//app.switchCamera();
	}

	public void onButtonClickedJoin()
	{
		// which GameObject?
		//if (name.CompareTo ("JoinButton") == 0) {
		//	onJoinButtonClicked ();
		//}
		//else if(name.CompareTo ("LeaveButton") == 0) {
		//	onLeaveButtonClicked ();
		//}

		onJoinButtonClicked();
	}

	public void onButtonClickedLeave()
	{
		// which GameObject?
		//if (name.CompareTo ("JoinButton") == 0) {
		//	onJoinButtonClicked ();
		//} else if (name.CompareTo ("LeaveButton") == 0) {
		//	onLeaveButtonClicked ();
		//}

		onLeaveButtonClicked();
	}



}
