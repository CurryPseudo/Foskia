using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
public class TextTrans : MonoBehaviour {
    private static TextTrans main;
    public static TextTrans Main {
        get {
            return main;
        }
    }
    public TextTransInstance Center {
        get {
            return center;
        }
    }
    private TextTransInstance center;
    public TextTransInstance Bottom {
        get {
            return bottom;
        }
    }
    private TextTransInstance bottom;

    public void Awake() {
        main = this;
        Func<string, TextTransInstance>  gen = (name) => {
            var t = transform.Find(name);
            var text = t.gameObject.GetComponent<Text>();
            return new TextTransInstance(text, this);
        };
        center = gen("Center");
        bottom = gen("Bottom");
    }
}

public class TextTransInstance {
    public Text text;
    public TextTrans trans;
    private ApCtrl.AlphaData ad;
    public TextTransInstance(Text text, TextTrans trans) {
        this.text = text;
        this.trans = trans;
        ad = ApCtrl.CreateAlphaData(ApCtrl.TextAlpha(text), trans);
    }
    public void FadeOut(float time) {
        ApCtrl.DisappearAlpha(ad, time);
    }
    public void FadeIn(float time) {
        ApCtrl.AppearAlpha(ad, time);
    }
    public void SetText(string content) {
        text.text = content;
    }
}