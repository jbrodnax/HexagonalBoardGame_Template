# HexagonalBoardGame_Template

## Managers

### GameManager
---

Manages the state of the game, which can be one of the following:

~~~C#
public enum GameState{
    GenerateGrid = 0,
    SpawnTeams = 1,
    AwaitTurn = 2,
    EndTurn = 3,
    GameOver = 4
}
~~~

Each state handles and initiates the processes that need to happen for the cooresponding state, as detailed below:

* **TKTK InitGameSettings**: Pulls game configuration options from the MainMenuManager. These options will then be passed to the appropriate managers.
* **GenerateGrid**: Calls upon the GridManager to build the board for the game. This should pass the selected tilemap to the GridManager.
* **SpawnTeams**: Calls upon the UnitManager to initialize the teams (TKTK change to Factions) selected in the main menu, along with all units associated with a team.
* **AwaitTurn**: Calls upon the UnitManager to begin a team's turn, and does nothing more until the turn ends. TKTK move CalculateMapTraversal call to UnitManager.
* **EndTurn**: Calls upon the UnitManager to handle ending the turn.
* **GameOver**: Ends the game. TKTK implement logic for changing to this state and calling post-game functionality.

### GridManager
---

TKTK

### MainMenuManager
---

TKTK

### MenuManager
---

TKTK

### TeamManager
---

TKTK

### UnitManager
---

TKTK

### SceneManager
---

TKTK

## Tiles

**TKTK Everything is based on Movement Cost**: Consider making everything 'traversable' and that it's just a matter of cost. This would probably require switching the movement cost calculations (for abilities) to be based on the 'exit' cost of a tile instead of the 'entry' cost. 

### Attributes:

* **AffectingAbility**: the ability currently affecting the tile. Overwrites the tile's highlighting functionality.
* **Cube**: Instance of the Cube class, which stores the tile's cube coordinates and implements cube coordinate formulas.
* **Neighbors**: An array of the tile's neighbors (initialized at the start of the game). TKTK this array should recalculate all neighbors each call incase a neighboring tile has been destroyed.
* **MovementCost**: The cost required to move onto the tile.
* **Distance**: Stores the total movement cost required to reach the tile from a source tile. This distance is calculated at the start of each turn.
* **isWalkable**: Defines whether or not a unit can occupy or pass through this tile. Stone tiles are not walkable by default. If a traversable tile is occupied by a unit, then it becomes temporarily not walkable while occupied by the unit.
* **isTargettable**: Differs from `isWalkable` in that the tile might not be walkable, but can still be targettable. Necessary for abilities that use `isWalkable` to determine LoS, but need to include tiles occupied by units in their affected tiles for the sake of targetability.


## Notes
* Turns consume action points. Turn ends after using all action points. Abilities consume x action points.
* Teams are constructed via having x,y,or z number of points to spend to create your army. Number of points to spend depends on game-type. 
* Factions have a reputation matrix that describes what other factions they can ally with during a game.
* Offensive abilities have tiers, each tier scales the stats of the ability.
* Units can level up, where leveling up allows them to pickup a new passive buff.
* Inkarnate - online tile map creator. (HexTML is a simpler alternative)
* AoE buffs affect tile movement costs.
* Implement who Vanari faction first along with all of their abilities (multiple instances of the faction at a time can play against each other).

## TODO Components

### Server
---

**Capabilities**

* Authentication / Authorization (flat?)
* Lobby Hosting / Game Creation
    * Map awerness. Uploading new maps to server?
* Game Management:
    * Relay and synchronize game event data across clients
    * Validate events of each player turn (movement, ability usage, buffs, unit level)
    * Validate requests i.e. is the request coming from a client whose turn it is?

**Design**

* Custom TCP protocol
* 1 request per turn. Sent at end of turn, summarizing events of the turn.
* Server validates the request, and the events of the request.
* If valid, relay updated game state to connected clients.
* If NOT valid, notify all clients of an invalid move and that the turn is still in affect.

## External Resources
* **Redblob Games**: https://www.redblobgames.com/grids/hexagons/
