using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    public int MapSize {get;set;}
    public Tilemap Tilemap { get; set; }

    void Awake(){
        if (Instance == null)
            Instance = this;
        
        DontDestroyOnLoad(this.gameObject);
    }
}
