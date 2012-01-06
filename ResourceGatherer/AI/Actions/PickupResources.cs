using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

public class PickupResources : Action
{
	public PickupResources()
	{
		actionName = "PickupResources";
	}

    private Transform targetGeyser;
    private int canCarry;
    private int carrying;
    private float countDown;
    private bool succeeded;
    
	public override ActionResult Start(Agent agent, float deltaTime)
	{
        if (actionContext.ContextItemExists ("carrying")) {
            carrying = actionContext.GetContextItem<int> ("carrying"); 
        } else {
            carrying = 0;
        }

        if (actionContext.ContextItemExists ("targetGeyser") && 
            actionContext.ContextItemExists ("canCarry") && 
            actionContext.ContextItemExists ("resourceTime")) 
        {
            targetGeyser = actionContext.GetContextItem<Transform>("targetGeyser");
            canCarry = actionContext.GetContextItem<int>("canCarry");
            // wait for resourceTime seconds while "mining"
            countDown = actionContext.GetContextItem<float>("resourceTime");
            succeeded = true;
		    return ActionResult.SUCCESS;
        } else {
            succeeded = false;
		    return ActionResult.FAILURE;
        }
	}

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        if (succeeded) {
            countDown -= deltaTime;
            if (countDown <= 0) {
		        return ActionResult.SUCCESS;
            } else {
                return ActionResult.RUNNING;
            }
        } else {
	        return ActionResult.FAILURE;
        }
	}

	public override ActionResult Stop(Agent agent, float deltaTime)
    {
        if (succeeded && carrying <= 0) {
            carrying = targetGeyser.GetComponent<GeyserController>().PullResources (canCarry);
            actionContext.AddContextItem<int> ("carrying", carrying);
	        return ActionResult.SUCCESS;
        } else {
	        return ActionResult.FAILURE;
        }
    }
}
