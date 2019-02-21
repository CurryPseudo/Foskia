using System;
using PseudoTools;
using UnityEngine;
namespace PseudoTools {
    public static class GoUtils {
        public static void ClearTransformChilds(Transform transform) {
            for(int i = 0; i < transform.childCount; i++) {
                UnityEngine.Object.DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
        public static GameObject AddAnEmptyGameObject(string name, Transform parent) {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }
        public static T GetOrAddComponent<T>(GameObject go) where T : MonoBehaviour {
            var t = go.GetComponent<T>();
            if(t == null) t = go.AddComponent<T>();
            return t;

        }
    }
}