using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneTransforer : MonoBehaviour {
    public static SceneTransforer main;
    public static SceneTransforer Main {
        get {
            return main;
        }
    }
    public void Awake() {
        DontDestroyOnLoad(gameObject);
        main = this;
    }
    public Func<bool> TransforToPoint(TransforPointIndex index, float blackScreenTime) {
        bool finished = false;
        StartCoroutine(TransforToPointCor(index, blackScreenTime, (b) => finished = b));
        return () => finished;
    }
    private IEnumerator TransforToPointCor(TransforPointIndex index, float blackScreenTime, Action<bool> setFinished) {
        setFinished(false);
        var pos = TransforPoints.Main.GetTransforPoint(index);
        Time.timeScale = 0;
        ColorScreen.Main.SetColor(Color.black, blackScreenTime);
        yield return new WaitForSecondsRealtime(blackScreenTime);
        var ppcs = PlayerPropertyCrossScene.Main;
        SceneManager.LoadScene(index.sceneName);
        yield return null;
        PlayerController.SpawnPos(pos);
        yield return null;
        if(ppcs != null) {
            PlayerPropertyCrossScene.Main = ppcs;
        }
        CameraPos.main.Position = PlayerController.Main.PositionWithEdgeCut;
        setFinished(true);
        ColorScreen.Main.SetColor(new Color(0,0,0,0), blackScreenTime);
        yield return new WaitForSecondsRealtime(blackScreenTime);
        Time.timeScale = 1;
        yield break;
    }
}