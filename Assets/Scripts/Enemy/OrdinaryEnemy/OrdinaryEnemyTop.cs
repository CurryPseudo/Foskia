using System;
using UnityEngine;
using PseudoTools;
using Sirenix.OdinInspector;
[RequireComponent(typeof(BlockMovement))]
public class OrdinaryEnemyTop : FiniteStateMachineMonobehaviour<OrdinaryEnemyTop> {
    private AudioSource sound;
    private BlockMovement mov;
    private InRangeRecorder inRange;
    private Vector2 originPos;
    public Collider2D enemyBox;
    [Header("Layer")]
    public LayerMask chaseLayer;
    [Header("Return")]
    public float hateRadius;
    //public float returningNotChaseRange;
    public float returningChaseCdTime;
    public AnimationCurve returnDistanceSpeedCurve;
    [LabelText("回程寻路导航图")]
    public NevPointMap returnNevMap;
    [Header("Guard")]
    public AnimationCurve guardDisVelocityCurve;
    [Header("Chase")]
    public float fastSpeed;
    public float fastTime;
    public float slowSpeed;
    public float slowTime;
    [InfoBox("追逐失去目标后停顿的时间")]
    public float guardTime;

    public Vector2 Velocity {
        get { 
            return mov.Velocity;
        }
        set {
            mov.Velocity = value;
        }
    }
    public OrdinaryEnemyChase chase;
    public void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hateRadius);
    }
    private void Awake() {
        mov = GetComponent<BlockMovement>();
        ChangeState<HomeState>(null);
        inRange = enemyBox.gameObject.AddComponent<InRangeRecorder>();
        originPos = mov.Position;
        sound = GetComponentInChildren<AudioSource>();
    }
    public bool ChaseProcess() {
        GameObject target = GetValidChaseTarget();
        if(target != null) {
            ChangeState<ChaseState>(new ChaseState(target));
            return true;
        }
        return false;
    }
    public GameObject GetValidChaseTarget() {
        GameObject target;
        if((target = GetChaseTarget()) != null && InRadius(hateRadius) && TargetInRadius(hateRadius)) {
            return target;
        }
        return null;
    }
    public GameObject GetChaseTarget() {
        foreach(var go in inRange.GetViews()) {
            if(LayerMaskUtils.IsIncluded(chaseLayer, go.layer)) {
                return go;
            }
        }
        return null;
    }
    public bool InRadius(float radius) {
        return (originPos - mov.Position).magnitude < radius;
    }
    public bool TargetInRadius(float radius) {
        var target = GetChaseTarget();

        return target != null && (VectorUtils.V32(target.transform.position) - originPos).magnitude < radius;
    }
    public class HomeState : StateNormal<HomeState>
    {
        public override void OnExcuteWithEvent(OrdinaryEnemyTop fsm) {
            fsm.Velocity = Vector2.zero;
            if(fsm.ChaseProcess()) return;
            if(!fsm.InRadius(0.2f)) {
                fsm.ChangeState<ReturnState>(null);
            }
        }
        public override string GetStateName()
        {
            return "Home";
        }
    }
    public class ReturnState : StateNormal<ReturnState> {
        //public bool chaseValid = true;
        public NevPointMap nevMap;
        public NevPointMap.Path path;
        public override void OnEnterWithEvent(OrdinaryEnemyTop fsm) {
            nevMap = fsm.returnNevMap;
            path = nevMap.CreatePath(()=>fsm.mov.Position, fsm.originPos);
        }
        public override void OnExcuteWithEvent(OrdinaryEnemyTop fsm) {
            if(fsm.ChaseProcess()) {
                return;
            }
            if(path.Finished) {
                fsm.ChangeState<HomeState>(null);
                return;
            }
            Vector2 dir = path.GetDir();
            float dis = (fsm.mov.Position - fsm.originPos).magnitude;
            float speed = fsm.returnDistanceSpeedCurve.Evaluate(dis);
            fsm.mov.Velocity = dir.normalized * speed;
        }
        public override string GetStateName() {
            return "Return";
        }
    }
    public class ChaseState : StateNormal<ChaseState> {
        public GameObject chaseTarget;
        public ChaseState() {
        }
        public ChaseState(GameObject chaseTarget) {
            this.chaseTarget = chaseTarget;
        }
        public override void OnEnterWithEvent(OrdinaryEnemyTop fsm) {
            fsm.chase = new OrdinaryEnemyChase(fsm, this);
            fsm.chase.ChangeState<OrdinaryEnemyChase.SlowState>(null);
        }
        public override void OnExcuteWithEvent(OrdinaryEnemyTop fsm) {
            fsm.chase.Update();
        }
        public override void OnExitWithEvent(OrdinaryEnemyTop fsm) {
            fsm.chase = null;
        }
        public override string GetStateName() {
            return "Chase";
        }
    }
    public class OrdinaryEnemyChase : FiniteStateMachine<OrdinaryEnemyChase> {
        private OrdinaryEnemyTop top;
        private ChaseState chaseState;
        private Vector2 velocity;

        public OrdinaryEnemyChase(OrdinaryEnemyTop top, ChaseState chaseState)
        {
            this.top = top;
            this.chaseState = chaseState;
        }

        public new void Update(){
            base.Update();
            if(top.GetValidChaseTarget() == null) {
                top.ChangeState<ReturnState>(null);
                return;
            }
            top.Velocity = velocity;
        }
        public class FastState : ChaseState<FastState>
        {
            public override IState GetNextState()
            {
                return new SlowState();
            }

            public override void OnEnterWithEvent(OrdinaryEnemyChase fsm) {
                base.OnEnterWithEvent(fsm);
                SoundManager.PlaySound(fsm.top.sound, "OrdinaryEnemy");
            }
            public override float GetSpeed(OrdinaryEnemyChase fsm)
            {
                return fsm.top.fastSpeed;
            }

            public override string GetStateName()
            {
                return "Fast";
            }

            public override float GetTime(OrdinaryEnemyChase fsm)
            {
                return fsm.top.fastTime;
            }
        }
        public class SlowState : StateNormal<SlowState> {
            public Timer timer;
            public override void OnEnterWithEvent(OrdinaryEnemyChase fsm) {
                SoundManager.PlaySound(fsm.top.sound, "OrdinaryEnemy2");
                timer = Timer.CreateATimer(fsm.top.slowTime, null, fsm.top);
            }
            public override void OnExcuteWithEvent(OrdinaryEnemyChase fsm) {
                Vector2 chasePoint = fsm.chaseState.chaseTarget.transform.position;
                var dir = chasePoint - fsm.top.mov.Position;
                fsm.velocity = dir.normalized * fsm.top.slowSpeed;
                if(timer.Value == 1) {
                    fsm.ChangeState<FastState>(null);
                }
            }
            public override string GetStateName() {
                return "Slow";
            }

        }
        public abstract class ChaseState<T> : StateNormal<T> where T : StateNormal<T> , new(){
            public Vector2 chasePoint;
            public Timer timer;
            public override void OnEnterWithEvent(OrdinaryEnemyChase fsm) {
                chasePoint = fsm.chaseState.chaseTarget.transform.position;
                timer = Timer.CreateATimer(GetTime(fsm), ()=>{
                }, fsm.top);
            }
            public override void OnExcuteWithEvent(OrdinaryEnemyChase fsm) {
                var dir = chasePoint - fsm.top.mov.Position;
                fsm.velocity = dir.normalized * GetSpeed(fsm);
                if(dir.magnitude < 0.1f || timer.Value == 1) {
                    fsm.ChangeState(GetNextState());
                }
            }
            public abstract float GetSpeed(OrdinaryEnemyChase fsm);
            public abstract float GetTime(OrdinaryEnemyChase fsm);
            public abstract IState GetNextState();
        }
    }
}