using System;
using UnityEngine;

public class AnimatorBrain : MonoBehaviour
{
    private readonly static int[] animations =
    {
        Animator.StringToHash("idle"),
        Animator.StringToHash("idle aiming"),
        Animator.StringToHash("idle crouching"),
        Animator.StringToHash("idle crouching aiming"),
        Animator.StringToHash("walk forward"),
        Animator.StringToHash("walk forward left"),
        Animator.StringToHash("walk forward right"),
        Animator.StringToHash("walk backward"),
        Animator.StringToHash("walk backward left"),
        Animator.StringToHash("walk backward right"),
        Animator.StringToHash("walk left"),
        Animator.StringToHash("walk right"),
        Animator.StringToHash("walk crouching forward"),
        Animator.StringToHash("walk crouching forward left"),
        Animator.StringToHash("walk crouching forward right"),
        Animator.StringToHash("walk crouching backward"),
        Animator.StringToHash("walk crouching backward left"),
        Animator.StringToHash("walk crouching backward right"),
        Animator.StringToHash("walk crouching left"),
        Animator.StringToHash("walk crouching right"),
        Animator.StringToHash("run forward"),
        Animator.StringToHash("run forward left"),
        Animator.StringToHash("run forward right"),
        Animator.StringToHash("run backward"),
        Animator.StringToHash("run backward left"),
        Animator.StringToHash("run backward right"),
        Animator.StringToHash("run left"),
        Animator.StringToHash("run right"),
        Animator.StringToHash("sprint forward"),
        Animator.StringToHash("sprint forward left"),
        Animator.StringToHash("sprint forward right"),
        Animator.StringToHash("sprint backward"),
        Animator.StringToHash("sprint backward left"),
        Animator.StringToHash("sprint backward right"),
        Animator.StringToHash("sprint left"),
        Animator.StringToHash("sprint right"),
        Animator.StringToHash("jump up"),
        Animator.StringToHash("jump air"),
        Animator.StringToHash("jump down"),
        Animator.StringToHash("Punch Cross"),
        Animator.StringToHash("Death"),
        Animator.StringToHash("death from the front"),
        Animator.StringToHash("death from the back"),
        Animator.StringToHash("death from right"),
        Animator.StringToHash("death from front headshot"),
        Animator.StringToHash("death from back headshot"),
        Animator.StringToHash("death crouching headshot front"),
        Animator.StringToHash("Dance"),
        Animator.StringToHash("turn 90 left"),
        Animator.StringToHash("turn 90 right"),
        Animator.StringToHash("crouching turn 90 left"),
        Animator.StringToHash("crouching turn 90 right"),
        Animator.StringToHash("Pistol Idle"),
        Animator.StringToHash("Pistol Shoot"),
        Animator.StringToHash("Pistol Reload"),
        Animator.StringToHash("None")
    };

    private Animator animator;
    private Animations[] currentAnimation;
    private bool[] layerLocked;
    private Action<int> DefaultAnimation;

    protected bool grounded = true;
    public bool Grounded { get => grounded; }

    protected void Initialize(int _layers, Animations _startingAnimation, Animator _animator, Action<int> _DefaultAnimation)
    {
        layerLocked = new bool[_layers];
        currentAnimation = new Animations[_layers];
        this.animator = _animator;
        this.DefaultAnimation = _DefaultAnimation;
        for (int i = 0; i < _layers; i++)
        {
            layerLocked[i] = false;
            currentAnimation[i] = _startingAnimation;
        }
    }

    public Animations GetCurrentAnimations(int _layer)
    {
        return currentAnimation[_layer];
    }

    public void SetLocked(bool _lockLayer, int _layer)
    {
        layerLocked[_layer] = _lockLayer;
    }

    public void Play(Animations _animation, int _layer, bool _lockLayer, bool _bypassLock, float _crossFade = .05f)
    {
        if (_animation == Animations.NONE)
        {
            DefaultAnimation(_layer);
            return;
        }

        if (layerLocked[_layer] && !_bypassLock) return;
        layerLocked[_layer] = _lockLayer;

        if (_bypassLock)
        {
            foreach (var item in animator.GetBehaviours<onExit>())
            {
                if (item.layerIndex == _layer)
                {
                    item.cancel = true;
                }
            }
        }

        if (currentAnimation[_layer] == _animation) return;

        currentAnimation[_layer] = _animation;
        animator.CrossFade(animations[(int)currentAnimation[_layer]], _crossFade, _layer);
    }
}

public enum Animations
{
    IDLE,
    IDLEAIMING,
    IDLECROUCHING,
    IDLECROUCHINGAIMING,
    WALKFORWARD,
    WALKFORWARDLEFT,
    WALKFORWARDRIGHT,
    WALKBACKWARD,
    WALKBACKWARDLEFT,
    WALKBACKWARDRIGHT,
    WALKLEFT,
    WALKRIGHT,
    WALKCROUCHINGFORWARD,
    WALKCROUCHINGFORWARDLEFT,
    WALKCROUCHINGFORWARDRIGHT,
    WALKCROUCHINGBACKWARD,
    WALKCROUCHINGBACKWARDLEFT,
    WALKCROUCHINGBACKWARDRIGHT,
    WALKCROUCHINGLEFT,
    WALKCROUCHINGRIGHT,
    RUNFORWARD,
    RUNFORWARDLEFT,
    RUNFORWARDRIGHT,
    RUNBACKWARD,
    RUNBACKWARDLEFT,
    RUNBACKWARDRIGHT,
    RUNLEFT,
    RUNRIGHT,
    SPRINTFORWARD,
    SPRINTFORWARDLEFT,
    SPRINTFORWARDRIGHT,
    SPRINTBACKWARD,
    SPRINTBACKWARDLEFT,
    SPRINTBACKWARDRIGHT,
    SPRINTLEFT,
    SPRINTRIGHT,
    JUMPUP,
    JUMPAIR,
    JUMPDOWN,
    PUNCHCROSS,
    DEATH,
    DEATHFROMTHEFRONT,
    DEATHFROMTHEBACK,
    DEATHFROMRIGHT,
    DEATHFROMFRONTHEADSHOT,
    DEATHFROMBACKHEADSHOT,
    DEATHCROUCHINGHEADSHOTFRONT,
    DANCE,
    TURN90LEFT,
    TURN90RIGHT,
    CROUCHINGTURN90LEFT,
    CROUCHINGTURN90RIGHT,
    PISTOLIDLE,
    PISTOLSHOOT,
    PISTOLRELOAD,
    NONE
}
