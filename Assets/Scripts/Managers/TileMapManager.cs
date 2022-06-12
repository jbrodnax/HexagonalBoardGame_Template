using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileReplacement[] spriteToPrefabLinks;


    void Awake(){
        
    }

    void Start(){
        GetAllTiles();
    }

    public void GetAllTiles(){
        var oddColOffset = grid.cellSize.x / 2;
        var ySpacing = Mathf.Sqrt(3)/2;
        var xSpacing = (3f/4f);
        tilemap.CompressBounds();

        foreach (var position in tilemap.cellBounds.allPositionsWithin) {
            if (!tilemap.HasTile(position)) {
                continue;
            }
            var tileSprite = tilemap.GetTile(position);
            var worldPos = grid.GetCellCenterWorld(position);
            Vector2 v2 = new Vector2(worldPos.x, worldPos.y);
            
            foreach (TileReplacement tr in spriteToPrefabLinks){
                if (tr.TileSprite.name.Equals(tileSprite.name)){
                    var newTile = Instantiate(tr.TilePrefab, v2, Quaternion.identity);
                    Vector2Int coords = new Vector2Int();
                    coords.x = Mathf.RoundToInt(v2.x/xSpacing);

                    if (coords.x % 2 != 0){
                        if (v2.y > 0 && v2.y < ySpacing)
                            coords.y = Mathf.RoundToInt((v2.y) / oddColOffset);
                        else if (v2.y > 0 && v2.y > ySpacing)
                            coords.y = Mathf.RoundToInt((v2.y) / (ySpacing + oddColOffset))+1;
                        else if (v2.y < 0)
                            coords.y = Mathf.RoundToInt((v2.y + oddColOffset) / (ySpacing));
                        else
                            Debug.Log($"Reached odd case: {coords}");
                    }else{
                        coords.y = Mathf.RoundToInt((v2.y) / ySpacing);
                    }

                    // Invert y-axis
                    coords.y = -coords.y;

                    
                    //newTile.DisplayCellCoords(OddqToCube(coords));
                    Debug.Log($"Found {tileSprite.name} Sprite:\nCell Position: {position}\nWorld Position: {worldPos}\nNew Tile: {newTile}");
                }
            }
        }
    }

    public Vector3Int OddqToCube(Vector2Int v2){
        var q = v2.x;
        var r = v2.y - (v2.x - (v2.x & 1)) / 2;
        return new Vector3Int(q, r, -q-r);
    }

    private void Update(){
       
    }
}
/*
[Serializable]
public struct TileReplacement{
    public TileBase TileSprite;
    public Tile TilePrefab;
}
*/