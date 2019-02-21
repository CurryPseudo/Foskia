using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using PseudoTools;
public class Lamps : MonoBehaviour {
    public SimplePendulemMove mov;
    public LightRenderer lr;
    public float healthSpeed = 1;
    private TransforPointIndex bornIndex;
    private AudioSource sound;
    public Vector2 Position {
        get {
            return VectorUtils.V32(lr.transform.position);
        }
        set {
            lr.transform.position = VectorUtils.V23(value, lr.transform.position.z);
        }
    }
    private Vector2 originPos;
    public void Awake() {
        TransforPoints.Main.go = transform.Find("BornPlace").gameObject;
        bornIndex = TransforPoints.Main.AddList();
        originPos = Position;
        sound = GetComponentInChildren<AudioSource>();
        StartCoroutine(playSound());
    }
    private IEnumerator playSound() {
        yield return null;
        while(true) {
            SoundManager.PlaySound(sound, "Lamp");
            yield return new WaitForSeconds(mov.cycle / 2);
            SoundManager.PlaySound(sound, "Lamp2");
            yield return new WaitForSeconds(mov.cycle / 2);
        }
    }
    public void Update() {
        mov.time += Time.deltaTime;
        Position = mov.Position + originPos;

        if(InRadius) {
            PlayerProperty.Main.PlayerHealth += healthSpeed * Time.deltaTime;
            PlayerProperty.Main.born = bornIndex;
        }
    }
    public bool InRadius {
        get {
            if(PlayerController.Main == null) return false;
            return (PlayerController.Main.Position - Position).magnitude < lr.viewMesh.radius;
        }
    }
}