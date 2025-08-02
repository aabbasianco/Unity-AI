using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine.InputSystem.LowLevel;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NpcController : MonoBehaviour
{
    private INPCState currentState;
    private Stack<INPCState> taskQueue = new Stack<INPCState>();

    [Header("NPC Settings")]
    public Transform[] patrolPoints;
    public Transform target; // مثلاً بازیکن
    private int patrolIndex = 0;

    [Header("Components")]
    public NavMeshAgent agent;
    public Animator animator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        ChangeState(new IdleState()); // شروع با Idle
    }

    private void Update()
    {
        currentState?.Update(this);
    }

    public void ChangeState(INPCState newState, bool saveCurrent = true)
    {
        if (currentState != null)
        {
            if (saveCurrent) taskQueue.Push(currentState);
            currentState.Exit(this);
        }

        currentState = newState;
        currentState.Enter(this);
    }

    public void ReturnToPreviousTask()
    {
        if (taskQueue.Count > 0)
        {
            currentState.Exit(this);
            currentState = taskQueue.Pop();
            currentState.Enter(this);
        }
        else
        {
            ChangeState(new IdleState(), false);
        }
    }

    // ===== ابزارهای کمکی =====
    //public void MoveTo(Vector3 destination, float speed)
    //{
    //    agent.speed = speed;
    //    agent.SetDestination(destination);
    //}
}
