using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using PseudoTools;
public class PrepareSceneJob : MonoBehaviour{
    private static string gamepadPath = "CtrlUI/gamepad";
    private static string keyboardPath = "CtrlUI/keyboard";
    public TransforPointIndex originBorn;
    public AnimationCurve ctrlPop;
    public float ctrlPopTime = 1.5f;
    public void Start() {
        SceneTransforer.Main.StartCoroutine(process(originBorn, ctrlPop, ctrlPopTime));
    }
    private static IEnumerator process(TransforPointIndex originBorn, AnimationCurve pop, float popTime) {
        ColorScreen.Main.SetColor(Color.black, 0);
        var getFinished = SceneTransforer.Main.TransforToPoint(originBorn, 1);
        yield return new WaitUntil(getFinished);
        PlayerProperty.Main.born = originBorn;
        yield return new WaitForSeconds(1);
        Func<bool> xoCtrlConnected = () => {
            var names = Input.GetJoystickNames();
            return names.Length != 0 && names[0] != "";
        };
        var path = xoCtrlConnected() ? gamepadPath : keyboardPath;
        CameraSprite ctrlSprite = new CameraSprite("CtrlPadUI", path, Vector2.right);
        var originScale = ctrlSprite.transform.localScale;
        Func<float> getScale = () => ctrlSprite.transform.localScale.x / originScale.x;
        Action<float> setScale = (f) => ctrlSprite.transform.localScale = originScale * f;
        ClassRef<float> scaleRef = new ClassRef<float>(setScale, getScale);
        Time.timeScale = 0;
        scaleRef.set(0);
        float timeCount = 0;
        while(timeCount < 1) {
            yield return null;
            timeCount += Time.unscaledDeltaTime / popTime;
            timeCount = Mathf.Clamp01(timeCount);
            scaleRef.set(pop.Evaluate(timeCount));
        }
        yield return new WaitUntil(() => Input.anyKey);
        var s = getScale();
        timeCount = 0;
        while(timeCount < 1) {
            yield return null;
            timeCount += Time.unscaledDeltaTime / 0.3f;
            timeCount = Mathf.Clamp01(timeCount);
            scaleRef.set(Mathf.Lerp(s, 0, timeCount));
        }
        ctrlSprite.Destroy();
        Time.timeScale = 1;
        
    }
}