using System;
using PseudoTools;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
namespace PseudoTools {
    [RequireComponent(typeof(BoxCollider2D))]
    public class BoxCollision : MonoBehaviour {
        public BoxCollider2D boxCollider2D;
        private Bounds bounds {
            get {
                return boxCollider2D.bounds;
            }
        }
        public Vector2 Center {
            get {
                return v32(bounds.center);
            }
        }
        private Vector2 size {
            get {
                return v32(bounds.size);
            }
        }
        public Rect BoundRect {
            get {
                return Bound2Rect(bounds);
            }
        }
        public static Rect Bound2Rect(Bounds bounds) {
            return new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y);
        }
        public int rayCount = 10;
        public float skinWidth = 0.05f;
        private float? checkCollision(LayerMask layer, Vector2 component, Vector2 dir) {
            return checkCollision(layer, component, dir, Center);
        }
        private float? checkCollision(LayerMask layer, Vector2 component, Vector2 dir, Vector2 originPos) {
            if(component == Vector2.zero) {
                return null;
            }
            float? result = float.PositiveInfinity;
            bool flag = false;
            foreach(var pos in getComponentPoints(component, originPos)) {
                var hit = Physics2D.Raycast(pos, dir.normalized, dir.magnitude, layer);
                if(hit) {
                    var dis = Mathf.Max(0, hit.distance - skinWidth);
                    result = Mathf.Min(dis, result.Value);
                    flag = true;
                }
            }
            if(flag) {
                return result;
            }
            else {
                return null;
            }
        }
        private float? Min(float? f1, float? f2) {
            if(f1 == null) return f2;
            if(f2 == null) return f1;
            return Mathf.Min(f1.Value, f2.Value);
        }
        public float? CheckMoveCollision(Vector2 dir, LayerMask layer) {
            return CheckMoveCollision(dir, layer, Center);
        }
        public float? CheckMoveCollision(Vector2 dir, LayerMask layer, Vector2 pos) {
            Vector2 component = VectorUtils.Do(dir, Mathf.Sign);
            Func<Vector2, float?> getCollision = (v) => {
                return checkCollision(layer, v, dir, pos);
            };
            return Min(getCollision(new Vector2(component.x, 0)), getCollision(new Vector2(0, component.y)));
        }
        public bool CheckStaticCollision(LayerMask layer, Vector2 pos) {
            foreach(var hit in CheckStaticCollisions(layer, pos)) {
                return true;
            }
            return false;
        }
        public bool CheckStaticCollision(LayerMask layer) {
            return CheckStaticCollision(layer, Center);
        }
        public IEnumerable<RaycastHit2D> CheckStaticCollisions(LayerMask layer) {
            return CheckStaticCollisions(layer, Center);
        }
        public int CheckStaticCollisions(LayerMask layer, RaycastHit2D[] hits) {
            int count = 0;
            foreach(var hit in CheckStaticCollisions(layer)) {
                hits[count] = hit;
                count++;
            }
            return count;
        }
        public IEnumerable<RaycastHit2D> CheckStaticCollisions(LayerMask layer, Vector2 pos) {
            Func<Vector2, RaycastHit2D> rayCast = (component) => {
                Vector2 vertical = new Vector2(-component.y, component.x); // -90degree
                Vector2 originPos = pos + VectorUtils.Multiply(size, component) / 2 + VectorUtils.Multiply(vertical, size) / 2 - vertical * skinWidth;
                float length = VectorUtils.Multiply(vertical, size).magnitude - 2 * skinWidth;
                return Physics2D.Raycast(originPos, -vertical, length, layer);
            };
            for(int i = -1; i <= 1; i++) {
                for(int j = -1; j <= 1; j++) {
                    if((i == 0 && j != 0) || (i != 0 && j == 0)) {
                        var hit = rayCast(new Vector2(i, j));
                        if(hit) {
                            yield return hit;
                        }
                    }
                }
            }
            yield break;
        }
        private Vector2 v32(Vector3 v3) {
            return VectorUtils.V32(v3);
        }
        IEnumerable<Vector2> getComponentPoints(Vector2 component) {
            return getComponentPoints(component, Center);
        }
        IEnumerable<Vector2> getComponentPoints(Vector2 component, Vector2 pos) {
            Vector2 vertical = new Vector2(-component.y, component.x); // -90degree
            Vector2 originPos = pos + VectorUtils.Multiply(size, component) / 2 + VectorUtils.Multiply(vertical, size) / 2 - vertical * skinWidth;
            yield return originPos;
            float length = VectorUtils.Multiply(vertical, size).magnitude - 2 * skinWidth;
            for(int i = 0; i < rayCount; i++) {
                yield return originPos - vertical * length / (float)rayCount  * (float)(i + 1);
            }
            yield break;
        }
    }
}