using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PseudoTools;
public class Swapable : MonoBehaviour {
    public Vector2 Position {
        get {
            return mov.Position;
        }
        set {
            mov.Position = value;
        }
    }
    public BlockMovement mov;
    public MeshRenderer renderer;
    public LightRenderer lightRenderer;
    private static float lightMaskDisappearTime = 0.2f;
    private bool lightMaskVisible;
    private ApCtrl.AlphaData lightMaskAlpha;
    private ApCtrl.AlphaData meshAlpha;
    public void SwapMove(Vector2 dir, float time, float disappearTime) {
        StartCoroutine(swapMoveUpdate(dir, time, disappearTime));
    }
    public bool SelectedUIActive {
        get {
            return lightMaskVisible;
        }
        set {
            if(lightMaskVisible && !value) {
                ApCtrl.DisappearAlpha(lightMaskAlpha, lightMaskDisappearTime);
                

            }
            else if(!lightMaskVisible && value) {
                ApCtrl.AppearAlpha(lightMaskAlpha, lightMaskDisappearTime);
            }
            lightMaskVisible = value;
        }
    }
    private IEnumerator swapMoveUpdate(Vector2 dir, float time, float disappearTime) {
        ApCtrl.DisappearAlpha(meshAlpha, disappearTime);
        yield return new WaitForSecondsRealtime(disappearTime);
        SwapMove(dir, time, (pos) => Position = pos, () => Position, this);
        renderer.enabled = false;
        var oriValue = mov.staticExtrusion;
        mov.staticExtrusion = false;
        Timer.CreateAUnscaledTimer(time, () => {
            renderer.enabled = true;
            mov.staticExtrusion = oriValue;
        }, this);
        yield return new WaitForSecondsRealtime(time);
        ApCtrl.AppearAlpha(meshAlpha, disappearTime);
        
    }
    public static void SwapMove(Vector2 dir, float time, Action<Vector2> setPos, Func<Vector2> getPos, MonoBehaviour mb) {
        Timer timer = Timer.CreateAUnscaledTimer(time, null, mb);
        var originPos = getPos();
        timer.timerUpdateAction += () => {
            setPos(originPos + dir * timer.Value);
        };
    }
    public void Awake()
    {
        lightMaskVisible = true;
        lightMaskAlpha = ApCtrl.CreateAlphaData(ApCtrl.LightRendererAlpha(lightRenderer), this);
        SelectedUIActive = false;
        meshAlpha = ApCtrl.CreateAlphaData(ApCtrl.SpineMeshAlpha(renderer), this);
    }
    public void Start() {
    }
}