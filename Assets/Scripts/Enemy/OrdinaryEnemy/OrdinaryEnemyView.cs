using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class OrdinaryEnemyView : MonoBehaviour {
	public OrdinaryEnemyTop top;
	public Color AttackColor;
	private SkeletonAnimation view;
	public void Awake() {
		view = GetComponent<SkeletonAnimation>();
	}
	public void Update() {
		if(top.Velocity.x > 0 ^ view.skeleton.FlipX) {
			view.skeleton.FlipX = !view.skeleton.FlipX;
		}
	}
}
