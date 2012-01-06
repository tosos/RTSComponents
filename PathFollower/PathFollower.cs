using UnityEngine;
using System.Collections;

public class PathFollower : Movable {
    public float unitSpeed = 1.0f;
    public float turnSpeed = 1.0f;
    public bool rotationInvariant = false;
    public bool occupyRisingStorm = false;

    private float rate = 0.0f;
    private float nextTime = -1.0f;

    private Vector3 lerpStart;
    private Vector3[] path = null;
    private int pathIndex = 0;
    private float angle;


    new void Awake () {
        base.Awake ();
        enabled = false;
    }

    void Start () {
        lerpStart = transform.position;
    }

	void Update () {
        if (Debug.isDebugBuild) {
            for (int i = 0; i < (path.Length - 1); i ++) {
                Debug.DrawLine (path[i], path[i+1], Color.white);
            }
        }

        if ((nextTime + Time.deltaTime) < rate) {
            float del = 0.0f;
            if (!rotationInvariant) {
                float rot = transform.rotation.eulerAngles.y;
                del = angle - rot;
                if (del > 180) {
                    del -= 360;
                } else if (del < -180) {
                    del += 360;
                }
            }
            float absDel = Mathf.Abs (del);
            // to big a turn, don't start walking yet
            if (absDel < 45) { 
                nextTime += Time.deltaTime;
                float ratio = nextTime / rate;
                transform.position = 
                    Vector3.Lerp (lerpStart, path[pathIndex], ratio);
            } 
            if (del > 3) {
                transform.Rotate (0.0f, turnSpeed * Time.deltaTime, 0.0f);
            } else if (del < -3) {
                transform.Rotate (0.0f, -turnSpeed * Time.deltaTime, 0.0f);
            }
        } else if (pathIndex < (path.Length - 1)) {
            pathIndex ++;
            NextTarget ();
        } else {
            Arrive ();
        }
    }

    private void NextTarget () {
        if (occupyRisingStorm) {
            GridManager grid = GridManager.GetInstance ();
            grid.SetUnOccupied (grid.GridIndex (lerpStart));
        }
        lerpStart = transform.position;
        if (occupyRisingStorm) {
            GridManager grid = GridManager.GetInstance ();
            grid.SetOccupied (grid.GridIndex (lerpStart));
        }
        Vector3 v = path[pathIndex] - lerpStart;
        rate = Vector3.Magnitude (v) / unitSpeed;
        if (!rotationInvariant) {
            angle = Vector3.Angle (Vector3.forward, v);
            if (v.x < 0) {
                angle = 360.0f - angle;
            }
        }
        nextTime = 0;
    }

    protected override void Arrive () {
        transform.position = path[path.Length - 1];
        enabled = false;
        base.Arrive ();
    }

    private void SetPath (Vector3[] p) {
        path = p;
        if (path == null || path.Length == 0) {
            enabled = false;
            base.Arrive ();
        }  else {
            enabled = true;
            pathIndex = 0;
            NextTarget ();
        }
    }

    protected override void SetDestinationInternal (int[] end) {
        GridManager grid = GridManager.GetInstance ();
        int[] start = grid.GridIndex (transform.position);

        SetPath (PathManager.FindPath (start, end));
    }

    public override bool CanReach (int[] end) {
        GridManager grid = GridManager.GetInstance ();
        int[] start = grid.GridIndex (transform.position);
        return PathManager.FindPath (start, end) != null;
    }

    public override float TravelDistance (int[] end) {
        GridManager grid = GridManager.GetInstance ();
        int[] start = grid.GridIndex (transform.position);
        Vector3[] path = PathManager.FindPath (start, end);
        return (path == null ? 0 : path.Length);
    }

}
