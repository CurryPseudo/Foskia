using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PseudoTools;
public class CircleViewRange : ViewRange {
    public float radius;
    public override float MaxRadius {
        get {
            return radius;
        }
    }
    public override bool isInRange(Vector2 pos) {
        var dir = pos - Position;
        if(dir.magnitude > radius) return false;
        if(!Physics2D.Raycast(Position, dir, dir.magnitude, solidLayer)) {
            return true;
        }
        return false;
    }
    public void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}