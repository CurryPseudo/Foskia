using UnityEngine;
[ImageEffectTransformsToLDR]
public class LightSystemCameraEffect : MonoBehaviour {
    private Material material = null;
    public float brightness = 1;
    public LightSystem lightSystem;
    public string layer;
    public void Awake() {
    }
    void OnRenderImage (RenderTexture sourceTexture, RenderTexture destTexture) {
    }
}