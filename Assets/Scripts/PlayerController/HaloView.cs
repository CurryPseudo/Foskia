using System;
using System.Collections.Generic;
using UnityEngine;
using PseudoTools;
[ReceiveEvent("PlayerSwapEnter")]
[ReceiveEvent("PlayerSwapExit")]
public class HaloView : ObserverMonoBehaviour { 
    public PlayerController controller;
    public AnimationCurve curve;
    public float maxDis;
    public float maxSpeed;
    public Vector3 lastPosition;
    private Vector3 originDelta;
    public float breathY;
    public float breathCycleTime = 1;
    public float originBreathValue = 0;
    public float breathValue = 0;
    private bool delayPosition = true;
    private List<ApCtrl.AlphaData> alphaDatas = new List<ApCtrl.AlphaData>();
    private Vector3 controllerBreathPos {
        get {
            return controller.transform.position + Vector3.up * Mathf.Sin((breathValue + originBreathValue) * Mathf.PI * 2) * breathY;
        }
    }
    public void SetPositionIgnoreDelay() {
        lastPosition = controllerBreathPos;
    }
    public void Awake() {
        foreach(var renderer in GetComponentsInChildren<SpriteRenderer>()) {
            alphaDatas.Add(ApCtrl.CreateAlphaData(ApCtrl.SpriteAlpha(renderer), this));
        }
    }
    public void Start() {
        lastPosition = controller.transform.position;
        originDelta = transform.position - controller.transform.position;
    }
    public void Update() {
        if(delayPosition) {
            transform.position -= controllerBreathPos - lastPosition;
        }
        lastPosition = controllerBreathPos;
    }
    public void FixedUpdate() {
        var dir = originDelta + controllerBreathPos - transform.position;
        var dirv2 = VectorUtils.V32(dir);
        float value = Mathf.Clamp01(dirv2.magnitude / maxDis);
        float target = curve.Evaluate(value);
        var targetSpeed = target * maxSpeed;
        var targetV = dir.normalized * targetSpeed;
        transform.position += targetV * Time.fixedDeltaTime;
        var scale = transform.localScale;
        if(dir.x != 0) {
            if(scale.x * dir.x < 0) {
                scale.x *= -1;
            }
        }
        transform.localScale = scale;

        breathValue += Time.fixedDeltaTime / breathCycleTime;
        if(breathValue > 1) {
            breathValue -= 1;
        }
    }
    public void ReceivePlayerSwapEnter(PlayerController player, float disappearTime) {
        Disappear(disappearTime);
        delayPosition = false;
    }
    public void Appear(float time) {
        foreach(var alphaData in alphaDatas) {
            ApCtrl.AppearAlpha(alphaData, time);
        }
    }
    public void Disappear(float time) {
        foreach(var alphaData in alphaDatas) {
            ApCtrl.DisappearAlpha(alphaData, time);
        }
    }
    public void ReceivePlayerSwapExit(PlayerController player, float time) {
        Appear(time);
        delayPosition = true;
    }
    public void OnDrawGizmos() {
        //Gizmos.DrawCube(controllerBreathPos, new Vector3(0.5f, 0.5f, 0.5f));
    }

}