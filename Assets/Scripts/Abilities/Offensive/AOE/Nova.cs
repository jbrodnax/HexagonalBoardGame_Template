using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nova : BaseAOE
{
    public override OffensiveAbility Init(float mod, Tile origin, BaseUnit callerUnit)
    {
        var ret = base.Init(mod, origin, callerUnit);

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

     public override bool Trigger(){
        var affectedTiles = GridManager.Instance.GetTilesInArea(AffectedArea);
        foreach(Tile t in affectedTiles){
            t.Highlight(TileEvent.Highlight);
            if (t.OccupiedUnit == null)
                continue;
            
            var player = t.OccupiedUnit;
            if (player == Unit)
                continue;
            var distance = GridManager.Instance.GetDistance(SourceTile, t);
            Debug.Log($"Affected Player Distance: {distance}");
            player.ApplyMagicDamage(GetAppliedDamageFallOff((int)distance));
        }
        return true;
    }
    public override void CleanUp(){
        GridManager.Instance.ResetAffectedTiles();
    }
}
