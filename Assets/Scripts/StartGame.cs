using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    public static int RightDiagnosis = 0;
    public static int Category = -1;
    public static int GivenDiagnosis = -1;
    public static int PlayerId = 1;
    public static List<int> CategoryIDs = new List<int>();
    public static List<int> DiagnosisIDs = new List<int>();
    [SerializeField] private string url;
    public GameObject SomethigWrongPanel;
    private bool isServerRespond = false;
    public Text ErrorText;
    public static string SERVERURL = "http://foreversick.somee.com/";

    public void Press() {
        StartCoroutine(routine: SendRequest());
        
    }
    private IEnumerator SendRequest()
    {
        UnityWebRequest request = UnityWebRequest.Get(SERVERURL +url+StartGame.Category);
        yield return request.SendWebRequest();
        isServerRespond = !(request.isHttpError || request.isNetworkError);
        //|| !int.TryParse(request.downloadHandler.text, out RightDiagnosis);
        //Debug.Log(isServerRespond);
        if (!isServerRespond)
        {
            //Debug.Log(request.error);
            ErrorText.text = request.error;
            SomethigWrongPanel.SetActive(true);
        }
        else
        {
            int.TryParse(request.downloadHandler.text, out RightDiagnosis);
            SceneManager.LoadScene("Main 2.0");
        }
        Debug.Log(RightDiagnosis);
    }
}
