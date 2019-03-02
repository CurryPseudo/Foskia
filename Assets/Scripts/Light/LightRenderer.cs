using System;
using System.Collections;
using UnityEngine;
using PseudoTools;
using Sirenix.OdinInspector;
public class LightRenderer : MonoBehaviour {
    [InlineEditor]
    public ViewMesh viewMesh;
    public Color lightColor;
    public float lightPow;
    public float brightness;
    public Material lightMaterial;
    public Material shadowMaterial;
    public string layer;
    private PointLight pointLight;
    public PointLight PointLight {
        get {
            return pointLight;
        }
    }
    public void Awake() {
        CreatePointLight();
    }
    void OnDestroy()
    {
        Destroy(pointLight);
    }
    private void CreatePointLight() {
        pointLight = gameObject.AddComponent<PointLight>();
        StartCoroutine(SyncPointLight());
    }
    private IEnumerator SyncPointLight() {
        while(true) {
            pointLight.lightColor = lightColor;
            pointLight.circleHitPoint.colliderLayer = viewMesh.layer;
            pointLight.circleHitPoint.radius = viewMesh.radius;
            pointLight.lightMaterial = lightMaterial;
            pointLight.shadowMaterial = shadowMaterial;
            pointLight.brightness = brightness;
            yield return null;
        }
    }
    public void Start() {
    }
}