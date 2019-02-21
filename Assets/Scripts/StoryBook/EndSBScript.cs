
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EndSBScript : MonoBehaviour {
    public void Start() {
        source = GameObject.Find("Bgm").GetComponent<AudioSource>();
        sourcePriorityAd = ApCtrl.CreateAlphaData(ApCtrl.AudioVolumeAlpha(source), this);
        StartCoroutine(process());
    } 
    AudioSource source = null;
    ApCtrl.AlphaData sourcePriorityAd = null;
    TextTransInstance btmText = null;
    TextTransInstance ctText = null;
    ImageTrans image = null;
    private IEnumerator process() {
        btmText = TextTrans.Main.Bottom;
        ctText = TextTrans.Main.Center;
        image = ImageTrans.Main;
        btmText.FadeOut(0);
        ctText.FadeOut(0);
        image.FadeOut(0);
        yield return null;
        yield return new WaitForSeconds(2);
        yield return normCtTxProcess("\"他来了。\"");
        yield return normImgProcess("end1", "少年终于来到了洞底，快要耗尽最后的气力");
        yield return normImgProcess("end2", "再次见到少年的公主泣不成声");
        yield return normCtTxProcess("\"我们没办法在一起。\"");
        yield return normImgProcess("end3", "公主向少年走来", "可每靠近一步，便痛苦一分");
        yield return normCtTxProcess("\"光明与黑暗无法结合。\"");
        yield return normCtTxProcess("光越来越黯淡，少年也越来越虚弱");
        yield return normImgProcess("end4", "最终，熄灭的少年倒在公主怀里");
        yield return normCtTxProcess("\"现在我们能真正在一起了。\"");
        yield return new WaitForSeconds(1);
        image.SetImage("endFinal");
        image.FadeIn(2);
        yield break;
    }
    private IEnumerator normImgProcess(string imageName, params string[] btmContents) {
        image.SetImage(imageName);
        image.FadeIn(2);
        yield return new WaitForSeconds(0.5f);
        for(int i = 0; i < btmContents.Length - 1; i++) {
            btmText.SetText(btmContents[i]);
            btmText.FadeIn(1);
            yield return new WaitForSeconds(1);
            yield return new WaitForSeconds(1);
            btmText.FadeOut(1);
            yield return new WaitForSeconds(1);
        }
        btmText.SetText(btmContents[btmContents.Length - 1]);
        btmText.FadeIn(1);
        yield return new WaitForSeconds(1);
        yield return new WaitForSeconds(1);
        image.FadeOut(1);
        yield return new WaitForSeconds(1f);
        btmText.FadeOut(1);
        yield return new WaitForSeconds(1);
    }
    private IEnumerator normCtTxProcess(string content) {
        ctText.SetText(content);
        ctText.FadeIn(1);
        yield return new WaitForSeconds(1);
        yield return new WaitForSeconds(1);
        ctText.FadeOut(1);
        yield return new WaitForSeconds(1.5f);
    }
}