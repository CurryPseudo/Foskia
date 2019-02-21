using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using PseudoTools;
public static class MixEffect {
    private static Material lightMixM;
    private static Material screenMixM;
    private static Color clearScreenColor;
    public static Color ClearScreenColor {
        get {
            return clearScreenColor;
        }
        set {
            clearScreenColor = value;
        }
    }
    public static Material ScreenMixM {
        get {
            return screenMixM;
        }
    }
    private static void setShader(Shader shader, ref Material m) {
        if(m == null || m.shader != shader) {
            m = new Material(shader);
        }
    }
    public static void SetLightMixShader(Shader lightMixShader) {
        setShader(lightMixShader, ref lightMixM);
    }
    public static void SetScreenMixShader(Shader screenMixShader) {
        setShader(screenMixShader, ref screenMixM);
    }
    public static void LightMixBlit(RenderTexture catchTexture, RenderTexture lightTexture) {
        RenderUtils.RenderActive(catchTexture, ()=>{
            lightMixM.SetTexture("_MainTex", lightTexture);
            var sucess = lightMixM.SetPass(0);
            Graphics.DrawMeshNow(CameraUtils.QuadMesh, VectorUtils.V32(Camera.main.transform.position), Quaternion.identity);
        });
    }
    public static void ScreenMixBlit(RenderTexture targetTex, IEnumerable<RenderTexture> sources) {
        RenderUtils.RenderActive(targetTex, ()=>{
            ScreenDrawMeshNow(sources);
        });
    }
    public static void ScreenDrawMeshNow(IEnumerable<RenderTexture> sources) {
        GL.Clear(true, true, new Color(0f,0f,0f,1), 0);
        foreach(var tex in sources) {
            screenMixM.SetTexture("_MainTex", tex);
            //screenMixM.SetColor("_ClearScreenColor", clearScreenColor);
            var sucess = screenMixM.SetPass(0);
            Graphics.DrawMeshNow(CameraUtils.QuadMesh, VectorUtils.V32(Camera.main.transform.position), Quaternion.identity);
        }
    }
}