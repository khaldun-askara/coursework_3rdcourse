using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class DiagnosisList
{
    public List<Diagnosis> diagnoses;
    public DiagnosisList() { diagnoses = new List<Diagnosis>(); }
    public DiagnosisList(List<Diagnosis> diagnoses)
    {
        this.diagnoses = diagnoses;
    }
    public void Add(Diagnosis diagnosis)
    {
        diagnoses.Add(diagnosis);
    }
}
[Serializable]
public class Diagnosis
{
    public int id;
    public string diagnosis_text;
    public string mcb_code;
    public Diagnosis() { }
    public Diagnosis(int id, string diagnosis_text, string mcb_code)
    {
        this.id = id;
        this.diagnosis_text = diagnosis_text;
        this.mcb_code = mcb_code;
    }
}
public class DiagnosesPequest : MonoBehaviour
{
    [SerializeField] private string url;
    public GameObject SomethigWrongPanel;
    private bool isServerRespond = false;
    public Text ErrorText;
    public static DiagnosisList diagnoses;
    public Dropdown dropdown;

    void Start()
    {
        StartCoroutine(routine: SendRequest());
    }
    private IEnumerator SendRequest()
    {
        UnityWebRequest request = UnityWebRequest.Get(StartGame.SERVERURL + url + StartGame.Category);
        yield return request.SendWebRequest();
        isServerRespond = !(request.isHttpError || request.isNetworkError);
        //Debug.Log(isServerRespond);
        if (!isServerRespond)
        {
            //Debug.Log(request.error);
            ErrorText.text = request.error;
            SomethigWrongPanel.SetActive(true);
        }
        else
        {
            //Debug.Log(request.downloadHandler.text);
            StartGame.DiagnosisIDs.Clear();
            diagnoses = JsonUtility.FromJson<DiagnosisList>(request.downloadHandler.text);
            List<string> temp = new List<string>();
            foreach (Diagnosis diagnosis in diagnoses.diagnoses)
            {
                temp.Add(diagnosis.diagnosis_text);
                StartGame.DiagnosisIDs.Add(diagnosis.id);
            }
            dropdown.AddOptions(temp);
            StartGame.GivenDiagnosis = StartGame.DiagnosisIDs[dropdown.value];
        }
    }
}
