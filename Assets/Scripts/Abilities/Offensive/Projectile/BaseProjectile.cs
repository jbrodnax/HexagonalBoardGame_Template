using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseProjectile : OffensiveAbility
{
    [SerializeField] public int MaxRange;
    [SerializeField] public bool Pierce;

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
    * Calculate all the possible tiles that could be affected by this projectile given its range.
    * LoS is considered in the calculation
    * TKTK - unless Pierce is true
    * Set the affected tile's accordingly.
    */   
    protected override void InitAffectedTiles(){
        
        var centerCube = SourceTile.nodeBase.GetCubeFromThis();
        var ringCubes = centerCube.Ring(centerCube, MaxRange);
        var affectedCubes = new List<Cube>();
        foreach(Cube c in ringCubes){
            foreach(Cube l in centerCube.DrawLine(c)){
                if (centerCube.ListContains(affectedCubes, l))
                    continue;
                affectedCubes.Add(l);
            }
        }

        var affectedTiles = GridManager.Instance.GetTilesFromCubes(affectedCubes);
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
        
        // TKTK - fix
        var r = destTile.GetComponent<SpriteRenderer>();
        if (r)
            r.color = Color.white;

        return;
    }

    private List<Tile> getLinePath(Tile dest){
        var line = SourceTile.nodeBase.GetCubeFromThis().DrawLine(dest.nodeBase.GetCubeFromThis());
        var path = GridManager.Instance.GetTilesFromCubes(line);

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

        // If the projectile can pierce, iterate over the path and apply damage to each unit in the path,
        // otherwise, stop after the first unit in the path is hit.
        foreach (Tile t in selectedPath){
            if (t.OccupiedUnit != null && t.OccupiedUnit != Unit){
                var u = t.OccupiedUnit;
                var damage = GetBaseDamage();
                switch(DamageType){
                    case DamageType.Magic:
                        u.ApplyMagicDamage(damage);
                        break;
                    case DamageType.Physical:
                        u.ApplyPhysicalDamage(damage);
                        break;
                    default:
                        Debug.Log("BaseProjectile:Trigger has invalid DamageType.");
                        return false;
                }

                if (!Pierce)
                    break;
            }
        }

        return true;
    }

    public override void CleanUp(){
        GridManager.Instance.ResetAffectedTiles();
        Destroy(TileEffectsController.gameObject);
        Destroy(gameObject);
    }
}
