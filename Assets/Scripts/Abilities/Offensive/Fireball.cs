using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : OffensiveAbility
{
    [SerializeField] public int MaxRange;

    /* Stores the last calculated path from clicking a tile. */
    private List<Tile> selectedPath;

    /* Stores the last path calculated from hovering over a tile. */
    private List<Tile> unselectedPath;

    /* Stores the last tile hovered over by the mouse. */
    private Tile tmpTile;


    public override OffensiveAbility Init(Tile origin, BaseUnit callerUnit){
        TileEffectsController = Instantiate(TileEffectsPrefab).Init(
            ShaderHighlight,
            ShaderDarken,
            HighlightBlender,
            DarkenBlender,
            TileEvent.UnHighlight,
            onEnter: MouseEnterHandler,
            onDown: MouseDownHandler
        );

        var ret = base.Init(origin, callerUnit);
        InitAffectedTiles();

        return ret;
    }

    /*
    * 'Move' ability will affect all tiles within range of the unit's max move distance.
    */   
    protected override void InitAffectedTiles(){
        /*
        var centerCube = SourceTile.nodeBase.GetCubeFromThis();
        var ringCubes = centerCube.Ring(centerCube, MaxRange);
        var ringTiles = new List<Tile>();
    
        foreach(Cube c in ringCubes){
            var tile = GridManager.Instance.GetTileByVector(c.GetAxial());
            if (tile == null)
                continue;
            
            ringTiles.Add(tile);
        }

        var affectedTiles = new List<Tile>();
        foreach(Tile t in ringTiles){
            var line = getLinePath(t);
            foreach(Tile rt in line){
                if (!affectedTiles.Contains(rt))
                    affectedTiles.Add(rt);
            }
        }
        */
        var affectedTiles = GridManager.Instance.FindReachable(SourceTile, MaxRange);
        foreach(Tile t in affectedTiles){
            t.SetAbilityEffectsController(TileEffectsController, true);
        }
    }

    /*
    * Unhighlights any previously 'unselectedPath' tiles.
    * Gets the line path between source tile and new tmp tile.
    * Saves the path to 'unselectedPath' and highlights each tile.
    */
    public void MouseEnterHandler(Tile destTile){
        if (unselectedPath != null)
            changePathHighlighting(unselectedPath, TileEvent.UnHighlight);

        unselectedPath = getLinePath(destTile);

        changePathHighlighting(unselectedPath, TileEvent.Highlight);
    }

    /*
     * Sets the selected tile as the new targetted tile.
     * Gets the line path from the source tile (where the associated player located),
     * to the selected targetted tile.
     * Highlights the path and saves it for use by trigger().
    */ 
    public void MouseDownHandler(Tile destTile){
        // TKTK - Targetting will be handled/tracked by the ability instead of the unit.
        TargetTile = destTile;

        // If the tmp tile is the same as the clicked one, no need to recalculate path.
        if (TargetTile == tmpTile){
            selectedPath = unselectedPath;
        }else{
            // Otherwise unhighlight the old path, calc the new one, and highlight it.
            changePathHighlighting(unselectedPath, TileEvent.UnHighlight);
            selectedPath = getLinePath(destTile);
            changePathHighlighting(selectedPath, TileEvent.Highlight);
        }

        Debug.Log($"Path set: {selectedPath}");
        
        // TKTK - fix
        var r = destTile.GetComponent<SpriteRenderer>();
        if (r)
            r.color = Color.white;

        return;
    }

    private List<Tile> getLinePath(Tile dest){
        var path = new List<Tile>();
        var line = GridManager.Instance.cube_linedraw(SourceTile.nodeBase.GetCubeFromThis(), dest.nodeBase.GetCubeFromThis());

        foreach(Cube c in line){
            var t = GridManager.Instance.GetTileByVector(c.GetAxial());
            if (t == null)
                continue;
            if (!t.Walkable && !t.OccupiedUnit){
                break;
            }
            path.Add(t);
        }

        if (!path.Contains(SourceTile))
            path.Add(SourceTile);

        return path;
    }

    /*
     * Applies un/highlighting to entire path.
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
        if (!TargetTile)
            return false;
        
        if (selectedPath == null)
            return false;

        // Validate the path against the range of this ability
        // -1 since the source tile is included in the selected path
        if (selectedPath.Count-1 > MaxRange){
            Debug.Log($"{AbilityName} has a max range of {MaxRange}.");
            return false;
        }

        var player = TargetTile.OccupiedUnit;
        if (player == null){
            Debug.Log("Target tile does not have a unit.");
            return true;
        }

        player.ApplyMagicDamage(GetBaseDamage());

        return true;
    }

    public override void CleanUp(){
        GridManager.Instance.ResetAffectedTiles();
    }
}
