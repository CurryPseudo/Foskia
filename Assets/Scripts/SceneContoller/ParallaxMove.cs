using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using PseudoTools;
using System;
namespace SceneController {
    public class ParallaxMove : SerializedMonoBehaviour {
        public Func<Vector2> basePosGet;
        public Vector2 degrees = new Vector2(1,1);
        public ClassRef<Vector2> pivotRef;
        private Vector2 originPos;
        private bool validTransform(Transform transform) {
            return transform != null;
        }
        public void Start() {
            originPos  = pivotRef.get();
        }
        public void Update() {
            var dir = basePosGet() - originPos;
            dir = VectorUtils.Multiply(dir, degrees);
            pivotRef.set(dir + originPos);

        }
        [TabGroup("通过某Go的子精灵来设置Degrees")]
        public DirectionSign xDirection;
        [TabGroup("通过某Go的子精灵来设置Degrees")]
        public DirectionSign yDirection;
        [TabGroup("通过某Go的子精灵来设置Degrees")]
        public GameObject targetGo;
        [ButtonGroup("设置Degrees")]
        public void SetDegrees() {
            if(targetGo != null) {
                Func<GameObject, Func<DirectionSign,DirectionSign>, Vector2> getPoint = (go, Fd) => {
                    return SubSpriteUtils.GetSubSpritesPoint(Fd(xDirection), Fd(yDirection), go);
                };

                Func<DirectionSign, DirectionSign> reverseDir = (d) => {
                    return (DirectionSign)(-(int)d);
                };
                Func<GameObject, Vector2> getVector = (go => getPoint(go, (d=>d)) - getPoint(go, reverseDir));
                
                Func<Func<float>,Func<float>,float> solveComponent = (GetV1, GetV2) => {
                    if(GetV2() == 0) {
                        return 0;
                    }
                    return 1 - GetV1() / GetV2();
                };
                Func<Vector2, Vector2, Vector2> getDegrees = (v1, v2) => {
                    float x = solveComponent(() => v1.x, () => v2.x);
                    float y = solveComponent(() => v1.y, () => v2.y);
                    return new Vector2(x,y);
                };
                degrees = getDegrees(getVector(gameObject), getVector(targetGo));
            }
        }
    }
}