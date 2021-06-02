using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ForeverSickWebApp
{
    [Serializable]
    public class QuestionList
    {
        public List<Question> questions { get; set;}
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
        public int id { get; set; }
        public string question_text { get; set; }
        public Question() { }
        public Question(int id, string question_text)
        {
            this.id = id;
            this.question_text = question_text;
        }
    }

    [Serializable]
    public class AnswerList
    {
        public List<Answer> answers { get; set; }
        public AnswerList() { answers = new List<Answer>(); }
        public AnswerList(List<Answer> answers)
        {
            this.answers = answers;
        }
        public void Add(Answer answer)
        {
            answers.Add(answer);
        }
    }
    [Serializable]
    public class Answer
    {
        public int id { get; set; }
        public string answer_text { get; set; }
        public Answer() { }
        public Answer(int id, string answer_text)
        {
            this.id = id;
            this.answer_text = answer_text;
        }
    }

    [Serializable]
    public class QuestionOnAnswer
    {
        public int question_id { get; set; }
        public string question_text { get; set; }
        public int answer_id { get; set; }
        public string answer_text { get; set; }
        public QuestionOnAnswer() { }
        public QuestionOnAnswer (int question_id, string question_text, int answer_id, string answer_text)
        {
            this.question_id = question_id;
            this.question_text = question_text;
            this.answer_id = answer_id;
            this.answer_text = answer_text;
        }
    }
    [Serializable]
    public class QuestionOnAnswerList
    {
        public List<QuestionOnAnswer> questionsOnAnswer { get; set; }
        public QuestionOnAnswerList() { questionsOnAnswer = new List<QuestionOnAnswer>(); }
        public QuestionOnAnswerList(List<QuestionOnAnswer> questionsOnAnswer)
        {
            this.questionsOnAnswer = questionsOnAnswer;
        }
        public void Add(QuestionOnAnswer questionOnAnswer)
        {
            questionsOnAnswer.Add(questionOnAnswer);
        }
    }
    [Serializable]
    public class answers_questions_for_diagnosesString
    {
        public int diagnosis_id { get; set; }
        public int question_id { get; set; }
        public int answer_id { get; set; }
    }
}
