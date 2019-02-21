using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using PseudoTools;
public class RedLightController {

    public ClassRef<Vector2> posRef;
    public ApCtrl.AlphaData spriteAlpha;
    public ApCtrl.AlphaData lightAlpha;
    private GameObject go;
    private MonoBehaviour mb;
    private ParticleSystem particleSystem;
    private ParticleSystem.EmissionModule emission;
    private RedLightController(){}
    public static RedLightController Create(PlayerController player, Vector2 _pos) {

        var prefab = player.redLightPrefab;
        var go = GameObject.Instantiate(prefab);
        go.transform.position = VectorUtils.V23(_pos, go.transform.position.z);
        var sr = go.GetComponentInChildren<SpriteRenderer>();
        var redLightTrans = go.transform;
        var rdLtCtrl = new RedLightController();
        rdLtCtrl.posRef = new ClassRef<Vector2>((pos) => redLightTrans.position = VectorUtils.V23(pos, redLightTrans.position.z), () => VectorUtils.V32(redLightTrans.position));
        rdLtCtrl.spriteAlpha = ApCtrl.CreateAlphaData(ApCtrl.SpriteAlpha(sr), player);
        rdLtCtrl.spriteAlpha.convertSpeed = player.redLightAlphaConvertSpeed;
        rdLtCtrl.lightAlpha = ApCtrl.CreateAlphaData(ApCtrl.LightRendererAlpha(go.GetComponentInChildren<LightRenderer>()), player);
        rdLtCtrl.lightAlpha.convertSpeed = player.redLightAlphaConvertSpeed;
        rdLtCtrl.go = go;
        rdLtCtrl.mb = player;
        rdLtCtrl.particleSystem = go.GetComponentInChildren<ParticleSystem>();
        rdLtCtrl.emission = rdLtCtrl.particleSystem.emission;
        rdLtCtrl.EmissionEnable = false;
        return rdLtCtrl;
    }
    public void Destroy() {
        mb.StopCoroutine(spriteAlpha.cor);
        mb.StopCoroutine(lightAlpha.cor);
        GameObject.Destroy(go);
    }
    public void StartCoroutine(IEnumerator i) {
        mb.StartCoroutine(i);
    }
    public IEnumerator MoveToPos(Vector2 pos, float time, AnimationCurve timeDisCurve) {
        var dir = pos - posRef.get();
        var dis = dir.magnitude;
        var originPos = posRef.get();
        float timeCount = 0;
        while(timeCount < 1) {
            yield return null;
            timeCount += Time.unscaledDeltaTime / time;
            timeCount = Mathf.Clamp01(timeCount);
            var normedDis = timeDisCurve.Evaluate(timeCount);
            posRef.set(originPos + dir.normalized * normedDis * dis);
        }
        yield break;
    }
    public IEnumerator MoveDirWithNormalOffset(Vector2 dir, float speed, AnimationCurve normalOffset) {
        var originPos = posRef.get();
        float time = dir.magnitude / speed;
        float timeCount = 0;
        while(timeCount < 1) {
            yield return null;
            timeCount += Time.unscaledDeltaTime / time;
            timeCount = Mathf.Clamp01(timeCount);
            var offset = normalOffset.Evaluate(timeCount);
            var dirNorm = new Vector2(dir.y, -dir.x).normalized; // Clockwise 90 degrees
            var dis = speed * timeCount * time;
            var pos = originPos + dir.normalized * dis + dirNorm * offset;
            posRef.set(pos);
        }
        yield break;
    }
    public bool EmissionEnable {
        get {
            return emission.enabled;
        }
        set {
            emission.enabled = value;
        }
    }
}
public class RedLightPlayerController {
    public PositionDistanceQueue playerPosQueue;
    public SinMove sinMove;
    public AnimationCurve distanceSpeedCurve;
    private float followAlphaBinding;
    private bool follow;
    private RedLightController rlCtrl;

    private RedLightPlayerController(){}
    public static RedLightPlayerController Create(PlayerController player, Vector2 _pos) {

		SinMove sm = new SinMove(player.rdLtSinMovDt);
        sm.data.originDeg = VectorUtils.Do(Vector2.zero, f => UnityEngine.Random.value * 360);
        var rdLtCtrl = new RedLightPlayerController();
        rdLtCtrl.playerPosQueue = player.playerPosDisQueue;
        rdLtCtrl.sinMove = sm;
        rdLtCtrl.distanceSpeedCurve = player.rdLtDisSpdCv;
        rdLtCtrl.rlCtrl = RedLightController.Create(player, _pos);
        player.StartCoroutine(rdLtCtrl.FollowCor());
        return rdLtCtrl;
    }
    public Action<float> GetAlphaBinding() {
        return (f) => {
                followAlphaBinding = f;
            };
    }

    public void Pause() {
        follow = false;
    }
    public IEnumerator SwapCor(PlayerController player, Vector2 target) {
        follow = false;
        var time = player.swapProperty.disappearTime;
        var playerPos = player.PositionWithoutHalo;
        rlCtrl.StartCoroutine(rlCtrl.MoveToPos(player.PositionWithoutHalo, time, player.rdLtToPlayerTimeDisCurve));
        yield return new WaitForSecondsRealtime(time);
        var dir = target - playerPos;
        var speed = player.swapProperty.speed;
        rlCtrl.EmissionEnable = true;
        rlCtrl.StartCoroutine(rlCtrl.MoveDirWithNormalOffset(dir
            , speed
            , player.rdLtSwapNormalDis));
        var swapTime = dir.magnitude / speed;
        ApCtrl.DisappearAlpha(rlCtrl.lightAlpha, swapTime);
        ApCtrl.DisappearAlpha(rlCtrl.spriteAlpha, swapTime);
        yield return new WaitForSecondsRealtime(swapTime);
        rlCtrl.EmissionEnable = false;
        yield return new WaitUntil(() => rlCtrl.lightAlpha.alphaRef.get() == 0);
        yield return new WaitForSecondsRealtime(2);
        rlCtrl.Destroy();
    }
    public IEnumerator FollowCor() {
        Func<Vector2> targetPosGet = () => sinMove.UpdatePos(playerPosQueue.PositionBefore);
        var velocity = Vector2.zero;
        var smtt = new SmoothMoveToTarget(targetPosGet
            , distanceSpeedCurve
            , rlCtrl.posRef.get
            , (v) => velocity = v);
        follow = true;
        yield return null;
        while(true) {
            if(!follow) {
                yield break;
            }
            smtt.Update();
            rlCtrl.spriteAlpha.TargehAlphaNormed = followAlphaBinding;
            rlCtrl.lightAlpha.TargehAlphaNormed = followAlphaBinding;
            rlCtrl.posRef.set(rlCtrl.posRef.get() + velocity * Time.deltaTime);
            if(rlCtrl.spriteAlpha.alphaRef.get() == 0) {
                follow = false;
                rlCtrl.Destroy();
                yield break;
            }
            yield return null;
        }
        
    }
}
public class RedLightEnemyController {
    private Vector2 enemyPos;
    private RedLightController rlCtrl;
    private PlayerController player;
    public static RedLightEnemyController Create(PlayerController player, Vector2 enemeyPos) {
        var rleCtrl = new RedLightEnemyController();
        rleCtrl.enemyPos = enemeyPos;
        var rlCtrl = RedLightController.Create(player, enemeyPos);
        ApCtrl.SetAlphaImmediately(rlCtrl.lightAlpha, 0);
        ApCtrl.SetAlphaImmediately(rlCtrl.spriteAlpha, 0);
        rleCtrl.rlCtrl = rlCtrl;
        rleCtrl.player = player;
        return rleCtrl;
    }
    public IEnumerator SwapCor() {
        var playerPos = player.PositionWithoutHalo;
        float appearTime = player.swapProperty.disappearTime;
        ApCtrl.AppearAlpha(rlCtrl.lightAlpha, appearTime);
        ApCtrl.AppearAlpha(rlCtrl.spriteAlpha, appearTime);
        yield return new WaitForSecondsRealtime(appearTime);
        var dir = playerPos - enemyPos;
        var speed = player.swapProperty.speed;
        rlCtrl.EmissionEnable = true;
        rlCtrl.StartCoroutine(rlCtrl.MoveDirWithNormalOffset(dir
            , speed
            , player.rdLtSwapNormalDis));
        var swapTime = dir.magnitude / speed;
        ApCtrl.DisappearAlpha(rlCtrl.lightAlpha, swapTime);
        ApCtrl.DisappearAlpha(rlCtrl.spriteAlpha, swapTime);
        yield return new WaitForSecondsRealtime(swapTime);
        yield return new WaitForSecondsRealtime(2);
        rlCtrl.Destroy();
    }
}