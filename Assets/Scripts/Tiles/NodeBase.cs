using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class NodeBase
{
    public NodeBase Connection {get; private set;}
    public float G {get; private set;}
    public float H {get; private set;}
    public float F => G + H;

    public Tile tile {get; set;}
    public float r {get; set;}
    public float q {get; set;}
    public Vector2 Coords {get; set;}

    public Vector2[] directions = new Vector2[]{
        new Vector2(1, 0),  //Down right
        new Vector2(1, -1), //Up right
        new Vector2(0, -1), //Up
        new Vector2(-1, 0), //Up left
        new Vector2(-1, 1), //Down left
        new Vector2(0, 1),  //Down
    };

    public NodeBase(Tile t, Vector2 coords){
        tile = t;
        this.Coords = coords;
        this.q = coords.x;
        this.r = coords.y;

        this.G = 0;
        this.H = 0;
    }
    public void SetConnection(NodeBase nodeBase) => Connection = nodeBase;

    public void SetG(float g) => G = g;

    public void SetH(float h) => H = h;

    public float GetAxialDistance(NodeBase targetNode){
        var dcol = Math.Abs(q - targetNode.q);
        var drow = Math.Abs(r - targetNode.r);
        return (dcol + Math.Max(0, ((drow - dcol)/2)));
    }

    public Cube GetCubeFromAxial(Vector2 v){
        return (new Cube(v.x, v.y, (-v.x - v.y)));
    }

    public Cube GetCubeFromThis(){
        return (GetCubeFromAxial(Coords));
    }

    /*
     * Returns a list of all Axial coords within a given distance of the this nodeBase instance's tile.
    */
    public List<Vector2> CalculateArea(float radius){
        List<Vector2> map = new List<Vector2>();

        var N = radius;
        var _N = (0 - radius);

        for (float q = _N; q <= N; q++){
            var rLower = Math.Max(_N, ((0-q)+_N));
            var rUpper = Math.Min(N, ((0-q)+N));

            for (float r = rLower; r <= rUpper; r++){
                map.Add(new Vector2(this.q + q, this.r + r));
            }
        }
        return map;
    }

    // TKTK - implement reachable
    public List<NodeBase> FindOptimalPath(NodeBase dest, bool onlyReachable = false){
        Debug.Log("Move: FindOptimalPath Enter");
        var path = new List<NodeBase>();

        var toSearch = new List<NodeBase>(){this};
        var processed = new List<NodeBase>();

        while (toSearch.Any()){
            var current = toSearch[0];
            foreach (var t in toSearch){
                if (t.F < current.F || t.F == current.F && t.H < current.H)
                    current = t;
            }

            processed.Add(current);
            toSearch.Remove(current);
            current.Reset();

            if (current == dest){
                var currentPathNode = dest;
                var count = 1000;
                while (currentPathNode != this){
                    path.Add(currentPathNode);
                    currentPathNode = currentPathNode.Connection;

                    count--;
                    if (count < 0) throw new Exception();
                }

                return path;
            }

            foreach (var neighborTile in current.tile.Neighbors.Where(
                t => t.Walkable && !processed.Contains(t.nodeBase)))
            {   
                var neighbor = neighborTile.nodeBase;
                var inSearch = toSearch.Contains(neighbor);
                var costToNeighbor = current.G + current.GetAxialDistance(neighbor);

                if (!inSearch || costToNeighbor < neighbor.G){
                    neighbor.SetG(costToNeighbor);
                    neighbor.SetConnection(current);

                    if (!inSearch){
                        neighbor.SetH(neighbor.GetAxialDistance(dest));
                        toSearch.Add(neighbor);
                    }
                }
            }
        }

        Debug.Log("Move: FindOptimalPath Exit");

        return null;
    }

    public void Reset() => G = H = 0;
}


public class Cube_Directions
{
    public Cube UP = new Cube(0, -1, 1);
    public Cube DOWN = new Cube(0, 1, -1);
    public Cube UP_RIGHT = new Cube(1, -1, 0);
    public Cube UP_LEFT = new Cube(-1, 0, 1);
    public Cube DOWN_RIGHT = new Cube(1, 0, -1);
    public Cube DOWN_LEFT = new Cube(-1, 1, 0);

    public Cube MapIndex(int i){
        switch(i){
            case 0:
                return DOWN_RIGHT;
            case 1:
                return UP_RIGHT;
            case 2:
                return UP;
            case 3:
                return UP_LEFT;
            case 4:
                return DOWN_LEFT;
            case 5:
                return DOWN;
            default:
                Debug.LogError("Cube direction does not exist.");
                return null;
        }
    }
}

public enum Cardinal{
    N = 0,
    NE = 1,
    SE = 2,
    S = 3,
    SW = 4,
    NW = 5
}

public class Cube
{
    public float q;
    public float r;
    public float s;

    public Cube(float _q, float _r, float _s){
        q = _q;
        r = _r;
        s = _s;
    }

    public override string ToString()
    {
        return $"({q}, {r}, {s})";
    }

    public Cube[] Directions = new Cube[]{
        new Cube(0, -1, 1), // N
        new Cube(1, -1, 0), // NE
        new Cube(1, 0, -1), // SE
        new Cube(0, 1, -1), // S
        new Cube(-1, 1, 0), // SW
        new Cube(-1, 0, 1), // NW
};

    public Vector2 GetAxial(){
        return (new Vector2(q,r));
    }

    public Cube Round(Cube a){
        Debug.Log($"Rounding: {a.ToString()}");
        var _q = Mathf.Round(a.q);
        var _r = Mathf.Round(a.r);
        var _s = Mathf.Round(a.s);

        var q_diff = Math.Abs(_q - a.q);
        var r_diff = Math.Abs(_r - a.r);
        var s_diff = Math.Abs(_s - a.s);

        if ((q_diff > r_diff) && (q_diff > s_diff)){
            _q = -_r-_s;
        }else if (r_diff > s_diff){
            _r = -_q-_s;
        }else{
            _s = -_q-_r;
        }
        var c = new Cube(_q,_r,_s);
        Debug.Log($"Resulted in: {c.ToString()}");
        return c;
    }

    public bool ListContains(List<Cube> list, Cube a){
        foreach(Cube c in list){
            if (c.q == a.q
                && c.r == a.r
                && c.s == c.r)
                {
                    return true;
                }
        }
        return false;
    }

    public Vector3 GetCubeAsVector3(){
        return (new Vector3(q,r,s));
    }
    public float GetDistanceTo(Cube b){
        return ((Math.Abs(this.q - b.q) + Math.Abs(this.r - b.r) + Math.Abs(this.s - b.s)) / 2);
    }

    public Cube lerp(Cube b, float t){
        return new Cube(
            (this.q + (b.q - this.q) * t),
            (this.r + (b.r - this.r) * t),
            (this.s + (b.s - this.s) * t)
        );
    }

    public Cube Add(Cube a, Cube b){
        return new Cube(
            a.q + b.q,
            a.r + b.r,
            a.s + b.s
        );
    }
    public Cube Scale(Cube a, int factor){
        return new Cube(
            a.q * factor,
            a.r * factor,
            a.s * factor
        );
    }

    // TKTK - fix this to accept an int direction referencing a neighbor
    public Cube Neighbor(Cube c, Cube d){
        return Add(c, d);
    }

    /*
     * Checks if this Cube instance has the same coordinates as Cube 'a'.
    */
    public bool Compare(Cube a){
        return (this.q == a.q && this.r == a.r && this.s == a.s);
    }

    /*
     * Creates a list of all cubes touched by a line drawn from this cube to a dest cube.
     * Default 'los' option will break the loop if a cube along the path cooresponds to a
     * non-traversable tile.
     * Note: when calculating with LoS, the source tile's traversability is ignored (i.e. the tile coorsponding to this cube)
    */
    public List<Cube> DrawLine(Cube dst, bool los = true, bool includeTargets = true){
        var N = GetDistanceTo(dst);
        List<Cube> results = new List<Cube>();
        var epsilon = new Cube((float)1e-6, (float)2e-6, (float)-3e-6);
        dst = Add(dst, epsilon);

        for (int i = 0; i <= N; i++){
            var current = Round(lerp(dst, (1.0f/N * i)));
            if (los == true
                && (!Compare(current))
                && (GridManager.Instance.isCubeTraversable(current) == false)){
                    if (includeTargets && GridManager.Instance.isCubeTargettable(current))
                        results.Add(current);
                    break;
                }

            results.Add(current);
        }
        
        return results;
    }

    /*
     * Gets a ring of hexes in cube coordinates given a center hex and radius.
    */
    public List<Cube> Ring(Cube center, int radius){
        var directions = new Cube_Directions();
        var results = new List<Cube>();
        var nextCube = Add(center, Scale(directions.DOWN_LEFT, radius));

        for (int i = 0; i < 6; i++){
            for (int j = 0; j < radius; j++){
                results.Add(nextCube);
                nextCube = Neighbor(nextCube, directions.MapIndex(i));
            }
        }

        return results;
    }

    /// <summary>
    /// Finds all hexes within a horizontal diamond shape originating from src, spreading in direction dir, with a range of `range`.
    /// </summary>
    /// <param name="src">Origin cube coordinates.</param>
    /// <param name="dir">Direction the diamond should be projected (as a neighboring hex tile).</param>
    /// <param name="range">The length of the sides of the diamond.</param>
    /// <returns>List of Cube coordinates that comprise the diamond.</returns>
    public List<Cube> DiamondCleave(Cube src, Cube dir, int range){
        var results = new List<Cube>();
        var qstart = Math.Min(0, ((int)dir.q * range));
        var qstop = Math.Max(0, ((int)dir.q * range));

        var rstart = Math.Min(0, ((int)dir.r * range));
        var rstop = Math.Max(0, ((int)dir.r * range));

        foreach (int q in Enumerable.Range(qstart, range+1))
        {
            foreach (int r in Enumerable.Range(rstart, range+1))
            {
                var s = -q - r;
                results.Add(Add(src, new Cube(q, r, s)));
            }
        }

        return results;
    }

    /*
     * Find all cube coordinates within 'distance' steps from this one.
     * Takes non-traversabel tiles into account.
     * Returns list of those cube coordinates.
    */
    public List<Cube> FindReachable(int distance){
        Debug.Log("Move: FindReachable Enter");
        Cube a = this;
        var directions = new Cube_Directions();
        List<Cube> visited = new List<Cube>();
        visited.Add(a);
        int i = 0;
        int m = 0;
        List<List<Cube>> fringes = new List<List<Cube>>(){new List<Cube>(){a}};
        for (int k = 1; k <= distance; k++){
            fringes.Add(new List<Cube>());
            i++;
            foreach(Cube b in fringes[k-1]){
                //Debug.Log($"Fringe Cube: {b.ToString()}");
                for (int j = 0; j < 6; j++){
                    m++;
                    var neighbor = Add(b, directions.MapIndex(j));
                    if(!ListContains(visited, neighbor)){
                        if (GridManager.Instance.isCubeTraversable(neighbor)){
                            visited.Add(neighbor);
                            fringes[k].Add(neighbor);
                        }
                    }
                    
                    if (m > 5000){
                        Debug.Log("m reached 500");
                        return visited;
                    }
                }
            }
            if (i > 100){
                Debug.Log("m reached 100");
                return visited;
            }
        }
        Debug.Log("Move: FindReachable Exit");
        return visited;
    }
}
