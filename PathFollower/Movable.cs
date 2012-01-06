using UnityEngine;
using System.Collections;

public abstract class Movable : Listenable {

    protected virtual void Arrive () {
        if (!networkView || Network.peerType == NetworkPeerType.Disconnected) {
            ArriveMessage ();
        } else {
            networkView.RPC ("ArriveMessage", RPCMode.All);
        }
    }

    [RPC]
    private void ArriveMessage () {
        BroadcastMessage ("OnArrived", null, 
            SendMessageOptions.DontRequireReceiver);
        Shout ("OnUnitArrived");
    }

    public void SetDestination (int[] end) {
        if (!networkView || Network.peerType == NetworkPeerType.Disconnected) {
            SetDestinationAll (transform.position, end[0], end[1]);
        } else {
            networkView.RPC ("SetDestinationAll", RPCMode.All, transform.position, end[0], end[1]);
        }
    }

    [RPC]
    private void SetDestinationAll (Vector3 current, int one, int two) {
        BroadcastMessage ("OnPathInitiated", null, 
            SendMessageOptions.DontRequireReceiver);
        Shout ("OnUnitPathInitiated");

        transform.position = current;

        int[] end = new int[2];
        end[0] = one;
        end[1] = two;
        SetDestinationInternal (end);
    }

    protected abstract void SetDestinationInternal (int[] end);

    public abstract bool CanReach (int[] end);

    public abstract float TravelDistance (int[] end);

}
