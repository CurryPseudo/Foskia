using PseudoTools;
using UnityEngine;
namespace PseudoTools {
    public static class RenderUtils {
        public static void RenderActive(RenderTexture t, System.Action a) {
            var lastRT = RenderTexture.active;
            RenderTexture.active = t;
            a();
            RenderTexture.active = lastRT;
        }
    }
}