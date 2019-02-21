using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PseudoTools;
public abstract class ViewRange : MonoBehaviour {
    public LayerMask inRangeLayer;
    public LayerMask solidLayer;
    protected GameObject[] inRangeGos = new GameObject[20];
    protected int inRangeGosCount = 0;
    public IEnumerable<GameObject> InRangeGos {
        get {
            for(int i = 0; i < inRangeGosCount; i++) {
                yield return inRangeGos[i];
            }
        }
    }
    public abstract float MaxRadius{
        get;
    }
    public int InRangeGosCount {
        get {
            return inRangeGosCount;
        }
    }
    public Vector2 Position {
        get {
            return VectorUtils.V32(transform.position);
        }

    }
    public void Update() {
        inRangeGosCount = 0;
        var colliders = Physics2D.OverlapCircleAll(Position, MaxRadius, inRangeLayer);
        foreach(var collider in colliders) {
            var pos = VectorUtils.V32(collider.bounds.center);
            var inRange = collider.gameObject.GetComponent<InRangeRecorder>();
            if(isInRange(pos)) {
                inRangeGos[inRangeGosCount] = collider.gameObject;
                inRangeGosCount++;
                if(inRange != null && isInRange(pos)) {
                    inRange.InRange(gameObject);
                    StartCoroutine(KeepCheckOutOfRange(inRange));
                }
            }
        }
    }

    public abstract bool isInRange(Vector2 pos);
    public IEnumerator KeepCheckOutOfRange(InRangeRecorder inRangeRecorder) {
        while(true) {
            yield return null;
            if(!isInRange(inRangeRecorder.transform.position)) {
                inRangeRecorder.OutRange(gameObject);
                yield break;
            }
        }
    }
}