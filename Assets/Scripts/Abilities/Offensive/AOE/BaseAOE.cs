using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAOE : OffensiveAbility
{
    [SerializeField] public float radius;
    [SerializeField] [Range(0.0f,1.0f)] private float damageFallOff;
    [SerializeField] private bool moveAble;
    //public List<Tile> AffectedTiles;
    protected List<Vector2> AffectedArea; 

    protected override void InitAffectedTiles(){
        AffectedArea = GridManager.Instance.CalculateArea(SourceTile.nodeBase.Coords, radius);
        var affectedTiles = GridManager.Instance.GetTilesInArea(AffectedArea);

        var centerCube = SourceTile.nodeBase.GetCubeFromThis();
        var ringCubes = centerCube.Ring(centerCube, (int)radius);
        
        foreach(Tile t in affectedTiles){
            t.SetAbilityEffectsController(TileEffectsController, true);
        }

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

}

/*
public struct AffectedTilesRing{
    public List<Tile> Tiles;
    public int RingLevel;
}
*/