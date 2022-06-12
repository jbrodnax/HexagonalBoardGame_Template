using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseAOE : OffensiveAbility
{
    [SerializeField] public float radius;
    [SerializeField] [Range(0.0f,1.0f)] private float damageFallOff;
    [SerializeField] private bool moveAble;
    //protected List<Tile> AffectedTiles;
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
        AffectedArea = SourceTile.nodeBase.CalculateArea(radius);
        var affectedTiles = GridManager.Instance.GetTilesInArea(AffectedArea);

        var centerCube = SourceTile.nodeBase.GetCubeFromThis();
        var ringCubes = centerCube.Ring(centerCube, (int)radius);
        
        foreach(Tile t in affectedTiles){
            t.SetAbilityEffectsController(TileEffectsController, true);
        }

        // Hightlight the outer ring of tiles (so it looks pretty lol)
        foreach(Cube c in ringCubes){
            var tile = GridManager.Instance.GetTileByVector(c.GetAxial());
            if (tile == null)
                continue;
            
            tile.Highlight(TileEvent.Highlight);
        }
    }

    // Applies the damage falloff to the rolled base damage of the ability.
    // level indicates the ring that we are interested in, where level = 0 is the tiles that neighbor the origin tile. 
    public float GetAppliedDamageFallOff(int level){
        return (appliedDamage -= ((damageFallOff * level) * appliedDamage));
    }

    public override bool Trigger(){
        // Gather up the affected units
        var tiles = GridManager.Instance.AffectedTiles;
        var units = tiles.ToList().Where(
                t => t.OccupiedUnit != null && t.OccupiedUnit != Unit
            ).Select(
                t => t.OccupiedUnit
            );

        // Calculate distance to affected units and apply the damage
        foreach (BaseUnit u in units){
            var dist = SourceTile.nodeBase.GetAxialDistance(u.OccupiedTile.nodeBase);
            var damage = GetAppliedDamageFallOff((int)dist);
            Debug.Log($"Affected Player Distance: {dist}");

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
