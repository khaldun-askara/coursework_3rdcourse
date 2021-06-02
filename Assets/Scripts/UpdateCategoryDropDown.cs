using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateCategoryDropDown : MonoBehaviour
{
    public Dropdown dropdown;
    public void UpdateCategory()
    {
        StartGame.Category = StartGame.CategoryIDs[dropdown.value];
        //Debug.Log("Category updated to " + StartGame.Category);
    }
}
