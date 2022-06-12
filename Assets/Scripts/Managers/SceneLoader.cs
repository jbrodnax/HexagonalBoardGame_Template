using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;
    void Awake(){
        if (Instance == null){
            Instance = this;
        }
    }

    public void Load(string scenename){
        SceneManager.LoadScene(scenename);
    }
}

/*
 ** Adding Scene Change to Button **
 * 1. Create 'ChangeSceneButton.cs' script
 * 2. Add script to the button as a script component
 * 3. Add a new OnClick element to the button
 * 4. Drag the script component under the RunTime Only field
 * 5. In the Function field to the left, select the script's function
 * 6. In the field under that, type the value to be passed to the selected function.
*/