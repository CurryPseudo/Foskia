using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PseudoTools;
using UnityEngine;
using Sirenix.OdinInspector;
namespace SceneController {
    public class SpriteLayerSetter : MonoBehaviour {
        public SpriteLayers layers;
        [Button]
        public void Set() {
            var d = new Dictionary<string, SpriteLayer>();
            foreach(var layer in layers.layers) {
                foreach(var s in layer.Value) {
                    d.Add(s.name, layer.Key);
                }
            }
            for(int i = 0; i < transform.childCount; i++) {
                var trans = transform.GetChild(i);
                var go = trans.gameObject;
                var sr = go.GetComponent<SpriteRenderer>();
                if(sr == null) continue;
                SpriteLayer layer = null;
                if(!d.TryGetValue(sr.sprite.name, out layer)) {
                    continue;
                }
                layer.SetSprite(sr);

            }
        }
        public SpriteLayersRegex regexLayers;
        [Button]
        public void SetRegex() {
            for(int i = 0; i < transform.childCount; i++) {
                var trans = transform.GetChild(i);
                var go = trans.gameObject;
                var sr = go.GetComponent<SpriteRenderer>();
                if(sr == null) continue;
                foreach(var pair in regexLayers.layers) {
                    Regex r = new Regex(pair.Value, RegexOptions.None);
                    if(r.IsMatch(sr.sprite.name)) {
                        pair.Key.SetSprite(sr);
                        break;
                    }
                }

            }
        }
    }
}