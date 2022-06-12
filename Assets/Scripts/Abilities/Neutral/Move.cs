using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : BaseAbility
{
    private float maxDistance;

    /* Stores the last calculated path from clicking a tile. */
    private List<Tile> selectedPath;

    /* Stores the last path calculated from hovering over a tile. */
    private List<Tile> unselectedPath;

    /* Stores the last tile hovered over by the mouse. */
    private Tile tmpTile;


    public virtual void Init(Tile origin, float dist, BaseUnit callerUnit){
        Debug.Log("Move: Init Enter");
        SourceTile = origin;
        maxDistance = dist;
        Unit = callerUnit;

        TileEffectsController = Instantiate(TileEffectsPrefab).Init(
            ShaderHighlight,
            ShaderDarken,
            HighlightBlender,
            DarkenBlender,
            TileEvent.UnHighlight,
            onEnter: MouseEnterHandler,
            onDown: MouseDownHandler
        );
        InitAffectedTiles();
        Debug.Log("Move: Init Enter");
    }

    /*
    * 'Move' ability will affect all tiles within range of the unit's max move distance.
    */   
    protected override void InitAffectedTiles(){
        var affectedTiles = GridManager.Instance.GetTilesInMovementRange((int)maxDistance);

        foreach(Tile t in affectedTiles){
            t.SetAbilityEffectsController(TileEffectsController, true);
        }
        Debug.Log("Move: InitAffectedTiles End");
    }

    /*
    * Unhighlights any previously 'unselectedPath' tiles.
    * Gets the optimal path between source tile and new tmp tile.
    * Saves the path to 'unselectedPath' and highlights each tile.
    */
    public void MouseEnterHandler(Tile destTile){
        if (!destTile.Walkable)
            return;

        if (unselectedPath != null)
            changePathHighlighting(unselectedPath, TileEvent.UnHighlight);

        unselectedPath = getPath(SourceTile, destTile);

        if (!unselectedPath.Contains(SourceTile))
            unselectedPath.Add(SourceTile);
        changePathHighlighting(unselectedPath, TileEvent.Highlight);

        tmpTile = destTile;
    }

    /*
     * Sets the selected tile as the new targetted tile.
     * Gets the optimal path from the source tile (where the associated player located),
     * to the selected targetted tile.
     * Highlights the path and saves it for use by trigger().
    */ 
    public void MouseDownHandler(Tile destTile){
        if (!destTile.Walkable)
            return;

        TargetTile = destTile;

        // If the tmp tile is the same as the clicked one, no need to recalculate path.
        if (TargetTile == tmpTile){
            selectedPath = unselectedPath;
        }else{
            // Otherwise unhighlight the old path, calc the new one, and highlight it.
            changePathHighlighting(unselectedPath, TileEvent.UnHighlight);

            selectedPath = getPath(SourceTile, destTile);

            if (!selectedPath.Contains(SourceTile))
                selectedPath.Add(SourceTile);
            changePathHighlighting(selectedPath, TileEvent.Highlight);
        }
        
        // TKTK - fix
        var r = destTile.GetComponent<SpriteRenderer>();
        if (r)
            r.color = Color.white;

        return;
    }

    /*
     * Unwinds the path from dst to src based on map traversal calculated by the grid
     * manager at the start of the turn.
    */
    private List<Tile> getPath(Tile src, Tile dst){
        var path = new List<Tile>();
        var current = dst;

        while (current != src){
            path.Add(current);
            current = current.Previous;
            if (current == null){
                Debug.Log($"Found null Tile.Previous");
                break;
            }
        }

        path.Add(current);
        path.Reverse();

        return path;
    }

    /*
     * Applies un/highlighting to entire path (list).
    */
    private void changePathHighlighting(List<Tile> path, TileEvent te){
        foreach(Tile t in path)
            t.Highlight(te);
    }

    /*
     * Called by Unit to trigger their selected ability (an instance of an ability)
    */
    public override bool Trigger(){
        Debug.Log("Ability Triggered");
        if (!TargetTile || !TargetTile.Walkable)
            return false;
        
        if (selectedPath == null)
            return false;

        Debug.Log($"Distance: {selectedPath.Count}");

        // Check if the unit can actually move that distance
        // -1 since the source tile is included in the selected path
        if (selectedPath.Count-1 > Unit.DistancePerTurn){
            Debug.Log($"{Unit.UnitName} can only move {Unit.DistancePerTurn} tiles per turn.");
            return false;
        }

        // Remove unit from previous tile's tracking
        if (Unit.OccupiedTile != null)
            Unit.OccupiedTile.OccupiedUnit = null;

        // Move the unit to the targetted tile
        Unit.transform.position = TargetTile.transform.position + new Vector3(0,0.4f,0);

        // Update Tile <--> Unit tracking
        TargetTile.OccupiedUnit = Unit;
        Unit.OccupiedTile = TargetTile;

        return true;
    }

    public override void CleanUp(){
        GridManager.Instance.ResetAffectedTiles();
        Destroy(TileEffectsController.gameObject);
        Destroy(gameObject);
    }
}
