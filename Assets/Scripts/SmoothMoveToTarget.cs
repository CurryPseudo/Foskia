using System;
using System.Collections;
using UnityEngine;
using PseudoTools;
[Serializable]
public class SmoothMoveToTarget {
    public Func<Vector2> targetPosGet;
    public AnimationCurve distanceSpeedCurve;
    public Func<Vector2> getPosition;
    public Action<Vector2> setVelocity;

    public SmoothMoveToTarget(Func<Vector2> targetPosGet, AnimationCurve distanceSpeedCurve, Func<Vector2> getPosition, Action<Vector2> setVelocity)
    {
        this.targetPosGet = targetPosGet;
        this.distanceSpeedCurve = distanceSpeedCurve;
        this.getPosition = getPosition;
        this.setVelocity = setVelocity;
    }

    public IEnumerator UpdateCor() {
        yield return null;
        while(true) {
            Update();
            yield return null;
        }
    }
    public void Update() {
        var v2 = targetPosGet();
        var dir = v2 - getPosition();
        float speed = distanceSpeedCurve.Evaluate(dir.magnitude);
        setVelocity(dir.normalized * speed);
    }
}