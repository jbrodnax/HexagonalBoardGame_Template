using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;

    //public BaseUnit SelectedUnit;
    public BaseUnit ActiveUnit;
    private List<ScriptableUnit> _units;
    private BaseUnit killed;
    void Awake(){
        Instance = this;

        // This is a feature of the special "Resources" folder in Unity
        // Load all ScriptableUnits from ./Assets/Resources/Units/*
        _units = Resources.LoadAll<ScriptableUnit>("Units").ToList();
    }

    void Update(){
        if (GameManager.Instance.GameState != GameState.AwaitTurn)
            return;
        if (!ActiveUnit)
            return;

        if (Input.GetKeyDown(KeyCode.Escape)){
            ActiveUnit.DeselectAbility();
        }

        // TKTK - remove 7/10
        if (Input.GetKeyDown(KeyCode.J)){
            TestBaseAbility ability = Instantiate(ActiveUnit.testBaseAbility);
            ability.Init(ActiveUnit.OccupiedTile, ActiveUnit);
        }

        foreach(AbilityBinding configItem in ActiveUnit.AbilitiesConfig){
            if (Input.GetKeyDown(configItem.binding)){
                ActiveUnit.SelectAbility(configItem);
                return;
            }
        }

        if (ActiveUnit.SelectedAbility != null){
            if (Input.GetKeyDown(KeyCode.Return)){
                triggerAbility();
                return;
            }

            if (!ActiveUnit.SelectedBinding.Item2)
                return;
            if (Input.GetKeyDown(ActiveUnit.SelectedBinding.Item1.binding)){
                triggerAbility();
            }
        }
    }

    // Spawn all the teams and then change gamestate to await first turn
    public void SpawnAllTeams(List<Faction> teams){
        TeamManager.Instance.Init(teams);

        foreach (Faction f in teams)
            SpawnTeam(f);

        GameManager.Instance.ChangeState(GameState.AwaitTurn);
    }

    // Instantiate all team members and add them to their cooresponding team based on faction
    private void SpawnTeam(Faction faction){
        // Just hardcoding spawning from Resources for now
        // TKTK - implement character/team creation menu and pass config here
        var unitCount = 1;
        Tile randomSpawnTile;
        BaseUnit spawnedUnit;
        List<BaseUnit> teamMembers = new List<BaseUnit>();

        for (int i = 0; i < unitCount; i++){
            var randomPrefab = GetRandomUnit<BaseUnit>(faction);
            spawnedUnit = Instantiate(randomPrefab);
            spawnedUnit.Init();
            // TKTK - temporary until specific map spawn points are implemented
            randomSpawnTile = GridManager.Instance.GetRandomTile();

            randomSpawnTile.SetUnit(spawnedUnit);
            teamMembers.Add(spawnedUnit);
        }

        TeamManager.Instance.AddTeamMembers(faction, teamMembers);
    }

    // Generic method with return type 'T' where 'T' is of type 'BaseUnit'
    // Randomly selects a scriptable unit's Prefab (from _units) who's faction is 'faction'
    private T GetRandomUnit<T>(Faction faction) where T : BaseUnit{
        return (T)_units.Where(u => u.Faction == faction).OrderBy(o => Random.value).First().UnitPrefab;
    }

    public void SelectUnit(BaseUnit unit){
        ActiveUnit.SetTargetUnit(unit);
        MenuManager.Instance.ShowSelectedUnit(unit);
    }

    /*
    * Triggered by mousedown event from tile that is not currently affected by an ability.
    * Sets the active unit's target tile to the selected one.
    * If a unit occupies that tile, then set them as the target unit.
    * Note - BaseUnit.SetTargetTile() handles un/highlighting
    */
    public void SelectTile(Tile tile){
        ActiveUnit.SetTargetTile(tile);
        if (tile.OccupiedUnit != null){
            SelectUnit(tile.OccupiedUnit);
        }
    }

    public void SelectAbility(AbilityBinding binding){
        if (!ActiveUnit.SelectAbility(binding))
            Debug.Log("Failed to select ability.");
    }

    /*
     * Instructs the active unit to trigger their selected ability.
     * If the ability succeeds, check if the ability ends the turn.
     * Same game state accordingly if so.
    */
    private void triggerAbility(){
        if (!ActiveUnit.TriggerAbility()){
            // TKTK - implement better failure notice.
            Debug.Log("Ability failed.");
            return;
        }

        if (ActiveUnit.SelectedAbility.EndsTurn)
            GameManager.Instance.ChangeState(GameState.EndTurn);
    }

    // TKTK - If an ability moves a unit, this should be called after moving the unit.
    // As it will implement additional logic regarding what should happen after a player moves,
    // e.g. now the unit is standing in fire. 
    public void UnitMoved(BaseUnit unit){
        return;
    }

    public void UnitDeath(BaseUnit unit){
        Debug.Log($"{unit.UnitName} has been slain!");
        killed = unit;
        // TKTK - update TeamManager and possibly GameManager?
    }

    public BaseUnit beginTurn(){
        TeamManager.Instance.nextTurn();
        TeamManager.Instance.ActiveTeam.nextTurn();
        ActiveUnit = TeamManager.Instance.ActiveTeam.ActiveUnit;
        MenuManager.Instance.ShowActiveUnit(ActiveUnit);
        ActiveUnit.OccupiedTile.Highlight(TileEvent.Highlight);
        return ActiveUnit;
    }

    // Called when GameState is changed to EndTurn (duh)
    public void endTurn(){
        // clear targets
        ActiveUnit.ClearTargets();

        // clear selected unit window?
        MenuManager.Instance.ShowSelectedUnit(null);

        // destroy units who were killed during turn
        Destroy(killed);

        // change gamestate to AwaitTurn
        GameManager.Instance.ChangeState(GameState.AwaitTurn);
    }
}
