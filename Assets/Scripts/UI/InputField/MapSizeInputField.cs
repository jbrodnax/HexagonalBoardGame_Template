using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MapSizeInputField : MonoBehaviour
{
    public int MapSize;
    public void SetMapSize(string strnum){
        try{
            MapSize = Int32.Parse(strnum);
        }catch (FormatException){
            Debug.Log($"Invalid Map size selection: {strnum}");
        }

        MainMenuManager.Instance.MapSize = MapSize;
    }

    public int GetMapSize(){
        return MapSize;
    }
}
