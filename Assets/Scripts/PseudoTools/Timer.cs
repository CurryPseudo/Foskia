using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Timer{
    private float value;
    private Coroutine coroutine;
    public float Value {
        get {
            return value;
        }
    }
    public Coroutine Coroutine {
        get {
            return coroutine;
        }
    }
    public event Action timerUpdateAction;
    private Timer() {
        value = 0;
    }
    private static IEnumerator TimerCoroutine(Timer timer, float time, System.Action afterTimerAction, Func<float> deltaTimeGetter, Func<IEnumerator> next) {
        float timeCount = 0;
        while(timeCount < time) {
            if(timer != null) {
                timer.value = timeCount / time;
                if(timer.timerUpdateAction != null) {
                    timer.timerUpdateAction();
                }
            }
            yield return next();
            timeCount += deltaTimeGetter();
        }
        if(timer != null) {
            timer.value = 1;
            if(timer.timerUpdateAction != null) {
                timer.timerUpdateAction();
            }
        }
        if(afterTimerAction != null) {
            afterTimerAction();
        }
        yield break;
    }
    public static Timer CreateATimer(float time, System.Action afterTimerAction, MonoBehaviour mb) {
        Timer timer = new Timer();
        timer.coroutine = mb.StartCoroutine(TimerCoroutine(timer, time, afterTimerAction, ()=>Time.deltaTime, ()=>null));
        return timer;
    }
    public static Timer CreateAFixedTimer(float time, System.Action afterTimerAction, MonoBehaviour mb) {
        Timer timer = new Timer();
        timer.coroutine = mb.StartCoroutine(TimerCoroutine(timer, time, afterTimerAction, ()=>Time.fixedDeltaTime, returnFixedUpdate));
        return timer;
    }
    public static Timer CreateAUnscaledFixedTimer(float time, System.Action afterTimerAction, MonoBehaviour mb) {
        Timer timer = new Timer();
        timer.coroutine = mb.StartCoroutine(TimerCoroutine(timer, time, afterTimerAction, ()=>Time.fixedUnscaledDeltaTime, returnFixedUpdate));
        return timer;
    }
    public static Timer CreateAUnscaledTimer(float time, System.Action afterTimerAction, MonoBehaviour mb) {
        Timer timer = new Timer();
        timer.coroutine = mb.StartCoroutine(TimerCoroutine(timer, time, afterTimerAction, ()=>Time.unscaledDeltaTime, ()=>null));
        return timer;
    }
    private static IEnumerator returnFixedUpdate() {
        yield return new WaitForFixedUpdate();
        yield break;
    }
    public static Coroutine BeginATimer(float time, System.Action afterTimerAction, MonoBehaviour mb) {
        return mb.StartCoroutine(TimerCoroutine(null, time, afterTimerAction, ()=>Time.deltaTime, ()=>null));
    }
}