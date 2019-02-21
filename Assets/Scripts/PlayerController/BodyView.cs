using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using PseudoTools;

[ReceiveEvent("PlayerSwapEnter")]
[ReceiveEvent("PlayerSwapExit")]
public class BodyView : ObserverMonoBehaviour {
	public PlayerController controller;
	public SkeletonAnimation animation;
	public BoxCollision predictGroundBox;
	public BoxCollision predictGroundTooCloseBox;
	[Header("Rush")]
	public GameObject rushLightPrefab;
	public float originRushLightAlpha = 0.7f;
	public int rushTimes;
	public float disappearTime;
	public float targetScaleY = 0.6f;
	[HideInInspector]
	[NonSerialized]
	public float originScaleY;
	public float ScaleY {
		get {
			return transform.localScale.y / originScaleY;
		}
		set {
			var scale = transform.localScale;
			scale.y = value * originScaleY;
			transform.localScale = scale;
		}
	}
	private ApCtrl.AlphaData alphaData;
	public MeshRenderer mr;
	void Awake() {
	}
	void Start () {
		alphaData = ApCtrl.CreateAlphaData(ApCtrl.SpineMeshAlpha(mr), this);
		alphaData.TargehAlphaNormed = 0;
		originScaleY = transform.localScale.y;
		controller.jumpEvent += () => {
			SetAnimate("jump up start", false);
			//animation.state.SetAnimation(0, "jump up start", false);
			Timer.BeginATimer(0.333f, ()=>{
				if(animation.AnimationName == "jump up start") {
					//animation.AnimationName = "jump up";
					SetAnimate("jump up", true);
				}
			},this);
			//animation.state.AddAnimation(0, "jump up", true, 0.333f);
		};
		controller.AddEnterEventBeforeEnter<PlayerController.RushState>(()=> {
			animation.timeScale = 0.3f / controller.rushTime;
			SetAnimate("rush", false);
			StartCoroutine(RushLightCoroutine());
			StartCoroutine(RushTranslateCoroutine());
		});
		controller.AddEnterEventBeforeExit<PlayerController.RushState>(()=> {
			animation.timeScale = 1;
		});
		controller.AddEnterEventBeforeEnter<PlayerController.HurtState>(()=> {
			animation.timeScale = 0.667f / controller.hurtStateTime;
			SetAnimate("hurt", false);
		});
		controller.AddEnterEventBeforeExit<PlayerController.HurtState>(()=> {
			animation.timeScale = 1;
		});

	}
	
	IEnumerator RushTranslateCoroutine() {
		float timeCount = 0;
		while(timeCount < controller.rushTime) {
			float timeValue = timeCount / controller.rushTime;
			float velocityValue = controller.rushSpeedTimeCurve.Evaluate(timeValue);
			float scaleY = Mathf.Lerp(1, this.targetScaleY, velocityValue);
			ScaleY = scaleY;
			yield return new WaitForFixedUpdate();
			timeCount += Time.fixedDeltaTime;
		}
		ScaleY = 1;
		yield break;
	}
	IEnumerator RushLightDisappearCoroutine(GameObject go) {
		float timeCount = 0;
		while(timeCount < disappearTime) {
			float alpha = Mathf.InverseLerp(disappearTime, 0, timeCount);
			go.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, Mathf.Lerp(0, originRushLightAlpha, alpha));
			yield return new WaitForFixedUpdate();
			timeCount += Time.fixedDeltaTime;
		}
		Destroy(go);
		yield break;
	}
	IEnumerator RushLightCoroutine() {
		float stepTime = controller.rushTime / (float)(rushTimes + 1);
		for(int i = 0; i < rushTimes; i++) {
			yield return new WaitForSeconds(stepTime);
			var go = Instantiate(rushLightPrefab);
			go.transform.position = VectorUtils.V32(controller.PositionWithoutHalo);
			var scale = go.transform.localScale;
			if(!controller.faceRight) {
				scale = VectorUtils.Multiply(scale, new Vector3(-1, 1, 1));
			}
			scale.y *= ScaleY;
			go.transform.localScale = scale;
			StartCoroutine(RushLightDisappearCoroutine(go));
		}
		yield break;
	}
	// Update is called once per frame
	void Update () {
		if(controller.currentStateName != "Hurt") {
			animation.skeleton.FlipX = !controller.faceRight; 
		}
		else {
			animation.skeleton.FlipX = controller.faceRight;
		}
		if(controller.currentStateName == "Idle" && Mathf.Abs(controller.Velocity.y) < 0.1f){
			if(Mathf.Abs(controller.Velocity.x) <= 0.1f) {
				SetAnimate("stand", true);
			}
			else {
				SetAnimate("run", true);
			}
		}
		if(controller.currentStateName == "Jump") {
			if(controller.Velocity.y < 2 && animation.AnimationName == "jump up" && animation.AnimationName.IndexOf("over") == -1) {
				//animation.loop = false;
				//animation.AnimationName = "jump transit";
				SetAnimate("jump transit", false);
			}
			else if(controller.Velocity.y < 0) {
				SetAnimate("jump down", true);
			}
		}
	}
	public void SetAnimate(string name, bool loop) {
		if(animation.AnimationName != name) {
			SpineController.SetAnimate(animation, name, loop);
		}
	}
	public void ReceivePlayerSwapEnter(PlayerController fsm, float time) {
		ApCtrl.DisappearAlpha(alphaData, time);
	}
	public void ReceivePlayerSwapExit(PlayerController fsm, float time) {
		ApCtrl.AppearAlpha(alphaData, time);
	}
}
