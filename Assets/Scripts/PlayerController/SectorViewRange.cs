using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PseudoTools;
public class SectorViewRange : ViewRange {
    public float radius;
    public float angle;
    public override float MaxRadius {
        get {
            return radius;
        }
    }
    public override bool isInRange(Vector2 pos) {
        var dir = pos - Position;
        if(dir.magnitude > radius) return false;
        bool canSee =!Physics2D.Raycast(Position, dir, dir.magnitude, solidLayer);
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        float[] angleRange = new float[2];
        int i = 0;
        foreach(var a in GetAngleRange()) {
            angleRange[i] = a;
            i++;
        }
        bool inAngleRange = angle >= angleRange[0] && angle <= angleRange[1];
        return canSee && inAngleRange;
    }
    public IEnumerable<float> GetAngleRange() {
        float rotate = transform.rotation.eulerAngles.z;
        yield return -rotate - angle / 2;
        yield return -rotate + angle / 2;
    }
    public IEnumerable<Vector2> GetAnglePointsRange() {
        Func<float, Vector2> angleToPoint = (a) => {
            float rad = a * Mathf.Deg2Rad;
            return Position + new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * radius;
        };
        return Algorithm.Map<float, Vector2>(GetAngleRange(), angleToPoint);
    }
    public void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
        foreach(var point in GetAnglePointsRange()) {
            Gizmos.DrawLine(Position, point);
        }
        
    }
}