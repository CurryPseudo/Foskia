using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PseudoTools;
using Sirenix.OdinInspector;

public class PlayerController : FiniteStateMachineMonobehaviour<PlayerController> {
	private static string prefabPath = "PlayerController";
	private static PlayerController main;
	public static PlayerController Main {
		get {
			return main;
		}
	}
	void Awake()
	{
		main = this;
		ChangeState<IdleState>(null);
		haloView = GetComponentInChildren<HaloView>();
		blockMovement = BlockMovement.AddBlockMovement(gameObject, airBox, solidLayer);
		blockMovement.staticExtrusion = true;
		jumpSkill.Init(()=> Input.GetButton("Jump"), this);
		rushSkill.Init(()=> Input.GetButton("Rush"), this);
		swapSkill.Init(()=> Input.GetButton("Swap"), this);
		swapCharges.Init(this);
		//swap energe update
		StartCoroutine(SwapDetect());
		cameraEdge = new CameraEdge();
		cameraEdge.edge = GameObject.Find("SceneEdge").GetComponent<BoxCollider2D>();
		cameraEdge.cameraPos = CameraPos.main;
		
		follow = new CameraSmoothFollow();
		follow.mainPos = true;
		follow.posGetter = () => cameraEdge.UpdatePos(blockMovement.Position);
		//follow.posGetter = () => blockMovement.Position;
		follow.distanceSpeedCurve = followDistanceSpeedCurve;
		follow.priority = 2;
		follow.Register(this);
		playerPosDisQueue = new PositionDistanceQueue(() => PositionWithoutHalo
			, posQueueDisMax
			, posQueueCountMax);
		StartCoroutine(playerPosDisQueue.Update());

		scaleAd = ApCtrl.CreateAlphaData(ApCtrl.TransformScaleXY(transform), this);
		StartCoroutine(TransforDetect());
		
		sound = GetComponentInChildren<AudioSource>();
	}
	void OnDestroy() {
		follow.Disregister();
	}
	public static void SpawnPos(Vector2 pos) {
		GameObject prefab = Resources.Load<GameObject>(prefabPath);
		var go = GameObject.Instantiate(prefab);
		var player = go.GetComponent<PlayerController>();
		player.Position = pos;
	}
	public BoxCollision airBox;
	public BoxCollision groundBox;
	public Vector2 Velocity {
		get {
			return blockMovement.Velocity;
		}
		set {
			blockMovement.Velocity = value;
		}
	}
	public Vector2 MoveDistance {
		get {
			return Velocity * Time.fixedDeltaTime;
		}
	}
	public Vector2 Position {
		get {
			return PositionWithoutHalo;
		}
		set {
			PositionWithoutHalo = value;
			haloView.SetPositionIgnoreDelay();
		}
	}
	public Vector2 PositionWithoutHalo {
		get {
			return blockMovement.Position;
		}
		set {
			blockMovement.Position = value;
		}
	}
	public Vector2 PositionWithEdgeCut {
		get {
			return cameraEdge.UpdatePos(PositionWithoutHalo);
		}
	}

	private HaloView haloView;
	private BlockMovement blockMovement;
	private ApCtrl.AlphaData scaleAd;
	[Header("Layer")]
	public LayerMask solidLayer;
	[Header("Idle")]
	public float idleHorizontalSpeed;
	public bool faceRight = true;
	[Header("Jump")]
	public JumpProperty[] jumpProperties = new JumpProperty[2];
	public bool[] jumpStatus = new bool[2];
	public Vector2 jumpAcc;
	public float maxFallSpeed;
	public event Action jumpEvent;
	public SkillButton jumpSkill;
	[Header("Rush")]
	public float rushMinSpeed;
	public float rushMaxSpeed;
	public AnimationCurve rushSpeedTimeCurve;
	public float rushTime;
	public SkillButton rushSkill;
	[ReadOnly]
	public bool rushValid = true;
	[Header("Hurt")]
	[ReadOnly]
	public bool hurtValid = true;
	public float hurtStateTime;
	public float hurtCdTime;
	public Vector2 hurtVelocityDirection;
	public float hurtSpeed;
	public AnimationCurve hurtSpeedTimeCurve;
	public CameraShakeEffect hurtShake;
	public float hurtCameraExpandTime = 1f;
	public float hurtCameraSizeExpandValue = 0.3f;
	public Color hurtColorScreen = new Color(0, 0, 0, 0.8f);
	public float hurtColorScreenTime = 0.2f;
	[Header("Swap")]
	[InlineProperty]
	[HideLabel]
	public SwapCharges swapCharges = new SwapCharges();
	public SkillButton swapSkill;
	public BoxCollisionCollect swapEnergeDetect;
	public CircleViewRange swapView;
	public SwapProperty swapProperty;
	private Swapable closestSwapable;
	[Header("Camera")]
	public AnimationCurve followDistanceSpeedCurve;
	private CameraEdge cameraEdge;
	private CameraSmoothFollow follow;
	[Header("RedLight")]
	public AnimationCurve rdLtDisSpdCv;
	public float posQueueDisMax;
	public int posQueueCountMax;
	public SinMoveData rdLtSinMovDt;
	public PositionDistanceQueue playerPosDisQueue;
	public GameObject redLightPrefab;
	public float redLightAlphaConvertSpeed;
	[LabelText("红球飞向主角时时间与距离关系(Normed)")]
	public AnimationCurve rdLtToPlayerTimeDisCurve;
	public AnimationCurve rdLtSwapNormalDis;
	[Header("Transfor")]
	public BoxCollisionCollect transforDetect;
	public float transforBlackScreenTime = 0.5f;
	public float jumpHorizontalSpeed {
		get {
			return idleHorizontalSpeed;
		}
	}
	private AudioSource sound;
	public bool OnGround() {
		//return Velocity.y <= 0 && groundBox.CheckStaticCollision(solidLayer);
		return airBox.CheckMoveCollision(Vector2.down * 0.02f, solidLayer) != null;
	}
	public void CutVelocity(ref Vector2 velocity, Vector2 component) {
		velocity = new Vector2((1 - component.x) * velocity.x, (1 - component.y) * velocity.y);
	}
	public IEnumerator JumpCoroutine(int index) {
		float timeCount = 0;
		JumpProperty p = jumpProperties[index];
		jumpStatus[index] = true;
		if(jumpEvent != null) {
			jumpEvent.Invoke();
		}
		while(timeCount < p.jumpTime && Input.GetButton("Jump")) {
			Velocity = VectorUtils.ChangeY(Velocity, p.jumpSpeed);
			yield return new WaitForFixedUpdate();
			timeCount += Time.fixedDeltaTime;
		}
		jumpStatus[index] = false;
		yield break;
	}
	public IEnumerator TransforDetect() {

		while(true) {
			var transfor = transforDetect.GetMinDisDetects<TransforProperty>();
			if(transfor != null) {
				var getFinish = SceneTransforer.main.TransforToPoint(transfor.index, transforBlackScreenTime);
				yield break;
			}
			yield return null;
		}
	}
	private IEnumerator SwapDetect() {
		
		while(true) {
			var swapEnerge = swapEnergeDetect.GetMinDisDetects<SwapEnergeProperty>();
			Action<Vector2> createRedLights  = (pos) => {
				RedLightPlayerController[] rls = new RedLightPlayerController[swapCharges.MaxSwapChargeCount];
				for(int i = 0; i < rls.Length; i++) {
					rls[i] = RedLightPlayerController.Create(this, pos);
				}
				swapCharges.FullCharge(rls);
			};
			if(swapEnerge != null && !swapCharges.Fulled) {
				SoundManager.PlaySound(sound, "SwapEnerge");
				swapEnerge.EnergeGeted(this, createRedLights);
			}
			yield return new WaitForFixedUpdate();
		}
	}
	private void SwapProcess() {
		if(swapView.InRangeGosCount == 0) return;
		var swapables = Algorithm.GetMonos<Swapable>(swapView.InRangeGos);
		Func<Swapable, GameObject> getGo = (sa) => sa.gameObject;
		Func<GameObject, float> getGoDis = (g) => (VectorUtils.V32(g.transform.position) - blockMovement.Position).magnitude;
		closestSwapable = Algorithm.GetMin(swapables, (sa) => getGoDis(getGo(sa)));
		if(closestSwapable != null && swapSkill.SkillValid && swapCharges.SwapValid) {
			if(!closestSwapable.SelectedUIActive) {
				closestSwapable.SelectedUIActive = true;
				closestSwapable.StartCoroutine(checkClosest(closestSwapable));
			}
			if(swapSkill.SkillPressed) {
				ChangeState<SwapState>(new SwapState(closestSwapable, current));
			}
		}
	}
	private IEnumerator checkClosest(Swapable swapable) {
		while(true) {
			if(swapable != closestSwapable || !swapSkill.SkillValid || !swapCharges.SwapValid) {
				swapable.SelectedUIActive = false;
				yield break;
			}
			yield return null;
		}
	}
	
	protected override void FixedUpdateBeforeFSMUpdate() {
		SwapProcess();
	}
	protected override void FixedUpdateAfterFSMUpdate() {

		if(Velocity.x > 0) {
			faceRight = true;
		}
		else if(Velocity.x < 0) {
			faceRight = false;
		}
	}
	private bool RushProcess() {
		if(rushValid && rushSkill.SkillValidPressed) {
			rushValid = false;
			rushSkill.UseSkill<RushState>();
			ChangeState<RushState>(new RushState(current));
			return true;
		}
		return false;
	}
	public bool HurtInvoke(bool hurtFromRight) {
		if(hurtValid && currentStateName != "Hurt") {
			ChangeState<HurtState>(new HurtState(hurtFromRight));
			return true;
		}
		return false;
	}
    public class IdleState : StateNormal<IdleState> {
		public override void OnEnterWithEvent(PlayerController fsm) {
		}
		public override void OnExcuteWithEvent(PlayerController fsm) {
			var v = Vector2.zero;
			v.x = Input.GetAxisRaw("Horizontal") * fsm.idleHorizontalSpeed;
			//v.y = 0;
			fsm.Velocity = v;
			if(!fsm.OnGround()) {
				fsm.ChangeState<JumpState>(null);
				return;
			}
			if(fsm.jumpSkill.SkillValidPressed) {
				fsm.jumpSkill.UseSkill();
				fsm.StartCoroutine(fsm.JumpCoroutine(0));
				return;
			}
			if(fsm.RushProcess()) {
				return;
			}
			fsm.rushValid = true;
		}
        public override string GetStateName()
        {
			return "Idle";
        }
    }
	public class JumpState : StateNormal<JumpState> {
		public bool secondJumped = false;
		public override void OnExcuteWithEvent(PlayerController fsm) {
			if(!fsm.jumpStatus[0] && !secondJumped && fsm.jumpSkill.SkillValidPressed) {
				fsm.jumpSkill.UseSkill();
				fsm.StartCoroutine(fsm.JumpCoroutine(1));
				secondJumped = true;
			}
			var v = fsm.Velocity;
			v.x = Input.GetAxisRaw("Horizontal") * fsm.idleHorizontalSpeed;
			v += fsm.jumpAcc * Time.fixedDeltaTime;
			v.y = Mathf.Max(-fsm.maxFallSpeed, v.y);
			fsm.Velocity = v;
			if(fsm.OnGround()) {
				fsm.ChangeState<IdleState>(null);
				return;
			}
			if(fsm.RushProcess()) {
				return;
			}

		}
		public override string GetStateName() {
			return "Jump";
		}
	}
	public class RushState : StateNormal<RushState> {
		public IState lastState;
		public float timeCount = 0;
		public PlayerController fsm;
		public RushState() {
			lastState = new IdleState();
		}
		public RushState(IState lastState) {
			this.lastState = lastState;
		}
		public override void OnEnterWithEvent(PlayerController fsm) {
			this.fsm = fsm;
		}
		public override void OnExcuteWithEvent(PlayerController fsm) {

			if(timeCount < fsm.rushTime) {
				timeCount += Time.fixedDeltaTime;
				float speedValue = fsm.rushSpeedTimeCurve.Evaluate(timeCount / fsm.rushTime);
				float speed = Mathf.Lerp(fsm.rushMinSpeed, fsm.rushMaxSpeed, speedValue);
				fsm.Velocity = Vector2.right * (fsm.faceRight ? 1 : -1) * speed;
			}
			else {
				ChangeToLastState();
			}
		}
		private void ChangeToLastState() {
			fsm.ChangeState(lastState);
		}
		public override string GetStateName() {
			return "Rush";
		}
	}
	public class HurtState : StateNormal<HurtState> {
		Timer timer;
		bool hurtFromRight;
		public HurtState() {

		}
		public HurtState(bool hurtFromRight) {
			this.hurtFromRight = hurtFromRight;
		}
		public override void OnEnterWithEvent(PlayerController fsm) {
			timer = Timer.CreateAFixedTimer(fsm.hurtStateTime, ()=>fsm.ChangeState<JumpState>(null), fsm);
			SoundManager.PlaySound(fsm.sound, "Hurt");
			fsm.StartCoroutine(hurtScreenColor(fsm));
			fsm.StartCoroutine(hurtScreenExpand(fsm));
		}
		private IEnumerator hurtScreenColor(PlayerController fsm) {
			var time = fsm.hurtColorScreenTime;
			ColorScreen.Main.SetColor(fsm.hurtColorScreen, time);
			yield return new WaitForSecondsRealtime(time);
			ColorScreen.Main.SetColor(new Color(0,0,0,0), time);
		}
		private IEnumerator hurtScreenExpand(PlayerController fsm) {
			var time = fsm.hurtCameraExpandTime;
			var originSize = Camera.main.orthographicSize;
			var deltaSize = fsm.hurtCameraSizeExpandValue;
			float timeCount = 0;
			while(timeCount < 1) {
				yield return null;
				timeCount += Time.unscaledDeltaTime / time;
				timeCount = Mathf.Clamp01(timeCount);
				Camera.main.orthographicSize = Mathf.Lerp(originSize, originSize + deltaSize, timeCount);
			}
			timeCount = 0;
			while(timeCount < 1) {
				yield return null;
				timeCount += Time.unscaledDeltaTime / time;
				timeCount = Mathf.Clamp01(timeCount);
				Camera.main.orthographicSize = Mathf.Lerp(originSize + deltaSize, originSize, timeCount);
			}
		}
		public override void OnExcuteWithEvent(PlayerController fsm) {
			var unsignedVelocity = fsm.hurtVelocityDirection.normalized * fsm.hurtSpeedTimeCurve.Evaluate(timer.Value) * fsm.hurtSpeed;
			fsm.Velocity = VectorUtils.Multiply(unsignedVelocity, new Vector2(hurtFromRight ? -1 : 1, 1));
		}
		public override void OnExitWithEvent(PlayerController fsm) {
			fsm.hurtValid = false;
			fsm.faceRight = hurtFromRight;
			Timer.BeginATimer(fsm.hurtCdTime, () => fsm.hurtValid = true, fsm);
		}
		public override string GetStateName() {
			return "Hurt";
		}
	}
    public class SwapState : StateNormal<SwapState>
    {
		
		IState last;
		Swapable target;
		Vector2 dir;
		public Vector2 targetPosition {
			get {
				return target.Position;
			}
		}
		public SwapState() {}
		public SwapState(Swapable target, IState last) {
			this.target = target;
			this.last = last;
		}
		public override void OnEnterWithEvent(PlayerController fsm) {
			SoundManager.PlaySound(fsm.sound, "Swap2", 1.4f);
			fsm.StartCoroutine(enterCoroutine(fsm));
		}
		private IEnumerator enterCoroutine(PlayerController fsm) {
			Time.timeScale = 0f;
			fsm.blockMovement.staticExtrusion = false;
			dir = targetPosition - fsm.PositionWithoutHalo;
			fsm.swapSkill.UseSkill<SwapState>();
			var binding = fsm.swapCharges.GetSwapCharge();
			binding.value = 0;
			fsm.StartCoroutine(binding.redLight.SwapCor(fsm, targetPosition));
			var enemyRedLight = RedLightEnemyController.Create(fsm, targetPosition);
			fsm.StartCoroutine(enemyRedLight.SwapCor());
			EventBus.Notify("PlayerSwapEnter", fsm, fsm.swapProperty.disappearTime);
			float time = dir.magnitude / fsm.swapProperty.speed;
			target.SwapMove(-dir, time, fsm.swapProperty.disappearTime);
			//ApCtrl.DisappearAlpha(fsm.scaleAd, fsm.swapProperty.disappearTime);
			yield return new WaitForSecondsRealtime(fsm.swapProperty.disappearTime);
			Swapable.SwapMove(dir, time, (pos) => fsm.blockMovement.Position = pos, () => fsm.PositionWithoutHalo, fsm);
			yield return new WaitForSecondsRealtime(time);
			fsm.blockMovement.staticExtrusion = true;
			EventBus.Notify("PlayerSwapExit", fsm, fsm.swapProperty.disappearTime);
			//ApCtrl.AppearAlpha(fsm.scaleAd, fsm.swapProperty.disappearTime);
			yield return new WaitForSecondsRealtime(fsm.swapProperty.disappearTime);
			Time.timeScale = 1;
			var v = fsm.blockMovement.Velocity;
			v *= 0;
			fsm.blockMovement.Velocity = v;
			fsm.ChangeState(last);
		}
		public override void OnExcuteWithEvent(PlayerController fsm) {

		}
		public override void OnExitWithEvent(PlayerController fsm) {
		}
        public override string GetStateName()
        {
			return "Swap";
        }
    }
	public class DeadState : StateNormal<DeadState> {

		public bool DeadAnimFinished {
			get {
				return deadAnimFinished;
			}
		}
		private bool deadAnimFinished = false;
		private BodyView body;
		private HaloView halo;
		private PlayerController player;
		public override void OnEnterWithEvent(PlayerController fsm) {
			fsm.blockMovement.Velocity = Vector2.zero;
			body = fsm.GetComponentInChildren<BodyView>();
			halo = fsm.GetComponentInChildren<HaloView>();
			player = fsm;
			player.StartCoroutine(stateCor());
		}
		private IEnumerator stateCor() {
			var disappearTime = 1.3333f;
			body.SetAnimate("die1", false);
			halo.Disappear(disappearTime);
			yield return new WaitForSecondsRealtime(disappearTime);
			body.SetAnimate("die2", true);
			yield return new WaitForSecondsRealtime(1);
			deadAnimFinished = true;
			yield break;
		}
		public override string GetStateName() {
			return "Dead";
		}
	}
    [Serializable]
	[InlineProperty]
	public class JumpProperty {
		public float jumpTime;
		public float jumpSpeed;
	}
	[Serializable]
	public class SkillButton {
		public float cdTime;
		private bool cdValid = true;
		public bool buttonReleased = true;
		private Func<bool> getButton;
		private PlayerController mb;
		public void Init(Func<bool> getButton, PlayerController mb) {
			this.getButton = getButton;
			this.mb = mb;
			mb.StartCoroutine(CoroutineUtils.FixedUpdateAction(()=> {
				if(!getButton()) {
					buttonReleased = true;
				}
			}));
		}
		public bool SkillValid {
			get {
				return cdValid && buttonReleased;
			}
		}
		public bool SkillValidPressed {
			get {
				return SkillValid && getButton();
			}
		}
		public bool SkillPressed {
			get {
				return getButton();
			}
		}
		public void UseSkill<C>() where C : StateNormal<C>, new() {
			buttonReleased = false;
			cdValid = false;
			Action a = null;
			a = ()=> {
				Timer.CreateAFixedTimer(cdTime, ()=> {
					cdValid = true;
				}, mb);
				mb.RemoveEnterEventBeforeExit<C>(a);
			};
			mb.AddEnterEventBeforeExit<C>(a);
		}
		public void UseSkill() {
			buttonReleased = false;
			cdValid = false;
			Timer.CreateAFixedTimer(cdTime, ()=> {
				cdValid = true;
			}, mb);

		}
	}
	[Serializable]
	public class SwapProperty {
		public float disappearTime;
		public float speed;

	}
}

