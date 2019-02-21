using UnityEngine;
using System;
namespace PseudoTools{
    public static class VectorUtils{
        public static Vector2 V32(Vector3 v3) {
            return new Vector2(v3.x, v3.y);
        }
        public static Vector3 V23(Vector2 v2, float z = 0) {
            return new Vector3(v2.x, v2.y, z);
        }
        public static Vector4 V24(Vector2 v2, float z = 0, float w = 0) {
            return new Vector4(v2.x, v2.y, z, w);
        }
        public static Vector3 Multiply(Vector3 v1, Vector3 v2) {
            return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        }
        public static Vector2 Multiply(Vector2 v1, Vector2 v2) {
            return new Vector2(v1.x * v2.x, v1.y * v2.y);
        }
        public static Vector3 Division(Vector3 v1, Vector3 v2) {
            return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
        }
        public static Vector2 Division(Vector2 v1, Vector2 v2) {
            return new Vector2(v1.x / v2.x, v1.y / v2.y);
        }
        public static Vector2 Do(Vector2 v1, Vector2 v2, Func<float, float, float> f) {
            return new Vector2(f(v1.x, v2.x), f(v1.y, v2.y));
        }
        public static Vector2 Do(Vector2 v1, Vector2 v2, Vector2 v3, Func<float, float, float, float> f) {
            return new Vector2(f(v1.x, v2.x, v3.x), f(v1.y, v2.y, v3.y));
        }
        public static Vector2 Do(Vector2 v1, Func<float, float> f) {
            return new Vector2(f(v1.x), f(v1.y));
        }
        public static Vector4 Do(Vector4 v1, Func<float, float> f) {
            return new Vector4(f(v1.x), f(v1.y), f(v1.z), f(v1.w));
        }
        public static Vector3 Do(Vector3 v1, Func<float, float> f) {
            return new Vector3(f(v1.x), f(v1.y), f(v1.z));
        }
        public static Vector2 DoComponent(Vector2 v, Vector2 component, Func<float, float> f) {
            Vector2 pureComponent = Do(component, (_f) => _f != 0 ? 1 : 0);
            return VectorUtils.Do(v, pureComponent, (_f, c) => c == 0 ? _f : f(_f));
        }
        public static Vector3 DoComponent(Vector3 v, Vector3 component, Func<float, float> f) {
            var pureComponent = Do(component, (_f) => _f != 0 ? 1 : 0);
            return VectorUtils.Do(v, pureComponent, (_f, c) => c == 0 ? _f : f(_f));
        }

        public static Vector2 ReplaceComponent(Vector2 v, Vector2 component , float f) {
            return DoComponent(v, component, (_f) => f);
        }
        public static Vector3 ReplaceComponent(Vector3 v, Vector3 component , float f) { 
            return DoComponent(v, component, (_f) => f);
        }
        public static void ReplaceComponent(ref Vector2 v, Vector2 component, float f) {
            v = ReplaceComponent(v, component, f);
        }
        public static Vector2 xComponent2 {
            get {
                return Vector2.right;
            }
        }
        public static Vector2 yComponent2 {
            get {
                return Vector2.up;
            }
        }
        public static Vector2 ChangeX(Vector2 v1, float value) {
            v1.x = value;
            return v1;
        }
        public static Vector2 ChangeY(Vector2 v1, float value) {
            v1.y = value;
            return v1;
        }
    }
}