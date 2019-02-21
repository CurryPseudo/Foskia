using System.Collections.Generic;
using PseudoTools;
using System;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
public class LightValueNormalizer : SerializedMonoBehaviour {
    [SerializeField]
    private float normedValue = 1;
    public float Value {
        get {
            return normedValue;
        }
        set {
            normedValue = value;
            normedValue = Mathf.Clamp01(normedValue);
            foreach(var setter in setters) {
                setter.SetValue(normedValue);
            }
        }
    }
    [OdinSerialize]
    [InlineProperty]
    public RendererNormedValueSetterProperty radiusProperty;
    [OdinSerialize]
    [InlineProperty]
    public RendererNormedValueSetterProperty brightnessProperty;
    private List<RendererNormedValueSetter> setters = new List<RendererNormedValueSetter>();
    public void Awake() {
        setters.Add(new RendererNormedValueSetter(radiusProperty, r => r.viewMesh.radius, (r, f) => r.viewMesh.radius = f));
        setters.Add(new RendererNormedValueSetter(brightnessProperty , r => r.brightness, (r, f) => r.brightness = f));
    }
    [Serializable]
    public class MinMax {
        public float min;
        public float max;
        public MinMax(float min, float max) {
            this.min = min;
            this.max = max;
        }
    }
    public class RendererNormedValueSetterProperty {
        public List<LightRenderer> renderers = new List<LightRenderer>();

        public Dictionary<LightRenderer, MinMax> minmaxs = new Dictionary<LightRenderer, MinMax>();
    }
    public class RendererNormedValueSetter {
        public RendererNormedValueSetterProperty property;
        private Dictionary<LightRenderer, float> originValue = new Dictionary<LightRenderer, float>();
        private Func<LightRenderer, float> valueGetter;
        private Action<LightRenderer, float> valueSetter;
        public RendererNormedValueSetter(RendererNormedValueSetterProperty property, Func<LightRenderer, float> valueGetter, Action<LightRenderer, float> valueSetter) {
            this.property = property;
            this.valueGetter = valueGetter;
            this.valueSetter = valueSetter;
            foreach(var renderer in property.renderers) {
                originValue[renderer] = valueGetter(renderer);
            }
        }
        public void SetValue(float normedValue) {
            foreach(var renderer in property.renderers) {
                var value = normedValue;
                if(property.minmaxs.ContainsKey(renderer)) {
                    value = Mathf.Lerp(property.minmaxs[renderer].min, property.minmaxs[renderer].max, normedValue);
                }
                valueSetter(renderer, originValue[renderer] * value);
            }
        }
    }
    
}