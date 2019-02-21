using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PseudoTools;
[RequireComponent(typeof(Camera))]
public class CameraPos : MonoBehaviour {
    public static CameraPos main {
        get {
            return GetCameraPos(Camera.main);
        }
    }
    public static CameraPos GetCameraPos(Camera camera) {
        var pos = camera.GetComponent<CameraPos>(); 
        if(pos == null) {
            pos = camera.gameObject.AddComponent<CameraPos>();
        }
        return pos;
    }
    public Vector2 Position {
        get {
            return VectorUtils.V32(transform.position);
        }
        set {
            transform.position = VectorUtils.V23(value, transform.position.z);
        }
    }
    [HideInInspector]
    [NonSerialized]
    public Camera camera = null;
    private LinkedList<CameraPosEffect> effects = new LinkedList<CameraPosEffect>();
    private Dictionary<CameraPosEffect, LinkedListNode<CameraPosEffect>> effectToNode = new Dictionary<CameraPosEffect, LinkedListNode<CameraPosEffect>>();
    public void Awake () {
        camera = GetComponent<Camera>();
        Debug.Assert(camera != null);
    }
    public void RegisterEffect(CameraPosEffect effect) {
        var node = effects.First;
        while(node != null) {
            if(node.Value.priority > effect.priority) {
                effects.AddBefore(node, effect);
                return;
            }
            node = node.Next;
        }
        effectToNode.Add(effect, effects.AddLast(effect));
    }
    public void DisRegisterEffect(CameraPosEffect effect) {
        var node = effectToNode[effect];
        effectToNode.Remove(effect);
        effects.Remove(node);
    }
    public void Update() {
        var pos = Position;
        foreach(var effect in effects) {
            pos = effect.UpdatePos(pos);
        }
        Position = pos;
    }
}