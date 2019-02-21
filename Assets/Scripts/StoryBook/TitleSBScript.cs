using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TitleSBScript : MonoBehaviour {
    public void Start() {
        source = GameObject.Find("Bgm").GetComponent<AudioSource>();
        sourcePriorityAd = ApCtrl.CreateAlphaData(ApCtrl.AudioVolumeAlpha(source), this);
        processCor = StartCoroutine(process());
    } 
    Coroutine processCor;
    AudioSource source = null;
    ApCtrl.AlphaData sourcePriorityAd = null;
    TextTransInstance btmText = null;
    TextTransInstance ctText = null;
    ImageTrans image = null;
    private IEnumerator detectSkip() {
        yield return new WaitUntil(() => Input.GetButtonDown("Jump"));
        StartCoroutine(final());
    }
    private IEnumerator process() {
        btmText = TextTrans.Main.Bottom;
        ctText = TextTrans.Main.Center;
        image = ImageTrans.Main;
        btmText.FadeOut(0);
        ctText.FadeOut(0);
        image.FadeOut(0);
        yield return null;
        yield return new WaitForSeconds(2);
        var skip = StartCoroutine(detectSkip());
        yield return normImgProcess("begin1", "少年来自光之一族");
        yield return normImgProcess("begin2", "那是他第一次见到黑暗的公主");
        yield return normCtTxProcess("\"她可真美。\"");
        yield return normImgProcess("begin3", "他对公主一见钟情，对方却转头跑开");
        yield return normCtTxProcess("\"你要去哪？\"");
        yield return normImgProcess("begin4", "少年一路追逐，逐渐走近黑暗的洞穴");
        yield return normCtTxProcess("\"哦不。\"");
        yield return normImgProcess("begin5", "少年坠入洞中，而公主仿佛就在洞穴的深处");
        StopCoroutine(skip);
        StartCoroutine(final());
        yield break;
    }
    private IEnumerator final() {
        btmText.FadeOut(1);
        ctText.FadeOut(1);
        image.FadeOut(1);
        ApCtrl.DisappearAlpha(sourcePriorityAd, 1);
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("PrepareMap");
        yield return null;
    }
    private IEnumerator normImgProcess(string imageName, string btmContent) {
        image.SetImage(imageName);
        image.FadeIn(1);
        btmText.SetText(btmContent);
        yield return new WaitForSeconds(0.5f);
        btmText.FadeIn(1);
        yield return new WaitForSeconds(1);
        yield return new WaitForSeconds(1);
        image.FadeOut(1);
        yield return new WaitForSeconds(0.5f);
        btmText.FadeOut(1);
        yield return new WaitForSeconds(1);
    }
    private IEnumerator normCtTxProcess(string content) {
        ctText.SetText(content);
        ctText.FadeIn(1);
        yield return new WaitForSeconds(1);
        yield return new WaitForSeconds(1);
        ctText.FadeOut(1);
        yield return new WaitForSeconds(2);
    }
}