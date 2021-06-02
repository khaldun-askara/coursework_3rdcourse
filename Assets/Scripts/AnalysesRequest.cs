using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;

[Serializable]
public class AnalysisList
{
    public List<Analysis> analyses;
    public AnalysisList() { analyses = new List<Analysis>(); }
    public AnalysisList(List<Analysis> questions)
    {
        this.analyses = questions;
    }
    public void Add(Analysis analysis)
    {
        analyses.Add(analysis);
    }
}
[Serializable]
public class Analysis
{
    public int id;
    public string analysis_text;
    public bool is_enum;

    public Analysis() { }

    public Analysis(int id, string analysis_text, bool is_enum)
    {
        this.id = id;
        this.analysis_text = analysis_text;
        this.is_enum = is_enum;
    }
}

public class AnalysesRequest : MonoBehaviour
{
    [SerializeField] private string listurl;
    [SerializeField] private string enumAnswerUrl;
    [SerializeField] private string numAnswerUrl;
    public GameObject SomethigWrongPanel;
    private bool isServerRespond = false;
    public Text ErrorText;
    public static AnalysisList analyses;
    public RectTransform prefabButton;
    public RectTransform parentContainer;
    public Text panelTitle;
    public RectTransform logElemPrefab;
    public RectTransform parentLog;
    public GameObject answerBackground;
    public Text answerText;
    public GameObject questionsPanel;
    public GameObject inGameButtons;
    void Start()
    {
        StartCoroutine(routine: SendRequest());
    }

    private IEnumerator SendRequest()
    {
        UnityWebRequest request = UnityWebRequest.Get(StartGame.SERVERURL + listurl);
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
            analyses = JsonUtility.FromJson<AnalysisList>(request.downloadHandler.text);
            if (analyses == null || analyses.analyses.Count == 0)
                panelTitle.text = "Индикаторов нет";
            foreach (Analysis analysis in analyses.analyses)
            {
                var instance = GameObject.Instantiate(prefabButton.gameObject) as GameObject;
                instance.transform.SetParent(parentContainer, false);
                Button newbutton = instance.transform.GetComponent<Button>();
                newbutton.GetComponentInChildren<Text>().text = /*"Проверить индикатор \"" +*/ UpFirstLetter(analysis.analysis_text) /*+ "\""*/;
                newbutton.onClick.AddListener(() =>
                {
                    StartCoroutine(routine: SendRequestAnswer(analysis.id, analysis.analysis_text, analysis.is_enum,
                                                              logElemPrefab, parentLog,
                                                              answerBackground, answerText,
                                                              questionsPanel, inGameButtons,
                                                              newbutton));
                });
            }
        }
    }

    private IEnumerator SendRequestAnswer(int id, string qtext, bool isenum,
        RectTransform logElemPrefab, RectTransform parentLog,
        GameObject answerBackground, Text answerText,
        GameObject questionsPanel, GameObject inGameButtons,
        Button buttonToDestroy)
    {
        buttonToDestroy.interactable = false;
        string urlofanswer = isenum ? enumAnswerUrl : numAnswerUrl;
        UnityWebRequest request = UnityWebRequest.Get(StartGame.SERVERURL + urlofanswer + StartGame.RightDiagnosis + "-" + id);
        //Debug.Log(StartGame.RightDiagnosis);
        yield return request.SendWebRequest();
        isServerRespond = !(request.isHttpError || request.isNetworkError);
        //Debug.Log(isServerRespond);
        if (!isServerRespond)
        {
            //Debug.Log(request.error);
            ErrorText.text = request.error;
            SomethigWrongPanel.SetActive(true);
            buttonToDestroy.interactable = true;
        }
        else
        {
            string atext = UpFirstLetter(request.downloadHandler.text);
            // создаём и добавляем новый элемент лога:
            var instance = GameObject.Instantiate(logElemPrefab.gameObject) as GameObject;
            instance.transform.SetParent(parentLog, false);
            Image newimage = instance.transform.GetComponent<Image>();
            newimage.GetComponentInChildren<Text>().text = "Проверено: " + qtext + "\nРезультат: " + atext;

            // выводим ответ в реплику
            answerBackground.SetActive(true);
            answerText.text = atext;
            // закрываем окно выбора вопроса
            questionsPanel.SetActive(false);
            inGameButtons.SetActive(true);
            // удаляем вопрос
            Destroy(buttonToDestroy.gameObject);
        }
    }
    private string UpFirstLetter(string str)
    {
        if (string.IsNullOrEmpty(str))
            return "";
        return char.ToUpper(str[0]) + str.Substring(1,str.Length-1);
    }
}
