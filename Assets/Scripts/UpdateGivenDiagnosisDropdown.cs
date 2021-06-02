using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateGivenDiagnosisDropdown : MonoBehaviour
{
    public Dropdown dropDown;
    public void UpdateGivenDiagnosis()
    {
        StartGame.GivenDiagnosis = StartGame.DiagnosisIDs[dropDown.value];
    }
}
