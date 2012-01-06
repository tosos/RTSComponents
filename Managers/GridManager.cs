using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour {
    public int gridLayer = 8;
    public int islandLayer = 9;
    public int elementXDimension;
    public int elementYDimension;
    public int elementXSize;
    public int elementYSize;

    public Color neutralColor;
    public Color player1Color;
    public Color player2Color;
    public Color player3Color;
    public Color player4Color;
    public Color player5Color;
    public Color player6Color;
    public Color player7Color;
    public Color player8Color;

    private Texture2D miniMap;

    public enum Edge : byte {
        West = 1,
        North = 2,
        East = 4,
        South = 8
    }
    private enum Meta : byte {
        // surface is used for detecting paths
        Surface = 16,
        // occupied determines whether something can be built
        Occupied = 32,
        // bridged determines if connecting "nextto" makes sense
        Bridged = 64,
        // valid path is only valid for the player, there are many surfaces which are not valid
        // especially at the start of the game
        // it might be possible to specify surfaces as they belong to a player..
        ValidPath = 128
    }

    public enum Ownership : byte {
        Mask = 15,
        Neutral = 0,
        Player1 = 1,
        Player2 = 2,
        Player3 = 3,
        Player4 = 4,
        Player5 = 5,
        Player6 = 6,
        Player7 = 7,
        Player8 = 8
    }

    public enum Neighbors : byte {
        NW = 1,
        N = 2,
        NE = 4,
        E = 8,
        SE = 16,
        S = 32,
        SW = 64,
        W = 128
    }

    private int player1Layer = 0;
    private int player2Layer = 0;
    private int player3Layer = 0;
    private int player4Layer = 0;
    private int player5Layer = 0;
    private int player6Layer = 0;
    private int player7Layer = 0;
    private int player8Layer = 0;

    public bool displayGrid = false;
    public bool displayConnectivity = false;
    public bool displayIsBridged = false;

    private Vector3 gridSize; 

    private int[,] surfaceGrid;
    private int[,] ownershipGrid;
    private int[,] neighborGrid;
    public List<int[]> connectableEdges;

    private Matrix4x4 guiMat;

    static private GridManager instance = null;
    static public GridManager GetInstance () {
        if (instance == null) {
            instance = (GridManager) FindObjectOfType(typeof(GridManager));
        }
        return instance;
    }

	void Awake () {
        if (instance != null) {
            Destroy (instance.gameObject);
        }
        instance = null;

        gridSize.x = elementXSize * elementXDimension;
        // gridSize.y = 500;
        gridSize.z = elementYSize * elementYDimension;
        surfaceGrid = new int[elementXDimension, elementYDimension];
        ownershipGrid = new int[elementXDimension, elementYDimension];
        neighborGrid = new int[elementXDimension, elementYDimension];

        connectableEdges = new List<int[]> ();

        BoxCollider box = (BoxCollider) collider;
        box.size = gridSize;
        box.center = gridSize * 0.5f;

        for (int x = 0; x < elementXDimension; x ++) {
            for (int y = 0; y < elementYDimension; y ++) {
                surfaceGrid[x,y] = 0;
                ownershipGrid[x,y] = (int)Ownership.Neutral;
            }
        }

        miniMap = new Texture2D (128, 128, TextureFormat.RGB24, false);
        for (int i = 0; i < 128; i ++) {
            for (int j = 0; j < 128; j ++) {
                miniMap.SetPixel (i, j, Color.black);
            }
        }
        miniMap.Apply ();

        player1Layer = LayerMask.NameToLayer ("player1");
        player2Layer = LayerMask.NameToLayer ("player2");
        player3Layer = LayerMask.NameToLayer ("player3");
        player4Layer = LayerMask.NameToLayer ("player4");
        player5Layer = LayerMask.NameToLayer ("player5");
        player6Layer = LayerMask.NameToLayer ("player6");
        player7Layer = LayerMask.NameToLayer ("player7");
        player8Layer = LayerMask.NameToLayer ("player8");
	}


    void Start () {
        InitializeBorder ();

        Dispatcher.GetInstance ().Register ("ResolutionChange", gameObject, true);
        guiMat = Matrix4x4.TRS (Vector3.zero, Quaternion.identity,
                                    new Vector3 (Screen.width / 1024.0f,
                                                 Screen.height / 768.0f, 1));
    }

    void ResolutionChange () {
        guiMat = Matrix4x4.TRS (Vector3.zero, Quaternion.identity,
                                    new Vector3 (Screen.width / 1024.0f,
                                                 Screen.height / 768.0f, 1));
    }

    void OnDrawGizmos () {
        if (displayGrid) {
            for (int x = 0; x < elementXDimension; x ++) {
                for (int y = 0; y < elementYDimension; y ++) {
                    Vector3[] v = GenerateCorners (x, y);
                    v[0] += Vector3.forward + Vector3.right;
                    v[1] += -Vector3.forward + Vector3.right;
                    v[2] += -Vector3.forward - Vector3.right;
                    v[3] += Vector3.forward - Vector3.right;
    
                    Gizmos.color = ((surfaceGrid[x,y] & 1) == 1) ? Color.red : Color.blue;
                    Gizmos.DrawLine (v[0], v[1]);
                    Gizmos.color = ((surfaceGrid[x,y] & 2) == 2) ? Color.green : Color.blue;
                    Gizmos.DrawLine (v[1], v[2]);
                    Gizmos.color = ((surfaceGrid[x,y] & 4) == 4) ? Color.red : Color.blue;
                    Gizmos.DrawLine (v[2], v[3]);
                    Gizmos.color = ((surfaceGrid[x,y] & 8) == 8) ? Color.green : Color.blue;
                    Gizmos.DrawLine (v[3], v[0]);
                    if ((surfaceGrid[x,y] & (int)Meta.Surface) == (int)Meta.Surface) {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine (v[0], v[2]);
                        Gizmos.DrawLine (v[3], v[1]);
                    }
                }
            }
        }

        if (displayConnectivity) {
            for (int x = 0; x < elementXDimension; x ++) {
                for (int y = 0; y < elementYDimension; y ++) {
                    Vector3[] v = GenerateCorners (x, y);
                    v[0] += Vector3.forward + Vector3.right;
                    v[1] += -Vector3.forward + Vector3.right;
                    v[2] += -Vector3.forward - Vector3.right;
                    v[3] += Vector3.forward - Vector3.right;
    
                    Gizmos.color = Color.white;
                    if ((neighborGrid[x,y] & 1) == 1) {
                        Gizmos.DrawSphere (v[1], .5f);
                    }
                    if ((neighborGrid[x,y] & 2) == 2) {
                        Gizmos.DrawSphere ((v[1] + v[2]) * .5f, .5f);
                    }
                    if ((neighborGrid[x,y] & 4) == 4) {
                        Gizmos.DrawSphere (v[2], .5f);
                    }
                    if ((neighborGrid[x,y] & 8) == 8) {
                        Gizmos.DrawSphere ((v[2] + v[3]) * .5f, .5f);
                    }
                    if ((neighborGrid[x,y] & 16) == 16) {
                        Gizmos.DrawSphere (v[3], .5f);
                    }
                    if ((neighborGrid[x,y] & 32) == 32) {
                        Gizmos.DrawSphere ((v[3] + v[0]) * .5f, .5f);
                    }
                    if ((neighborGrid[x,y] & 64) == 64) {
                        Gizmos.DrawSphere (v[0], .5f);
                    }
                    if ((neighborGrid[x,y] & 128) == 128) {
                        Gizmos.DrawSphere ((v[0] + v[1]) * .5f, .5f);
                    }
                }
            }
        }

        if (displayIsBridged) {
            for (int x = 0; x < elementXDimension; x ++) {
                for (int y = 0; y < elementYDimension; y ++) {
                    int[] ind = new int[2];
                    ind[0] = x;
                    ind[1] = y;
                    Vector3 c = GridCenter (ind);
    
                    Gizmos.color = Color.red;
                    if ((surfaceGrid[x,y] & (int)Meta.Bridged) == (int)Meta.Bridged) {
                        Gizmos.DrawSphere (c, 2.5f);
                    }
                }
            }
        }
    }

    void OnGUI () {
        GUI.depth = 0;
        GUI.matrix = guiMat;

        GUI.Label (new Rect (11, 678, 85, 85), miniMap);
    }

    private void InitializeBorder () {
/*
        Vector2 half = gridSize * 0.5f;
        LineRenderer borderRender = GetComponent<LineRenderer>();
        borderRender.SetVertexCount (5);
        borderRender.SetPosition (0, new Vector3 (-half.x, 0, -half.y));
        borderRender.SetPosition (1, new Vector3 (-half.x, 0, half.y));
        borderRender.SetPosition (2, new Vector3 (half.x, 0, half.y));
        borderRender.SetPosition (3, new Vector3 (half.x, 0, -half.y));
        borderRender.SetPosition (4, new Vector3 (-half.x, 0, -half.y));
*/
    }

    private Vector3[] GenerateCorners (int x, int y) {
        Vector3[] v = new Vector3 [4];
        v[0].x = transform.position.x + x * elementXSize;
        v[0].y = 0;
        v[0].z = transform.position.z + y * elementYSize;
        v[1] = v[0] + Vector3.forward * elementYSize;
        v[2] = v[1] + Vector3.right * elementXSize;
        v[3] = v[2] - Vector3.forward * elementYSize;
        return v;
    }

    private int TestCorners (int x, int y, out bool surface) {
        Vector3[] v = GenerateCorners (x, y);
        bool[] corners = new bool[4];
        surface = false;
        for (int i = 0; i < 4; i ++) {
            if (Physics.Raycast (v[i] + Vector3.up * 1e9f, Vector3.down, 
                                 Mathf.Infinity, 1 << islandLayer)) {
                corners[i] = true;
                surface = true;
            }
        }
        int res = 0;
        if (surface) {
            if (!corners[0] && !corners[1]) res |= 1 << 0;
            if (!corners[1] && !corners[2]) res |= 1 << 1;
            if (!corners[2] && !corners[3]) res |= 1 << 2;
            if (!corners[3] && !corners[0]) res |= 1 << 3;
        }
        return res;
    }

    public Vector3 GridSize () {
        return gridSize;
    }

    public Vector3 GridCenter (int[] ind) {
        Vector3 ret;
        ret.x = elementXSize * ind[0] + elementXSize * 0.5f;
        ret.y = 0;
        ret.z = elementYSize * ind[1] + elementYSize * 0.5f;
        return ret + transform.position;
    }

    public int[] GridIndex (Vector3 pos) {
        pos -= transform.position;
        int[] ret = new int[2];
        ret[0] = Mathf.FloorToInt (pos.x  / elementXSize);
        ret[1] = Mathf.FloorToInt (pos.z / elementYSize);
        return ret;
    }

    public int[] MouseGridIndex () {
        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
        RaycastHit hitInfo;
        Vector3 pos = Vector3.zero;
        if (Physics.Raycast (ray.origin, ray.direction, out hitInfo,
                Mathf.Infinity, 1 << gridLayer)) {
            pos = hitInfo.point;
        }
        return GridIndex (pos);
    }

    public Vector3 CenterOnGrid (Vector3 pos) {
        return GridCenter (GridIndex (pos));
    }

    private bool ValidIndex (int[] grid) {
        return grid[0] > 0 && grid[1] > 0 && 
            grid[0] < elementXDimension && grid[1] < elementYDimension;
    }

    public bool HasSurface (int[] grid) {
        if (!ValidIndex(grid)) return false;
        return (surfaceGrid[grid[0],grid[1]] & (int)Meta.Surface) == (int)Meta.Surface;
    }

    public void SetSurface (int[] grid) {
        if (ValidIndex(grid)) {
            surfaceGrid[grid[0],grid[1]] |= (int)Meta.Surface;
            UpdateNeighbors(grid);
/*
            Texture2D map = BuildPanel.GetInstance().GetMiniMap ();
            map.SetPixel (grid[0], grid[1], Color.white);
            map.Apply ();
*/
        }
    }

    public void SetSurfaceEmpty (int[] grid) {
        if (ValidIndex(grid)) {
            int allEdges = (int)(Edge.North | Edge.South | Edge.West | Edge.East);
            surfaceGrid[grid[0],grid[1]] &= ~((int)Meta.Surface | allEdges);
            UpdateNeighbors(grid);
/*
            miniMap.SetPixel (grid[0], grid[1], Color.black);
            miniMap.Apply ();
*/
        }
    }

    public bool EdgeConnectable (int[] grid, Edge edge) {
        if (!ValidIndex(grid)) return false;
        return (surfaceGrid[grid[0],grid[1]] & (int)edge) == (int)edge;
    }

    public bool IndexConnectable (int [] grid) {
        if (!ValidIndex(grid)) return false;
        // test all edges at once
        int allEdges = (int)(Edge.North | Edge.South | Edge.West | Edge.East);
        return (surfaceGrid[grid[0],grid[1]] & allEdges) > 0;
    }

    public void SetEdgeConnectable (int[] grid, Edge edge) {
        if (ValidIndex(grid)) {
            if (!IndexConnectable(grid)) {
                connectableEdges.Add (grid);
            }
            surfaceGrid[grid[0],grid[1]] |= (int)edge;
            UpdateNeighbors(grid);
        }
    }

    public void SetEdgeUnconnectable (int[] grid, Edge edge) {
        if (ValidIndex(grid)) {
            surfaceGrid[grid[0],grid[1]] &= ~(int)edge;
            if (!IndexConnectable(grid)) {
                connectableEdges.Remove (grid);
            }
            UpdateNeighbors(grid);
        }
    }

    public void ClearConnectivity (int[] grid) {
        int allEdges = (int)(Edge.North | Edge.South | Edge.West | Edge.East);
        if (ValidIndex(grid)) {
            surfaceGrid[grid[0],grid[1]] &= ~allEdges;
            if (connectableEdges.Contains (grid)) {
                connectableEdges.Remove (grid);
            }
            UpdateNeighbors(grid);
        }
    }

    public bool IsOccupied (int[] grid) {
        if (!ValidIndex(grid)) return false;
        return (surfaceGrid[grid[0],grid[1]] & (int)Meta.Occupied) == (int)Meta.Occupied;
    }

    public void SetOccupied (int[] grid) {
        if (ValidIndex(grid)) surfaceGrid[grid[0],grid[1]] |= (int)Meta.Occupied;
    }

    public void SetUnOccupied (int[] grid) {
        if (ValidIndex(grid)) surfaceGrid[grid[0],grid[1]] &= ~(int)Meta.Occupied;
    }

    public bool IsBridged (int[] grid) {
        if (!ValidIndex(grid)) return false;
        return (surfaceGrid[grid[0],grid[1]] & (int)Meta.Bridged) == (int)Meta.Bridged;
    }

    public void SetBridged (int[] grid) {
        if (ValidIndex(grid)) surfaceGrid[grid[0],grid[1]] |= (int)Meta.Bridged;
    }

    public void SetUnBridged (int[] grid) {
        if (ValidIndex(grid)) surfaceGrid[grid[0],grid[1]] &= ~(int)Meta.Bridged;
    }

    public bool IsValidPath (int[] grid) {
        if (!ValidIndex (grid)) return false;
        return (surfaceGrid[grid[0],grid[1]] & (int)Meta.ValidPath) == (int)Meta.ValidPath;
    }

    public void SetValidPath (int [] grid) {
        if (ValidIndex (grid)) surfaceGrid[grid[0],grid[1]] |= (int)Meta.ValidPath;
    }

    public void SetInvalidPath (int [] grid) {
        if (ValidIndex (grid)) surfaceGrid[grid[0],grid[1]] &= ~(int)Meta.ValidPath;
    }

    public void SetOwner (int[] grid, Ownership owner) {
        if (ValidIndex(grid)) {
            ownershipGrid[grid[0],grid[1]] = (int)owner;
            Color c = neutralColor;
            switch (owner) {
                case Ownership.Player1:
                    c = player1Color;
                    break;
                case Ownership.Player2:
                    c = player2Color;
                    break;
                case Ownership.Player3:
                    c = player3Color;
                    break;
                case Ownership.Player4:
                    c = player4Color;
                    break;
                case Ownership.Player5:
                    c = player5Color;
                    break;
                case Ownership.Player6:
                    c = player6Color;
                    break;
                case Ownership.Player7:
                    c = player7Color;
                    break;
                case Ownership.Player8:
                    c = player8Color;
                    break;
            }
            miniMap.SetPixel (grid[0], grid[1], c);
            miniMap.Apply ();
        }
    }

    public void SetOwner (int[] grid, int owner) {
        if (owner == 0) {
            SetOwner (grid, Ownership.Neutral);
        } else if (owner == player1Layer) {
            SetOwner (grid, Ownership.Player1);
        } else if (owner == player2Layer) {
            SetOwner (grid, Ownership.Player2);
        } else if (owner == player3Layer) {
            SetOwner (grid, Ownership.Player3);
        } else if (owner == player4Layer) {
            SetOwner (grid, Ownership.Player4);
        } else if (owner == player5Layer) {
            SetOwner (grid, Ownership.Player5);
        } else if (owner == player6Layer) {
            SetOwner (grid, Ownership.Player6);
        } else if (owner == player7Layer) {
            SetOwner (grid, Ownership.Player7);
        } else if (owner == player8Layer) {
            SetOwner (grid, Ownership.Player8);
        }
    }

    public void SetOwner (int[] grid, string owner) {
        if (owner == "neutral") {
            SetOwner (grid, Ownership.Neutral);
        } else if (owner == "player1") {
            SetOwner (grid, Ownership.Player1);
        } else if (owner == "player2") {
            SetOwner (grid, Ownership.Player2);
        } else if (owner == "player3") {
            SetOwner (grid, Ownership.Player3);
        } else if (owner == "player4") {
            SetOwner (grid, Ownership.Player4);
        } else if (owner == "player5") {
            SetOwner (grid, Ownership.Player5);
        } else if (owner == "player6") {
            SetOwner (grid, Ownership.Player6);
        } else if (owner == "player7") {
            SetOwner (grid, Ownership.Player7);
        } else if (owner == "player8") {
            SetOwner (grid, Ownership.Player8);
        }
    }

    public void SetOwnerNeutral (int[] grid) {
        SetOwner (grid, Ownership.Neutral);
    }
    
    public Ownership GetOwner (int[] grid) {
        if (!ValidIndex(grid)) return Ownership.Neutral;
        return (Ownership) ownershipGrid[grid[0], grid[1]];
    }

    public bool IsOwner (int[] grid, Ownership owner) {
        if (!ValidIndex(grid)) return false;
        return ownershipGrid[grid[0], grid[1]] == (int)owner;
    }

    public bool IsOwner (int[] grid, string owner) {
        if (!ValidIndex(grid)) return false;
        if (owner == "neutral" && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Neutral) {
            return true;
        } else if (owner == "player1" && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Player1) {
            return true;
        } else if (owner == "player2" && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Player2) {
            return true;
        } else if (owner == "player3" && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Player3) {
            return true;
        } else if (owner == "player4" && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Player4) {
            return true;
        } else if (owner == "player5" && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Player5) {
            return true;
        } else if (owner == "player6" && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Player6) {
            return true;
        } else if (owner == "player7" && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Player7) {
            return true;
        } else if (owner == "player8" && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Player8) {
            return true;
        } else {
            return false;
        }
    }

    public bool IsOwner (int[] grid, int owner) {
        if (!ValidIndex(grid)) return false;
        if (owner == 0 && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Neutral) {
            return true;
        } else if (owner == player1Layer && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Player1) {
            return true;
        } else if (owner == player2Layer && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Player2) {
            return true;
        } else if (owner == player3Layer && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Player3) {
            return true;
        } else if (owner == player4Layer && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Player4) {
            return true;
        } else if (owner == player5Layer && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Player5) {
            return true;
        } else if (owner == player6Layer && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Player6) {
            return true;
        } else if (owner == player7Layer && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Player7) {
            return true;
        } else if (owner == player8Layer && ownershipGrid[grid[0], grid[1]] == (int)Ownership.Player8) {
            return true;
        } else {
            return false;
        }
    }

    public bool IsNeutral (int[] grid) {
        if (!ValidIndex(grid)) return false;
        return ownershipGrid[grid[0], grid[1]] == (int)Ownership.Neutral;
    }

    public bool IsMine (int[] grid, int layer) {
        return IsOwner (grid, layer);
    }

    public Transform GetOccupant (int[] grid, LayerMask layer) {
        Vector3 center = GridCenter (grid);
        RaycastHit hitInfo;
        if (Physics.Raycast (center + Vector3.up * 150.0f, -Vector3.up, out hitInfo,
                             Mathf.Infinity, layer)) 
        {
            return hitInfo.transform;
        }
        return null;
    }

    public bool IsNeighbor (int[] node, int[] neighbor) {
        int dx = neighbor[0] - node[0];
        int dy = neighbor[1] - node[1];

        Neighbors node_dir;
        if (dy > 0) {
            if (dx < 0) {
                node_dir = Neighbors.NW;
            } else if (dx == 0) {
                node_dir = Neighbors.N;
            } else /* if (dx > 0) */ {
                node_dir = Neighbors.NE;
            }
        } else if (dy == 0) {
            if (dx < 0) {
                node_dir = Neighbors.W;
            } else if (dx == 0) {
                return false;
            } else /* if (dx > 0) */ {
                node_dir = Neighbors.E;
            }
        } else /* if (dy < 0) */ {
            if (dx < 0) {
                node_dir = Neighbors.SW;
            } else if (dx == 0) {
                node_dir = Neighbors.S;
            } else /* if (dx > 0) */ {
                node_dir = Neighbors.SE;
            }
        }
        return (neighborGrid[node[0],node[1]] & (int)node_dir) == (int)node_dir;
    }

    private void UpdateNeighbors (int[] node) {
        int[] neighbor = new int[2];
        for (int x = -1; x <= 1; x ++) {
            for (int y = -1; y <= 1; y ++) {
                if (x == 0 && y == 0) continue;
                neighbor[0] = node[0] + x;
                neighbor[1] = node[1] + y;
                Neighbors node_dir;
                Neighbors neighbor_dir;
                if (HasNeighbor (node, neighbor, out node_dir, out neighbor_dir)) {
                    neighborGrid[node[0], node[1]] |= (int)node_dir;
                    neighborGrid[neighbor[0], neighbor[1]] |= (int)neighbor_dir;
                } else {
                    neighborGrid[node[0], node[1]] &= ~((int)node_dir);
                    neighborGrid[neighbor[0], neighbor[1]] &= ~((int)neighbor_dir);
                }
            }
        }
    }

    private bool HasNeighbor (int[] node, int[] neighbor, 
                              out Neighbors node_dir, out Neighbors neighbor_dir) 
    {
        node_dir = Neighbors.N;
        neighbor_dir = Neighbors.S;

        int dx = neighbor[0] - node[0];
        int dy = neighbor[1] - node[1];

        if (dy > 0) {
            if (dx < 0) {
                node_dir = Neighbors.NW;
                neighbor_dir = Neighbors.SE;
            } else if (dx == 0) {
                node_dir = Neighbors.N;
                neighbor_dir = Neighbors.S;
            } else /* if (dx > 0) */ {
                node_dir = Neighbors.NE;
                neighbor_dir = Neighbors.SW;
            }
        } else if (dy == 0) {
            if (dx < 0) {
                node_dir = Neighbors.W;
                neighbor_dir = Neighbors.E;
            } else if (dx == 0) {
                return false;
            } else /* if (dx > 0) */ {
                node_dir = Neighbors.E;
                neighbor_dir = Neighbors.W;
            }
        } else /* if (dy < 0) */ {
            if (dx < 0) {
                node_dir = Neighbors.SW;
                neighbor_dir = Neighbors.NE;
            } else if (dx == 0) {
                node_dir = Neighbors.S;
                neighbor_dir = Neighbors.N;
            } else /* if (dx > 0) */ {
                node_dir = Neighbors.SE;
                neighbor_dir = Neighbors.NW;
            }
        }

        if (!HasSurface (node) || !HasSurface (neighbor)) {
            return false;
        }

        bool nodeOpen = !IndexConnectable (node);
        bool neighborOpen = !IndexConnectable (neighbor);

        bool nodeBridged = IsBridged (node);
        bool neighborBridged = IsBridged (neighbor);
        
        if (nodeOpen && neighborOpen) {
            return true;
        } 

        Edge nodeEdge = Edge.North;
        Edge neighborEdge = Edge.South;

        // only allow diagonals if both surfaces are open as above
        if (dx != 0 && dy != 0) {
            return false;
        } else if (dx > 0) {
            nodeEdge = Edge.East;
            neighborEdge = Edge.West;
        } else if (dx < 0) {
            nodeEdge = Edge.West;
            neighborEdge = Edge.East;
        } else if (dy > 0) {
            nodeEdge = Edge.North;
            neighborEdge = Edge.South;
        } else if (dy < 0) {
            nodeEdge = Edge.South;
            neighborEdge = Edge.North;
        }

        bool nodeConnection = EdgeConnectable (node, nodeEdge);
        bool neighborConnection = EdgeConnectable (neighbor, neighborEdge);

        if (nodeConnection && neighborConnection && (nodeBridged || neighborBridged)) {
            return true;
        }

        if (!nodeConnection && !neighborConnection && !(nodeBridged || neighborBridged)) {
            return true;
        }

        return false;
    }

}
