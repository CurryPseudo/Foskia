using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BTScript;
using PseudoTools;
using Spine.Unity;

public class BigEnemy : GroundEnemy {
    public SectorViewRange attackRange;
    public SkeletonAnimation anim;
    public CameraShakeEffect shake;
    public void Awake () {

    }
    public void Start() {

    }
    public override void _Update() {
        base._Update();
        anim.skeleton.FlipX = faceDirX > 0;
        tree.SetCondBool("playerInAttackRange", ContainPlayerInRange(attackRange));
        tree.SetCondBool("couldWalkThrough", CouldWalkThrough);
    }
    protected override void RegisterNode(NodeRegistry nr) {
        base.RegisterNode(nr);
        nr.AddAction<PlayJumpNode>("playJump");
        nr.AddAction<JumpUntilFallToGroundNode>("jumpUntilFallToGround");
        nr.AddAction<ScreenShakeNode>("screenShake");
    }
    public float JumpTime {
        get {
            return jumpVerticalSpeed * 2 / gravityAcc;
        }
    }
    public class PlayJumpNode : ControllerNode<BigEnemy>
    {
        float jumpTimeCount = -1;
        public override ExecState Evaluate()
        {
            var anim = controller.anim;
            if(jumpTimeCount == -1) {
                SpineController.SetAnimate(anim, "jump", false, false, 1 / controller.JumpTime);
                jumpTimeCount = 0;
                return ExecState.Running;
            }
            else if(jumpTimeCount < 1) {
                if(anim.AnimationName != "jump") {
                    return ExecState.Fail;
                }
                jumpTimeCount += Time.deltaTime / controller.JumpTime;
                return ExecState.Running;
            }
            else {
                return ExecState.Success;
            }
            
        }
        public override void Reset() {
            jumpTimeCount = -1;
        }
    }
    public class JumpUntilFallToGroundNode : ControllerNode<BigEnemy>
    {
        public float timeCount = -1;
		public override ExecState Evaluate() {
            if(timeCount == -1) {
                controller.XSpeed = controller.jumpHorizontalSpeed;
                controller.YSpeed = controller.jumpVerticalSpeed;
                controller.mov.StartCoroutine(keepSetXSpeed());
                timeCount = 0;
                return ExecState.Running;
            }
            else if(!controller.OnGround && timeCount < 0.5f) {
                timeCount += Time.deltaTime;
                controller.YSpeed -= controller.gravityAcc * Time.deltaTime;
                controller.YSpeed = Math.Max(-controller.maxSpeedY, controller.YSpeed);
                return ExecState.Running;
            }
            else {
                return ExecState.Success;
            }
		}
		private IEnumerator keepSetXSpeed() {
			Coroutine c = controller.StartCoroutine(xSpeed());
			yield return new WaitForSeconds(0.5f);
			controller.StopCoroutine(c);
		}
		private IEnumerator xSpeed() {
			while(true) {
				controller.mov.Velocity = VectorUtils.ReplaceComponent(controller.mov.Velocity, VectorUtils.xComponent2, controller.jumpHorizontalSpeed * controller.faceDirX);
				yield return null;
			}
			
		}
        public override void Reset() {
            timeCount = -1;
        }
    }
    public class ScreenShakeNode : ControllerNode<BigEnemy> {
        public override ExecState Evaluate() {
            CameraShakeEffect.ShakeTime(controller.shake, 0.5f, controller);
            return ExecState.Success;
        }
    }
}