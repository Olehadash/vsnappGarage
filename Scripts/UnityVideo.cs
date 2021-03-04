using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// this is an example of using Agora Unity SDK
// It demonstrates:
// How to enable video
// How to join/leave channel
// 
public class UnityVideo : MonoBehaviour {

	// PLEASE KEEP THIS App ID IN SAFE PLACE
	// Get your own App ID at https://dashboard.agora.io/
	private static string appId = "58de5f787c6848feb866522f1998391e";
	public static bool success = false;
	
	// load agora engine
	public void loadEngine(string appId)
	{
		// start sdk
		Debug.Log ("agora_: initializeEngine");
		if (mRtcEngine != null) {
			Debug.Log ("agora_: Engine exists. Please unload it first 1 !");
			return;
		}
		
		// init engine
		mRtcEngine = IRtcEngine.getEngine (appId);
		
		// enable log
		mRtcEngine.SetLogFilter (LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);

		
	}

	

	public void join(string channel)
	{
		Debug.Log ("agora_:" + "calling join (channel = " + channel + ")");

		if (mRtcEngine == null)
			return;

		// set callbacks (optional)
		mRtcEngine.OnJoinChannelSuccess = onJoinChannelSuccess;
		mRtcEngine.OnUserJoined = onUserJoined;
		mRtcEngine.OnUserOffline = onUserOffline;
		mRtcEngine.EnableVideo();
		mRtcEngine.EnableVideoObserver();
		mRtcEngine.JoinChannel(channel, null, 0);

		/**/

		Debug.Log ("agora_: " + "initializeEngine done");
	}

	public string getSdkVersion () {
		return IRtcEngine.GetSdkVersion ();
	}

	public void leave()
	{
		GlobalParameters.IsBusy = false;
		//Debug.Log ("agora_:" + "calling leave");
		success = false;
		if (mRtcEngine == null)
			return;

		// leave channel
		mRtcEngine.LeaveChannel();
		mRtcEngine.DisableVideoObserver();

	}

	public void switchCamera()
    {
		mRtcEngine.SwitchCamera();
	}

	// unload agora engine
	public void unloadEngine()
	{
		//Debug.Log ("agora_:" + "calling unloadEngine");

		// delete
		if (mRtcEngine != null) {
			IRtcEngine.Destroy ();
			mRtcEngine = null;
		}
		
	}

	// accessing GameObject in Scnene1
	// set video transform delegate for statically created GameObject
	public void onSceneHelloVideoLoaded()
	{
		//Debug.Log("onSceneHelloVideoLoaded");
		GameObject go = DisplayCamManager.GetInstance.GetLocal;
		VideoSurface o = go.AddComponent<VideoSurface> ();
		o.SetEnable(true);
		switchCamera();
		
	}

	// instance of agora engine
	public IRtcEngine mRtcEngine;
	public IRtcEngine2 mRtcEngine2;
	// implement engine callbacks

	public uint mRemotePeer = 0; // insignificant. only record one peer

	private void onJoinChannelSuccess (string channelName, uint uid, int elapsed)
	{
		GlobalParameters.IsBusy = true;
		//Debug.Log ("agora_:" + "JoinChannelSuccessHandler: uid = " + uid);
	}

	// When a remote user joined, this delegate will be called. Typically
	// create a GameObject to render video on it
	private void onUserJoined(uint uid, int elapsed)
	{
		success = true;
		//Debug.Log ("agora_:"+"onUserJoined: uid = " + uid);
		// this is called in main thread
		// find a game object to render video stream from 'uid'
		GameObject go = DisplayCamManager.GetInstance.GetRemote;
		//Debug.Log("agora_:" + "onUserJoined: GEted GO ");
		go.SetActive(true);
		
		VideoSurface o = go.AddComponent<VideoSurface> ();
		//Debug.Log("agora_:" + "onUserJoined:Video Surface Added");
		o.SetForUser (uid);
		o.SetEnable (true);
		mRemotePeer = uid;
		//Debug.Log("agora_:" + "onUserJoined:Video Surface set video");
	}

	

	// When remote user is offline, this delegate will be called. Typically
	// delete the GameObject for this user
	private void onUserOffline(uint uid, USER_OFFLINE_REASON reason)
	{
		//Debug.Log ("agora_:" + "onUserOffline: uid = " + uid);
		TestHome.GetInstance.onLeaveButtonClicked();
		SceneManager.LoadScene(0);
	}

	private void onTransformDelegate (uint uid, string objName, ref Transform transform)
	{
		if (uid == 0) {
			transform.position = new Vector3 (0f, 2f, 0f);
			transform.localScale = new Vector3 (2.0f, 2.0f, 1.0f);
			//transform.Rotate (0f, 1f, 0f);
		}
	}

	public void EnableVideo(bool pauseVideo)
	{

		if (mRtcEngine != null)
		{
			if (!pauseVideo)
			{
				mRtcEngine.EnableVideo();
			}
			else
			{
				mRtcEngine.DisableVideo();
			}
		}
	}
}
