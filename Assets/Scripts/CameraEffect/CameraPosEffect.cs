using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraPosEffect {
    public CameraPos cameraPos;
    public bool mainPos;
    public int priority = 0;
    private Coroutine disRegCorou;
    private Coroutine regTimeCorou;
	public float CameraHalfWidth() {
		return cameraPos.camera.orthographicSize * cameraPos.camera.aspect;
	}
	public float CameraHalfHeight() {
		return cameraPos.camera.orthographicSize;
	}
    public CameraPosEffect(){}
    public CameraPosEffect(CameraPosEffect cpe) {
        cameraPos = cpe.cameraPos;
        mainPos = cpe.mainPos;
        priority = cpe.priority;
    }
    public virtual Vector2 UpdatePos(Vector2 pos) {
        return pos;
    }
    public virtual void Register(MonoBehaviour mb) {
        if(mainPos) cameraPos = CameraPos.main;
        cameraPos.RegisterEffect(this);
        disRegCorou = cameraPos.StartCoroutine(disregisterWhileDestroy(mb));
    }
    private IEnumerator disregisterWhileDestroy(MonoBehaviour mb) {
        yield return new WaitUntil(() => mb == null);
        cameraPos.DisRegisterEffect(this);
    }
    public void Disregister() {
        if(cameraPos == null) return;
        cameraPos.DisRegisterEffect(this);
        cameraPos.StopCoroutine(disRegCorou);
        disRegCorou = null;
        if(regTimeCorou != null) {
            cameraPos.StopCoroutine(regTimeCorou);
            regTimeCorou = null;
        }
    }
    private IEnumerator disregisterWhileTimeOut(float time) {
        yield return new WaitForSeconds(time);
        Disregister();
    }
    public virtual void Register(MonoBehaviour mb, float time) {
        Register(mb);
        mb.StartCoroutine(disregisterWhileTimeOut(time));
    }

}