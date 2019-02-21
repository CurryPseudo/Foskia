using System;
using System.Collections.Generic;
using UnityEngine;
using PseudoTools;

[Serializable]
public class SwapCharges {
    public class BindingFloat {
        public float value;
        public RedLightPlayerController redLight;
        public void ApplyBind() {
            if(redLight == null) return;
            redLight.GetAlphaBinding()(value);
        }
    }
    private BindingFloat[] times = new BindingFloat[2];       
    private int chargesCount = 0;
    public int ChargesCount {
        get {
            return chargesCount;
        }
    }
    public float swapChargeTime;
    private PlayerController player;
    public int MaxSwapChargeCount {
        get {
            return times.Length;
        }
    }
    public bool SwapValid {
        get {
            Func<IEnumerable<float>, bool> NotZeroOr = (fs) => {
                foreach(var f in fs) {
                    if(f != 0) return true;
                }
                return false;
            };
            return NotZeroOr(Algorithm.Map(times, t => t.value));
        }
    }
    public IEnumerable<float> Times {
        get {
            foreach(var t in times) {
                if(t.value != 0) {
                    yield return t.value;
                }
            }
        }
    }
    public bool Fulled {
        get {
            foreach(var t in times) {
                if(t.value != 1) {
                    return false;
                }
            }
            return true;
        }
    }
    public void Init(PlayerController player) {
        this.player = player;
        for(int i = 0; i < times.Length; i++) {
            times[i] = new BindingFloat();
            times[i].redLight = null;
        }
        player.StartCoroutine(CoroutineUtils.FixedUpdateAction(update));
        
    }
    public void update() {
        for(int i = 0; i < times.Length; i++) {
            times[i].value -= Time.fixedDeltaTime / swapChargeTime;
            if(times[i].value < 0) times[i].value = 0;
        }
        int w = 0;
        int r = 0;
        while(r < times.Length) {
            times[r].ApplyBind();
            if(times[r].value != 0) {
                times[w] = times[r];
                w++;
            }
            r++;
        }
        chargesCount = w;
    }
    public void FullCharge(IEnumerable<RedLightPlayerController> bindings) {
        for(int i = 0; i < times.Length; i++) {
            if(times[i].value != 0) {
                times[i].value = 0;
                times[i].ApplyBind();
            }
            else {
                break;
            }
        }
        int index = 0;
        foreach(var binding in bindings) {
            times[index] = new BindingFloat();
            times[index].value = 1;
            times[index].redLight = binding;
            times[index].ApplyBind();
            index++;
        }
    }
    public BindingFloat GetSwapCharge() {
        for(int i = times.Length - 1; i >= 0; i--) {
            if(times[i].value != 0) {
                return times[i];
            }
        }
        return null;
    }
    public void Replace(SwapCharges last) {
        for(int i = 0; i < times.Length; i++) {
            times[i] = last.times[i];
        }
        chargesCount = last.ChargesCount;
    }
    public void ReBind(IEnumerable<RedLightPlayerController> bindings) {
        int index = 0;
        foreach(var bind in bindings) {
            Debug.Assert(index < ChargesCount);
            times[index].redLight = bind;
            index++;
        }
    }
}