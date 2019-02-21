using System.Collections;
using System.Collections.Generic;
using PseudoTools;
using UnityEngine;

[System.Serializable]
public class CameraEdge : CameraPosEffect {
	public BoxCollider2D edge;
	
	public override Vector2 UpdatePos (Vector2 pos) {
		pos.x = Mathf.Clamp(pos.x, edge.bounds.min.x + CameraHalfWidth(), edge.bounds.max.x - CameraHalfWidth());
		pos.y = Mathf.Clamp(pos.y, edge.bounds.min.y + CameraHalfHeight(), edge.bounds.max.y - CameraHalfHeight());
		return pos;
	}
}
