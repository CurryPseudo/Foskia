using UnityEngine;
using Sirenix.OdinInspector;
[ExecuteInEditMode]
public class SpineScreenColorSetter : MonoBehaviour {
    public Color TintColor;
    
    void OnEnable() {
        if(Application.isPlaying) {
            updateColor();
        }
    }
    void updateColor() {
        try{
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetColor("_Color", TintColor);
        GetComponent<MeshRenderer>().SetPropertyBlock(mpb);
        }
        catch {

        }
    }
    void Update() {
        if(!Application.isPlaying) {
            updateColor();
        }
    }
}