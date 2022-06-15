using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    private float radius;
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileReplacement[] spriteToPrefabLinks;
    [SerializeField] private Tile _tileGrassPrefab, _tileMountainPrefab;
    [SerializeField] private Transform camTransform;
    private Camera mainCamera;

    public Grid Grid { get { return grid; } }
    public static GridManager Instance;
    private Dictionary<Vector2, Tile> _tiles;

    public Queue<Tile> AffectedTiles;
    public List<Tile> OptimalPath;

    public Dictionary<Tile,Tile> TraversalConnections;
    public Dictionary<Tile,int> TraversalCosts;


    void Awake(){
        Instance = this;
    }

    void Start(){
        
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.V)){
            ToggleDistanceCost();
        }
    }

    // Instantiate tiles to create grid/board
    public void GenerateAxialHexagram(){
        radius = (float)MainMenuManager.Instance.MapSize;
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
                newTile.Init(v2, false);
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


    /// <summary>
    /// Finds all sprites in the tilemap and places the corresponding real Tile on top of them.
    /// Tilemap is destroyed afterwards.
    /// Transforms the camera to fit the map size and changes the gamestate to spawn the teams.
    /// </summary>
    public void GenerateBoardFromTilemap(){
        _tiles = new Dictionary<Vector2, Tile>();
        AffectedTiles = new Queue<Tile>();

        var oddColOffset = grid.cellSize.x / 2;
        var ySpacing = Mathf.Sqrt(3)/2;
        //var xSpacing = (3f/4f);
        tilemap.CompressBounds();

        foreach (var position in tilemap.cellBounds.allPositionsWithin) {
            if (!tilemap.HasTile(position)) {
                continue;
            }
            var tileSprite = tilemap.GetTile(position);
            var worldPos = grid.GetCellCenterWorld(position);
            Vector2 v2 = new Vector2(worldPos.x, worldPos.y);
            
            // Find the right tile prefab and instantiate it over the tilemap sprite.
            foreach (TileReplacement tr in spriteToPrefabLinks){
                if (tr.TileSprite.name.Equals(tileSprite.name)){
                    var newTile = Instantiate(tr.TilePrefab, v2, Quaternion.identity);
                    
                    newTile.Init(v2, true);
                    var axialCoords = newTile.nodeBase.Coords;
                    _tiles[axialCoords] = newTile;
                }
            }
        }

        // Reposition camera to center of board
        camTransform.transform.position = new Vector3(0, 0, -5);
        mainCamera = Camera.main;
        mainCamera.orthographicSize = ((float)tilemap.size.x / 2) + 0.5f;

        TilemapRenderer tmRend = tilemap.GetComponent<TilemapRenderer>();
        DestroyImmediate(tmRend);
        DestroyImmediate(tilemap);

        // Populate neighbor tracking for each tile
        initTileNeighbors();
        
        // Change GameManager's State after creating the board
        GameManager.Instance.ChangeState(GameState.SpawnTeams);
    }

    /// <summary>
    /// Convert Odd-q offset coordinates to Cube coordinates as a Vector3Int.
    /// </summary>
    /// <param name="v2">Offset coordinates</param>
    /// <returns>Vector3Int containing Cube coordinates.</returns>
    public Vector3Int OddqToCube(Vector2Int v2){
        var q = v2.x;
        var r = v2.y - (v2.x - (v2.x & 1)) / 2;
        return new Vector3Int(q, r, -q-r);
    }

    /// <summary>
    /// Convert Odd-q offset coordinates to Axial coordinates as a Vector2Int
    /// </summary>
    /// <param name="v2">Offset coordinates</param>
    /// <returns>Vector2Int containing Axial coordinates.</returns>
    public Vector2Int OddqToAxial(Vector2Int v2){
        var q = v2.x;
        var r = v2.y - (v2.x - (v2.x & 1)) / 2;
        return new Vector2Int(q, r);
    }

    public void ToggleDistanceCost(){
        foreach (KeyValuePair<Vector2,Tile> item in _tiles){
            item.Value.ToggleDisplayCost();
        }
    }

    public Tile GetRandomTile(){
        return _tiles.Where(t => t.Value.Walkable).OrderBy(t => UnityEngine.Random.value).First().Value;
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

    /*
     * Checks if the tile cooresponding to the given cube coordinates is traversable.
     * If the tile DNE, then the tile is not traversable unless 'existance = false'.
    */
    public bool isCubeTraversable(Cube a, bool existance = true){
        var tile = GetTileByVector(a.GetAxial());
        if (tile == null)
            return (existance ? false : true);

        return (tile.Walkable);
    }

    public bool isCubeTargettable(Cube a){
        var tile = GetTileByVector(a.GetAxial());
        if (tile == null)
            return false;
        
        if (!tile.Walkable && tile.OccupiedUnit == null)
            return false;
        
        if (!tile.Walkable && tile.OccupiedUnit != null)
            return true;
        
        Debug.Log("Reached odd case in isCubeTargettable.");
        return false;
    }

    public Tile GetTileByVector(Vector2 v){
        Tile tile = null;
        _tiles.TryGetValue(v, out tile);
        return tile;
    }

    public List<Tile> GetTilesFromCubes(List<Cube> cubes){
        List<Tile> tiles = new List<Tile>();
        foreach(Cube c in cubes){
            
            var t = GetTileByVector(c.GetAxial());
            if (t != null)
                tiles.Add(t);
        }
        return tiles;
    }

    public List<Tile> GetTilesInArea(List<Vector2> area){
        List<Tile> tiles = new List<Tile>();
        foreach(var v in area){
            var tmp = GetTileByVector(v);
            if (tmp == null)
                continue;
            tiles.Add(tmp);
        }
        return tiles;
    }

    public List<Tile> GetTilesInMovementRange(int range){
        return _tiles.Select(t => t.Value).Where(t => t.Distance <= range).ToList();
    }

    /*
     * Implements Dijkstra's algorithm to calculate the cost from the src tile
     * to all other tiles on the map. Called at the start of each turn, setting
     * the src tile as the newly active unit's occupied tile.
    */
    public void CalculateMapTraversal(Tile src){
        var Q = new List<Tile>();

        foreach (KeyValuePair<Vector2,Tile> item in _tiles){
            if (!item.Value.Walkable)
                continue;
            Q.Add(item.Value);
            item.Value.Distance = item.Value.Infinity;
            item.Value.Previous = null;
        }

        Q.Add(src);
        src.Distance = 0;

        while(Q.Count > 0){
            var u = Q.OrderBy(t => t.Distance).First();
            Q.Remove(u);

            foreach(Tile t in u.Neighbors){
                if (!Q.Contains(t))
                    continue;

                var alt = u.Distance + t.MovementCost;
                if (alt < t.Distance && u.Distance != u.Infinity){
                    t.Distance = alt;
                    t.Previous = u;
                }
            }
        }
    }

    public void DisplayTileNumbers(){
        foreach (KeyValuePair<Vector2,Tile> item in _tiles){
            item.Value.ToggleDisplayCost();
        }
    }
}

[Serializable]
public struct TileReplacement{
    public TileBase TileSprite;
    public Tile TilePrefab;
}