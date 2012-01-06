using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

public class MoveToGridIndex : Action
{
	public MoveToGridIndex()
	{
		actionName = "MoveToGridIndex";
	}

	public override ActionResult Start(Agent agent, float deltaTime)
	{
        if (actionContext.ContextItemExists("targetGridIndex")) {
            int[] end = actionContext.GetContextItem<int[]>("targetGridIndex");
            actionContext.RemoveContextItem("targetGridIndex");

            agent.Avatar.GetComponent<Movable>().SetDestination (end);

            actionContext.AddContextItem<bool>("waitingForArrival", true);
		    return ActionResult.SUCCESS;
        } else {
		    return ActionResult.FAILURE;
        }
	}

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        if (!actionContext.ContextItemExists("waitingForArrival")) {
            return ActionResult.FAILURE;
        } else if (!actionContext.GetContextItem<bool>("waitingForArrival")) {
            return ActionResult.SUCCESS;
        } else {
            return ActionResult.RUNNING;
        }
	}

    public override ActionResult Stop(Agent agent, float deltaTime) {
        return ActionResult.SUCCESS;    
    }
}
