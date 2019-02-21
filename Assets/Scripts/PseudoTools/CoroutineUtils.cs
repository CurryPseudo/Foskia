using UnityEngine;
using System.Collections;
using PseudoTools;
using System;
namespace PseudoTools {
    public class CoroutineUtils {
        public static IEnumerator UpdateAction(Action action) {
            while(true) {
                action();
                yield return null;
            }
        }
        public static IEnumerator FixedUpdateAction(Action action) {
            while(true) {
                action();
                yield return new WaitForFixedUpdate();
            }
        }
    }

}