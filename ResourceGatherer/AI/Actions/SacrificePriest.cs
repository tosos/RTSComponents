using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

public class SacrificePriest : Action
{
	public SacrificePriest()
	{
		actionName = "SacrificePriest";
	}

    private GameObject priest;

	public override ActionResult Start(Agent agent, float deltaTime)
	{
        priest = actionContext.GetContextItem<GameObject>("targetPriest");
        // TODO start the ritual animation
		return ActionResult.SUCCESS;
	}

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        Dispatcher.GetInstance ().Dispatch ("PriestSacrificed", priest.layer);
		return ActionResult.SUCCESS;
	}
}
