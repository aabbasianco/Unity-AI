using UnityEngine;

public class checkGrounded : StateMachineBehaviour
{
    [SerializeField] private bool grounded;
    [SerializeField] private bool unlockLayer;
    [SerializeField] private Animations animation;
    [SerializeField] private bool lockLayer;
    [SerializeField] private float crossFade;

    private AnimatorBrain animatorBrain;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animatorBrain = animator.GetComponent<AnimatorBrain>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (grounded == animatorBrain.Grounded)
        {
            animatorBrain.SetLocked(!unlockLayer, layerIndex);
            animatorBrain.Play(animation, layerIndex, lockLayer, false, crossFade);
            PlayerController.instance.jumping = false;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
