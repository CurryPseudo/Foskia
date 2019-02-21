using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using PseudoTools;

public class SinMove {

    public SinMoveData data;

    public SinMove(SinMoveData data)
    {
        this.data = data;
    }

    public Vector2 UpdatePos(Vector2 pos) {
        var offset = data.range;
        var c = VectorUtils.Do(data.originDeg, f => Mathf.Sin(f * Mathf.Deg2Rad));
        c = VectorUtils.Do(c, f => f * 2 - 1);
        offset = VectorUtils.Multiply(offset, c);
        pos += offset;
        data.originDeg += data.speedDeg * Time.deltaTime;
        return pos;
    }
}
[Serializable]
public struct SinMoveData {
    public Vector2 range;
    public Vector2 originDeg;
    public Vector2 speedDeg;
}