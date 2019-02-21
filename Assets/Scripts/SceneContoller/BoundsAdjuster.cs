using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using PseudoTools;
public class BoundsAdjuster : MonoBehaviour {
    public GameObject otherGo;

    [Button("AdjustEqual")]
    public void AdjustEqual() {
        Rect r = SubSpriteBounds(gameObject);
        Rect or = SubSpriteBounds(otherGo);
        Vector2 scale = VectorUtils.Do(or.size, r.size, (f1, f2) => f1 / f2);
        Func<Vector3, Func<Vector2, Vector2>, Vector3> v223 = (v3, a) => VectorUtils.V23(a(VectorUtils.V32(v3)), v3.z);
        transform.localScale = v223(transform.localScale, s => VectorUtils.Multiply(s, scale));
        r = SubSpriteBounds(gameObject);
        Vector2 offset = or.center - r.center;
        transform.position += VectorUtils.V23(offset);
    }
    public Rect SubSpriteBounds(GameObject go) {
        SpriteRenderer[] renderers = go.GetComponentsInChildren<SpriteRenderer>();
        Vector2 min = getPoint(renderers, b => b.min, new Vector2(1,1) * float.MaxValue, (v1,v2)=>VectorUtils.Do(v1,v2,Mathf.Min));
        Vector2 max = getPoint(renderers, b => b.max, new Vector2(1,1) * float.MinValue, (v1,v2)=>VectorUtils.Do(v1,v2,Mathf.Max));
        return new Rect(min, max - min);
    }
    
    private Vector2 getPoint(SpriteRenderer[] renderers, Func<Bounds, Vector2> getBoundsPoint, Vector2 initValue, Func<Vector2, Vector2, Vector2> replace) {
        foreach(var renderer in renderers) {
            var boundPoint = getBoundsPoint(renderer.bounds);
            initValue = replace(initValue, boundPoint);
        }
        return initValue;
    }
}