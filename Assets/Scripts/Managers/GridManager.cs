using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [SerializeField] public float radius;
    [SerializeField] private Tile _tileGrassPrefab, _tileMountainPrefab;
    [SerializeField] private Transform camTransform;
    private Camera mainCamera;

    public static GridManager Instance;
    private Dictionary<Vector2, Tile> _tiles;

    public Queue<Tile> AffectedTiles;
    public List<Tile> OptimalPath;

    void Awake(){
        Instance = this;
    }

    // Instantiate tiles to create grid/board
    public void GenerateAxialHexagram(){
        _tiles = new Dictionary<Vector2, Tile>();
        AffectedTiles = new Queue<Tile>();
        bool colorOffset = false;

        var N = radius;
        var _N = (0 - radius);
        for (var q = _N; q <= N; q++){
            var lower = Math.Max(_N, (0-q)-N);
            var upper = Math.Min(N, (0-q)+N);
            for (var r = lower; r <= upper; r++){

                var tilePrefab = UnityEngine.Random.Range(0, 6) < 1 ? _tileMountainPrefab : _tileGrassPrefab;
                var v = new Vector2(q, r);
                v.x = q * (Mathf.Sqrt(3) / 2);
                v.y = (0 - r) - (q / 2);
                var newTile = Instantiate(tilePrefab, v, Quaternion.identity);

                var v2 = new Vector2(q,r);
                newTile.Init(v2, colorOffset);
                _tiles[v2] = newTile;

                colorOffset = colorOffset ? false : true;
            }
        }

        // Reposition camera to center of board
        camTransform.transform.position = new Vector3(0, 0, -5);
        mainCamera = Camera.main;
        mainCamera.orthographicSize = radius + 0.5f;

        // Populate neighbor tracking for each tile
        initTileNeighbors();

        // Change GameManager's State after creating the board
        GameManager.Instance.ChangeState(GameState.SpawnTeams);
    }

    public Tile GetRandomTile(){
        return _tiles.Where(t => t.Value.Walkable).OrderBy(t => UnityEngine.Random.value).First().Value;
    }
    // Returns random tile from the left-hand side of the board
    public Tile GetLeftHandSpawnTile(){
        return _tiles.Where(t => (t.Key.x < radius/2) && t.Value.Walkable).OrderBy(t => UnityEngine.Random.value).First().Value;
    }

    // Returns random tile from the right-hand side of the board
    public Tile GetRightHandSpawnTile(){
        return _tiles.Where(t => (t.Key.x > radius/2) && t.Value.Walkable).OrderBy(t => UnityEngine.Random.value).First().Value;
    }

    public void ResetAffectedTiles(){
        Debug.Log("Resetting Affected Tiles");
        Tile tile;
        while(AffectedTiles.Count != 0){
            tile = AffectedTiles.Dequeue();
            tile.Reset();
        }
    }

    public void BoardReset(){
        foreach(KeyValuePair<Vector2,Tile> t in _tiles){
            t.Value.Reset();
        }
    }

    private void initTileNeighbors(){
        List<Tile> neighbors;
        Tile value = null;

        foreach (KeyValuePair<Vector2, Tile> tile in _tiles){
            neighbors = new List<Tile>();
            var x = tile.Key.x;
            var y = tile.Key.y;

            // Top and Bottom
            if (_tiles.TryGetValue(new Vector2(x, y-1), out value))
                neighbors.Add(value);
            if (_tiles.TryGetValue(new Vector2(x, y+1), out value))
                neighbors.Add(value);
            // RHS
            if (_tiles.TryGetValue(new Vector2(x+1, y), out value))
                neighbors.Add(value);
            if (_tiles.TryGetValue(new Vector2(x+1, y-1), out value))
                neighbors.Add(value);

            // LHS
            if (_tiles.TryGetValue(new Vector2(x-1, y), out value))
                neighbors.Add(value);
            if (_tiles.TryGetValue(new Vector2(x-1, y+1), out value))
                neighbors.Add(value);
            
            tile.Value.SetNeighbors(neighbors);
        }
    }

    public float GetDistance(Tile src, Tile dst){
        return (GetDistance(src.nodeBase.Coords, dst.nodeBase.Coords));
    }

    public float GetDistance(Vector2 a, Vector2 b){
        return ((Mathf.Abs(a.x - b.x) + Mathf.Abs(a.x + a.y - b.x - b.y) + Mathf.Abs(a.y - b.y))/2);
    }

    public Tile GetTileByVector(Vector2 v){
        Tile tile = null;
        _tiles.TryGetValue(v, out tile);
        if (tile == null){
            Debug.Log($"Failed to get tile at vector ({v.x}, {v.y})");
        }
        return tile;
    }

    public List<Tile> FindReachable(Tile src, int distance){
        List<Tile> visited = new List<Tile>();
        visited.Add(src);

        List<List<Tile>> fringes = new List<List<Tile>>();
        List<Tile> origin = new List<Tile>();
        origin.Add(src);
        fringes.Add(origin);

        for (int k = 1; k <= distance; k++){
            var tmp = new List<Tile>();
            fringes.Add(tmp);

            foreach(Tile t in fringes[k-1]){
                foreach (Tile neighbor in t.Neighbors){
                    if (!visited.Contains(neighbor)){
                        // Add the tile to visited if it has a player on it, but don't branch from it
                        if (!neighbor.Walkable && neighbor.OccupiedUnit != null){
                            visited.Add(neighbor);
                            continue;
                        } else if (neighbor.Walkable){
                            visited.Add(neighbor);
                            fringes[k].Add(neighbor);
                        }
                    }
                }
            }
        }

        return visited;
    }

    public List<Tile> FindPath(Tile src, Tile dst, bool onlyReachable = false){
        List<Tile> path = new List<Tile>();

        var toSearch = new List<NodeBase>() {src.nodeBase };
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

            if (current == dst.nodeBase){
                var currentPathTile = dst.nodeBase;
                var count = 1000;
                while (currentPathTile != src.nodeBase){
                    path.Add(currentPathTile.tile);
                    currentPathTile = currentPathTile.Connection;

                    count--;
                    if (count < 0) throw new Exception();
                }

                return path;
            }

            foreach (var neighborTile in current.tile.Neighbors.Where(t => t.Walkable && !processed.Contains(t.nodeBase))){
                if (onlyReachable && !neighborTile.Reachable)
                    continue;
                var neighbor = neighborTile.nodeBase;
                var inSearch = toSearch.Contains(neighbor);
                var costToNeighbor = current.G + current.GetDistance(neighbor);

                if (!inSearch || costToNeighbor < neighbor.G){
                    neighbor.SetG(costToNeighbor);
                    neighbor.SetConnection(current);

                    if (!inSearch){
                        neighbor.SetH(neighbor.GetDistance(dst.nodeBase));
                        toSearch.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }

    // Find all vectors/tiles within the given radius from center
    public List<Vector2> CalculateArea(Vector2 center, float radius){
        List<Vector2> map = new List<Vector2>();

        var N = radius;
        var _N = (0 - radius);

        for (float q = _N; q <= N; q++){
            var rLower = Math.Max(_N, ((0-q)+_N));
            var rUpper = Math.Min(N, ((0-q)+N));

            for (float r = rLower; r <= rUpper; r++){
                map.Add(new Vector2(center.x + q, center.y + r));
            }
        }
        return map;
    }

    public List<Tile> GetTilesInArea(List<Vector2> area){
        List<Tile> tiles = new List<Tile>();
        foreach(var v in area){
            var tmp = GetTileAtPosition(v);
            if (tmp == null)
                continue;
            tiles.Add(tmp);
        }
        return tiles;
    }

    // Get the tile referenced by Vector2 position or return null
    public Tile GetTileAtPosition(Vector2 pos)
    {
        if(_tiles.TryGetValue(pos, out var tile))
            return tile;

        return null;
    }

    public float lerp(float a, float b, float t){
        return (a + (b - a) * t);
    }

    public Cube cube_lerp(Cube a, Cube b, float t){
        return new Cube(
            lerp(a.q, b.q, t),
            lerp(a.r, b.r, t),
            lerp(a.s, b.s, t)
        );
    }

    public List<Cube> cube_linedraw(Cube a, Cube b){
        var N = a.GetDistanceTo(b);
        List<Cube> results = new List<Cube>();

        for (var i = 0; i <= N; i++)
            results.Add(cube_lerp(a, b, (1.0f/N * i)).Round(true));
        return results;
    }
}
