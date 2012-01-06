using UnityEngine;
using System.Collections;

public class ResourceDepot : MonoBehaviour {

    public Vector2[] dropOffsets;

	void OnConstructionFinished () {
        int layer = LayerMask.NameToLayer (transform.root.gameObject.tag);
	    foreach (Vector2 drop in dropOffsets) {
            GridManager grid = GridManager.GetInstance ();
            int[] ind = grid.GridIndex (transform.position);

            int[] off = new int[2];
            off[0] = ind[0] + (int)drop.x;
            off[1] = ind[1] + (int)drop.y;
            ResourceManager.GetInstance (layer).AddDepot (off);
        }
	}

    void OnSalvage () {
        RemoveDepot ();
    }

    void OnDying () {
	    RemoveDepot ();
    }

    void RemoveDepot () {
        int layer = LayerMask.NameToLayer (transform.root.gameObject.tag);
	    foreach (Vector2 drop in dropOffsets) {
            GridManager grid = GridManager.GetInstance ();
            int[] ind = grid.GridIndex (transform.position);

            int[] off = new int[2];
            off[0] = ind[0] + (int)drop.x;
            off[1] = ind[1] + (int)drop.y;
            ResourceManager.GetInstance (layer).RemoveDepot (off);
        }
    }

    public void PutResources (int res) {
        int layer = LayerMask.NameToLayer (transform.root.gameObject.tag);
        ResourceManager.GetInstance (layer).EarnResources (res);
    }

    void OnDrawGizmos () {
        foreach (Vector2 drop in dropOffsets) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere (transform.position + Vector3.right*drop.x*5 + Vector3.forward*drop.y*5, 1);
        }
    }
}
