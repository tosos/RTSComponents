using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour {

    public static int startingResources = 8000;

    private int encryptor = 0;
    private int resources;
    private List<int[]> depots;
    private List<GeyserController> geysers;

    public bool encryptRuntime = false;

    static private Dictionary<int,ResourceManager> instances = null;
    static public ResourceManager GetInstance (int owner) {
        if (instances == null) {
            instances = new Dictionary<int,ResourceManager>();
        }
        if (!instances.ContainsKey (owner)) {
            foreach (ResourceManager panel in FindObjectsOfType(typeof(ResourceManager))) {
                if (panel.gameObject.layer == owner) {
                    instances.Add (owner, panel);
                    break;
                }
            }
        }
        return instances[owner];
    }

    void Awake () {
        if (encryptRuntime) {
            encryptor = (int)(Random.value * 100000000.0f);
        }

        resources = startingResources ^ encryptor;
        depots = new List<int[]> ();
        geysers = new List<GeyserController> ();
    }

    public int CurrentResources () {
        return resources ^ encryptor;
    }

    public void AddDepot (int[] ind) {
        depots.Add (ind);
    }

    public void RemoveDepot (int[] ind) {
        depots.Remove (ind);
    }

    public void AddGeyser (GeyserController geyser) {
        geysers.Add (geyser);
    }

    public void RemoveGeyser (GeyserController geyser) {
        geysers.Remove (geyser);
    }

    public bool LinkedGeyser (GeyserController geyser) {
        return geysers.Contains (geyser);
    }

    public int[] ShortestDepotPath (int[] startInd, Movable mover) {
        int[] shortestInd = null;
        float shortestLen = Mathf.Infinity;
        foreach (int[] stopInd in depots) {
            float len = mover.TravelDistance (stopInd);
            if (len < shortestLen && mover.CanReach (stopInd)) {
                shortestInd = stopInd;
                shortestLen = len;
            }
        }
        return shortestInd;
    }

    public GeyserController NearestGeyser (Vector3 pos) {
        GridManager grid = GridManager.GetInstance ();
        int[] start = grid.GridIndex (pos);
        GeyserController nearest = null;
        Vector3[] shortestPath = null;
        foreach (GeyserController gey in geysers) {
            if (gey.Resources() <= 0) {
                continue;
            }
            int[] end = grid.GridIndex (gey.transform.position);
            Vector3[] path = PathManager.FindPath (start, end);
            if (path == null) {
                continue;
            }
            if (shortestPath == null || path.Length < shortestPath.Length) {
                shortestPath = path;
                nearest = gey;
            }
        }
        return nearest;
    }

    public void EarnResources (int res) {
        resources = ((resources ^ encryptor) + res) ^ encryptor; 
    }

    public bool EnoughResources (int res) {
        return ((resources ^ encryptor) >= res);
    }

    public bool SpendResources (int res) {
        if ((resources ^ encryptor) < res) {
            return false;
        }
        resources = ((resources ^ encryptor) - res) ^ encryptor; 
        return true;
    }
}
