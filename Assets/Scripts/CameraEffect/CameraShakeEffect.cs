using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PseudoTools;

[System.Serializable]
public class CameraShakeEffect : CameraPosEffect{
	public Vector2 shakePower;
	public Vector2 shakeSpeed;
	private Vector2 shakeValue;
	public override Vector2 UpdatePos(Vector2 pos) {
		shakeValue += shakeSpeed * Time.deltaTime;
		var shakeNoise = VectorUtils.Do(shakeValue, (f) => Mathf.PerlinNoise(f, 0));
		shakeNoise = VectorUtils.Do(shakeNoise, (f) => 2 * f - 1);
		shakeNoise = VectorUtils.Multiply(shakeNoise, shakePower);
		return pos + shakeNoise;
	}
	public override void Register(MonoBehaviour mb) {
		base.Register(mb);
		shakeValue = VectorUtils.Do(shakeSpeed, (f) => Random.value * f);
	}
	public CameraShakeEffect(CameraShakeEffect cse) : base(cse) {
		shakePower = cse.shakePower;
		shakeSpeed = cse.shakeSpeed;
	}
	public static void ShakeTime(CameraShakeEffect cse, float time, MonoBehaviour mb) {
		var ncse = new CameraShakeEffect(cse);
		ncse.Register(mb, time);
	}

}
