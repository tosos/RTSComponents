using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

public class NearestOwnedGeyser : Action
{
	public NearestOwnedGeyser()
	{
		actionName = "NearestOwnedGeyser";
	}

    private GeyserController geyser;

    public override ActionResult Start(Agent agent, float deltaTime)
    {
        geyser = null;
        if (actionContext.ContextItemExists ("targetGeyser")) {
            // early termination check
            Transform targetGeyser = actionContext.GetContextItem<Transform>("targetGeyser");
            if (targetGeyser) {
                geyser = targetGeyser.GetComponent<GeyserController>();
            }
        }
		return ActionResult.SUCCESS;
    }

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        if (geyser == null || geyser.Resources () <= 0) {
            ResourceManager rm = ResourceManager.GetInstance (agent.Avatar.layer);
            geyser = rm.NearestGeyser (agent.Avatar.transform.position);
        }
		return (geyser == null ? ActionResult.FAILURE : ActionResult.SUCCESS);
    }

	public override ActionResult Stop(Agent agent, float deltaTime)
    {
        if (geyser != null) {
            actionContext.AddContextItem<Transform>("targetGeyser", geyser.transform);
            GridManager grid = GridManager.GetInstance ();
            int[] index = grid.GridIndex (geyser.transform.position);
            actionContext.AddContextItem<int[]>("targetGridIndex", index);
        } else {
            actionContext.AddContextItem<float>("activity", 0);
        }
		return ActionResult.SUCCESS;
	}
}
