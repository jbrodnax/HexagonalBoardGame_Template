using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapSelectionDropDown : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI selectionTextBox;
    // Start is called before the first frame update
    void Start()
    {
        TMP_Dropdown dropdown = gameObject.GetComponent(typeof(TMP_Dropdown)) as TMP_Dropdown;
        dropdown.options.Clear();

        var ddItems = new List<string>();
        ddItems.Add("Select a Scene");
        ddItems.Add("Map1");
        ddItems.Add("Map2 (Not Implemented)");

        foreach (var item in ddItems){
            dropdown.options.Add(new TMP_Dropdown.OptionData(){text = item});
        }
        
        dropdown.onValueChanged.AddListener(delegate{DropdownItemSelected(dropdown);});
    }

    void DropdownItemSelected(TMP_Dropdown dropdown){
        int index = dropdown.value;
        selectionTextBox.text = dropdown.options[index].text;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
