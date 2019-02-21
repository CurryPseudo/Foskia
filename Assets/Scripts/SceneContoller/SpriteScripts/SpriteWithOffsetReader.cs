using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Text.RegularExpressions;
using System;
using PseudoTools;


namespace SceneController {
	public class SpriteWithOffsetReader : MonoBehaviour {
		
		public TextAsset jsonFile;
		
		private IEnumerable<SpriteRenderer> spriteRenderers {
			get {
				for(int i = 0; i < transform.childCount; i++) {
					var t = transform.GetChild(i);
					var s = t.gameObject.GetComponent<SpriteRenderer>();
					if(s != null) {
						yield return s;
					}
				}
				
			}
		}
		// Update is called once per frame
		[Button]
		public void SetSpriteWithOffset() {

			SpriteWithOffset[] os = SpriteWithOffset.CreateFromJson(jsonFile.text);
			Dictionary<string, SpriteWithOffset> d = new Dictionary<string, SpriteWithOffset>();

			foreach(var o in os) {
				d.Add(o.name, o);
			}
			foreach(var sr in spriteRenderers) {
				SpriteWithOffset o = null;
				if(!d.TryGetValue(sr.sprite.name, out o)) {
					Debug.LogWarning(sr.sprite.name + " cant found in json");
					continue;
				}
				float pixelsPerUnit = o.width / sr.bounds.size.x;
				//Debug.Log(pixelsPerUnit);
				Vector2 leftTop = new Vector2(sr.bounds.min.x, sr.bounds.max.y);
				var go = sr.gameObject;
				go.transform.position += VectorUtils.V23(VectorUtils.V32(transform.position) - leftTop);
				var offset = new Vector2(o.x, -o.y) / (pixelsPerUnit);
				go.transform.position += VectorUtils.V23(offset);
			}
		}
		
	}
	[Serializable]
	public class SpriteWithOffset {
		public string name;
		public float x;
		public float y;
		public float width;
		public float height;
		private static string pat = "\"(.*)\":{\"x\":([-0-9.]+),\"y\":([-0-9.]+),\"width\":([-0-9.]+),\"height\":([-0-9.]+)}";
		public static SpriteWithOffset[] CreateFromJson(string json) {
			List<SpriteWithOffset> list = new List<SpriteWithOffset>();
			foreach(var s in fromJson(json)) {
				list.Add(s);
			}
			return list.ToArray();
		}
		private static IEnumerable<SpriteWithOffset> fromJson(string json) {
			Regex r = new Regex(pat, RegexOptions.None);
			Match m = r.Match(json);
			while(m.Success) {
				var s = new SpriteWithOffset();
				s.name = m.Groups[1].Value;
				s.x = float.Parse(m.Groups[2].Value);
				s.y = float.Parse(m.Groups[3].Value);
				s.width = float.Parse(m.Groups[4].Value);
				s.height = float.Parse(m.Groups[5].Value);
				yield return s;
				m = m.NextMatch();
			}
		}
		public static int CreateFromJson(string json, SpriteWithOffset[] results) {
			int count = 0;
			foreach(var s in fromJson(json)) {
				results[count] = s;
				count++;
			}
			return count;
		}
	}
}