using System.Collections;
using System.Net.NetworkInformation;
using UnityEngine;

public class onExit : StateMachineBehaviour
{
    [SerializeField] private Animations animation;
    [SerializeField] private bool lockLayer;
    [SerializeField] private float crossFade;
    [HideInInspector] public bool cancel = false;
    [HideInInspector] public int layerIndex = -1;


    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        this.layerIndex = layerIndex;
        cancel = false;
        PlayerController.instance.StartCoroutine(WaitCo());

        IEnumerator WaitCo()
        {
            yield return new WaitForSeconds(stateInfo.length - crossFade);

            if (cancel) yield break;

            AnimatorBrain target = animator.GetComponent<AnimatorBrain>();
            target.SetLocked(false, layerIndex);
            target.Play(animation, layerIndex, lockLayer, false, crossFade);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

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
