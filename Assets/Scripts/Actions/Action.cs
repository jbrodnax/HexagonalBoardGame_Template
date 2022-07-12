using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Action", menuName = "Action")]
public class Action : ScriptableObject
{
    public new string name;

    /// <summary>
    /// Describes the range of coordinates that can be targeted upon selecting the parent ability.
    /// If range is 0, then we assume that no target coordinate is required.
    /// </summary>
    [SerializeField] public int range;

    /// <summary>
    /// Describes the shape of targetable coordinates given the range.
    /// </summary>
    [SerializeField] public ActionShape rangeShape;

    /// <summary>
    /// Describes the distance of affected coordinates upon using the parent ability.
    /// </summary>
    [SerializeField] public int radius;

    /// <summary>
    /// Describes the shape of affected coordinates for this action given the radius.
    /// </summary>
    [SerializeField] public ActionShape radiusShape;

    /// <summary>
    /// Determines if the action should consider line-of-sight.
    /// </summary>
    [SerializeField] public bool lineOfSite;

    /// <summary>
    /// Determines if the action should move the unit to the end coordinate (E.g. charge, leap, etc).
    /// </summary>
    [SerializeField] public bool movesUnit;

    /// <summary>
    /// The starting Cube coordinate of the action.
    /// </summary>
    private Cube origin;
    public Cube Origin {get { return origin; } set { origin = value; } }

    /// <summary>
    /// The resulting end Cube coordinate of the action. (Not dependent on moving an actual unit).
    /// </summary>
    private Cube end;
    public Cube End {get { return end; } set { end = value; } }
    /// <summary>
    /// The direction that the unit is facing.
    /// </summary>
    private Cardinal direction;
    public Cardinal Direction {get { return direction; } set { direction = value; } }
    /// <summary>
    /// List of Cube coordinates affected by execution of the action/
    /// </summary>
    private List<Cube> affectedCoordinates;
    public List<Cube> AffectedCoordinates {get { return affectedCoordinates; } set { affectedCoordinates = value; } }
    /// <summary>
    /// If the action requires a target, this will store all the coordinates that can be targeted based on the range and shape.
    /// </summary>
    private List<Cube> targetableCoordinates;
    public List<Cube> TargetableCoordinates {get { return targetableCoordinates; } set { targetableCoordinates = value; } }

    /// <summary>
    /// Initializes the action. If the action provides a targetable range, it will set "targetableCoordinates" appropriately.
    /// </summary>
    /// <param name="startPos">The origin of the action.</param>
    /// <param name="facing">The Cardinal direction that the unit is facing.</param>
    public void Init(Cube startPos, Cardinal facing){
        origin = startPos;
        direction = facing;
        end = null;
        affectedCoordinates = null;
        targetableCoordinates = null;
        Debug.Log($"Initializing {this.ToString()}");

        // Calculate targetable coordinates
        if (range > 0){
            switch(rangeShape){
                case ActionShape.Diamond:
                    // TKTK - implement LoS
                    var cubeDirection = origin.CardinalToCube(direction);
                    targetableCoordinates = origin.Diamond(origin, cubeDirection, range);
                    break;
                case ActionShape.Ring:
                    // TKTK - implement LoS
                    targetableCoordinates = origin.Ring(origin, range);
                    break;
                case ActionShape.Line:
                    // TKTK - need to readdress this in regards to:
                    // 1. How should targetting with actions like charge work?
                    // 2. How should 'piercing' units work?
                    // Currently this implementation only allows "charging" in a Cardinal direction,
                    // and units present in a coord will not cause los.
                    var destination = origin.Add(origin, origin.Scale(origin, range));
                    targetableCoordinates = origin.DrawLine(destination, los: lineOfSite, includeTargets:false);
                    break;
                default:
                    Debug.LogError("Action received invalid Range Shape.");
                    return;
            }
        // Otherwise the end coordinate has to the same as the origin
        }else{
            end = origin;
        }

        return;
    }

    /// <summary>
    /// Called by the parent ability once the "end" coordinate is set, it will set "affectedCoordinates" appropriately.
    /// </summary>
    public void Execute(){
        //Debug.Log($"Executing {this.ToString()}");
        if (origin == null || end == null)
            Debug.LogError("Action.Execute called before Action had been initialized.");
        
        switch(radiusShape){
            case ActionShape.Diamond:
                // TKTK - implement LoS
                var cubeDirection = origin.CardinalToCube(direction);
                affectedCoordinates = origin.Diamond(origin, cubeDirection, radius);
                break;
            case ActionShape.Ring:
                // TKTK - implement LoS
                affectedCoordinates = origin.Ring(origin, radius);
                break;
            case ActionShape.Line:
                // TKTK - Currently this implementation only allows "charging" in a Cardinal direction,
                // and units present in a coord will not cause los.
                affectedCoordinates = origin.DrawLine(end, los: lineOfSite, includeTargets: false);
                break;
            default:
                Debug.LogError("Action received invalid Range Shape.");
                return;
        }

        return;
    }

    public override string ToString()
    {
        var endcoord = (end != null) ? (end.ToString()) : ("null");
        var str = $"Action {name}:\n\tRange: {range}\n\tRadius: {radius}\n\tOrigin: {origin}\n\t{endcoord}";
        return str;
    }
}

public enum ActionShape{
    Diamond = 1,
    Ring = 2,
    Line = 3
}