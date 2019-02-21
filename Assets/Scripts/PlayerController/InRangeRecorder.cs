using UnityEngine;
using System.Collections.Generic;
public class InRangeRecorder : MonoBehaviour {
    private HashSet<GameObject> viewRanges = new HashSet<GameObject>();
    public string viewInfo;
    public void InRange(GameObject go) {
        if(!viewRanges.Contains(go)) {
            viewRanges.Add(go);
        }
    }
    public void OutRange(GameObject go) {
        viewRanges.Remove(go);
    }
    public IEnumerable<GameObject> GetViews() {
        foreach(var go in viewRanges) {
            yield return go;
        }
        yield break;
    }
    public bool InView() {
        return viewRanges.Count != 0;
    }
    public void Update() {
        viewInfo = "";
        foreach(var go in GetViews()) {
            viewInfo += go.ToString();
            viewInfo += "\n";
        }
    }
}