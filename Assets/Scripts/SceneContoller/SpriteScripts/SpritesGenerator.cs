using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Text.RegularExpressions;
using System;
using PseudoTools;

namespace SceneController {
    public class SpritesGenerator : MonoBehaviour {
        public List<Sprite> sprites = new List<Sprite>();
        [Button]
        public void Generate() {
            GoUtils.ClearTransformChilds(transform);
            Func<string, GameObject> findGo = name => {
                var t = transform.Find(name);
                if(t != null) return t.gameObject;
                return null;
            };
            
            foreach(var s in sprites) {
                var go = findGo(s.name);
                if(go == null) {
                    go = GoUtils.AddAnEmptyGameObject(s.name, transform);
                }
                var renderer = go.GetComponent<SpriteRenderer>();
                if(renderer == null) renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = s;
            }
        }
    }
}