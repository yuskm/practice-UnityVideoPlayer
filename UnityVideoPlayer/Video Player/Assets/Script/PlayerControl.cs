﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PlayerControl : MonoBehaviour {

	private const int mStepCount = 16;		// 16 step sequencer
	private const int mDisplayCount = 9;   

	private List< List<bool> > mStepState;	// 16 step x 9 display 分の step data を保存
	private List<GameObject> mTrack;		// 9 display 分 の track
	private int mCurrentDisplay;			// 操作中のdisplay番号 
	                                        // -1 は 操作中ではない。 

	private bool mIsMouseDown;

	private GameObject mCanvas;	// いる？
	private GameObject mStepTogglePanel;

////////////////////////////////////////////////////////////////////////
	// 左クリックされた場所のオブジェクトを取得
	GameObject getLClickObject() {
		GameObject result = null;
		if ( Input.GetMouseButtonDown(0) ) {
// ymiya[
// image を hit検出
// colider
//			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//			RaycastHit hit = new RaycastHit();
//			if (Physics.Raycast(ray, out hit)){
//				result = hit.collider.gameObject;
//			}

// mouse pos
/**/		PointerEventData pointer = new PointerEventData (EventSystem.current);
/**/		pointer.position = Input.mousePosition;
/**/		List<RaycastResult> raycastResult = new List<RaycastResult> ();
/**/		EventSystem.current.RaycastAll (pointer, raycastResult);
/**/		if (raycastResult.Count > 0) {
/**/			result = raycastResult[0].gameObject;
/**/		}
// ymiya ]
		}
		return result;
	}

////////////////////////////////////////////////////////////////////////
	// 左クリックされた場所のオブジェクトを取得
	GameObject getRClickObject() {
		GameObject result = null;
		if( Input.GetMouseButtonDown(1) ) {
			// mouse pos
			PointerEventData pointer = new PointerEventData (EventSystem.current);
			pointer.position = Input.mousePosition;
			List<RaycastResult> raycastResult = new List<RaycastResult> ();
			EventSystem.current.RaycastAll (pointer, raycastResult);
			if (raycastResult.Count > 0) {
				result = raycastResult[0].gameObject;
			}
		}
		return result;
	}
////////////////////////////////////////////////////////////////////////
	// step sequencer の state を view に 渡す際に利用
	public List<bool> GetStepState(int idx) {
		return mTrack[mCurrentDisplay].GetComponent<Track> ().GetStepState ();
	}

////////////////////////////////////////////////////////////////////////
	// step sequencer の state を view が通知する際に利用する
	public void SetStepState(int idx, bool val) {
		mTrack[mCurrentDisplay].GetComponent<Track> ().SetStepState(idx,val);
	}
		
////////////////////////////////////////////////////////////////////////
	// 一定時間経過後に step button 消去
	IEnumerator StepButtonErase(float time) {
		yield return new WaitForSeconds (time);
		mCurrentDisplay = -1;			// 画面の step button を非表示

//ymiya[ 2017.10.10
//canvas そのものではなく、 panel のみで制御するように変更した。
		// step 制御用のボタンが配置された canvas を非表示に。
//		Canvas canvas = mCanvas.GetComponent<Canvas> ();
//		canvas.enabled = false;
		mStepTogglePanel.SetActive (false);
//ymiya]
	}

////////////////////////////////////////////////////////////////////////
	// clock generator が step timing を通知 
	public void OnStep(float delay) {
		for (int i = 0; i < mDisplayCount; i++) {
			mTrack [i].GetComponent<Track> ().OnStep (delay);
		}
	}

////////////////////////////////////////////////////////////////////////
	// Use this for initialization
	void Start () {
		mCurrentDisplay = -1;	// どの display も操作中ではない
		mIsMouseDown = false;

		mStepState =  new List< List<bool> >( mDisplayCount );
		for (int i = 0; i < mDisplayCount; i++) {
			List<bool> listA = new List<bool> ( mStepCount );
			for (int j = 0; j < mStepCount; j++) {
				listA.Add (false);
			}
			mStepState.Add(listA); 
		}

		GameObject trackRsc = (GameObject)Resources.Load("Prefab/Track");

		List<AudioClip> audioClip = new List<AudioClip>(mDisplayCount);
		audioClip.Add( (AudioClip) Resources.Load("HT",typeof(AudioClip) ) );

		mTrack = new List<GameObject>(mDisplayCount);
		for (int i = 0; i < mDisplayCount; i++) {
			GameObject track = Instantiate (trackRsc);

			track.GetComponent<Track> ().SetupClip (audioClip [0]);

			mTrack.Add (track);
		}
			
		mCanvas = GameObject.Find("Canvas").gameObject;
		Canvas canvas = mCanvas.GetComponent<Canvas> ();
		canvas.enabled = true;

//		mStepTogglePanel = GameObject.Find("StepButtonPanel").gameObject;
		mStepTogglePanel = mCanvas.transform.Find("StepButtonPanel").gameObject;
		mStepTogglePanel.SetActive (false);

//ymiya[ 
// 必要ないのでは？
//		ButtonControl buttonControl = mCanvas.GetComponent<ButtonControl> (); 
//		buttonControl.SetToggleActive (false);
//ymiya]	
	}

////////////////////////////////////////////////////////////////////////
	// Update is called once per frame
	void Update () {
		// 左クリックされた Game Object 取得
		GameObject lClickObj = getLClickObject();
		if ( lClickObj != null ) {
			if ( ( lClickObj.name == "Panel11RawImage" ) || 
				 ( lClickObj.name == "Panel12RawImage" )
			){
				OpenLoadFileDialog loadFileDlg = lClickObj.GetComponent<OpenLoadFileDialog> ();
				string filePath = "";
				if (loadFileDlg.Open(ref filePath)) {
					VideoPlayer videoPlayer = lClickObj.GetComponent<VideoPlayer> ();
					videoPlayer.url = "file://" + filePath;
//					videoPlayer.targetCameraAlpha = 0.5F;
					videoPlayer.Play();
				}
			} else {
			}
			Debug.Log("name="+lClickObj.name);
		}

		GameObject rClickObj = getRClickObject();
		//		GameObject clickObj = null;
		if (rClickObj != null) {
			if ( rClickObj.name == "Panel11RawImage" ) {
				mCurrentDisplay = 4;	// 当該画面の step button を表示
				Canvas canvas = mCanvas.GetComponent<Canvas> ();
				canvas.enabled = true;
				mStepTogglePanel.SetActive (true);
				StartCoroutine(StepButtonErase(6.0F)); // 3 sec 後に step button 削除
//ymiya[
// とりあえず、でっち上げ
// button を active にするタイミングで、current track の state に更新する。
// current track を切り替えるたびに、このメンバー関数をコールしなければならない。
				ButtonControl buttonControl = mCanvas.GetComponent<ButtonControl> (); 
				buttonControl.SetToggleOn (GetStepState(mCurrentDisplay));
//ymiya]
			}
		}
	}

////////////////////////////////////////////////////////////////////////
	public void SetupTrack(int index) {
		mCurrentDisplay = 4;	// 当該画面の step button を表示
//ymiya[
// とりあえず、でっち上げ
// button を active にするタイミングで、current track の state に更新する。
// current track を切り替えるたびに、このメンバー関数をコールしなければならない。
		ButtonControl buttonControl = mCanvas.GetComponent<ButtonControl> (); 
		buttonControl.SetToggleOn (GetStepState(index));
//ymiya]	
	}
}