using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PseudoTools;
public class ColorScreen : MonoBehaviour {
    private static string colorSpritePath = "Square";
    private bool inited;
    private Camera camera;
    private Color targetColor = new Color(0,0,0,0);
    private float timeCount = 0;
    private float time = 0;
    private Color originColor = new Color(0,0,0,0);
    private CameraSprite colorSprite;
    
    public static ColorScreen Main {
        get {
            var main = GoUtils.GetOrAddComponent<ColorScreen>(Camera.main.gameObject);
            if(!main.inited) {
                main.Init();
            }
            return main;
        }
    }
    public void Init() {
        colorSprite = new CameraSprite("ColorScreen", colorSpritePath);
        inited = true;
        colorSprite.sr.color = targetColor;
        
    }
    public void Update() {
        if(time == 0) {
            colorSprite.sr.color = targetColor;
        }
        else {
            timeCount += Time.unscaledDeltaTime / time;
            timeCount = Mathf.Clamp01(timeCount);
            colorSprite.sr.color = Color.Lerp(originColor, targetColor, Mathf.Pow(timeCount, 2.2f));
        }
        colorSprite.adjustTransform();
    }
    public void SetColor(Color color, float duration) {
        time = duration;
        timeCount = 0;
        targetColor = color;
        originColor = colorSprite.sr.color;
    }
}