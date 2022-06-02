using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAbility : MonoBehaviour
{
    [SerializeField] public bool isTargetRequired;
    [SerializeField] public bool EndsTurn;
    [SerializeField] public Color ShaderHighlight;
    [SerializeField] public Color ShaderDarken;
    [SerializeField] [Range(0f,1f)] public float HighlightBlender;
    [SerializeField] [Range(0f,1f)] public float DarkenBlender;
    [SerializeField] public string AbilityName;
    [SerializeField] public AbilityType AbilityType;
    [SerializeField] public TileEffects TileEffectsPrefab;
    
    protected Tile sourceTile;
    public Tile SourceTile {get {return sourceTile;} set {sourceTile = value;}}
    private Tile targetTile;
    public Tile TargetTile {get{return targetTile;}set{targetTile=value;}}
    protected BaseUnit unit;
    public BaseUnit Unit {get {return unit;} set {unit = value;}}
    protected TileEffects tileEffectsController;
    public TileEffects TileEffectsController {get{return tileEffectsController;} set{tileEffectsController = value;}}
    
    /*
     * Forced method - Determine all potentially affected tiles.
     * Set the affected tiles' EffectsController.
     * GridManager will track the affected tiles.
     * Ability CleanUp() should call GridManager's cleanup method.
    */
    protected abstract void InitAffectedTiles();
    public abstract bool Trigger();
    public abstract void CleanUp();
}

public enum AbilityType{
    SingleTarget = 0,
    AOE = 1,
    Cleave = 2,
    Projectile = 3,
    Movement = 4
}
