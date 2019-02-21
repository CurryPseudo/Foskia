using System;
using System.Collections;
using UnityEngine;
using PseudoTools;
using Sirenix.OdinInspector;
public class LightRenderer : MonoBehaviour {
    [InlineEditor]
    public ViewMesh viewMesh;
    [ReadOnly]
    public RenderTexture targetTexture;
    [ReadOnly]
    public Material LightViewMeshMaterial;
    public string lightMeshLayerName;
    public Color lightColor;
    public float lightPow;
    public int blurSize = 1;
    public LightSystem lightSystem;
    public float brightness;
    public string layer;
    public void Start() {
    }
    public void Awake() {
    }
}