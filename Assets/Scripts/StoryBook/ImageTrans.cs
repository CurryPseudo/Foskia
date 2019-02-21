using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
public class ImageTrans : MonoBehaviour {
    private static ImageTrans main;
    public static ImageTrans Main {
        get {
            return main;
        }
    }
    private static string animsPath = "Anims/";
    public IEnumerable<Sprite> AnimSprites {
        get {
            var sprites = Resources.LoadAll<Sprite>(animsPath);
            return sprites;
        }
    }
    private Dictionary<string, Sprite> map = new Dictionary<string, Sprite>();
    private Image image;
    private Image black;
    private ApCtrl.AlphaData ad;
    
    public void Awake() {
        main = this;
        image = transform.Find("Image").gameObject.GetComponent<Image>();
        black = transform.Find("Black").gameObject.GetComponent<Image>();
        foreach(var s in AnimSprites) {
            map.Add(s.name, s);
        }
        ad = ApCtrl.CreateAlphaData(ApCtrl.ImageAlpha(black), this);
    }  
    public void FadeOut(float time) {
        ApCtrl.AppearAlpha(ad, time);
    }
    public void FadeIn(float time) {
        ApCtrl.DisappearAlpha(ad, time);
    }
    public void SetImage(string name) {
        image.sprite = map[name];
    }

}