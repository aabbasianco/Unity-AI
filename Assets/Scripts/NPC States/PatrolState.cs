using UnityEngine;

public class PatrolState : INPCState
{
    private int currentPoint = 0;

    public void Enter(NpcController npc)
    {
        Debug.Log("NPC started patrolling");
    }

    public void Update(NpcController npc)
    {

    }

    public void Exit(NpcController npc)
    {
        Debug.Log("NPC stopped patrolling");
    }
}
