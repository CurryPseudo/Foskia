using PseudoTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using SceneController;
[CreateAssetMenu]
public class SpriteLayersRegex : SerializedScriptableObject {
	public Dictionary<SpriteLayer, string> layers;
}