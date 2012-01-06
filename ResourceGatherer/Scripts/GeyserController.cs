using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeyserController : DisplayBar {

    public int maxResources = 2000;
    public ParticleEmitter[] steams;

    private int resources;
    private float[] maxEnergies;
    private float[] minEnergies;

    private Transform platform;

    private List<int> owners;

    new void Awake () {
        base.Awake ();
        platform = transform.Find ("GeyserPlatform");
        platform.gameObject.SetActiveRecursively (false);

        resources = maxResources;
        
        maxEnergies = new float[steams.Length];
        minEnergies = new float[steams.Length];
        for (int i = 0; i < steams.Length; i ++) {
            maxEnergies[i] = steams[i].maxEnergy;
            minEnergies[i] = steams[i].minEnergy;
        }

        owners = new List<int> ();
    }

    public int Resources () {
        return resources;
    }

    public bool Empty () {
        return resources <= 0;
    }

    public int PullResources (int res) {
        if (resources < res) {
            // NOTE this is a race condition in multiplayer when resources are
            // near 0 should have minimal impact on the gameplay
            res = resources;
        } 
        if (res != 0) {
            if (Network.peerType == NetworkPeerType.Disconnected || networkView.isMine) {
                OwnerPull (res);
            } else {
                networkView.RPC ("OwnerPull", networkView.owner, res);
            }
        }
        return res;
    }

    [RPC]
    private void OwnerPull (int res) {
        resources -= res;
        if (resources < 0) {
            resources = 0;
        }
        if (Network.peerType == NetworkPeerType.Disconnected) {
            UpdateResources (resources);
        } else {
            networkView.RPC ("UpdateResources", RPCMode.All, resources);
        }
    }

    [RPC]
    private void UpdateResources (int res) {
        resources = res;
        if (resources <= 0) {
            currentRatio = 0.0f;
            foreach (ParticleEmitter steam in steams) {
                steam.enabled = false;
            }

            foreach (int own in owners) {
                ResourceManager.GetInstance (own).RemoveGeyser (this);
            }
            owners.Clear ();
            if (platform.gameObject.active == false) {
                RemoveGeyser ();
            }
        } else {
            currentRatio = (float)resources / (float)maxResources;
            for (int i = 0; i < steams.Length; i ++) {
                steams[i].maxEnergy = maxEnergies[i] * currentRatio;
                steams[i].minEnergy = minEnergies[i] * currentRatio;
            }
        }
    }

    // technically not a draggable, still this is called by the level manager when it positions it
    [RPC]
    private void PlaceDraggable () {
        SendMessageUpwards ("MarkDirty", 
            SendMessageOptions.DontRequireReceiver);

	    GridManager grid = GridManager.GetInstance ();
        int[] ind = grid.GridIndex (transform.position);
        int[] off = new int[2];

        int surfaceCount = 0;
        for (int i = -1; i <= 1; i ++) {
            for (int j = -1; j <= 1; j ++) {
                off[0] = ind[0] + i;
                off[1] = ind[1] + j;
                if (grid.HasSurface (off)) {
                    surfaceCount ++;
                }
            }
        }

        if (surfaceCount <= 3) {
            platform.gameObject.SetActiveRecursively (true);

            for (int i = -1; i <= 1; i ++) {
                for (int j = -1; j <= 1; j ++) {
                    off[0] = ind[0] + i;
                    off[1] = ind[1] + j;
                    grid.SetSurface (off);
                    if (i == -1) {
                        grid.SetEdgeConnectable (off, GridManager.Edge.West);
                    } else if (i == 1) {
                        grid.SetEdgeConnectable (off, GridManager.Edge.East);
                    }
                    if (j == -1) {
                        grid.SetEdgeConnectable (off, GridManager.Edge.South);
                    } else if (j == 1) {
                        grid.SetEdgeConnectable (off, GridManager.Edge.North);
                    }
                }
            }
        }

        for (int i = -1; i <= 1; i ++) {
            for (int j = -1; j <= 1; j ++) {
                off[0] = ind[0] + i;
                off[1] = ind[1] + j;
                grid.SetSurface (off);
                grid.SetOccupied (off);
            }
        }
    }

    private void RemoveGeyser () {
	    GridManager grid = GridManager.GetInstance ();
        int[] ind = grid.GridIndex (transform.position);
        int[] off = new int[2];
        
        for (int i = -1; i <= 1; i ++) {
            for (int j = -1; j <= 1; j ++) {
                off[0] = ind[0] + i;
                off[1] = ind[1] + j;
                grid.SetUnOccupied (off);
            }
        }

        gameObject.SetActiveRecursively (false);
        Renderer[] renderers  = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) {
            r.enabled = false;
        }
                
        SendMessageUpwards ("MarkDirty", 
            SendMessageOptions.DontRequireReceiver);
    }

    private void Reachable (int owner) {
        if (!owners.Contains (owner)) {
            ResourceManager.GetInstance (owner).AddGeyser (this);
            owners.Add (owner);
        }

	    GridManager grid = GridManager.GetInstance ();
        int[] ind = grid.GridIndex (transform.position);
        int[] off = new int[2];
        for (int i = -1; i <= 1; i ++) {
            for (int j = -1; j <= 1; j ++) {
                off[0] = ind[0] + i;
                off[1] = ind[1] + j;
                grid.SetValidPath (off);
            }
        }

        if (platform.gameObject.active) {
            if (Network.peerType == NetworkPeerType.Disconnected) {
                SetOwner (owner);
            } else {
                networkView.RPC ("SetOwner", RPCMode.All, owner);
            }
        }
    }

    [RPC]
    private void SetOwner (int owner) {
        gameObject.tag = LayerMask.LayerToName (owner);

	    GridManager grid = GridManager.GetInstance ();
        int[] ind = grid.GridIndex (transform.position);
        int[] off = new int[2];
        for (int i = -1; i <= 1; i ++) {
            for (int j = -1; j <= 1; j ++) {
                off[0] = ind[0] + i;
                off[1] = ind[1] + j;
                grid.SetOwner (off, owner);
            }
        }
    }
}
