using PseudoTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using SceneController;

[Serializable]
public class SpriteLayer {
	public string name;
	public string layer;
	public string sortingLayer;
	public int sortingOrder;
	public Vector2 parallaxMoveDegree;
	public void SetSprite(SpriteRenderer sr) {
		sr.gameObject.layer = LayerMask.NameToLayer(layer);
		sr.sortingLayerName = sortingLayer;
		sr.sortingOrder = sortingOrder;
		var parallaxMove = GoUtils.GetOrAddComponent<ParallaxMove>(sr.gameObject);
		parallaxMove.degrees = parallaxMoveDegree;
		parallaxMove.basePosGet = () => VectorUtils.V32(Camera.main.transform.position);
		Func<Vector2> srPosGet = () => VectorUtils.V32(sr.transform.position);
		Action<Vector2> srPosSet = (pos) => sr.transform.position = VectorUtils.V23(pos, sr.transform.position.z);
		parallaxMove.pivotRef = new ClassRef<Vector2>(srPosSet, srPosGet);
	}
}
[CreateAssetMenu]
public class SpriteLayers : SerializedScriptableObject {
	public Dictionary<SpriteLayer, List<Sprite>> layers;
	
}