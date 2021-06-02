using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameResult : MonoBehaviour
{
    [SerializeField] private string urlToPostRound;
    public GameObject SomethigWrongPanel;
    private bool isServerRespond = false;
    public Text ErrorText;
    public GameObject finalpanel;
    public Text finaltext;
    public Dropdown dropdown;

    public void CheckResult()
    {
        StartCoroutine(routine: SendRequest());
    }

    private IEnumerator SendRequest()
    {
        //Request request = new Request();
        //request.player_id = StartGame.PlayerId;
        //request.right_diagnosis_id = StartGame.RightDiagnosis;
        //request.given_diagnosis_id = StartGame.GivenDiagnosis;
        WWWForm formData = new WWWForm();
        Request request = new Request()
        {
            player_id = StartGame.PlayerId,
            right_diagnosis_id = StartGame.RightDiagnosis,
            given_diagnosis_id = StartGame.GivenDiagnosis
        };
        string gameresultJSON = JsonUtility.ToJson(request);
        //Debug.Log(gameresultJSON);
        UnityWebRequest requestDiagnosis = UnityWebRequest.Post(StartGame.SERVERURL + urlToPostRound, formData);
        byte[] requestBytes = Encoding.UTF8.GetBytes(gameresultJSON);
        UploadHandler uploadHandler = new UploadHandlerRaw(requestBytes);
        requestDiagnosis.uploadHandler = uploadHandler;
        requestDiagnosis.SetRequestHeader("Content-Type", "application/json; charset = UTF-8");
        yield return requestDiagnosis.SendWebRequest();
        isServerRespond = !(requestDiagnosis.isHttpError || requestDiagnosis.isNetworkError);
        //Debug.Log(isServerRespond);
        if (!isServerRespond)
        {
            //Debug.Log(requestDiagnosis.error);
            ErrorText.text = requestDiagnosis.error;
            SomethigWrongPanel.SetActive(true);
        }
        else
        {
            int diagnosisnumber = StartGame.DiagnosisIDs.FindIndex(x => x == StartGame.RightDiagnosis);
            Debug.Log(diagnosisnumber);
            string rightDiagnosisName = dropdown.options[diagnosisnumber].text;
            Debug.Log(rightDiagnosisName);
            finalpanel.SetActive(true);
            finaltext.text = "Вы дали ";
            if (StartGame.GivenDiagnosis == StartGame.RightDiagnosis)
                finaltext.text += "верный диагноз!";
            else finaltext.text += "неверный диагноз. Верный диагноз: " + rightDiagnosisName + ".";
        }
    }
}
public class Request
{
    public int player_id;
    public int right_diagnosis_id;
    public int given_diagnosis_id;
}
