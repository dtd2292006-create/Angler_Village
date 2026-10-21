using UnityEngine;

public class PlayerCatchStateBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator,
        AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerAnimationController controller =
            animator.GetComponent<PlayerAnimationController>();
        if (controller != null) controller.NotifyCatchEntered();
    }

    public override void OnStateExit(Animator animator,
        AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerCatchVisual visual = animator.GetComponent<PlayerCatchVisual>();
        if (visual != null) visual.ClearCaughtFish();

        PlayerAnimationController controller =
            animator.GetComponent<PlayerAnimationController>();
        if (controller != null) controller.NotifyCatchExited();
    }
}
