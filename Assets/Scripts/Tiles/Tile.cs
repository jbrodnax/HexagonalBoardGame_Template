using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 'abstract' - indicates that the thing being modified (class Tile in this case) has an incomplete implementation,
//      i.e. this is a base class that is inheritted and should never be instantiated on its own.
public abstract class Tile : MonoBehaviour
{
    // 'protected' - same as private, except that derived (inheritted) Tiles can use it
    [SerializeField] protected SpriteRenderer _renderer;
    [SerializeField] [Range(0f, 1f)] protected float _highBrightness;
    [SerializeField] [Range(0f, 1f)] protected float _darkenStrength;
    // Defines whether or not a tile is traversable (e.g. grass vs mountain)
    [SerializeField] private bool _isWalkable;
    [SerializeField] private int movementCost;
    
    private TextMeshPro displayText;
    private int distance;
    public int Distance {get{return distance;}set{distance = value;displayText.text = $"{value}";}}
    public Tile Previous;
    public int Infinity = 10000;

    public int MovementCost {get{return movementCost;}set{movementCost = value;}}
    public NodeBase nodeBase;
    public string TileName;
    public BaseUnit OccupiedUnit;

    // Walkable is true if this terrain is traversable and is not currently occupied by another unit
    public bool Walkable => _isWalkable && OccupiedUnit == null;
    public bool Reachable {get; set;}

    public List<Tile> Neighbors;
    protected Color _highlighted;
    protected Color _original;
    public Color OriginalColor {get {return _original;} set {_original = value;}}

    delegate Color handleHighlight(Tile t, TileEvent te);
    handleHighlight EffectsDelegate;

    private TileEffects AbilityEffectsController;

    // Virtual methods can be overridden by classes that inherit this class
    public virtual void Init(Vector2 coords, bool altColor)
    {
        nodeBase = new NodeBase(this, coords);
        this.name = $"Tile {coords.x} {coords.y}";
        Reachable = false;
        EffectsDelegate = _defaultHighlight;
        distance = Infinity;
        Previous = null;
        displayText = GetComponentInChildren<TextMeshPro>();
        displayText.enabled = false;
    }

    public void SetNeighbors(List<Tile> neighbors){
        Neighbors = neighbors;
    }

    // Player (Hero) clicked this tile to move to
    void OnMouseDown(){
        if (AbilityEffectsController != null){
            AbilityEffectsController.OnMouseDown(this);
            return;
        }

        UnitManager.Instance.SelectTile(this);
    }

    void OnMouseEnter()
    {
        MenuManager.Instance.ShowTileInfo(this);

        if (AbilityEffectsController != null){
            AbilityEffectsController.OnMouseEnter(this);
            return;
        }
    
        Highlight(TileEvent.Highlight);
    }

    void OnMouseExit()
    {
        MenuManager.Instance.ShowTileInfo(null);

        if (AbilityEffectsController != null){
            AbilityEffectsController.OnMouseExit(this);
            return;
        }

        Highlight(TileEvent.UnHighlight);
    }


    // Ensures that the request ShadeSwitch option is set. Nothing happens if it already is.
    public void Highlight(TileEvent te){
        _renderer.color = EffectsDelegate(this, te);
    }

    private Color _defaultHighlight(Tile t, TileEvent te){
        var original = t.OriginalColor;

        switch(te){
            case TileEvent.Highlight:
                return _highlighted;
            case TileEvent.UnHighlight:
                return original;
            default:
                return original;
        }
    }

    // Called when spawning units since we don't want to change the GameState
    public bool SetUnit(BaseUnit unit){
        if (!Walkable)
            return false;

        if (unit.OccupiedTile != null)
            unit.OccupiedTile.OccupiedUnit = null;
        
        unit.transform.position = transform.position;
        OccupiedUnit = unit;
        unit.OccupiedTile = this;

        return true;
    }

    public void SetAbilityEffectsController(TileEffects te, bool setReachable){
        // New ability takes over. Unset the old one and revert color
        if (AbilityEffectsController != null)
            _renderer.color = AbilityEffectsController.Unset(this);
        
        // Set the new one and get the configured init color
        AbilityEffectsController = te;
        _renderer.color = te.InitPreview(this);

        // Set the coloring delegate
        EffectsDelegate = AbilityEffectsController.TileEffectsHighlight;

        // Allow grid manager to track tiles affected by abilities
        GridManager.Instance.AffectedTiles.Enqueue(this);

        if (setReachable){
            Reachable = true;
        }
    }

    /*
     * ONLY CALL FROM GRIDMANAGER
     * (Also: Distance and Previous should be reset no sooner than after the turn has ended)
    */
    public void Reset(){
        if (AbilityEffectsController != null){
            _renderer.color = AbilityEffectsController.Unset(this);
            EffectsDelegate = _defaultHighlight;
            AbilityEffectsController = null;
            Reachable = false;
            return;
        }
        _renderer.color = _original;
    }

    public void ToggleDisplayCost(){
        displayText.enabled = !displayText.enabled;
    }

    public void DisplayCellCoords(Vector2Int v3){
        TextMeshPro displayText = GetComponentInChildren<TextMeshPro>();
        displayText.text = $"({v3.x},{v3.y})";
    }
}

public enum TileEvent{
    Highlight = 0,
    UnHighlight = 1
}

