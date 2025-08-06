using UnityEngine;

public class AnimatorBrain : MonoBehaviour
{
    private readonly static int[] animations =
    {
        Animator.StringToHash("Idle 1"),
        Animator.StringToHash("Idle 2"),
        Animator.StringToHash("Idle 3"),
        Animator.StringToHash("Walk Forward"),
        Animator.StringToHash("Walk Backward"),
        Animator.StringToHash("Run Forward"),
        Animator.StringToHash("Run Backward"),
        Animator.StringToHash("Jump Start"),
        Animator.StringToHash("Jump Air"),
        Animator.StringToHash("Jump Land"),
        Animator.StringToHash("Punch Cross"),
        Animator.StringToHash("Death"),
        Animator.StringToHash("Dance"),
        Animator.StringToHash("Crouch Idle"),
        Animator.StringToHash("Crouch Forward"),
        Animator.StringToHash("Pistol Idle"),
        Animator.StringToHash("Pistol Shoot"),
        Animator.StringToHash("Pistol Reload"),
        Animator.StringToHash("None")
    };
}

public enum Animations
{
    IDLE1,
    IDLE2,
    IDLE3,
    WALKFWD,
    WALKBWD,
    RUNFWD,
    RUNBWD,
    JUMPSTART,
    JUMPAIR,
    JUMPLAND,
    PUNCHCROSS,
    DEATH,
    DANCE,
    CROUCHIDLE,
    CROUCHFWD,
    PISTOLIDLE,
    PISTOLSHOOT,
    PISTOLRELOAD,
    NONE
}
