using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChangeSceneButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI selectionTextBox;

    public void ChangeScene(){
        string scene = selectionTextBox.text;

        if (scene.CompareTo("Select a Scene") == 0){
            Debug.Log("Please select a scene from the dropdown menu. Dumbass.");
            return;
        }
        if (scene.CompareTo("Map2 (Not Implemented)") == 0){
            Debug.Log("Map2 is not yet implemented. Can you not read?");
            return;
        }
        SceneLoader.Instance.Load(scene);
    }
}
