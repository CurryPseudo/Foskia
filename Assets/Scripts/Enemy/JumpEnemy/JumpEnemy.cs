using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BTScript;
using PseudoTools;
using Spine.Unity;

public class JumpEnemy : GroundEnemy
{
	public CircleViewRange attackRange;
	public SkeletonAnimation anim;
	public void Awake() {
	}
	public void Start() {
	}
	public override void _Update() {
		base._Update();
		anim.skeleton.FlipX = faceDirX > 0;
		tree.SetCondBool("playerInAttackRange", ContainPlayerInRange(attackRange)); 
	}
}
