using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance;

    private Dictionary<Faction,Team> _teams;
    private Queue<Team> _teamTurnQueue;
    private Team _activeTeam;
    public Team ActiveTeam
    {
        get {return _activeTeam;}
        set {_activeTeam = value;}
    }
    void Awake()
    {
        Instance = this;
    }

    public void Init(List<Faction> factions){
        Team newTeam;
        _teams = new Dictionary<Faction, Team>();
        _teamTurnQueue = new Queue<Team>();

        foreach(Faction faction in factions){
            newTeam = generateTeam(faction);
            _teams[faction] = newTeam;
            _teamTurnQueue.Enqueue(newTeam);
        }
    }

    private Team generateTeam(Faction f){
        if (_teams.ContainsKey(f))
            return _teams[f];
        
        return (new Team());
    }

    public void AddTeamMembers(Faction faction, List<BaseUnit> members){
        Team team = _teams[faction];
        string name;

        if (faction == Faction.Red)
            name = "Red";
        else if (faction == Faction.Blue)
            name = "Blue";
        else
            name = "Bad??";

        // TKTK - seems a bit risky to rely on this
        if (team.TeamName == null){
            team.Init(members, name);
        }
    }

    public void nextTurn(){
        // Pull next team from queue
        Team nextTeam = _teamTurnQueue.Dequeue();
        // Place them at the end of the queue
        _teamTurnQueue.Enqueue(nextTeam);
        // Set them as the active team
        _activeTeam = nextTeam;
    }
}
