using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TKTK - make this an abstract class and implement actual Team class for each supported faction
public class Team
{
    private List<BaseUnit> _teamMembers;
    private Queue<BaseUnit> _turnQueue;
    private BaseUnit _activeUnit;
    private string _teamName;
    public BaseUnit ActiveUnit
    {
        get {return _activeUnit;}
        set {_activeUnit = value;}
    }
    public string TeamName
    {
        get {return _teamName;}
        set {_teamName = value;}
    }

    public void Init(List<BaseUnit> members, string teamName){
        _teamMembers = new List<BaseUnit>();
        _turnQueue = new Queue<BaseUnit>();
        foreach(BaseUnit member in members){
            _teamMembers.Add(member);
            //TKTK - randomize queue ordering
            _turnQueue.Enqueue(member);
        }

        _teamName = teamName;
    }
    public void AddMember(BaseUnit b){
        _teamMembers.Add(b);
        _turnQueue.Enqueue(b);
        b.UnitTeam = this;
    }

    public void nextTurn(){
        BaseUnit nextUnit = _turnQueue.Dequeue();
        _turnQueue.Enqueue(nextUnit);
        _activeUnit = nextUnit;

        return;
    }
}
