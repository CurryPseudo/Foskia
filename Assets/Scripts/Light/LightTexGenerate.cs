using UnityEditor;
using UnityEngine;
using System.IO;
using Sirenix.OdinInspector;
[CreateAssetMenu]
public class LightTexGenerate : ScriptableObject {
    public string path;
    public int width;
    public float minRadius;
    public float pow;
    [Button]
    public void Generate() {
        Texture2D tex = new Texture2D(width, width, TextureFormat.RGBA32, false, true);
        float radius = width / 2;
        for(int i = 0; i < width; i++) {
            for(int j = 0; j < width; j++) {
                Color c = new Color(1,1,1,0);
                float a = new Vector2(i - radius, j - radius).magnitude;
                a = Mathf.InverseLerp(radius, radius * minRadius, a);
                a = Mathf.Clamp01(a);
                a = Mathf.Pow(a, pow);
                c.a = a;
                tex.SetPixel(i, j, c);
                //Debug.Log("Save to png " + (i * width + j).ToString() + "/" + (width * width).ToString());
            }
        }
        
        Debug.Log("Start writting png");
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Debug.Log("Finished writting png");
    }
}