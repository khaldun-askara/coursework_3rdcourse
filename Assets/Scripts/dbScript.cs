using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mono.Data.Sqlite;
using System.Data;
using System;
using UnityEngine.UI;

public class QuesAnsw
{
    private string question;
    private List<string> answers = new List<string>();

    public QuesAnsw(string question, string answer)
    {
        this.question = question;
        this.answers.Add(answer);
    }
    public QuesAnsw() { }
    public string Question
    {
        get => question;
    }

    public void AddAnswer(string answer)
    {
        this.answers.Add(answer);
    }

    public string Answers
    {
        get => String.Join("; ", answers);
    }
}

public class dbScript : MonoBehaviour
{
    public RectTransform prefab;
    public RectTransform parent;
    public List<QuesAnsw> currentQues = new List<QuesAnsw>();
    public Text logtext;
    public GameObject answerbackground;
    public GameObject questionsPanel;
    public GameObject ingameButtons;
    public Text answertext;
    public int TimeOfAnswer = 10;
    private void Start()
    {
        using (Mono.Data.Sqlite.SqliteConnection Connect = new Mono.Data.Sqlite.SqliteConnection(@"Data Source=C:\Users\Aisen Sousuke\OneDrive\Изображения\PSD\рисун очки\game shit\illness; Version=3;"))
        {
            Connect.Open();
            Mono.Data.Sqlite.SqliteCommand Command = new Mono.Data.Sqlite.SqliteCommand
            {
                Connection = Connect,
                CommandText = @"SELECT diseases.id, diseases.disease, questions.ID, questions.question, symptoms.id, symptoms.symptom
                                FROM answers
                                JOIN diseases ON answers.disease_id = diseases.id
                                JOIN symptoms ON answers.symptom_id = symptoms.id
                                JOIN questions ON answers.question_id = questions.id
                                WHERE diseases.ID IN (SELECT ID FROM diseases  ORDER BY RANDOM() LIMIT 1)
                                ORDER BY questions.id"
            };
            Mono.Data.Sqlite.SqliteDataReader sqlReader = Command.ExecuteReader();
            int prevQuesID = -1;
            int quescount = -1;
            while(sqlReader.Read())
            {
                //Debug.Log(sqlReader.GetString(1));
                int curQuesID = sqlReader.GetInt32(2);
                // посление из прошлого: планировалось, что у вопросов могут быть составные ответы,
                // поэтому нам надо проверять, добавляли ли мы уже такой вопрос
                // если да, то просто добавляем к последнему вопросу ответ
                // если нет, добавляем новый вопрос
                if (curQuesID == prevQuesID)
                    currentQues[quescount].AddAnswer(sqlReader.GetString(5));
                else
                {
                    currentQues.Add(new QuesAnsw(sqlReader.GetString(3), sqlReader.GetString(5)));
                    prevQuesID = curQuesID;
                    quescount++;
                }

            }
            Connect.Close();
        }

        foreach (QuesAnsw ques in currentQues)
        {
            var instance = GameObject.Instantiate(prefab.gameObject) as GameObject;
            instance.transform.SetParent(parent, false);
            Button newbutton = instance.transform.GetComponent<Button>();
            newbutton.GetComponentInChildren<Text>().text = ques.Question;
            newbutton.onClick.AddListener(() => { Listener(logtext, answerbackground, 
                answertext, ques.Question, ques.Answers, questionsPanel, ingameButtons); Destroy(newbutton.gameObject);
            }) ;
        }
        //string res = "";
        //foreach (QuesAnsw ques in currentQues)
        //    res += ques.Question + " " + ques.Answers + "\n";
        //Debug.Log(res);
    }

    public void Listener(Text logtext, GameObject answerbackground, 
        Text answertext, string qtext, string atext, 
        GameObject questionsPanel, GameObject ingameButtons)
    { 
        logtext.text +="<b>" + qtext + "</b>\n" + atext + "\n";
        answerbackground.SetActive(true);
        answertext.text = atext;
        questionsPanel.SetActive(false);
        ingameButtons.SetActive(true);
    }
}
