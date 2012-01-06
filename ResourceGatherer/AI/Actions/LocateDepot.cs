using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

public class LocateDepot : Action
{
	public LocateDepot()
	{
		actionName = "LocateDepot";
	}

    private int[] lastTarget = null;

	public override ActionResult Start(Agent agent, float deltaTime)
	{
        GridManager grid = GridManager.GetInstance ();
        int[] ind = grid.GridIndex (agent.Avatar.transform.position);
        // if (lastTarget == null) {
            ResourceManager rm = ResourceManager.GetInstance (agent.Avatar.layer);
            lastTarget = rm.ShortestDepotPath (ind, agent.Avatar.GetComponent<Movable>());
        // }
		return ActionResult.SUCCESS;
	}

	public override ActionResult Stop(Agent agent, float deltaTime)
    {
        actionContext.AddContextItem<int[]>("targetGridIndex", lastTarget);
		return ActionResult.SUCCESS;
    } 
}
