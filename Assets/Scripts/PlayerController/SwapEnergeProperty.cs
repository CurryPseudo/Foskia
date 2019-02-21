using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using PseudoTools;
using Spine.Unity;
public class SwapEnergeProperty : MonoBehaviour {
    public AnimationCurve timeDistanceCurve;
    public float disappearTime;
    public float appearTime;
    public AnimationCurve timeRotateSpeedCurve;
    public SkeletonAnimation anim;
    public LightRenderer lightRenderer;
    private Vector2 originPos;
    private ApCtrl.AlphaData meshAp;
    private ApCtrl.AlphaData lightAp;
    public Vector2 Position {
        get {
            return VectorUtils.V32(transform.position);
        }
        set {
            transform.position = VectorUtils.V23(value, transform.position.z);
        }
    }
    
    public void Start() {
        originPos = Position;
    }
    public void EnergeGeted(PlayerController player, Action<Vector2> afterAnim) {
        StartCoroutine(EnergeGetedCorou(player, afterAnim));
    }
    public IEnumerator EnergeGetedCorou(PlayerController player, Action<Vector2> afterAnim) {
        anim.state.TimeScale = 0;
        anim.state.SetAnimation(0, "animation", false);;
        GetComponent<BoxCollider2D>().enabled = false;
        var dir = (Position.x - player.PositionWithoutHalo.x) > 0 ? Vector2.right : Vector2.left;
        var movTimer = Timer.CreateATimer(disappearTime, null, this);
        movTimer.timerUpdateAction += () => {
            float dis = timeDistanceCurve.Evaluate(movTimer.Value);
            Vector2 target = dir.normalized * dis + originPos;
            Position = target;
        };
        var rotTimer = Timer.CreateATimer(disappearTime, null, this);
        rotTimer.timerUpdateAction += () => {
            float rotSpeed = timeRotateSpeedCurve.Evaluate(rotTimer.Value);
            transform.Rotate(Vector3.forward * rotSpeed * Time.deltaTime);
        };
        if(meshAp == null) {
            var mesh = anim.GetComponent<MeshRenderer>();
            meshAp = ApCtrl.CreateAlphaData(ApCtrl.SpineMeshAlpha(mesh), this);
        }
        ApCtrl.DisappearAlpha(meshAp, disappearTime);
        if(lightAp == null) {
            lightAp = ApCtrl.CreateAlphaData(ApCtrl.LightRendererAlpha(lightRenderer), this);
        }
        ApCtrl.DisappearAlpha(lightAp, disappearTime);
        yield return new WaitForSeconds(disappearTime);
        afterAnim(Position);
        yield return new WaitForSeconds(appearTime);
        Position = originPos;
        transform.rotation = Quaternion.identity;
        ApCtrl.AppearAlpha(meshAp, disappearTime);
        ApCtrl.AppearAlpha(lightAp, disappearTime);
        GetComponent<BoxCollider2D>().enabled = true;
        anim.state.TimeScale = 1;
        anim.state.SetAnimation(0, "animation", true);;
    }
}