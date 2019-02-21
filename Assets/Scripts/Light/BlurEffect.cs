using UnityEngine;
using UnityEngine.Rendering;
public static class BlurEffect {
    static Material blurM;
    static float _blurSize = 2;
    static int _iterTimes = 2;
    public static void SetIterTimes(int iterTimes) {
        _iterTimes = iterTimes;
    }
    public static void SetBlurSize(float blurSize) {
        _blurSize = blurSize;
    }
    public static void SetShader(Shader blurShader) {
        if(blurM == null || blurM.shader != blurShader) {
            blurM = new Material(blurShader);
        }
    }
    public static void BlurBlit(ref RenderTexture source) {
        //RenderTexture blurTempA = RenderTexture.GetTemporary(source.descriptor);
        //Graphics.Blit(source, blurTempA);
        RenderTexture blurTempB = RenderTexture.GetTemporary(source.descriptor);
        for(int i = 0; i < _iterTimes; i++) {
            float iterationOffs = i * 1.0f;
            var blurSize = _blurSize + iterationOffs;
            blurM.SetFloat("_blurSize", blurSize);
            // vertical
            blurM.SetVector("_component", new Vector4(0, 1, 0, 0));
            Graphics.Blit(source, blurTempB, blurM, 0);
            //horizontal
            blurM.SetVector("_component", new Vector4(1, 0, 0, 0));
            Graphics.Blit(blurTempB, source, blurM, 0);
        }
        RenderTexture.ReleaseTemporary(blurTempB);
    }
}