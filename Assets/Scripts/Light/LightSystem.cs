using UnityEngine;
using UnityEngine.Rendering;
using PseudoTools;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;
using System;
public class LightSystem : SerializedMonoBehaviour {
//public class LightSystem : MonoBaehaviour {
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
    public IEnumerator UpdateRuntimeData(LayerRuntimeData runtimeData) { 
        while(true) {
            CameraUtils.CopyCameraPosAndSize(runtimeData.catchCamera, Camera.main);
            ScreenTextureAllocator.allocateTexture(ref runtimeData.catchTexture, ()=>runtimeData.catchCamera.targetTexture = runtimeData.catchTexture);
            yield return null;
        }

    }
    public void OnEnable() {
        Color clearColor = new Color(0f, 0f, 0f, 0f);
        //Set up catch cameras.
        runtimeDatasMap.Clear();
        foreach(var pair in lightLayerMap) {
            var runtimeData = new LayerRuntimeData();
            var go = GoUtils.AddAnEmptyGameObject("Catch " + pair.Key, transform);
            var camera = CameraUtils.AddRenderCamera(go, pair.Value.catchLayer);
            camera.depth = -2;
            camera.backgroundColor = clearColor;
            runtimeData.catchCamera = camera;
            runtimeData.catchTexture = ScreenTextureAllocator.generateRenderTexture();
            runtimeData.catchCamera.targetTexture = runtimeData.catchTexture;
            StartCoroutine(UpdateRuntimeData(runtimeData));
            runtimeDatasMap.Add(pair.Key, runtimeData);
        }
        MixEffect.SetLightMixShader(LightMixMesh);
        MixEffect.SetScreenMixShader(ScreenMix);
        BlurEffect.SetShader(BlurShader);
        finalTexture = ScreenTextureAllocator.generateRenderTexture();
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
    private RenderTextureDescriptor GenCameraTex(Camera camera, RenderTextureFormat format, float scale = 1) {
        return new RenderTextureDescriptor(Mathf.RoundToInt(camera.pixelWidth * scale), Mathf.RoundToInt(camera.pixelHeight * scale), format);
    }
    private int GetTemporaryRTId(string idName, CommandBuffer cb, RenderTextureDescriptor desc) {
        var id = Shader.PropertyToID(idName);
        cb.GetTemporaryRT(id, desc);
        return id;
    }
    public void OnRenderObject() {
        //Set up layer-light dictionary

        if(Camera.current == Camera.main) {
            var camera = Camera.main;
            CommandBuffer cb = new CommandBuffer();

            var lightTexId = GetTemporaryRTId("_LightMap", cb, GenCameraTex(camera, RenderTextureFormat.ARGBFloat, 0.3f));
            var shadowTexId = GetTemporaryRTId("_ShadowMap", cb, GenCameraTex(camera, RenderTextureFormat.RFloat, 0.3f));
            var diffuseTexId = GetTemporaryRTId("_DiffuseMap", cb, GenCameraTex(camera, RenderTextureFormat.ARGBFloat));

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
                
                cb.SetRenderTarget(lightTexId);
                cb.ClearRenderTarget(false, true, Color.black);
                foreach(var renderer in lightsMap[layer]) {
                    cb.SetRenderTarget(shadowTexId);
                    cb.ClearRenderTarget(false, true, Color.black);
                    renderer.PointLight.DrawShadowMesh(ref cb);
                    cb.SetRenderTarget(lightTexId);
                    renderer.PointLight.DrawLightMesh(ref cb, shadowTexId);
                }
                // Blur light tex.
                //BlurEffect.SetBlurSize(lightLayer.blurSize);
                //BlurEffect.SetIterTimes(lightLayer.iterTimes);
                //BlurEffect.BlurBlit(ref runtimeData.lightTexture);
                //DrawMesh(lightTexLayer, runtimeData.blurCamera, runtimeData.lightTexture);
                // Mix catch texture with light texture
                cb.Blit(runtimeData.catchTexture, diffuseTexId);
                cb.SetRenderTarget(diffuseTexId);
                DrawScreenTex(cb, lightTexId, camera, LightMixMesh);
                cb.SetRenderTarget(finalTexture);
                DrawScreenTex(cb, diffuseTexId, camera, ScreenMix);
            }
            Graphics.ExecuteCommandBuffer(cb);
        }
    }
    public void DrawScreenTex(CommandBuffer cb, RenderTargetIdentifier tex, Camera camera, Shader shader) {
        cb.SetGlobalTexture("_MainTex", tex);
        cb.DrawMesh(CameraUtils.QuadMesh, Matrix4x4.Translate(VectorUtils.V32(camera.transform.position)), new Material(shader));
    }
    public void DrawMesh(string layer, Camera camera, RenderTexture tex) {
        var mpb = new MaterialPropertyBlock();
        mpb.SetTexture("_MainTex", tex);
        Graphics.DrawMesh(CameraUtils.QuadMesh, VectorUtils.V32(Camera.main.transform.position), Quaternion.identity, MixEffect.ScreenMixM, LayerMask.NameToLayer(layer), camera, 0, mpb);
    }
    
    [System.Serializable]
    public class LightLayer {
        public LayerMask catchLayer;
        public float depth = 0;
    }
    public class LayerRuntimeData {
        public Camera catchCamera;
        public RenderTexture catchTexture;
    }
}