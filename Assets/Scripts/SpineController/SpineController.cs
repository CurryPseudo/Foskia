using UnityEngine;
using Spine.Unity;
public static class SpineController {
	public static void SetAnimate(SkeletonAnimation animation, string name, bool loop, bool reset = false, float timeScale = 1) {
        if(reset || name != animation.AnimationName) {
            animation.state.TimeScale = timeScale;
            animation.state.SetAnimation(0, name, loop);
        }
    }
    public static void SetAnimateOffset(SkeletonAnimation animation, float offset) {
        animation.state.GetCurrent(0).TrackTime = offset;
    }
}