using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    [SerializeField] private GameObject _selectedHeroObject, _tileObject, _tileUnitObject, _activeUnitObject;

    void Awake(){
        Instance = this;
    }

    public void ShowTileInfo(Tile tile){
        string message;

        if (tile == null){
            _tileObject.SetActive(false);
            _tileUnitObject.SetActive(false);
            return;
        }

        // Check if tile is traversable
        message = tile.TileName;
        if (!tile.Walkable)
            message += "(Not Walkable)";

        _tileObject.GetComponentInChildren<Text>().text = message;
        _tileObject.SetActive(true);

        if (tile.OccupiedUnit){
            _tileUnitObject.GetComponentInChildren<Text>().text = tile.OccupiedUnit.UnitName + $" ({tile.OccupiedUnit.Health})";
            _tileUnitObject.SetActive(true);
        }
    }

    public void ShowSelectedUnit(BaseUnit unit){
        if (unit == null){
            _selectedHeroObject.SetActive(false);
            return;
        }
        _selectedHeroObject.GetComponentInChildren<Text>().text = unit.UnitName;
        _selectedHeroObject.SetActive(true);
    }

    public void ShowActiveUnit(BaseUnit unit){
        if (unit == null){
            _activeUnitObject.SetActive(false);
            return;
        }
        _activeUnitObject.GetComponentInChildren<Text>().text = unit.UnitName;
        _activeUnitObject.SetActive(true);
    }
}
