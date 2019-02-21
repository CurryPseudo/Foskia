using UnityEngine;
using System;
using System.Collections.Generic;
using PseudoTools;
public class BoxCollisionCollect : MonoBehaviour {
    public BoxCollision box;
    public LayerMask layer;
    public int maxHits = 20;
    private RaycastHit2D _hit;
    private int hitCount = 0;
    private Vector2 _dir;
    private RaycastHit2D[] hits;
    void Awake() {
        hits = new RaycastHit2D[maxHits];
    }
    void FixedUpdate() {
        hitCount = box.CheckStaticCollisions(layer, hits);
    }
    private IEnumerable<RaycastHit2D> getHits() {
        for(int i = 0; i < hitCount; i++) {
            yield return hits[i];
        }
    }
    public IEnumerable<T> GetDetects<T>() where T : MonoBehaviour {
        foreach(var hit in getHits()) {
            _hit = hit;
            _dir = VectorUtils.V32(_hit.collider.bounds.center - box.boxCollider2D.bounds.center);
            var t = hit.collider.gameObject.GetComponent<T>();
            if(t != null) yield return t;
        }
    }
    public bool HasDetect<T>() where T : MonoBehaviour {
        foreach(var t in GetDetects<T>()) {
            return true;
        }
        return false;
    }
    public T GetMinDisDetects<T>(Func<T, bool> validCondition = null) where T : MonoBehaviour {
        T t;
        RaycastHit2D hit;
        Vector2 dir;
        GetMinDisDetects(out t, out hit, out dir, validCondition);
        return t;
    }
    public void GetMinDisDetects<T>(out T minT, out Vector2 minDir, Func<T, bool> validCondition = null) where T : MonoBehaviour {
        RaycastHit2D hit;
        GetMinDisDetects(out minT, out hit, out minDir, validCondition);
    }
    public void GetMinDisDetects<T>(out T minT, out RaycastHit2D minHit, out Vector2 minDir, Func<T, bool> validCondition = null) where T : MonoBehaviour {
        if(validCondition == null) validCondition = (t) => true;
        float minDis;
        var _minHit = new RaycastHit2D();
        var _minDir = Vector2.zero;
        
        minT = Algorithm.GetMin(GetDetects<T>(), (t) => _dir.magnitude, out minDis, () => {
            _minHit = _hit;
            _minDir = _dir;
        });
        minHit = _minHit;
        minDir = _minDir;
    }
}