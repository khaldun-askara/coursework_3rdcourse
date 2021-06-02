using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class QuestionList
{
    public List<Question> questions;
    public QuestionList() { questions = new List<Question>(); }
    public QuestionList(List<Question> questions)
    {
        this.questions = questions;
    }
    public void Add(Question question)
    {
        questions.Add(question);
    }
}
[Serializable]
public class Question
{
    public int id;
    public string question_text;
    public Question() { }
    public Question(int id, string question_text)
    {
        this.id = id;
        this.question_text = question_text;
    }
}

public class QuestionsRequest : MonoBehaviour
{
    [SerializeField] private string url;
    [SerializeField] private string urlofanswer;
    public GameObject SomethigWrongPanel;
    private bool isServerRespond = false;
    public Text ErrorText;
    public static QuestionList questions;
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
        UnityWebRequest request = UnityWebRequest.Get(StartGame.SERVERURL + url + StartGame.RightDiagnosis);
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
            questions = JsonUtility.FromJson<QuestionList>(request.downloadHandler.text);
            //Debug.Log(questions.questions[0].question_text);
            if (questions == null || questions.questions.Count == 0)
                panelTitle.text = "Вопросов нет";
            foreach (Question question in questions.questions)
            {
                var instance = GameObject.Instantiate(prefabButton.gameObject) as GameObject;
                instance.transform.SetParent(parentContainer, false);
                Button newbutton = instance.transform.GetComponent<Button>();
                newbutton.GetComponentInChildren<Text>().text = question.question_text;
                newbutton.onClick.AddListener(() => { StartCoroutine(routine: SendRequestAnswer(question.id,
                                                                                                question.question_text, 
                                                                                                logElemPrefab, 
                                                                                                parentLog,
                                                                                                answerBackground, 
                                                                                                answerText, 
                                                                                                questionsPanel, 
                                                                                                inGameButtons, newbutton));});
            }
        }
    }
    private IEnumerator SendRequestAnswer(int id, string qtext, 
        RectTransform logElemPrefab, RectTransform parentLog,
        GameObject answerBackground, Text answerText,
        GameObject questionsPanel, GameObject inGameButtons,
        Button buttonToDestroy)
    {
        buttonToDestroy.interactable = false;
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
            string atext = request.downloadHandler.text;
            // создаём и добавляем новый элемент лога:
            var instance = GameObject.Instantiate(logElemPrefab.gameObject) as GameObject;
            instance.transform.SetParent(parentLog, false);
            Image newimage = instance.transform.GetComponent<Image>();
            newimage.GetComponentInChildren<Text>().text = "Q: " + qtext + "\nA: " + atext;

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
}
