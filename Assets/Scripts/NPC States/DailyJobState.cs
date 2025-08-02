using UnityEngine;

public class DailyJobState : INPCState
{
    public void Enter(NpcController npc)
    {
        Debug.Log("NPC started daily job");
    }

    public void Update(NpcController npc)
    {

    }

    public void Exit(NpcController npc)
    {
        Debug.Log("NPC finished daily job");
    }
}
