using UnityEngine;
using UnityEngine.Rendering;
using PseudoTools;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;
public class LightSystem : SerializedMonoBehaviour {
//public class LightSystem : MonoBaehaviour {
    public static LightSystem main{
        get {
            return GameObject.Find("LightSystem").GetComponent<LightSystem>();
        }
    }
    public string lightTexLayer;
    //[ReadOnly]
    public LayerMask LightTexLayerMask {
        get {
            return LayerMask.GetMask(lightTexLayer);
        }
    }
    public string finalTexLayer;
    public LayerMask finalTexLayerMask {
        get {
            return LayerMask.GetMask(finalTexLayer);
        }
    }
    public Shader BlurShader;
    public Shader LightViewMesh;
    public Shader LightMixMesh;
    public Shader ScreenMix;
    public Dictionary<string, LightLayer> lightLayerMap = new Dictionary<string, LightLayer>();
    [ReadOnly]
    public Dictionary<string, LayerRuntimeData> runtimeDatasMap = new Dictionary<string, LayerRuntimeData>();
    private Material lightMeshM = null;
    public RenderTexture finalTexture;
    private List<string> layers;
    private LayerMask originMainCameraCullingMask;
    public void Awake() {
        DontDestroyOnLoad(gameObject);
    }
    public void Start() {
    }
    public void OnEnable() {
        Func<LayerMask, string, Camera> createCamera = (cullingLayer, goName) => {
            var go = GoUtils.AddAnEmptyGameObject(goName, transform);
            var camera = CameraUtils.AddRenderCamera(go, cullingLayer);
            camera.depth = -2;
            StartCoroutine(CoroutineUtils.UpdateAction(() => CameraUtils.CopyCameraPosAndSize(camera, Camera.main)));
            return camera;
        };
        Color clearColor = new Color(0f, 0f, 0f, 0f);
        //Set up catch cameras.
        runtimeDatasMap.Clear();
        foreach(var pair in lightLayerMap) {
            var runtimeData = new LayerRuntimeData();
            var camera = createCamera(pair.Value.catchLayer, "Catch " + pair.Key);
            camera.backgroundColor = clearColor;
            runtimeData.catchCamera = camera;
            runtimeData.lightTexture = ScreenTextureAllocator.generateRenderTexture(0.3f);
            StartCoroutine(CoroutineUtils.UpdateAction(() => ScreenTextureAllocator.allocateTexture(ref runtimeData.lightTexture, 0.3f)));
            runtimeData.mixTexture = ScreenTextureAllocator.generateRenderTexture();
            runtimeData.catchTexture = ScreenTextureAllocator.generateRenderTexture();
            runtimeData.catchCamera.targetTexture = runtimeData.catchTexture;
            StartCoroutine(CoroutineUtils.UpdateAction(() => ScreenTextureAllocator.allocateTexture(ref runtimeData.catchTexture, ()=>runtimeData.catchCamera.targetTexture = runtimeData.catchTexture)));
            StartCoroutine(CoroutineUtils.UpdateAction(() => ScreenTextureAllocator.allocateTexture(ref runtimeData.mixTexture)));
            runtimeDatasMap.Add(pair.Key, runtimeData);
        }
        MixEffect.SetLightMixShader(LightMixMesh);
        MixEffect.SetScreenMixShader(ScreenMix);
        BlurEffect.SetShader(BlurShader);
        
        finalTexture = ScreenTextureAllocator.generateRenderTexture();
        StartCoroutine(CoroutineUtils.UpdateAction(() => ScreenTextureAllocator.allocateTexture(ref finalTexture)));
        layers = new List<string>(lightLayerMap.Keys);
        layers.Sort((ll, rl) => lightLayerMap[ll].depth.CompareTo(lightLayerMap[rl].depth));
        originMainCameraCullingMask = Camera.main.cullingMask;
        Camera.main.cullingMask = finalTexLayerMask;
    }
    public void OnDisable() {
        if(Camera.main != null && Camera.main.cullingMask == finalTexLayerMask) {
            Camera.main.cullingMask = originMainCameraCullingMask;
        }
    }
    public void Update() {
        DrawMesh(finalTexLayer, Camera.main, finalTexture);
    }
    public void OnRenderObject() {
        //Set up layer-light dictionary
        if(Camera.current == Camera.main) {
            var renderers = MonoBehaviour.FindObjectsOfType<LightRenderer>();
            var lightsMap = new Dictionary<string, List<LightRenderer>>();
            foreach(var layer in lightLayerMap.Keys) {
                lightsMap.Add(layer, new List<LightRenderer>());
            }
            foreach(var renderer in renderers) {
                string layer = renderer.layer;
                if(lightsMap.ContainsKey(layer)) {
                    lightsMap[layer].Add(renderer);
                }
            }
            if(lightMeshM == null) {
                lightMeshM = new Material(LightViewMesh);
            }
            foreach(var layer in lightLayerMap.Keys) {
                var runtimeData = runtimeDatasMap[layer];
                var lightLayer = lightLayerMap[layer];
                RenderUtils.RenderActive(runtimeData.lightTexture, ()=> {
                    GL.Clear(true, true, new Color(0,0,0,1), 0);
                    foreach(var renderer in lightsMap[layer]) {
                        var mesh = renderer.viewMesh.mesh;
                        lightMeshM.SetVector("_LightPos", renderer.viewMesh.pos);
                        lightMeshM.SetFloat("_Radius", renderer.viewMesh.radius);
                        lightMeshM.SetFloat("_Pow", renderer.lightPow);
                        lightMeshM.SetFloat("_BrightNess", renderer.brightness);
                        lightMeshM.SetColor("_LightColor", renderer.lightColor);
                        var success = lightMeshM.SetPass(0);
                        Graphics.DrawMeshNow(renderer.viewMesh.mesh, Matrix4x4.identity);
                        
                    }
                });
                // Blur light tex.
                BlurEffect.SetBlurSize(lightLayer.blurSize);
                BlurEffect.SetIterTimes(lightLayer.iterTimes);
                BlurEffect.BlurBlit(ref runtimeData.lightTexture);
                //DrawMesh(lightTexLayer, runtimeData.blurCamera, runtimeData.lightTexture);
                // Mix catch texture with light texture
                Graphics.Blit(runtimeData.catchTexture, runtimeData.mixTexture);
                MixEffect.LightMixBlit(runtimeData.mixTexture, runtimeData.lightTexture);
            }
            
            MixEffect.ScreenMixBlit(finalTexture, getMixTextures());
        }
    }
    public void DrawMesh(string layer, Camera camera, RenderTexture tex) {
        var mpb = new MaterialPropertyBlock();
        mpb.SetTexture("_MainTex", tex);
        Graphics.DrawMesh(CameraUtils.QuadMesh, VectorUtils.V32(Camera.main.transform.position), Quaternion.identity, MixEffect.ScreenMixM, LayerMask.NameToLayer(layer), camera, 0, mpb);
    }
    private IEnumerable<RenderTexture> getMixTextures() {
        foreach(var layer in layers) {
            yield return runtimeDatasMap[layer].mixTexture;
        }
    }
    
    [System.Serializable]
    public class LightLayer {
        public LayerMask catchLayer;
        public float depth = 0;
        public float blurSize = 2;
        public int iterTimes = 2;
        public bool lighted = true;
    }
    public class LayerRuntimeData {
        public Camera catchCamera;
        public RenderTexture lightTexture;
        public RenderTexture catchTexture;
        public RenderTexture mixTexture;
    }
}