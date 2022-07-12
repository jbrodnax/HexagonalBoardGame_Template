using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseCleave : OffensiveAbility
{
    [SerializeField] public int range;

    protected List<Vector2> AffectedArea; 

    public override OffensiveAbility Init(Tile origin, BaseUnit callerUnit)
    {
        var ret = base.Init(origin, callerUnit);

        //Create the TileEffects instance for this ability
        TileEffectsController = Instantiate(TileEffectsPrefab).Init(
            ShaderHighlight, 
            ShaderDarken, 
            HighlightBlender, 
            DarkenBlender,
            TileEvent.UnHighlight
        );
        InitAffectedTiles();
        return ret;
    }

    protected override void InitAffectedTiles(){
        Debug.Log($"InitAffectedTiles: {AbilityName}");
        var sourceCube = SourceTile.nodeBase.GetCubeFromThis();
        // TKTK - hardcoded NE direction parameter. Change once better directions are implemented.
        var affectedCubes = sourceCube.Diamond(sourceCube, new Cube(1, -1, 0), range);
        foreach (Cube c in affectedCubes){
            Debug.Log($"{c.ToString()}");
        }
        var affectedTiles = GridManager.Instance.GetTilesFromCubes(affectedCubes);

        foreach (Tile t in affectedTiles){
            t.SetAbilityEffectsController(TileEffectsController, true);
        }
    }

    public override bool Trigger(){
        // Gather up the affected units
        var tiles = GridManager.Instance.AffectedTiles;
        var units = tiles.ToList().Where(
                t => t.OccupiedUnit != null && t.OccupiedUnit != Unit
            ).Select(
                t => t.OccupiedUnit
            );

        foreach (BaseUnit u in units){
            var damage = GetBaseDamage();

            switch(DamageType){
                case DamageType.Magic:
                    u.ApplyMagicDamage(damage);
                    return true;
                case DamageType.Physical:
                    u.ApplyPhysicalDamage(damage);
                    return true;
                default:
                    Debug.Log("BaseAOE:Trigger has invalid DamageType.");
                    return false;
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
