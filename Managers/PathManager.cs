using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathManager : MonoBehaviour {
    public int pathSearchLimit = 1024;

    static private PathManager instance = null;
    static public PathManager GetInstance () {
        if (instance == null) {
            instance = (PathManager) FindObjectOfType(typeof(PathManager));
        }
        return instance;
    }

    private class Node {
        public Node edge;
        public float g_score;
        public float h_score;
        public float f_score;
        public Vector3 p;
        static public int Comparison (Node a, Node b) {
            if (a.f_score < b.f_score) {
                return -1;
            } else if (a.f_score > b.f_score) {
                return 1;
            } else {
                return 0;
            }
        }
    }

    private class PathInstance {
        public PathInstance () {
            start = new int[2];
            end = new int[2];
        }
        public float lastUsed;
        public int[] start;
        public int[] end;
        public Vector3[] path;
        public static int Comparison (PathInstance a, PathInstance b) {
            if (a.lastUsed < b.lastUsed) {
                return -1;
            } else if (a.lastUsed > b.lastUsed) {
                return 1;
            } else {
                return 0;
            }
        }
    } 

    public int cacheLength = 10;
    private PriorityQueue<PathInstance> cache;
	void Awake () {
        if (instance != null) {
            Destroy (instance.gameObject);
        }
        instance = null;
        cache = new PriorityQueue<PathInstance> ();
        cache.comparator = PathInstance.Comparison;
    }

    private float SqDist (Vector3 start, Vector3 end) {
        float sqdist = (start - end).sqrMagnitude;
        return sqdist;
    }

    private void ReconstructPath (Node node, List<Vector3> Path) {
        Path.Insert (0, node.p);
        if (node.edge != null) {
            ReconstructPath (node.edge, Path);
        }
    }

    private List<Vector3> Neighbors (Vector3 node) {
        GridManager grid = GridManager.GetInstance ();
        List<Vector3> neighbors = new List<Vector3> ();
        int[] nodeInd;
        nodeInd = grid.GridIndex (node);

        int[] neighborInd = new int[2];
        for (int x = -1; x <= 1; x ++) {
            for (int y = -1; y <= 1; y ++) {
                if (x == 0 && y == 0) continue;
                neighborInd[0] = nodeInd[0] + x;
                neighborInd[1] = nodeInd[1] + y;
                if (grid.IsNeighbor (nodeInd, neighborInd)) { 
                    neighbors.Add (grid.GridCenter (neighborInd));
                }
            }
        }
        return neighbors;
    }

    private bool LineOfSite (Vector3 s, Vector3 e) {
        GridManager grid = GridManager.GetInstance ();
        int[] sInd = grid.GridIndex (s);
        int[] eInd = grid.GridIndex (e);
        int[] ind = new int[2];

        int dx = eInd[0] - sInd[0];
        int dy = eInd[1] - sInd[1];
        int f = 0;
        int sy = 1;
        if (dy < 0) {
            sy = -1;
            dy = -dy;
        }
        int sx = 1;
        if (dx < 0) {
            sx = -1;
            dx = -dx;
        }
        if (dx >= dy) {
            while (sInd[0] != eInd[0]) {
                f += dy;
                if (!grid.HasSurface (sInd)) {
                    return false;
                }
                if (f >= dx) {
                    sInd[1] += sy;
                    f -= dx;
                    if (!grid.HasSurface (sInd)) {
                        return false;
                    }
                }
                sInd[0] += sx;
            }
        } else {
            while (sInd[1] != eInd[1]) {
                f += dx;
                if (!grid.HasSurface (sInd)) {
                    return false;
                }
                if (f >= dy) {
                    sInd[0] += sx;
                    f -= dy;
                    if (!grid.HasSurface (ind)) {
                        return false;
                    }
                }
                sInd[1] += sy;
            }
        }
        return true;
    }

    static public Vector3[] FindPath (int[] start, int[] end) {
        PathManager mgr = GetInstance ();

        Vector3[] outPath;
        if (mgr.QueryCache (start, end, out outPath)) {
            return outPath;
        }

        int pathSearched = 0;

        GridManager grid = GridManager.GetInstance ();
        if (!grid.HasSurface (start) || !grid.HasSurface (end)) {
            return null;
        }

        List<Node> closed = new List<Node> ();
        PriorityQueue<Node> open = new PriorityQueue<Node> ();
        open.comparator = Node.Comparison;

        Vector3 goal = grid.GridCenter (end);

        Node s = new Node ();
        s.edge = null;
        s.g_score = 0.0f;
        s.h_score = mgr.SqDist (s.p, goal);
        s.f_score = s.h_score;
        s.p = grid.GridCenter (start);
        
        open.Add (s);

        while (!open.Empty ()) {
            Node node = open.Top;
            open.Remove (open.Top);

            // if it ends, end.
            if (node.p == goal) {
                List<Vector3> path = new List<Vector3> ();
                mgr.ReconstructPath (node, path);
                Vector3[] outpath = path.ToArray ();
                mgr.CachePath (start, end, outpath);
                return outpath;
            }

            closed.Add (node);

            List<Vector3> neighbors = mgr.Neighbors(node.p);
            foreach (Vector3 n in neighbors) {
                if (closed.FindIndex ((a) => a.p == n) >= 0) {
                    continue;
                }
                float temp_g = node.g_score + mgr.SqDist(node.p, n);
                Node f = open.Find ((a) => a.p == n);
                if (f == null) {
                    f = new Node ();
                    f.edge = node;
                    f.g_score = temp_g;
                    f.h_score = mgr.SqDist(n, goal);
                    f.f_score = f.g_score + f.h_score;
                    f.p = n;
                    open.Add (f);
                } else if (temp_g < f.g_score) {
                    f.edge = node; 
                    f.g_score = temp_g;
                    f.f_score = f.g_score + f.h_score;
                    open.Update (f);
                }
            }
            if (pathSearched >= mgr.pathSearchLimit) {
                return null;
            } else {
                pathSearched ++;
            }
        }
        return null;
    }

    private bool QueryCache (int[] start, int[] end, out Vector3[] outPath) {
        PathInstance pi = cache.Find (
            (a) => a.start[0] == start[0] && a.start[1] == start[1] && 
                   a.end[0] == end[0] && a.end[1] == end[1]);
        if (pi == null) {
            outPath = null;
            return false;
        } else {
            Debug.Log ("---------------------- Cache hit ------------------------");
            outPath = pi.path;
            pi.lastUsed = Time.time;
            cache.Update (pi);
            return true;
        }
    }

    private void CachePath (int[] start, int[] end, Vector3[] path) {
        Debug.Log ("---------------------- Cache miss -----------------------");
        PathInstance pi = new PathInstance ();
        pi.lastUsed = Time.time;
        Debug.Log ("caching start " + start[0] + "," + start[1] + " end " + end[0] + "," + end[1]);
        pi.start[0] = start[0];
        pi.start[1] = start[1];
        pi.end[0] = end[0];
        pi.end[1] = end[1];
        pi.path = path;
        cache.Add (pi);
        if (cache.Count > cacheLength) {
            cache.Remove (cache.Top);
        }
    }
}
