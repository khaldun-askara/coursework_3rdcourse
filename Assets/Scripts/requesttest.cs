using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//[Serializable]
//public class QuestionList
//{
//    public List<Question> questions;
//    public QuestionList() { questions = new List<Question>(); }
//    public QuestionList(List<Question> questions)
//    {
//        this.questions = questions;
//    }
//    public void Add(Question question)
//    {
//        questions.Add(question);
//    }
//}
//[Serializable]
//public class Question
//{
//    public int id;
//    public string question_text;
//    public Question() { }
//    public Question(int id, string question_text)
//    {
//        this.id = id;
//        this.question_text = question_text;
//    }
//}
public class requesttest : MonoBehaviour
{
    [SerializeField] private string url;

    private IEnumerator SendRequest()
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        //UnityWebRequest.Post(url, )

        yield return request.SendWebRequest();

        //Debug.Log(request.downloadHandler.text);
        //List<Question> questions = JsonUtility.FromJson<List<Question>>("{\"users\":" + request.downloadHandler.text + "}");
        QuestionList questions = JsonUtility.FromJson<QuestionList>(request.downloadHandler.text);
        //Debug.Log(questions.questions[0].question_text);
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(routine: SendRequest());
    }
}
