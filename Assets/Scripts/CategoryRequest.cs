using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CategoryRequest : MonoBehaviour
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
    public void Reload()
    {
        if (StartGame.Category != -1)
            StartCoroutine(routine: SendRequest());
    }

    private IEnumerator SendRequest()
    {
        UnityWebRequest request = UnityWebRequest.Get(StartGame.SERVERURL + url);
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
            StartGame.CategoryIDs.Clear();
            diagnoses = JsonUtility.FromJson<DiagnosisList>(request.downloadHandler.text);
            List<string> temp = new List<string>();
            foreach (Diagnosis diagnosis in diagnoses.diagnoses)
            {
                temp.Add(diagnosis.diagnosis_text);
                StartGame.CategoryIDs.Add(diagnosis.id);
            }
            dropdown.ClearOptions();
            dropdown.AddOptions(temp);
            StartGame.Category = StartGame.CategoryIDs[dropdown.value];
            //Debug.Log(dropdown.options.Count);
            //Debug.Log(dropdown.options[0].text);
        }
    }
}
