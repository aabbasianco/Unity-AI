using UnityEngine;

public class ChaseState : INPCState
{
    public void Enter(NpcController npc)
    {
        Debug.Log("NPC started chasing");
        npc.animator.SetBool("run", true);
        npc.animator.SetBool("walk", false);
    }

    public void Update(NpcController npc)
    {

    }

    public void Exit(NpcController npc)
    {
        Debug.Log("NPC stopped chasing");
        npc.animator.SetBool("walk", true);
        npc.animator.SetBool("run", false);
    }
}
