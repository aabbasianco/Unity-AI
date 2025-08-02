using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class IdleState : INPCState
{
    public void Enter(NpcController npc)
    {
        Debug.Log("NPC entered Idle");
        npc.animator.SetBool("idle", true);
    }

    public void Update(NpcController npc)
    {

    }

    public void Exit(NpcController npc)
    {
        Debug.Log("NPC exiting Idle");
        npc.animator.SetBool("idle", false);
    }
}
