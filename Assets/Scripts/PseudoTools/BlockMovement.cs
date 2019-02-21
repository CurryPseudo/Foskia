using System;
using System.Collections.Generic;
using PseudoTools;
using UnityEngine;
namespace PseudoTools {
    public class BlockMovement : MonoBehaviour {
        private Vector2 velocity;
        public BoxCollision blockBoxCollision;
        public LayerMask blockLayer;
        public bool staticExtrusion = false;
        public Vector2 Velocity {
            get {
                return velocity;
            }
            set {
                velocity = value;
            }
        }
        public Vector2 Position {
            get {
                return position;
            }
            set {
                position = value;
                
            }
        }
        private Vector2 position {
            get {
                return blockBoxCollision.Center;
            }
            set {
                var lastPos = VectorUtils.V32(transform.position);
                transform.position = VectorUtils.V23(lastPos + value - blockBoxCollision.Center, transform.position.z);
            }
        }
        public Vector2 MoveDistance {
            get {
                return Velocity * Time.fixedDeltaTime;
            }
        }
        public static BlockMovement AddBlockMovement(GameObject go, BoxCollision box, LayerMask layer) {
            var result = go.AddComponent<BlockMovement>();
            result.blockBoxCollision = box;
            result.blockLayer = layer;
            return result;
        }
        public void BlockMoving(Vector2 mov) {
            Action<Vector2> process = (component) => {//up or right
                Vector2 vertical = new Vector2(1 - component.x, 1 - component.y);
                float? minHit = blockBoxCollision.CheckMoveCollision(VectorUtils.Multiply(mov, component) , blockLayer);
                if(minHit == null) return;
                mov = VectorUtils.Multiply(mov, vertical) + minHit.Value * component * VectorUtils.Do(mov, Mathf.Sign);
            };
            process(Vector2.right);
            position += mov.x * Vector2.right;
            process(Vector2.up);
            position += mov.y * Vector2.up;
        }
        public void StepSolidXY() {
            Vector2 moveDistance = velocity * Time.fixedDeltaTime;
            BlockMoving(moveDistance);
        }

        public void FixedUpdate() {
            StepSolidXY();
        }
        public void Update() {
            if(staticExtrusion) {
                StaticExtrusion();
            }
        }
        public void StaticExtrusion() {
            Rect nowRect = bounds2Rect(blockBoxCollision.boxCollider2D.bounds);
            Vector2 center = nowRect.center;
            float l = nowRect.size.magnitude / 2;
            var colliders = Physics2D.OverlapCircleAll(center, l, blockLayer);
            Rect[] rects = new Rect[colliders.Length];
            bool[] skips = new bool[colliders.Length];
            int index = 0;
            foreach(var collider in colliders) {
                rects[index] = bounds2Rect(collider.bounds);
                if(collider == blockBoxCollision.boxCollider2D) {
                    skips[index] = true;
                }
                index++;
            }
            Action<Action<Rect>> foreachRect = (f) => {
                index = 0;
                while(index < rects.Length) {
                    if(!skips[index]) {
                        f(rects[index]);
                    }
                    index++;
                }
            };
            Vector2 offset = Vector2.zero;
            Rect originRect = nowRect;
            foreachRect((r) => {
                nowRect = originRect;
                nowRect.position += offset;
                if(nowRect.Overlaps(r)) {
                    Vector2 dir = nowRect.center - r.center;
                    Vector2 halfSizeSum = r.size / 2 + nowRect.size / 2;
                    Vector2 absDir = VectorUtils.Do(dir, Mathf.Abs);
                    var mov = halfSizeSum - absDir;
                    var c = mov.x < mov.y ? Vector2.right : Vector2.up;
                    mov = VectorUtils.Multiply(mov, c);
                    //mov = dir.normalized / VectorUtils.Multiply(absDir, c).magnitude * mov.magnitude;
                    mov = VectorUtils.Multiply(mov, VectorUtils.Do(dir, (f) => f >= 0 ? 1 : -1));
                    offset += mov;
                }
            });
            position += offset;




        }
        private Rect bounds2Rect(Bounds bounds) {
            return BoxCollision.Bound2Rect(bounds);
        }
    }
}