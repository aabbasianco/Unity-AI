using UnityEngine;

public class AttackState : INPCState
{
    public void Enter(NpcController npc)
    {
        Debug.Log("NPC attacking!");
    }

    public void Update(NpcController npc)
    {

    }

    public void Exit(NpcController npc)
    {
        Debug.Log("NPC stopped attacking");
    }
}

