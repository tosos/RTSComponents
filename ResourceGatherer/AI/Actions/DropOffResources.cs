using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

public class DropOffResources : Action
{
	public DropOffResources()
	{
		actionName = "DropOffResources";
	}

    private bool startSucceded;
    private int carrying;

    public override ActionResult Start(Agent agent, float deltaTime) 
    {
        if (actionContext.ContextItemExists ("carrying")) {
            carrying = actionContext.GetContextItem<int> ("carrying");
            startSucceded = true;
            return ActionResult.SUCCESS;
        } else {
            startSucceded = false;
            return ActionResult.FAILURE;
        }
    }

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        if (startSucceded) {
            ResourceManager rm = ResourceManager.GetInstance (agent.Avatar.layer);
            rm.EarnResources (carrying);
		    return ActionResult.SUCCESS;
        } else {
            return ActionResult.FAILURE;
        }
	}

    public override ActionResult Stop(Agent agent, float deltaTime)
    {
        if (startSucceded) {
            actionContext.AddContextItem<int> ("carrying", 0);
        }
	    return ActionResult.SUCCESS;
    }
}
