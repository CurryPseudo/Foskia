using UnityEngine;
public class BodyAnimatorView : MonoBehaviour {
    public Animator animator;
    public PlayerController controller;
    public void Awake() {
        controller.jumpEvent += ()=> {
            animator.SetTrigger("jump");
        };
        controller.AddEnterEventBeforeEnter<PlayerController.IdleState>(()=> {
            animator.SetTrigger("hitground");
        });
    }
    public void Update() {
        if(transform.localScale.x * controller.Velocity.x < 0) {
            var scale = transform.localScale;
            scale.x = -scale.x;
            transform.localScale = scale;
        }
        animator.SetFloat("vx", controller.Velocity.x);
        Debug.Log(animator.GetFloat("vx"));
        animator.SetFloat("vy", controller.Velocity.y);
        animator.SetBool("vxzero", controller.Velocity.x == 0);
    }
}