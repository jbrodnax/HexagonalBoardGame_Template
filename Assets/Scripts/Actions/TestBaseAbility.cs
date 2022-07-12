using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBaseAbility : MonoBehaviour
{
    [SerializeField] public Color TargetableColor;
    [SerializeField] public Color AffectedColor;
    [SerializeField] [Range(0f,1f)] public float HighlightBlender;
    [SerializeField] [Range(0f,1f)] public float DarkenBlender;
    [SerializeField] public string AbilityName;
    [SerializeField] public TileEffects TileEffectsPrefab;
    [SerializeField] public List<Action> Actions;
    [SerializeField] public int MinimumDamage;
    [SerializeField] public int MaximumDamage;
    [SerializeField] public int APCost;

    private Stack<Action> uninitActions;
    private Stack<Action> initActions;
    protected Tile sourceTile;
    public Tile SourceTile {get {return sourceTile;} set {sourceTile = value;}}
    protected BaseUnit callerUnit;
    public BaseUnit CallerUnit {get {return callerUnit;} set {callerUnit = value;}}
    private List<Cube> affectedCoordinates;
    private List<Cube> targetableCoordinates;
    private List<Tile> affectedTiles;
    private List<Tile> targetableTiles;
    private Action currentAction;
    private Action targetable;


    public void Init(Tile src, BaseUnit unit){
        sourceTile = src;
        callerUnit = unit;

        uninitActions = new Stack<Action>();
        initActions = new Stack<Action>();
        affectedCoordinates = new List<Cube>();

        Actions.Reverse();
        foreach(Action action in Actions){
            uninitActions.Push(action);
        }

        var origin = sourceTile.nodeBase.GetCubeFromThis();
        while (nextAction() == true){
            
            currentAction.Init(origin, Cardinal.NE);

            // If "end" is not set, then there are targetable coordinates
            if (currentAction.End == null)
            {
                // Set targetable action.
                // Fetch targetable coordinates and Tiles, and set their controller
                targetable = currentAction;
                targetableCoordinates = targetable.TargetableCoordinates;
                if (targetableCoordinates != null)
                {
                    targetableTiles = GridManager.Instance.GetTilesFromCubes(targetableCoordinates);
                    foreach (Tile t in targetableTiles)
                    {
                        t.SetAbilityController(this, TargetableColor);
                    }
                }

                break;
            }

            // There are no targetable coordinates, so execute the action
            currentAction.Execute();

            // Fetch affected coordinates, if any
            foreach (Cube a in currentAction.AffectedCoordinates){
                addUniqueCube(a, affectedCoordinates);
            }
            // Set "end" as the next action's origin
            origin = currentAction.End;
        }

        // Find affected Tiles from coordinates and set their controller
        affectedTiles = GridManager.Instance.GetTilesFromCubes(affectedCoordinates);
        foreach (Tile t in affectedTiles)
            t.SetAbilityController(this, AffectedColor);

    }

    public void MouseEnterHandler(Tile dst){
        if (targetable == null)
            return;

        // Get the dest Tile coordinate and set it as the end for the current action
        var target = dst.nodeBase.GetCubeFromThis();
        targetable.End = target;

        
        if (uninitActions.Count > 0)
        {
            var origin = targetable.End;
            while (nextAction() == true)
            {
                // TKTK - hardcoded direction
                currentAction.Init(origin, Cardinal.NE);
                origin = currentAction.End;
            }
        }

        // TKTK - hardcoded direction
        executeChain(Cardinal.NE);

        return;
    }

    public void MouseDownHandler(Tile dst){
        // TKTK - set ability to Locked state
        return;
    }

    public void MouseExitHandler(Tile dst){
        // TKTK - does anything need to happen here?
        return;
    }

    public Color AbilityHighlightHandler(Tile tile, TileEvent tevent){
        var color = tile.GetColor;

        switch(tevent){
            case TileEvent.Highlight:
                return Color.Lerp(color, Color.white, HighlightBlender);
            case TileEvent.UnHighlight:
                return tile.AbilityBaseColor;
            default:
                return color;
        }
    }

    private void addUniqueCube(Cube a, List<Cube> list){
        foreach (Cube c in list){
            if (a.Compare(c) == true)
                return;
        }

        list.Add(a);
    }

    private bool nextAction(){
        if (currentAction != null)
        {
            initActions.Push(currentAction);
            Debug.Log($"Pushed: {currentAction.ToString()}");
        }

        return uninitActions.TryPop(out currentAction);
    }

    private void updateAffectedTiles(){
        // Gather up all the new affected coordinates
        foreach (Action action in initActions){
            foreach (Cube c in action.AffectedCoordinates)
                addUniqueCube(c, affectedCoordinates);
        }

        affectedTiles = GridManager.Instance.GetTilesFromCubes(affectedCoordinates);
        foreach (Tile tile in affectedTiles)
            tile.SetAbilityController(this, AffectedColor);
    }

    /// <summary>
    /// Execute the actions that have been initialized, while updating their origin.
    /// Updates affectedTiles.
    /// </summary>
    /// <param name="direction">The direction that unit is facing.</param>
    private void executeChain(Cardinal direction){
        Action prev = null;
        var actionChain = new Stack<Action>(initActions.ToArray());
        foreach (Action action in actionChain){
            if (prev != null)
            {
                action.Origin = prev.End;
                action.Direction = direction;
            }
            Debug.Log($"executeChain: {action.ToString()}");
            action.Execute();
            prev = action;
        }

        foreach (Tile tile in affectedTiles){
            tile.UnsetAbilityController();
        }

        // Remove previously affected Tiles and coordinates
        affectedTiles.Clear();
        affectedCoordinates.Clear();

        // Re-render targetable Tiles controllers
        foreach (Tile tile in targetableTiles){
            tile.SetAbilityController(this, TargetableColor);
        }
        updateAffectedTiles();
    }
}
