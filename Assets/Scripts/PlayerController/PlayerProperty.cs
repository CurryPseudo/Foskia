using UnityEngine;
using PseudoTools;
using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(PlayerController), typeof(LightValueNormalizer), typeof(CircleViewRange))]
[ReceiveEvent("PlayerSwapEnter")]
[ReceiveEvent("PlayerSwapExit")]
public class PlayerProperty : ObserverMonoBehaviour {
    private static PlayerProperty main;
    public static PlayerProperty Main {
        get {
            return main;
        }
    }
    private LightValueNormalizer lightNorm;
    private float playerHealth = 1;
    public float decreaseTime = 300;
    public AnimationCurve healthLightValueCurve;
    public BoxCollisionCollect damageDetect;
    [LabelText("生命值被怪物侦测范围曲线")]
    public AnimationCurve healthViewValueCurve;
    private CircleViewRange circleView;
    private float originCircleRadius;
    [LabelText("受伤速率")]
    [ValidateInput("validDamageVelocity")]
    public float damageSpeed;
    public TransforPointIndex born;
    public float deadBlackScreenTime = 0.5f;
    private bool damageValid = true;
    [Header("ToEndStory")]
    public BoxCollisionCollect toEndStoryDetect;
    private bool endStoryDetectValid = true;
    public AnimationCurve toEndStoryCameraMove;
    public bool validDamageVelocity(float value) {
        return value != 0;
    }
    [OnInspectorGUI]
    public float PlayerHealth {
        get {
            return playerHealth;
            
        }
        set {
            bool dead = value < 0 && playerHealth > 0;
            playerHealth = Mathf.Clamp01(value);
            lightNorm.Value = healthLightValueCurve.Evaluate(playerHealth);
            circleView.radius = originCircleRadius * healthViewValueCurve.Evaluate(playerHealth);
            if(dead) {
                SceneTransforer.Main.StartCoroutine(playerDead());
            }
        }
    }
    private PlayerController controller;
    private static IEnumerator playerDead() {
        Func<PlayerController> c = () => PlayerController.Main;
        Func<PlayerProperty> p = () => PlayerProperty.Main;
        yield return new WaitUntil(()=> c().currentStateName == "Idle");
        var deadState = new PlayerController.DeadState();
        c().ChangeState<PlayerController.DeadState>(deadState);
        yield return new WaitUntil(() => deadState.DeadAnimFinished);
        var getFinished = SceneTransforer.Main.TransforToPoint(p().born, p().deadBlackScreenTime);
        yield return new WaitUntil(getFinished);
        PlayerProperty.Main.PlayerHealth = 1;
        Time.timeScale = 1;
        yield break;
    }
    private void Awake() {
        main = this;
        lightNorm = GetComponent<LightValueNormalizer>();
        controller = GetComponent<PlayerController>();
        circleView = GetComponent<CircleViewRange>();
        originCircleRadius = circleView.radius;
        //bornProcess();
    }
    private void Update() {
        if(PlayerHealth > 0) {
            PlayerHealth -= 1 / decreaseTime * Time.deltaTime;
        }
    }
    private void FixedUpdate()
    {   
        if(PlayerHealth > 0) {
            DamageDetect();
        }
        ToEndStoryDetect();
    }
    public void ToEndStoryDetect() {
        if(!endStoryDetectValid) return;
        if(toEndStoryDetect.GetMinDisDetects<ToEndStoryScene>() != null) {
            endStoryDetectValid = false;
            SceneTransforer.Main.StartCoroutine(ToEndStoryCor());
        }
    }
    public IEnumerator ToEndStoryCor() {
        yield return new WaitUntil(() => controller.currentStateName == "Idle");
        yield return new WaitForSeconds(0.5f);
        var loopBgm = GameObject.Find("LoopBgm").GetComponent<AudioSource>();
        var ad = ApCtrl.CreateAlphaData(ApCtrl.AudioVolumeAlpha(loopBgm), this);
        ApCtrl.DisappearAlpha(ad, 2);
        controller.enabled = false;
        controller.Velocity = Vector2.zero;
        var bodyView = controller.GetComponentInChildren<BodyView>();
        bodyView.enabled = false;
        bodyView.SetAnimate("stand", true);
        controller.GetComponentInChildren<SkeletonAnimation>().skeleton.FlipX = false;
        ColorScreen.Main.SetColor(Color.black, 2);
        StartCoroutine(toEndStoryCameraMov());
        yield return new WaitForSecondsRealtime(2);
        SceneManager.activeSceneChanged += destroySomething;
        SceneManager.LoadScene("EndAnim");

    }
    private IEnumerator toEndStoryCameraMov() {
        float timeCount = 0;
        while(true) {
            Camera.main.orthographicSize = toEndStoryCameraMove.Evaluate(timeCount);
            yield return null;
            timeCount += Time.deltaTime;
        }
    }
    private void destroySomething(Scene s1, Scene s2) {

        Destroy(GameObject.Find("LoopBgm"));
        Destroy(GameObject.Find("LightSystem"));
        Destroy(CameraPos.main.gameObject);
        SceneManager.activeSceneChanged -= destroySomething;
    }
    public void DamageDetect() {
        if(!damageValid) return;
        float damage = 0;
        bool damageFromRight = true;
        Func<bool> damageHappend = () => {
            DamageProperty damageProperty;
            Vector2 dir;
            damageDetect.GetMinDisDetects<DamageProperty>(out damageProperty, out dir);
            if(damageProperty == null) return false;
            damageFromRight = dir.x > 0;
            damage = damageProperty.damageValue;
            return true;
        };
        if(damageHappend() && controller.HurtInvoke(damageFromRight)) {
            float time = damage / damageSpeed;
            var timer = Timer.CreateAFixedTimer(time, null, this);
            timer.timerUpdateAction += () => {
                PlayerHealth -= damageSpeed * Time.fixedDeltaTime;
            };
        }
    }
    public void ReceivePlayerSwapEnter(PlayerController player, float time) {
        damageValid = false;
    }
    public void ReceivePlayerSwapExit(PlayerController player, float time) {
        damageValid = true;
    }

}