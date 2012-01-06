using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

public class CapturePriest : Action
{
	public CapturePriest()
	{
		actionName = "CapturePriest";
	}

    private bool startSucceded;
    private GameObject targetPriest;

    public override ActionResult Start(Agent agent, float deltaTime) 
    {
        if (actionContext.ContextItemExists("targetPriest")) {
            startSucceded = true;
            targetPriest = actionContext.GetContextItem<GameObject>("targetPriest");
		    return ActionResult.SUCCESS;
        } else {
            startSucceded = false;
		    return ActionResult.FAILURE;
        }
    }

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        if (startSucceded) {
            targetPriest.SetActiveRecursively (false);
		    return ActionResult.SUCCESS;
        } else {
		    return ActionResult.FAILURE;
        }
	}
}
