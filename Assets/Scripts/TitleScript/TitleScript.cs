using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScript : MonoBehaviour {

	public Image pressKey;
	public AnimationCurve pressAfterSize;
	public AnimationCurve pressAfterAlpha;
	public float pressTime;
	private Coroutine pressKeyCor;
	public AudioSource bgm;
	public AnimationCurve pressKeyAlphaCurve;
	private ApCtrl.AlphaData bgmAd;
	// Use this for initialization
	void Start () {
		bgmAd = ApCtrl.CreateAlphaData(ApCtrl.AudioVolumeAlpha(bgm), this);
		StartCoroutine(mainProcess());
		pressKeyCor = StartCoroutine(pressKeyAnim());
	}
	private IEnumerator pressKeyAnim() {
		float timeCount = 0;
		while(true) {
			var col = pressKey.color;
			col.a = pressKeyAlphaCurve.Evaluate(timeCount);
			pressKey.color = col;
			yield return null;
			timeCount += Time.deltaTime;
		}
	}
	private IEnumerator pressAfterAnim() {
		float timeCount = 0;
		var scale = pressKey.transform.localScale;
		while(true) {
			var col = pressKey.color;
			col.a = pressAfterAlpha.Evaluate(timeCount);
			pressKey.color = col;
			float c = pressAfterSize.Evaluate(timeCount);
			pressKey.transform.localScale = scale * c;
			yield return null;
			timeCount += Time.deltaTime;
		}
	}
	private IEnumerator mainProcess() {

		ColorScreen.Main.SetColor(Color.black, 0);
		yield return null;
		ColorScreen.Main.SetColor(Color.clear, 2);
		yield return new WaitForSeconds(2);
		bgm.Play();
		yield return new WaitForSeconds(1);
		yield return new WaitUntil(() => Input.anyKey);
		SoundManager.PlaySound(bgm, "TitleButton");
		StopCoroutine(pressKeyCor);
		StartCoroutine(pressAfterAnim());
		yield return new WaitForSeconds(pressTime);

		ColorScreen.Main.SetColor(Color.black, 2);
		ApCtrl.DisappearAlpha(bgmAd, 2);
		yield return new WaitForSeconds(2);
		SceneManager.LoadScene("TitleAnim");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
