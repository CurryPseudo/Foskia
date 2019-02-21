using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using Sirenix.Serialization;
using PseudoTools;
[CreateAssetMenu]
public class TransforPoints : SerializedScriptableObject {
    private static TransforPoints main;
    public static TransforPoints Main {
        get {
            if(main == null) {
                main = Resources.Load<TransforPoints>("TransforPoints");
            }
            return main;
        }
    }
    public GameObject go;
    [Button]
    public TransforPointIndex AddList() {
        
        var sceneName = go.scene.name;
        var tpi = new TransforPointIndex(0, go.scene.name);
        if(!map.ContainsKey(sceneName)) {
            map.Add(sceneName, new List<Vector2>());
        }
        var index = map[sceneName].Count;
        map[sceneName].Add(VectorUtils.V32(go.transform.position));
        return new TransforPointIndex(index, sceneName);

    }
    public Vector2 GetTransforPoint(TransforPointIndex index) {
        return map[index.sceneName][index.index];
    }
    public Dictionary<string, List<Vector2>> map = new Dictionary<string, List<Vector2>>();
}
[Serializable]
public struct TransforPointIndex {
    public int index;
    public string sceneName;

    public TransforPointIndex(int index, string sceneName)
    {
        this.index = index;
        this.sceneName = sceneName;
    }
}