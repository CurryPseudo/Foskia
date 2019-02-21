using UnityEngine;
using System;
using PseudoTools;
[Serializable]
public class RotationMove {
    public float time;
    public float rotDegSpeed;
    public float originRotDeg;
    public Quaternion Rotation {
        get {
            float deg = originRotDeg + rotDegSpeed * time;
            return Quaternion.EulerRotation(0, 0, deg);
        }
    }
}