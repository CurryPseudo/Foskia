using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BTScript;
using PseudoTools;
using Spine.Unity;
public abstract class GroundEnemy : MonoBehaviour, NodeRegistryProvider {

    int NodeRegistryProvider.priority
    {
        get
        {
			return 0;
        }
    }
	public CircleViewRange view;
	public BlockMovement mov;
	public float horizontalSpeed = 1;
	public float gravityAcc = 2;
	public float maxSpeedY = 2;
	public float jumpVerticalSpeed = 9;
	public float jumpHorizontalSpeed = 6;
	public int faceDirX = 1;
	public BehaviourTree tree;
	protected PlayerController player;
	public void OnEnable() {
		StartCoroutine(ActionUpdate());
	}
	protected bool CouldWalkThrough {
		get {
			BoxCollision box = mov.blockBoxCollision;
			Rect rect = box.BoundRect;
			bool forwardGound = box.CheckMoveCollision(Vector2.down * 0.1f, mov.blockLayer, box.Center + Vector2.right * faceDirX * rect.size.x) != null;
			bool forwardWall = box.CheckMoveCollision(Vector2.right * 0.2f * faceDirX, mov.blockLayer) != null;
			return forwardGound && !forwardWall;
			//return !forwardWall;
		}
	}
	protected bool OnGround {
		get {
			return mov.blockBoxCollision.CheckMoveCollision(Vector2.down * 0.1f, mov.blockLayer) != null;
		}
	}
	public float XSpeed {
		get {
			return mov.Velocity.x;
		}
		set {
			mov.Velocity = VectorUtils.ReplaceComponent(mov.Velocity, VectorUtils.xComponent2, faceDirX * value);
		}
	}
	public float YSpeed {
		get {
			return mov.Velocity.y;
		}
		set {
			mov.Velocity = VectorUtils.ReplaceComponent(mov.Velocity, VectorUtils.yComponent2, value);
		}
	}
	IEnumerator ActionUpdate() {
		yield return null;
		while(true) {
            _Update();
			yield return new WaitForFixedUpdate();
		}
	}
    public PlayerController FindPlayerInRange(ViewRange range) {
        return Algorithm.FirstValid(Algorithm.Map(range.InRangeGos, (go) => go.GetComponentInParent<PlayerController>()), (t) => t != null);
    }
    public bool ContainPlayerInRange(ViewRange range) {
        return FindPlayerInRange(range) != null;
    }
    public virtual void _Update() {
        player = FindPlayerInRange(view);
        tree.SetCondBool("playerInView", player != null);
        tree.SetCondBool("onGround", OnGround);
        tree.SetCondFloat("xSpeed", mov.Velocity.x);
    }
    NodeRegistry NodeRegistryProvider.GetRegistry(NodeRegistry prev)
    {
		var nr = prev.Derive();
		RegisterNode(nr);
		return nr;
    }
	protected virtual void RegisterNode(NodeRegistry nr) {
		nr.AddAction<RunToEdgeNode>("runToEdge");
		nr.AddAction<TurnAroundNode>("turnAround");
		nr.AddAction<SpineAnimateNode>("playAnim");
		nr.AddAction<SpineAnimateOffsetNode>("animOffset");
		nr.AddAction<FallToGroundNode>("fallToGround");
		nr.AddAction<FaceToPlayerNode>("faceToPlayer");
		nr.AddAction<JumpNode>("jump");
		nr.AddAction<StandNode>("stand");
		nr.AddAction<PlaySoundNode>("playSound");
	}
	public abstract class GroundEnemyActionNode : ControllerNode<GroundEnemy> {
	}
    public class RunToEdgeNode : GroundEnemyActionNode
    {
        public override ExecState Evaluate()
        {
			if(controller.CouldWalkThrough) {
				controller.XSpeed = controller.horizontalSpeed;
				return ExecState.Running;
			}
			else {
				controller.XSpeed = 0;
				return ExecState.Success;
			}
        }
    }
	public class FaceToPlayerNode : GroundEnemyActionNode {
		public override ExecState Evaluate() {
			if(controller.player == null) return ExecState.Fail;
			controller.faceDirX = (controller.player.PositionWithoutHalo.x - controller.mov.Position.x) > 0 ? 1 : -1;
			return ExecState.Success;
		}
	}
	public class TurnAroundNode : GroundEnemyActionNode {
		public override ExecState Evaluate() {
			controller.faceDirX = -controller.faceDirX;
			return ExecState.Success;
		}
	}
	public class FallToGroundNode : GroundEnemyActionNode {
		public override ExecState Evaluate() {
			if(controller.OnGround) {
				return ExecState.Success;
			} 
			controller.YSpeed -= controller.gravityAcc * Time.deltaTime;
			controller.YSpeed = Math.Max(-controller.maxSpeedY, controller.YSpeed);
			return ExecState.Running;
		}
	}
	public class JumpNode : GroundEnemyActionNode {
		public override ExecState Evaluate() {
			if(controller.OnGround) {
				controller.XSpeed = controller.jumpHorizontalSpeed;
				controller.YSpeed = controller.jumpVerticalSpeed;
				return ExecState.Running;
			}
			else {
				return ExecState.Success;
			}
		}
	}
	public class StandNode : GroundEnemyActionNode {
		public override ExecState Evaluate() {
			controller.mov.Velocity = VectorUtils.ReplaceComponent(controller.mov.Velocity, VectorUtils.xComponent2, 0);
			return ExecState.Success;
		}
	}
	public class PlaySoundNode : GroundEnemyActionNode {
		string soundName;
		float volumeScale;
		float timeCount = -1;
		float time = 0;
		public override void ReadArgs(ActionNodeArgs args) {
			soundName = args.GetString("name", "");
			volumeScale = args.GetFloat("volume", 1);
		}
		public override ExecState Evaluate() {
			if(timeCount == -1) {
				var sound = controller.GetComponentInChildren<AudioSource>();
				var clip = SoundManager.Main[soundName];
				time = clip.length;
				SoundManager.PlaySound(sound, soundName, volumeScale);
				timeCount = 0;
				return ExecState.Running;
			}
			else if(timeCount >= time) {
				return ExecState.Success;
			}
			else {
				timeCount += Time.deltaTime;
				return ExecState.Running;
			}
		}
		public override void Reset() {
			timeCount = -1;
		}
	}
}