using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SoundManager {
    public static SoundManager main;
    public static SoundManager Main {
        get {
            if(main == null) {
                main = new SoundManager();
            }
            return main;
        }
    }
    public AudioClip this[string name] {
        get {
            return Resources.Load<AudioClip>("Sounds/" + name);
        }
    }
    public static void PlaySound(AudioSource source, string name) {
        source.PlayOneShot(Main[name]);
    }
    public static void PlaySound(AudioSource source, string name, float volume) {
        source.PlayOneShot(Main[name], volume);
    }
}