using System;
using System.Collections;
using System.Collections.Generic;
using PseudoTools;
using UnityEngine;
public class CameraSprite {
    private static string spriteLayer = "FinalTex";
    public SpriteRenderer sr;
    private Camera camera;
    private Vector2 componentScaled;
    public Transform transform {
        get {
            return sr.transform;
        }
    }
    public CameraSprite(string goName, string spritePath) : this(goName, spritePath, Vector2.one) {
        
    }
    public CameraSprite(string goName, string spritePath, Vector2 componentScaled) {
        camera = Camera.main;
        var go = new GameObject(goName);
        go.transform.SetParent(camera.transform, false);
        go.layer = LayerMask.NameToLayer(spriteLayer);
        sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>(spritePath);
        var layers = SortingLayer.layers;
        sr.sortingLayerName = layers[layers.Length - 1].name;
        sr.sortingOrder = 999;
        this.componentScaled = componentScaled;
        adjustTransform();
    }
    public void adjustTransform() {
        sr.transform.position = camera.transform.position;
        sr.transform.position += Vector3.forward; // could rendered by camera
        var b = sr.bounds;
        Vector2 size = VectorUtils.V32(b.size);
        Vector2 camHalfSize = new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize);
        Vector2 camSize = camHalfSize * 2;
        Vector2 deltaScale = VectorUtils.Division(camSize, size);
        if(componentScaled != Vector2.one) {
            if(componentScaled == Vector2.zero) {
                deltaScale = Vector2.zero;
            }
            else {
                deltaScale = componentScaled.x == 0 ? Vector2.one * deltaScale.y : Vector2.one * deltaScale.x;
            }
        }
        Vector3 scale = sr.transform.localScale;
        scale = VectorUtils.Multiply(scale, VectorUtils.V23(deltaScale, 1));
        sr.transform.localScale = scale;
    }
    public void Destroy() {
        GameObject.Destroy(sr.gameObject);
    }
}