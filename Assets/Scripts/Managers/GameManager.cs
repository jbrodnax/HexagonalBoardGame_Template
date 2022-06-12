using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool _redTeam;
    [SerializeField] private bool _blueTeam;
    [SerializeField] private bool _yellowTeam;
    [SerializeField] private bool _prupleTeam;
    [SerializeField] private bool _orangeTeam;
    [SerializeField] private bool _greenTeam;

    public static GameManager Instance;
    public GameState GameState;

    void Awake(){
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        ChangeState(GameState.GenerateGrid);
    }

    public void ChangeState(GameState newState){
        GameState = newState;
        switch (newState){
            case GameState.GenerateGrid:
                //GridManager.Instance.GenerateAxialHexagram();
                GridManager.Instance.GenerateBoardFromTilemap();
                break;
            case GameState.SpawnTeams:
                List<Faction> teamList = new List<Faction>();
                if (_redTeam)
                    teamList.Add(Faction.Red);
                if (_blueTeam)
                    teamList.Add(Faction.Blue);
                UnitManager.Instance.SpawnAllTeams(teamList);
                break;
            case GameState.AwaitTurn:
                var activeUnit = UnitManager.Instance.beginTurn();
                GridManager.Instance.CalculateMapTraversal(activeUnit.OccupiedTile);
                break;
            case GameState.EndTurn:
                UnitManager.Instance.endTurn();
                //GridManager.Instance.BoardReset();
                //GridManager.Instance.ResetAffectedTiles();
                break;
            case GameState.GameOver:
                Debug.Log("GAME OVER!");
                Application.Quit();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }
}

public enum GameState{
    GenerateGrid = 0,
    SpawnTeams = 1,
    AwaitTurn = 2,
    EndTurn = 3,
    GameOver = 4
}