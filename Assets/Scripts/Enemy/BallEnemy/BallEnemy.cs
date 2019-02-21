using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PseudoTools;
public class BallEnemy : MonoBehaviour {
    public SmoothMoveToTarget smtt;
    public BlockMovement mov;
    public void Awake() {
        var originPos = mov.Position;
        smtt = new SmoothMoveToTarget(() => originPos, smtt.distanceSpeedCurve, () => mov.Position, (v) => mov.Velocity = v);
        StartCoroutine(smtt.UpdateCor());
    }
}