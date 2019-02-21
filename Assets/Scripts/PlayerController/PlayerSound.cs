using UnityEngine;
using PseudoTools;
public class PlayerSound : MonoBehaviour {
    public AudioSource source;
    public AudioClip rush;
    public AudioClip foot;
    public AudioClip fall;
    public PlayerController controller;
    public float footStepTime = 0.3f;
    public SimpleTimer footTimer;
    public void Awake() {
        controller.AddEnterEventBeforeEnter<PlayerController.IdleState>(() => {
            if(controller.lastStateName == "Jump") {
                source.PlayOneShot(fall, 0.7f);
            }
        });
        footTimer = new SimpleTimer(this, footStepTime);
        controller.AddEnterEventBeforeEnter<PlayerController.RushState>(() => {
            source.PlayOneShot(rush);
        });
    }
    public void Update() {
        System.Func<bool> running = ()=>controller.currentStateName == "Idle" && controller.Velocity.x != 0;
        if(running()) {
            if(!footTimer.IsTiming) {
                /*footTimer.BeginTiming(()=>{
                    if(running()) {
                        source.PlayOneShot(foot);
                    }
                });
                */
                source.PlayOneShot(foot, 1.3f);
                footTimer.BeginTiming();
            }
        }       
    }
}