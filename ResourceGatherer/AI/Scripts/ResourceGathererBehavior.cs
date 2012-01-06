using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.BehaviorTrees;

public class ResourceGathererBehavior : BTActivationManager
{
    public int canCarry = 200;
    public float resourceTime = 0.5f;

    private bool isConstructed = false;

    public AudioClip[] acknowledgements;
    public AudioClip[] confirmations;

    protected void Awake () {
        transform.parent.GetComponent<RAINAgent>().enabled = false;
    }

	public override void InitBehavior(Agent actor)
	{
        actor.actionContext.AddContextItem<int> ("canCarry", canCarry);
        actor.actionContext.AddContextItem<float> ("resourceTime", resourceTime);
	}

	protected override void PreAction(Agent actor, float deltaTime)
	{
	}

    void OnConstructionFinished () {
        isConstructed = true;
        if (Network.peerType == NetworkPeerType.Disconnected || transform.parent.networkView.isMine)
        {
            transform.parent.GetComponent<RAINAgent>().enabled = true;
        }
    }

    void OnDying () {
        isConstructed = false;
    }

    void OnSelected (bool isSelected) {
        if (isConstructed && isSelected) {
            PlayAcknowledgement ();
        }
    }

    void OnPathInitiated () {
        if (Network.peerType == NetworkPeerType.Disconnected || transform.parent.networkView.isMine)
        {
            Agent agent = transform.parent.GetComponent<RAINAgent>().Agent;
            agent.actionContext.RemoveContextItem ("waitingForArrival");
        }
    }

    void OnArrived () {
        if (Network.peerType == NetworkPeerType.Disconnected || transform.parent.networkView.isMine)
        {
            Agent agent = transform.parent.GetComponent<RAINAgent>().Agent;
            agent.actionContext.AddContextItem<bool> ("waitingForArrival", false);
        }
    }

    void OnClickCommand (Ray ray) {
        if (!isConstructed) return;

        Agent agent = transform.parent.GetComponent<RAINAgent>().Agent;

        if (agent.actionContext.ContextItemExists ("waitingForArrival")) {
            // if we're currently walking, stop because we just got a new command
            agent.actionContext.RemoveContextItem ("waitingForArrival");
        }

        RaycastHit hit;
        if (Physics.Raycast (ray.origin, ray.direction, out hit,
                Mathf.Infinity, ConfigManager.GetInstance ().geyserLayer)) 
        {
            agent.actionContext.AddContextItem<float>("activity", 1);
            agent.actionContext.AddContextItem<Transform>("targetGeyser", hit.transform);
            PlayConfirmation ();
        } 
        else if (Physics.Raycast (ray.origin, ray.direction, out hit,
                Mathf.Infinity, ConfigManager.GetInstance ().depotLayer)) 
        {
            agent.actionContext.AddContextItem<float>("activity", 1);
            agent.actionContext.AddContextItem<Transform>("targetDepot", hit.transform);
            PlayConfirmation ();
        } 
        else 
        {
            GridManager grid = GridManager.GetInstance ();
            agent.actionContext.AddContextItem<float>("activity", 0);
            int[] end = grid.MouseGridIndex ();
            if (grid.HasSurface (end) && grid.IsValidPath (end)) {
                Debug.Log ("Setting the end point");
                agent.actionContext.AddContextItem<int[]>("targetGridIndex", end);
                PlayConfirmation ();
            }
        }
    }

    private void PlayAcknowledgement () {
        if (acknowledgements.Length > 0) {
            int index = Random.Range (0, acknowledgements.Length);
            audio.PlayOneShot (acknowledgements[index]);
        }
    }

    private void PlayConfirmation () {
        if (confirmations.Length > 0) {
            int index = Random.Range (0, confirmations.Length);
            audio.PlayOneShot (confirmations[index]);
        }
    }
}
