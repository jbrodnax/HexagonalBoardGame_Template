using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    public int MapSize {get;set;}

    void Awake(){
        if (Instance == null)
            Instance = this;
        
        DontDestroyOnLoad(this.gameObject);
    }
}
