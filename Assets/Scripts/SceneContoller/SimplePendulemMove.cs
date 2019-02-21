using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PseudoTools;
[Serializable]
public class SimplePendulemMove {
    public float radius;
    public Vector2 center;
    public float cycle;
    public float maxAngle;
    public float originTime;
    public float time;
    public Vector2 Position {
        get {
            var t = time + originTime;
            var f = 2 * Mathf.PI / cycle;
            var a = maxAngle / 2 * Mathf.Sin(t * f); 
            a -= 90;
            a *= Mathf.Deg2Rad;
            var offset = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            offset *= radius;
            return center + offset;
        }
    }
}