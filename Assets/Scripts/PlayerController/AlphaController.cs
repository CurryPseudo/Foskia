using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using PseudoTools;
public static class ApCtrl {
    public class AlphaData {
        public Coroutine cor = null;
        public AlphaRef alphaRef;
        public float originAlpha = 1;
        public float convertSpeed = 0;
        public float targetAlpha = 1;
        public MonoBehaviour mb;
        public float TargehAlphaNormed {
            get {
                return targetAlpha / originAlpha;
            }
            set {
                targetAlpha = originAlpha * value;
            }
        }
    }
    public class AlphaRef : ClassRef<float>
    {
        public AlphaRef(Action<float> set, Func<float> get) : base(set, get)
        {
        }
    }
    public static AlphaData CreateAlphaData(AlphaRef colorRef, MonoBehaviour mb) {
        AlphaData ad = new AlphaData();
        ad.alphaRef = colorRef;
        ad.originAlpha = ad.alphaRef.get();
        ad.targetAlpha = ad.originAlpha;
        ad.mb = mb;
        ad.cor = mb.StartCoroutine(ctrlAlpha(ad));
        return ad;
    }
    private static IEnumerator ctrlAlpha(AlphaData ad) {
        while(true) {
            try {
                var nowAlpha = ad.alphaRef.get();
                if(nowAlpha != ad.targetAlpha) {
                    var deltaAlpha = ad.convertSpeed * Time.unscaledDeltaTime;
                    var normedDeltaAlpha = Mathf.Clamp01(deltaAlpha / Mathf.Abs(nowAlpha - ad.targetAlpha));
                    ad.alphaRef.set(Mathf.Lerp(nowAlpha, ad.targetAlpha, normedDeltaAlpha));
                }
            }catch {
                yield break;
            }
            yield return null;
        }
    }
    public static void DisappearAlpha(AlphaData ad, float time) {
        ad.targetAlpha = 0;
        if(time == 0) {
            ad.alphaRef.set(0);
        }
        else {
            ad.convertSpeed = ad.originAlpha / time;
        }
        
    }
    public static void SetAlphaImmediately(AlphaData ad, float a) {
        ad.targetAlpha = a * ad.originAlpha;
        ad.convertSpeed = 0;
        ad.alphaRef.set(ad.targetAlpha);
    }
    public static void AppearAlpha(AlphaData ad, float time) {
        ad.targetAlpha = ad.originAlpha;
        if(time == 0) {
            ad.alphaRef.set(ad.originAlpha);
        }
        else {
            ad.convertSpeed = ad.originAlpha / time;
        }
    }
    public static AlphaRef TextAlpha(Text text) {
        return ColorAlpha(new ClassRef<Color>((c) => text.color = c.linear, () => text.color.linear));
    }
    public static AlphaRef ColorAlpha(ClassRef<Color> color) {
        Func<float> get = () => color.get().a;
        Action<float> set = (a) => {
            var col = color.get();
            col.a = a;
            color.set(col);
        };
        return new AlphaRef(set, get);
    }
    public static AlphaRef SpriteAlpha(SpriteRenderer renderer) {
        return new AlphaRef((c) => renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, c), () => renderer.color.a);
    }
    public static AlphaRef LightRendererAlpha(LightRenderer renderer) {
        Func<float> get = () => renderer.brightness;
        Action<float> set = (a) => {
            renderer.brightness = a;
            if(a == 0) {
                renderer.viewMesh.enabled = false;
            }
            else {
                renderer.viewMesh.enabled = true;
            }
        };
        return new AlphaRef(set, get);
    }
    public static AlphaRef TransformScaleXY(Transform transform) {
        Vector2 originScaleXY = VectorUtils.V32(transform.localScale);
        Func<float> getAlpha = () => transform.localScale.x / originScaleXY.x;
        Action<float> setAlpha = (f) => transform.localScale = VectorUtils.V23(originScaleXY * f, transform.localScale.z);
        return new AlphaRef(setAlpha, getAlpha);
    }
    public static AlphaRef ImageAlpha(Image image) {
        return ColorAlpha(SRGBToLiner(new ClassRef<Color>((c) => image.color = c, () => image.color)));
        //return ColorAlpha(new ClassRef<Color>((c) => image.color = c, () => image.color));
    }
    public static ClassRef<Color> SRGBToLiner(ClassRef<Color> srgb) {
        return new ClassRef<Color>((c) => srgb.set(VectorUtils.Do(c, f => Mathf.Pow(f, 1 / 2.2f))), () => VectorUtils.Do(srgb.get(), f => Mathf.Pow(f, 2.2f)));
    }
    public static AlphaRef SpineMeshAlpha(MeshRenderer renderer) {

        AlphaRef r;
        var mpb = new MaterialPropertyBlock();
        mpb.SetColor("_Color", renderer.material.GetColor("_Color"));
        renderer.SetPropertyBlock(mpb);
        r = new AlphaRef((c) => {
                var col = mpb.GetColor("_Color");
                col.a = c;
                mpb.SetColor("_Color", col);
                renderer.SetPropertyBlock(mpb);
            }, () => mpb.GetColor("_Color").a);
        return r;
    }
    public static AlphaRef AudioVolumeAlpha(AudioSource source) {
        return new AlphaRef(f => source.volume = f, () => source.volume);
    }
    private static Coroutine controllAlpha(AlphaData ad, float time, float targetAlpha) {
        var alphaRef = ad.alphaRef;
        float nowAlpha = ad.alphaRef.get();
        var realTime = time * Mathf.Abs(targetAlpha - nowAlpha) / ad.originAlpha;
        if(ad.cor != null) {
            ad.mb.StopCoroutine(ad.cor);
        }
        Timer timer = null;
        timer = Timer.CreateAUnscaledTimer(time, () => {if(ad.cor == timer.Coroutine) ad.cor = null;}, ad.mb);
        timer.timerUpdateAction += () => {
            alphaRef.set(Mathf.Lerp(nowAlpha, targetAlpha, timer.Value));
            
        };
        ad.cor = timer.Coroutine;
        return timer.Coroutine;
    }
}