using System;
using System.Collections;
using System.Collections.Generic;
using PseudoTools;
using UnityEngine;
public class CameraSmoothFollow : CameraPosEffect {
    public Func<Vector2> posGetter;
    public AnimationCurve distanceSpeedCurve;
    private Vector2 lastPos;
    private bool firstUpdate = true;
    public override Vector2 UpdatePos(Vector2 pos) {
        //if(firstUpdate) {
        //    lastPos = pos;
        //    firstUpdate = false;
        //}
        //var target = posGetter();
        //var dir = target - lastPos;
        //float speed = distanceSpeedCurve.Evaluate(dir.magnitude);
        //lastPos += dir.normalized * speed * Time.deltaTime;
        var target = posGetter();
        var dir = target - pos;
        float speed = distanceSpeedCurve.Evaluate(dir.magnitude);
        pos += dir.normalized * speed * Time.unscaledDeltaTime;
        return pos;
    }
}