using System;
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
        Animator.StringToHash("Walk Left"),
        Animator.StringToHash("Walk Right"),
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

    public void Play(Animations _animation, int _layer, bool _lockLayer, bool _bypassLock, float _crossFade = .2f)
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
    IDLE1,
    IDLE2,
    IDLE3,
    WALKFWD,
    WALKBWD,
    WALKLEFT,
    WALKRIGHT,
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
