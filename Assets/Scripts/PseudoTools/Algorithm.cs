using System;
using System.Collections.Generic;
using PseudoTools;
using UnityEngine;
namespace PseudoTools {
    public static class Algorithm {
        public static T GetMin<T>(IEnumerable<T> ts, Func<T, float> getValue) {
            float minValue;
            return GetMin<T>(ts, getValue, out minValue);
        }
        public static T GetMin<T>(IEnumerable<T> ts, Func<T, float> getValue, out float minValue, Action minAction = null) {
            if(minAction == null) minAction = ()=>{};
            T minT = default(T);
            minValue = float.PositiveInfinity;
            foreach(var t in ts) {
                var value = getValue(t);
                if(value < minValue) {
                    minValue = value;
                    minAction();
                    minT = t;
                }
            }
            return minT;

        }
        public static IEnumerable<B> Map<A, B>(IEnumerable<A> _as, Func<A, B> a2b) {
            foreach(var a in _as) {
                yield return a2b(a);
            }
        }
        public static IEnumerable<T> Filter<T>(IEnumerable<T> ts, Func<T, bool> valid) {
            foreach(var t in ts) {
                if(valid(t)) yield return t;
            }
        }
        public static IEnumerable<T> GetMonos<T>(IEnumerable<GameObject> gos) where T : MonoBehaviour {
            Func<GameObject, T> getT = (go) => {
                Debug.Assert(go != null);
                return go.GetComponent<T>();
            };
            return Filter<T>(Map<GameObject, T>(gos, getT), (t) => t != null);
        }
        public static T FirstValid<T>(IEnumerable<T> ts, Func<T, bool> valid) {
            foreach(var t in ts) {
                if(valid(t)) {
                    return t;
                }
            }
            return default(T);
        }
    }
    public class ClassRef<T> {
        public Action<T> set;
        public Func<T> get;

        public ClassRef(Action<T> set, Func<T> get)
        {
            this.set = set;
            this.get = get;
        }
    }
}