using BTScript;
using Spine.Unity;
using PseudoTools;
using UnityEngine;
public abstract class ControllerNode<T> : ActionNode where T : MonoBehaviour {
    protected T controller;
    public override void _Init(BehaviourTree tree) {
        controller = tree.gameObject.GetComponent<T>();
    }
}
public class SpineAnimateNode : ActionNode
{
    SkeletonAnimation animation;
    public string animateName = "";
    public bool loop;
    public float timeScale;
    public bool reset;
    public override void ReadArgs(ActionNodeArgs args) {
        animateName = args.GetString("name", "");
        loop = args.GetBool("loop", false);
        reset = args.GetBool("reset", false);
        timeScale = args.GetFloat("timeScale", 1);

    }
    public override void _Init(BehaviourTree tree) {
        base._Init(tree);
        animation = tree.GetComponentInChildren<SkeletonAnimation>();
        
    }
    public override ExecState Evaluate()
    {
        SpineController.SetAnimate(animation, animateName, loop, reset, timeScale);
        return ExecState.Success;
    }
}
public class SpineAnimateOffsetNode : ActionNode {
    SkeletonAnimation animation;
    float offset;
    public override void ReadArgs(ActionNodeArgs args) {
        offset = args.GetFloat("offset", 0);
    }
    public override void _Init(BehaviourTree tree) {
        base._Init(tree);
        animation = tree.GetComponentInChildren<SkeletonAnimation>();
    }
    public override ExecState Evaluate()
    {
        SpineController.SetAnimateOffset(animation, offset);
        return ExecState.Success;
    }

}