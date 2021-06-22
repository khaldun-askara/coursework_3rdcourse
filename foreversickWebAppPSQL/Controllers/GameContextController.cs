using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace foreversickWebAppPSQL.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GameContextController : ControllerBase
    {
        private IConfiguration configuration;
        public GameContextController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        //private readonly string sConnStr = new NpgsqlConnectionStringBuilder
        //{
        //    Host = "localhost",
        //    Port = 5432,
        //    Database = "foreversickdb",
        //    Username = Environment.GetEnvironmentVariable("POSTGRESQL_USERNAME"),
        //    Password = Environment.GetEnvironmentVariable("POSTGRESQL_PASSWORD"),
        //    AutoPrepareMinUsages = 2,
        //    MaxAutoPrepare = 10
        //}.ConnectionString;

        //private readonly string sConnStr2 = new NpgsqlConnectionStringBuilder
        //{
        //    Host = "batyr.db.elephantsql.com",
        //    Port = 5432,
        //    Database = "jwgtctko",
        //    Username = "jwgtctko",
        //    Password = "MMvL3tq-f1Qi2vxBj9EFTtpQZKVPOrE5",
        //    AutoPrepareMinUsages = 2,
        //    MaxAutoPrepare = 10
        //}.ConnectionString;
        string sConnStr => configuration["ConnectionStrings:DB"];
        string sConnStr2 => configuration["ConnectionStrings:REMOTEDB"];

        [HttpGet("[action]")]
        // GET: GameContext/RandomDiagnosis
        // берёт весь список диагнозов, про которые есть хоть что-то, и выдаёт один из них по желанию левой пятки троюродной бабули Хейлсберга
        public int RandomDiagnosis()
        {
            int diagnosis = -1;
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                var sCommand = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT ID
                                FROM diagnoses
                                WHERE
                                      ID IN (SELECT diagnosis_id FROM answers_questions_for_diagnoses) OR
                                      ID IN (SELECT diagnosis_id FROM enumerated_indicators_of_diagnoses) OR
                                      ID IN (SELECT diagnosis_id FROM numerical_indicators_of_diagnoses)
                                ORDER BY RANDOM() LIMIT 1",
                };
                using (var sqlReader = sCommand.ExecuteReader())
                    if (sqlReader.Read())
                        diagnosis = sqlReader.GetInt32(0);
            }
            return diagnosis;
        }

        [HttpGet("[action]/{сategory_id}")]
        // GET: GameContext/RandomDiagnosis/сategory_id
        // возвращает рандомный диагноз из категории
        public ActionResult RandomDiagnosis(int сategory_id)
        {
            int diagnosis = -1;
            DiagnosisList diagnosisList = new DiagnosisList();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT id FROM diagnoses WHERE rec_code LIKE (SELECT REC_CODE FROM diagnoses WHERE id = @parent_id LIMIT 1)||'%' AND mkb_code IS NOT NULL
                                    AND (ID IN (SELECT diagnosis_id FROM answers_questions_for_diagnoses) OR
                                         ID IN (SELECT diagnosis_id FROM enumerated_indicators_of_diagnoses) OR
                                         ID IN (SELECT diagnosis_id FROM numerical_indicators_of_diagnoses))
                                         ORDER BY RANDOM() LIMIT 1"
                };
                NpgsqlParameter сategoryParam = new NpgsqlParameter("@parent_id", сategory_id);
                Command.Parameters.Add(сategoryParam);
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    if (sqlReader.Read())
                        diagnosis = sqlReader.GetInt32(0);
            }
            if (diagnosis == -1) return BadRequest(diagnosis);
            return Ok(diagnosis);
        }

        [HttpGet("[action]/{diagnosis_id}")]
        // GET: GameContext/Questions/diagnosis_id
        // возвращает список вопросов для конкретного диагноза
        public string Questions(int diagnosis_id)
        {
            QuestionList list_of_questions = new QuestionList();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT player_questions.id, player_questions.name
                                    FROM player_questions
                                    JOIN answers_questions_for_diagnoses ON answers_questions_for_diagnoses.question_id = player_questions.id
                                    WHERE diagnosis_id = @diagnosis_id"
                };
                NpgsqlParameter diagnosisParam = new NpgsqlParameter("@diagnosis_id", diagnosis_id);
                Command.Parameters.Add(diagnosisParam);
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    while (sqlReader.Read())
                    {
                        Question question = new Question(sqlReader.GetInt32(0), sqlReader.GetString(1));
                        list_of_questions.Add(question);
                    }
            }
            string res = JsonSerializer.Serialize<QuestionList>(list_of_questions);
            return res;
        }
        [HttpGet("[action]/{diagnosis_id}")]
        // GET: GameContext/AnswersOnQuestions/diagnosis_id
        // возвращает список вопрос-ответ для конкретного диагноза
        public string AnswersOnQuestions(int diagnosis_id)
        {
            QuestionOnAnswerList list_of_questions = new QuestionOnAnswerList();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT question_id, player_questions.name, answer_id, patient_answers.name
                                    FROM answers_questions_for_diagnoses
                                    JOIN player_questions ON answers_questions_for_diagnoses.question_id = player_questions.id
                                    JOIN patient_answers ON answers_questions_for_diagnoses.answer_id = patient_answers.id
                                    WHERE diagnosis_id = @diagnosis_id"
                };
                NpgsqlParameter diagnosisParam = new NpgsqlParameter("@diagnosis_id", diagnosis_id);
                Command.Parameters.Add(diagnosisParam);
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    while (sqlReader.Read())
                    {
                        QuestionOnAnswer questionOnAnswer = new QuestionOnAnswer(sqlReader.GetInt32(0),
                                                                                 sqlReader.GetString(1),
                                                                                 sqlReader.GetInt32(2),
                                                                                 sqlReader.GetString(3));
                        list_of_questions.Add(questionOnAnswer);
                    }
            }
            string res = JsonSerializer.Serialize<QuestionOnAnswerList>(list_of_questions);
            return res;
        }
        //[HttpGet("[action]")]
        //// GET: GameContext/Questions/
        //// возвращает список всех вопросов
        //public string Questions()
        //{
        //    QuestionList list_of_questions = new QuestionList();
        //    using (var sConn = new NpgsqlConnection(sConnStr))
        //    {
        //        sConn.Open();
        //        NpgsqlCommand Command = new NpgsqlCommand
        //        {
        //            Connection = sConn,
        //            CommandText = @"SELECT player_questions.id, player_questions.name
        //                            FROM player_questions"
        //        };
        //        using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
        //            while (sqlReader.Read())
        //            {
        //                Question question = new Question(sqlReader.GetInt32(0), sqlReader.GetString(1));
        //                list_of_questions.Add(question);
        //            }
        //    }
        //    string res = JsonSerializer.Serialize<QuestionList>(list_of_questions);
        //    return res;
        //}
        //[HttpGet("[action]")]
        //// GET: GameContext/Answers/
        //// возвращает список всех ответов
        //public string Answers()
        //{
        //    AnswerList list_of_answers = new AnswerList();
        //    using (var sConn = new NpgsqlConnection(sConnStr))
        //    {
        //        sConn.Open();
        //        NpgsqlCommand Command = new NpgsqlCommand
        //        {
        //            Connection = sConn,
        //            CommandText = @"SELECT id, name
        //                            FROM patient_answers"
        //        };
        //        using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
        //            while (sqlReader.Read())
        //            {
        //                Answer answer = new Answer(sqlReader.GetInt32(0), sqlReader.GetString(1));
        //                list_of_answers.Add(answer);
        //            }
        //    }
        //    string res = JsonSerializer.Serialize<AnswerList>(list_of_answers);
        //    return res;
        //}

        [HttpGet("[action]")]
        // GET: GameContext/Analyses
        // возвращает список ВСЕХ возможных осмотров/анализов
        public string Analyses()
        {
            AnalysisList analysisList = new AnalysisList();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT id, name, type_is_enum
                                    FROM patient_indicators"
                };
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    while (sqlReader.Read())
                    {
                        Analysis analysis = new Analysis(sqlReader.GetInt32(0), sqlReader.GetString(1), sqlReader.GetBoolean(2));
                        analysisList.Add(analysis);
                    }
            }
            string res = JsonSerializer.Serialize<AnalysisList>(analysisList);
            return res;
        }

        //[HttpGet("[action]")]
        ////GET: GameContext/Diagnoses
        //// возвращает список ВСЕХ диагнозов
        //public string Diagnoses()
        //{
        //    DiagnosisList diagnosisList = new DiagnosisList();
        //    using (var sConn = new NpgsqlConnection(sConnStr))
        //    {
        //        sConn.Open();
        //        NpgsqlCommand Command = new NpgsqlCommand
        //        {
        //            Connection = sConn,
        //            CommandText = @"SELECT id,
        //                                   mkb_name,
        //                                   mkb_code
        //                            FROM diagnoses
        //                            WHERE mkb_code IS NOT NULL"
        //        };
        //        using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
        //            while (sqlReader.Read())
        //            {
        //                Diagnosis diagnosis = new Diagnosis(sqlReader.GetInt32(0), sqlReader.GetString(1), sqlReader.GetString(2));
        //                diagnosisList.Add(diagnosis);
        //            }
        //    }
        //    var options = new JsonSerializerOptions
        //    {
        //        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
        //        WriteIndented = true
        //    };
        //    string res = JsonSerializer.Serialize<DiagnosisList>(diagnosisList, options);
        //    return res;
        //}

        [HttpGet("[action]/{substring}")]
        //GET: GameContext/DiagnosesBySubstring
        // возвращает список ВСЕХ диагнозов, в названии которых есть подстрока substring
        public string DiagnosesBySubstring(string substring)
        {
            DiagnosisList diagnosisList = new DiagnosisList();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT id,
                                           mkb_name,
                                           mkb_code
                                    FROM diagnoses
                                    WHERE mkb_code IS NOT NULL AND
                                    (mkb_name ILIKE '%' || @substring || '%')"
                };
                NpgsqlParameter substring_param = new NpgsqlParameter("@substring", substring);
                Command.Parameters.Add(substring_param);
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    while (sqlReader.Read())
                    {
                        Diagnosis diagnosis = new Diagnosis(sqlReader.GetInt32(0), sqlReader.GetString(1), sqlReader.GetString(2));
                        diagnosisList.Add(diagnosis);
                    }
            }
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };
            string res = JsonSerializer.Serialize<DiagnosisList>(diagnosisList, options);
            return res;
        }

        [HttpGet("[action]/{substring}")]
        //GET: GameContext/NumericalIndicatorsBySubstring
        // возвращает список ВСЕХ числовых индикаторов, в названии которых есть подстрока substring
        public string NumericalIndicatorsBySubstring(string substring)
        {
            NumericalIndicatorList indicatorsList = new NumericalIndicatorList();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT indicator_id,
                                           name,
                                           min_value,
                                           max_value,
                                           normal_min,
                                           normal_max,
                                           units_name,
                                           accuracy
                                    FROM numerical_indicators_ranges
                                    JOIN patient_indicators ON numerical_indicators_ranges.indicator_id = patient_indicators.id
                                    WHERE name ILIKE '%' || @substring || '%'"
                };
                NpgsqlParameter substring_param = new NpgsqlParameter("@substring", substring);
                Command.Parameters.Add(substring_param);
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    while (sqlReader.Read())
                    {
                        NumericalIndicator indicator = new NumericalIndicator(sqlReader.GetInt32(0), 
                                                                              sqlReader.GetString(1),
                                                                              sqlReader.GetDouble(2),
                                                                              sqlReader.GetDouble(3),
                                                                              sqlReader.GetDouble(4),
                                                                              sqlReader.GetDouble(5),
                                                                              sqlReader.GetString(6),
                                                                              sqlReader.GetInt32(7));
                        indicatorsList.Add(indicator);
                    }
            }

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };
            string res = JsonSerializer.Serialize<NumericalIndicatorList>(indicatorsList, options);
            return res;
        }

        [HttpGet("[action]/{substring}")]
        //GET: GameContext/QuestionsBySubstring
        // возвращает список ВСЕХ вопросов, в тексте которых есть подстрока substring
        public string QuestionsBySubstring(string substring)
        {
            QuestionList questionList = new QuestionList();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT id,
                                           name
                                    FROM player_questions
                                    WHERE name ILIKE '%' || @substring || '%'"
                };
                NpgsqlParameter substring_param = new NpgsqlParameter("@substring", substring);
                Command.Parameters.Add(substring_param);
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    while (sqlReader.Read())
                    {
                        Question question = new Question(sqlReader.GetInt32(0), sqlReader.GetString(1));
                        questionList.Add(question);
                    }
            }
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };
            string res = JsonSerializer.Serialize<QuestionList>(questionList, options);
            return res;
        }

        [HttpGet("[action]/{substring}")]
        //GET: GameContext/AnswersBySubstring
        // возвращает список ВСЕХ ответов, в тексте которых есть подстрока substring
        public string AnswersBySubstring(string substring)
        {
            AnswerList answerList = new AnswerList();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT id,
                                           name
                                    FROM patient_answers
                                    WHERE name ILIKE '%' || @substring || '%'"
                };
                NpgsqlParameter substring_param = new NpgsqlParameter("@substring", substring);
                Command.Parameters.Add(substring_param);
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    while (sqlReader.Read())
                    {
                        Answer answer = new Answer(sqlReader.GetInt32(0), sqlReader.GetString(1));
                        answerList.Add(answer);
                    }
            }
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };
            string res = JsonSerializer.Serialize<AnswerList>(answerList, options);
            return res;
        }

        [HttpGet("[action]/suggestions")]
        //GET: GameContext/Diagnoses/suggestions
        // возвращает список ВСЕХ диагнозов, для которых есть предложения пользователей
        public string Diagnoses()
        {
            DiagnosisList diagnosisList = new DiagnosisList();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT id,
                                           mkb_name,
                                           mkb_code
                                    FROM diagnoses
                                    WHERE id IN (SELECT diagnosis_id FROM user_suggestions)"
                };
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    while (sqlReader.Read())
                    {
                        Diagnosis diagnosis = new Diagnosis(sqlReader.GetInt32(0), sqlReader.GetString(1), sqlReader.GetString(2));
                        diagnosisList.Add(diagnosis);
                    }
            }
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };
            string res = JsonSerializer.Serialize<DiagnosisList>(diagnosisList, options);
            return res;
        }

        [HttpGet("[action]/{diagnosis_id}")]
        //GET: GameContext/Suggestions/diagnosis_id
        // возвращает список предложений для конкретного диагноза
        public string Suggestions(int diagnosis_id)
        {
            UserSuggestionList userSuggestionList = new UserSuggestionList();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT diagnosis_id, symptoms, visible_signs, questions_and_answers, id
                                    FROM user_suggestions WHERE diagnosis_id = @diagnosis_id"
                };
                NpgsqlParameter diagnosisParam = new NpgsqlParameter("@diagnosis_id", diagnosis_id);
                Command.Parameters.Add(diagnosisParam);
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    while (sqlReader.Read())
                    {
                        UserSuggestion suggestion = new UserSuggestion(sqlReader.GetInt32(4), sqlReader.GetInt32(0), sqlReader.GetString(1), sqlReader.GetString(2), sqlReader.GetString(3));
                        userSuggestionList.Add(suggestion);
                    }
            }
            string res = JsonSerializer.Serialize<UserSuggestionList>(userSuggestionList);
            return res;

        }

        [HttpGet("[action]/{сategory_id}")]
        //GET: GameContext/Diagnoses/сategory_id
        //возвращает список диагнозов из категории
        public string Diagnoses(int сategory_id)
        {
            DiagnosisList diagnosisList = new DiagnosisList();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT id, MKB_NAME, MKB_CODE FROM diagnoses WHERE rec_code LIKE (SELECT REC_CODE FROM diagnoses WHERE id = @parent_id LIMIT 1)||'%' AND mkb_code IS NOT NULL"
                    //AND (ID IN (SELECT diagnosis_id FROM answers_questions_for_diagnoses) OR
                    //      ID IN (SELECT diagnosis_id FROM enumerated_indicators_of_diagnoses) OR
                    //      ID IN (SELECT diagnosis_id FROM numerical_indicators_of_diagnoses))"
                };
                NpgsqlParameter сategoryParam = new NpgsqlParameter("@parent_id", сategory_id);
                Command.Parameters.Add(сategoryParam);
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    while (sqlReader.Read())
                    {
                        Diagnosis diagnosis = new Diagnosis(sqlReader.GetInt32(0), sqlReader.GetString(1), sqlReader.GetString(2));
                        diagnosisList.Add(diagnosis);
                    }
            }
            string res = JsonSerializer.Serialize<DiagnosisList>(diagnosisList);
            return res;
        }

        [HttpGet("[action]")]
        //GET: GameContext/DiagnosesСategory
        // возвращает список категорий для диагноза
        public string DiagnosesСategory()
        {
            DiagnosisList diagnosisList = new DiagnosisList();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT id,
                                           rec_code,
                                           mkb_code,
                                           mkb_name,
                                           id_parent
                                    FROM diagnoses as parents WHERE
                                        (SELECT COUNT(*) FROM diagnoses WHERE rec_code LIKE RTRIM(parents.rec_code)||'%' AND
                                         (ID IN (SELECT diagnosis_id FROM answers_questions_for_diagnoses) OR
                                          ID IN (SELECT diagnosis_id FROM enumerated_indicators_of_diagnoses) OR
                                          ID IN (SELECT diagnosis_id FROM numerical_indicators_of_diagnoses))) > 0
                                         AND mkb_code IS NULL AND id_parent IS NOT NULL"
                };
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    while (sqlReader.Read())
                    {
                        Diagnosis diagnosis = new Diagnosis(sqlReader.GetInt32(0), sqlReader.GetString(3), "");
                        diagnosisList.Add(diagnosis);
                    }
            }
            string res = JsonSerializer.Serialize<DiagnosisList>(diagnosisList);
            return res;
        }

        //[HttpGet("[action]")]
        ////GET: GameContext/AllDiagnosesСategory
        //// возвращает список основных категорий
        //public string AllDiagnosesСategory()
        //{
        //    DiagnosisList diagnosisList = new DiagnosisList();
        //    using (var sConn = new NpgsqlConnection(sConnStr))
        //    {
        //        sConn.Open();
        //        NpgsqlCommand Command = new NpgsqlCommand
        //        {
        //            Connection = sConn,
        //            CommandText = @"SELECT id as parentid,
        //                                   rec_code as parent_rec_code,
        //                                   mkb_code,
        //                                   mkb_name,
        //                                   id_parent
        //                            FROM diagnoses WHERE mkb_code IS NULL AND id_parent IS NULL"
        //        };
        //        using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
        //            while (sqlReader.Read())
        //            {
        //                Diagnosis diagnosis = new Diagnosis(sqlReader.GetInt32(0), sqlReader.GetString(3), "");
        //                diagnosisList.Add(diagnosis);
        //            }
        //    }
        //    string res = JsonSerializer.Serialize<DiagnosisList>(diagnosisList);
        //    return res;
        //}

        //[HttpGet("[action]/{category_id}")]
        //// GET: GameContext/AllDiagnosesСategory/category_id
        //// возвращает список основных категорий
        //public string AllDiagnosesСategory(int category_id)
        //{
        //    DiagnosisList diagnosisList = new DiagnosisList();
        //    using (var sConn = new NpgsqlConnection(sConnStr))
        //    {
        //        sConn.Open();
        //        NpgsqlCommand Command = new NpgsqlCommand
        //        {
        //            Connection = sConn,
        //            CommandText = @"SELECT id as parentid,
        //                                   rec_code as parent_rec_code,
        //                                   mkb_code,
        //                                   mkb_name,
        //                                   id_parent
        //                            FROM diagnoses WHERE mkb_code IS NULL AND id_parent = @category_id"
        //        };
        //        NpgsqlParameter category_idParam = new NpgsqlParameter("@category_id", category_id);
        //        Command.Parameters.Add(category_idParam);
        //        using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
        //            while (sqlReader.Read())
        //            {
        //                Diagnosis diagnosis = new Diagnosis(sqlReader.GetInt32(0), sqlReader.GetString(3), "");
        //                diagnosisList.Add(diagnosis);
        //            }
        //    }
        //    string res = JsonSerializer.Serialize<DiagnosisList>(diagnosisList);
        //    return res;
        //}

        [HttpGet("[action]/{diagnosis_id}")]
        // GET: GameContext/VisibleSigns/diagnosis_id
        // возвращает список внешних проявлений для конкретного диагноза
        public string VisibleSigns(int diagnosis_id)
        {
            VisibleSignList visibleSignList = new VisibleSignList();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT id, name, readable_id
                                    FROM visible_signs
                                    JOIN visible_signs_of_diagnoses ON visible_signs.id = visible_signs_of_diagnoses.visible_sign_id
                                    WHERE diagnosis_id = @diagnosis_id"
                };
                NpgsqlParameter diagnosisParam = new NpgsqlParameter("@diagnosis_id", diagnosis_id);
                Command.Parameters.Add(diagnosisParam);
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    while (sqlReader.Read())
                    {
                        VisibleSign visibleSign = new VisibleSign(sqlReader.GetInt32(0), sqlReader.GetString(1), sqlReader.GetString(2));
                        visibleSignList.Add(visibleSign);
                    }
                sConn.Close();
            }
            string res = JsonSerializer.Serialize<VisibleSignList>(visibleSignList);
            return res;
        }

        [HttpGet("[action]/{diagnosis_id}-{question_id}")]
        // GET: GameContext/AnswerOnQuestion/diagnosis_id-question_id
        // возвращает ответ на конкретный вопрос для конкретного диагноза
        public ActionResult AnswerOnQuestion(int diagnosis_id, int question_id)
        {
            string answer = string.Empty;
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT name
                                    FROM patient_answers
                                    JOIN answers_questions_for_diagnoses ON patient_answers.id = answers_questions_for_diagnoses.answer_id
                                    WHERE diagnosis_id = @diagnosis_id AND question_id = @question_id"
                };
                NpgsqlParameter diagnosisParam = new NpgsqlParameter("@diagnosis_id", diagnosis_id);
                NpgsqlParameter questionParam = new NpgsqlParameter("@question_id", question_id);
                Command.Parameters.Add(diagnosisParam);
                Command.Parameters.Add(questionParam);
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    if (sqlReader.Read())
                        answer = sqlReader.GetString(0);
            }
            if (string.IsNullOrEmpty(answer))
                return BadRequest();
            return Ok(answer);
        }

        [HttpGet("[action]/{diagnosis_id}-{question_id}")]
        // GET: GameContext/DiagnosisQuestionValidation/diagnosis_id-question_id
        // возвращает 1, если на вопрос есть ответ для этого диагноза (то есть есть пара диагноз-вопрос в таблице)
        // иначе, очевидно, 0
        public int DiagnosisQuestionValidation(int diagnosis_id, int question_id)
        {
            int count = 0;
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT count(*)
                                    FROM answers_questions_for_diagnoses
                                    WHERE diagnosis_id = @diagnosis_id
                                    AND question_id = @question_id;"
                };
                NpgsqlParameter diagnosisParam = new NpgsqlParameter("@diagnosis_id", diagnosis_id);
                NpgsqlParameter questionParam = new NpgsqlParameter("@question_id", question_id);
                Command.Parameters.Add(diagnosisParam);
                Command.Parameters.Add(questionParam);

                int.TryParse(Command.ExecuteScalar().ToString(), out count);
                sConn.Close();
            }
            return count;
        }

        [HttpGet("[action]/{diagnosis_id}-{indicator_id}")]
        // GET: GameContext/AnswerOnEnumAnalysis/diagnosis_id-indicator_id
        // возвращает ответ на перечислимый индикатор для конкретного диагноза
        // если у конкретного диагноза нет специфичного значения индикатора, возвращается нормальный
        public string AnswerOnEnumAnalysis(int diagnosis_id, int indicator_id)
        {
            // тут два случая: у диагноза есть специфичные для него значения этого показателя
            // например, у какой-нибудь болезни кожа пятнышками
            // а может оказаться так, что у болезни этот показатель в норме
            // короче, я просто сначала ищу специфичные, если не нахожу, ищу нормалньные
            List<string> answers = new List<string>();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT name
                                    FROM enumerated_indicators_of_diagnoses
                                    JOIN enumerated_indicators_values ON enumerated_indicators_of_diagnoses.value_id = enumerated_indicators_values.id
                                    WHERE diagnosis_id = @diagnosis_id AND indicator_id = @indicator_id"
                };
                NpgsqlParameter diagnosisParam = new NpgsqlParameter("@diagnosis_id", diagnosis_id);
                NpgsqlParameter indicatorParam = new NpgsqlParameter("@indicator_id", indicator_id);
                Command.Parameters.Add(diagnosisParam);
                Command.Parameters.Add(indicatorParam);
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    while (sqlReader.Read())
                        answers.Add(sqlReader.GetString(0));
            }
            if (answers.Count > 0)
                return string.Join(", ", answers);
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT name
                                    FROM enumerated_indicators_values
                                    JOIN enumerated_indicators_values_in_types ON enumerated_indicators_values.id = enumerated_indicators_values_in_types.value_id
                                    WHERE indicator_id = @indicator_id AND isnormal_bool = true"
                };
                NpgsqlParameter indicatorParam = new NpgsqlParameter("@indicator_id", indicator_id);
                Command.Parameters.Add(indicatorParam);
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    while (sqlReader.Read())
                        answers.Add(sqlReader.GetString(0));
            }
            return string.Join(", ", answers);
        }

        // выдаёт рандомное значение между минимальным и максимальным с заданной точностью
        public string GetRandomParam(double min, double max, int accuracy)
        {
            var rand = new Random();
            double res = (min + rand.NextDouble() * (max - min));
            string answer = String.Format("{0:F" + accuracy + "}", res);
            return answer;
        }
        [HttpGet("[action]/{diagnosis_id}-{indicator_id}")]
        // GET: GameContext/AnswerOnNumerAnalysis/diagnosis_id-indicator_id
        // возвращает ответ на числовой индикатор для конкретного диагноза
        // если у конкретного диагноза нет специфичного значения индикатора,
        // возвращается случайное число между минимальной и максимальной нормой, учитывая заданную точность
        public string AnswerOnNumerAnalysis(int diagnosis_id, int indicator_id)
        {
            double min = -1, max = -1;
            string units_name = string.Empty;
            int accuracy = 2;
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT value_min, value_max, units_name, accuracy
                                    FROM numerical_indicators_of_diagnoses
                                    JOIN numerical_indicators_ranges ON numerical_indicators_of_diagnoses.indicator_id = numerical_indicators_ranges.indicator_id
                                    WHERE diagnosis_id = @diagnosis_id AND numerical_indicators_of_diagnoses.indicator_id = @indicator_id"
                };
                NpgsqlParameter diagnosisParam = new NpgsqlParameter("@diagnosis_id", diagnosis_id);
                NpgsqlParameter indicatorParam = new NpgsqlParameter("@indicator_id", indicator_id);
                Command.Parameters.Add(diagnosisParam);
                Command.Parameters.Add(indicatorParam);
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    if (sqlReader.Read())
                    {
                        min = sqlReader.GetDouble(0);
                        max = sqlReader.GetDouble(1);
                        units_name = sqlReader.GetString(2);
                        accuracy = sqlReader.GetInt32(3);
                    }
            }
            if (min != -1 || max != -1)
                return GetRandomParam(min, max, accuracy) + " " + units_name;
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT normal_min, normal_max, units_name, accuracy
                                    FROM numerical_indicators_ranges
                                    WHERE indicator_id = @indicator_id"
                };
                NpgsqlParameter indicatorParam = new NpgsqlParameter("@indicator_id", indicator_id);
                Command.Parameters.Add(indicatorParam);
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    if (sqlReader.Read())
                    {
                        min = sqlReader.GetDouble(0);
                        max = sqlReader.GetDouble(1);
                        units_name = sqlReader.GetString(2);
                        accuracy = sqlReader.GetInt32(3);
                    }
            }
            return GetRandomParam(min, max, accuracy) + " " + units_name;
        }

        [HttpGet("[action]/{diagnosis_id}")]
        //GET:GameContext/NumericalIndicators/diagnosis_id
        // возвращает список числовых индикаторов для конкретного диагноза
        public string NumericalIndicators(int diagnosis_id)
        {
            NumericalIndicatorInDiagnosisList indicatorsList = new NumericalIndicatorInDiagnosisList();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT diagnosis_id,
                                           numerical_indicators_of_diagnoses.indicator_id,
                                           name,
                                           min_value,
                                           max_value,
                                           normal_min,
                                           normal_max,
                                           units_name,
                                           accuracy,
                                           value_min,
                                           value_max
                                    FROM numerical_indicators_of_diagnoses
                                    JOIN patient_indicators ON numerical_indicators_of_diagnoses.indicator_id = patient_indicators.id
                                    JOIN numerical_indicators_ranges ON patient_indicators.id = numerical_indicators_ranges.indicator_id
                                    WHERE diagnosis_id = @diagnosis_id"
                };
                NpgsqlParameter diagnosisParam = new NpgsqlParameter("@diagnosis_id", diagnosis_id);
                Command.Parameters.Add(diagnosisParam);
                using (NpgsqlDataReader sqlReader = Command.ExecuteReader())
                    while (sqlReader.Read())
                    {
                        NumericalIndicator indicator = new NumericalIndicator(sqlReader.GetInt32(1),
                                                                              sqlReader.GetString(2),
                                                                              sqlReader.GetDouble(3),
                                                                              sqlReader.GetDouble(4),
                                                                              sqlReader.GetDouble(5),
                                                                              sqlReader.GetDouble(6),
                                                                              sqlReader.GetString(7),
                                                                              sqlReader.GetInt32(8));
                        NumericalIndicatorInDiagnosis indicatorInDiagnosis = new NumericalIndicatorInDiagnosis(sqlReader.GetInt32(0), 
                                                                                                               indicator,
                                                                                                               sqlReader.GetDouble(9),
                                                                                                               sqlReader.GetDouble(10));
                        indicatorsList.Add(indicatorInDiagnosis);
                    }
            }
            string res = JsonSerializer.Serialize<NumericalIndicatorInDiagnosisList>(indicatorsList);
            return res;
        }

        [HttpPost("[action]")]
        // POST: GameContext/NumericalIndicators/
        // добавляет индикатор к диагнозу, указывая минимальное и максимальное значение этого индикатора при этом диагнозе
        public int NumericalIndicators(num_indicator_in_diagnosis request)
        {
            int res = -1;
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"INSERT INTO numerical_indicators_of_diagnoses (diagnosis_id, 
                                                                                   indicator_id,
                                                                                   value_min, 
                                                                                   value_max)
                                    VALUES (@diagnosis_id, @indicator_id, @value_min, @value_max)"
                };
                NpgsqlParameter diagnosisParam = new NpgsqlParameter("@diagnosis_id", request.diagnosis_id);
                NpgsqlParameter indicator_idParam = new NpgsqlParameter("@indicator_id", request.indicator_id);
                NpgsqlParameter value_minParam = new NpgsqlParameter("@value_min", request.value_min);
                NpgsqlParameter value_maxParam = new NpgsqlParameter("@value_max", request.value_max);
                Command.Parameters.Add(diagnosisParam);
                Command.Parameters.Add(indicator_idParam);
                Command.Parameters.Add(value_minParam);
                Command.Parameters.Add(value_maxParam);
                res = Command.ExecuteNonQuery();
                sConn.Close();
            }
            return res;
        }

        [HttpPut("[action]/{old_indicator_id}")]
        // PUT: GameContext/NumericalIndicatorsUpdate
        // изменяет числовой индикатор диагноза
        public void NumericalIndicatorsUpdate(int old_indicator_id, num_indicator_in_diagnosis request)
        {
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"UPDATE numerical_indicators_of_diagnoses
                                    SET indicator_id = @indicator_id, value_min = @value_min, value_max = @value_max
                                    WHERE diagnosis_id = @diagnosis_id AND indicator_id = @old_indicator_id"
                };
                NpgsqlParameter diagnosis_idParam = new NpgsqlParameter("@diagnosis_id", request.diagnosis_id);
                NpgsqlParameter old_indicator_idParam = new NpgsqlParameter("@old_indicator_id", old_indicator_id);
                NpgsqlParameter indicator_idParam = new NpgsqlParameter("@indicator_id", request.indicator_id);
                NpgsqlParameter value_minParam = new NpgsqlParameter("@value_min", request.value_min);
                NpgsqlParameter value_maxParam = new NpgsqlParameter("@value_max", request.value_max);
                Command.Parameters.Add(diagnosis_idParam);
                Command.Parameters.Add(old_indicator_idParam);
                Command.Parameters.Add(indicator_idParam);
                Command.Parameters.Add(value_minParam);
                Command.Parameters.Add(value_maxParam);
                Command.ExecuteNonQuery();
                sConn.Close();
            }
        }

        [HttpDelete("[action]/{diagnosis_id}-{indicator_id}")]
        // DELETE:GameContext/NumericalIndicatorsDelete
        // удаляет числовой индикатор для диагноза
        public void NumericalIndicatorsDelete(int diagnosis_id, int indicator_id)
        {
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"DELETE FROM numerical_indicators_of_diagnoses WHERE diagnosis_id = @diagnosis_id AND indicator_id = @indicator_id"
                };
                NpgsqlParameter diagnosis_idParam = new NpgsqlParameter("@diagnosis_id", diagnosis_id);
                NpgsqlParameter indicator_idParam = new NpgsqlParameter("@indicator_id", indicator_id);
                Command.Parameters.Add(diagnosis_idParam);
                Command.Parameters.Add(indicator_idParam);
                Command.ExecuteNonQuery();
                sConn.Close();
            }
        }

        [HttpPost("[action]")]
        // POST: GameContext/NumericalIndicator/
        // добавляет индикатор
        public int NumericalIndicator(NumericalIndicator indicator)
        {
            int res = -1;
            int indicator_id = 0;
            // добавление индикатора в таблицу индикаторов
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"INSERT INTO patient_indicators (name, type_is_enum)
                                    VALUES (@indicator_name, FALSE)
                                    RETURNING id;"
                };
                NpgsqlParameter nameParam = new NpgsqlParameter("@indicator_name", indicator.name);
                Command.Parameters.Add(nameParam);
                int.TryParse(Command.ExecuteScalar().ToString(), out indicator_id);
                sConn.Close();
            }
            // добавление индикатора в таблицу значений
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"INSERT INTO numerical_indicators_ranges (indicator_id, min_value, max_value, normal_min, normal_max, units_name, accuracy)
                                    VALUES (@indicator_id, 
                                            @min_value, 
                                            @max_value, 
                                            @normal_min, 
                                            @normal_max, 
                                            @units_name, 
                                            @accuracy)"
                };
                NpgsqlParameter indicator_idParam = new NpgsqlParameter("@indicator_id", indicator_id);
                NpgsqlParameter min_valueParam = new NpgsqlParameter("@min_value", indicator.min_value);
                NpgsqlParameter max_valueParam = new NpgsqlParameter("@max_value", indicator.max_value);
                NpgsqlParameter normal_minParam = new NpgsqlParameter("@normal_min", indicator.normal_min);
                NpgsqlParameter normal_maxParam = new NpgsqlParameter("@normal_max", indicator.normal_max);
                NpgsqlParameter units_nameParam = new NpgsqlParameter("@units_name", indicator.units_name);
                NpgsqlParameter accuracyParam = new NpgsqlParameter("@accuracy", indicator.accuracy);
                Command.Parameters.Add(indicator_idParam);
                Command.Parameters.Add(min_valueParam);
                Command.Parameters.Add(max_valueParam);
                Command.Parameters.Add(normal_minParam);
                Command.Parameters.Add(normal_maxParam);
                Command.Parameters.Add(units_nameParam);
                Command.Parameters.Add(accuracyParam);
                res = Command.ExecuteNonQuery();
                sConn.Close();
            }
            return res;
        }

        [HttpGet("[action]/{diagnosis_id}-{indicator_id}")]
        // GET: GameContext/DiagnosisQuestionValidation/diagnosis_id-indicator_id
        // возвращает 1, если для диагноза указан этот диагноз(то есть есть пара диагноз-индикатор в таблице), иначе 0
        public int DiagnosisNumericalIndicatorValidation(int diagnosis_id, int indicator_id)
        {
            int count = 0;
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT count(*)
                                    FROM numerical_indicators_of_diagnoses
                                    WHERE diagnosis_id = @diagnosis_id
                                    AND indicator_id = @indicator_id;"
                };
                NpgsqlParameter diagnosisParam = new NpgsqlParameter("@diagnosis_id", diagnosis_id);
                NpgsqlParameter indicator_idParam = new NpgsqlParameter("@indicator_id", indicator_id);
                Command.Parameters.Add(diagnosisParam);
                Command.Parameters.Add(indicator_idParam);

                int.TryParse(Command.ExecuteScalar().ToString(), out count);
                sConn.Close();
            }
            return count;
        }

        [HttpPost("[action]")]
        // POST: GameContext/PostGameResult/player_id-right_diagnosis_id-given_diagnosis_id
        // записывает результат игры в бд
        public int PostGameResult(Request request/*int player_id, int right_diagnosis_id, int given_diagnosis_id*/)
        {
            int res = -1;
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"INSERT INTO game_round (player_id, game_date, right_diagnosis, given_diagnosis)
                                    VALUES (@player_id, date('now'), @right_diagnosis, @given_diagnosis)"
                };
                NpgsqlParameter playerParam = new NpgsqlParameter("@player_id", request.player_id);
                NpgsqlParameter right_diagnosisParam = new NpgsqlParameter("@right_diagnosis", request.right_diagnosis_id);
                NpgsqlParameter given_diagnosisParam = new NpgsqlParameter("@given_diagnosis", request.given_diagnosis_id);
                Command.Parameters.Add(playerParam);
                Command.Parameters.Add(right_diagnosisParam);
                Command.Parameters.Add(given_diagnosisParam);
                res = Command.ExecuteNonQuery();
                sConn.Close();
            }
            return res;
        }

        [HttpPost("[action]")]
        // POST: GameContext/Question
        // добавляет вариант вопроса
        public int Question(Question question)
        {
            int res = -1;
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"INSERT INTO player_questions (name)
                                    VALUES (@question)
                                    RETURNING id"
                };
                NpgsqlParameter question_name = new NpgsqlParameter("@question", question.question_text);
                Command.Parameters.Add(question_name);
                int.TryParse(Command.ExecuteScalar().ToString(), out res);
                sConn.Close();
            }
            return res;
        }
        [HttpPost("[action]")]
        // POST: GameContext/Answer
        // добавляет вариант ответа
        public int Answer(Answer answer)
        {
            int res = -1;
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"INSERT INTO patient_answers (name)
                                    VALUES (@answer)
                                    RETURNING id"
                };
                NpgsqlParameter answer_name = new NpgsqlParameter("@answer", answer.answer_text);
                Command.Parameters.Add(answer_name);
                int.TryParse(Command.ExecuteScalar().ToString(), out res);
                sConn.Close();
            }
            return res;
        }
        [HttpPost("[action]")]
        // POST: GameContext/Suggestion
        public ActionResult Suggestion(Body body)
        {
            int res = -1;
            if (body.id < 0 || string.IsNullOrEmpty(body.simptoms + body.questions + body.visuals))
                return BadRequest();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"INSERT INTO user_suggestions (diagnosis_id, symptoms, visible_signs, questions_and_answers)
                                    VALUES (@diagnosis_id, @symptoms, @visible_signs, @questions_and_answers)"
                };
                NpgsqlParameter diagnosis_id = new NpgsqlParameter("@diagnosis_id", body.id);
                NpgsqlParameter symptoms = new NpgsqlParameter("@symptoms", body.simptoms);
                NpgsqlParameter visible_signs = new NpgsqlParameter("@visible_signs", body.visuals);
                NpgsqlParameter questions_and_answers = new NpgsqlParameter("@questions_and_answers", body.questions);
                Command.Parameters.Add(diagnosis_id);
                Command.Parameters.Add(symptoms);
                Command.Parameters.Add(visible_signs);
                Command.Parameters.Add(questions_and_answers);
                //Command.Parameters.Add("@diagnosis_id", (NpgsqlTypes.NpgsqlDbType)System.Data.DbType.Int32).Value = body.id;
                //Command.Parameters.Add("@symptoms", (NpgsqlTypes.NpgsqlDbType)System.Data.DbType.AnsiString).Value = body.simptoms;
                //Command.Parameters.Add("@visible_signs", (NpgsqlTypes.NpgsqlDbType)System.Data.DbType.AnsiString).Value = body.visuals;
                //Command.Parameters.Add("@questions_and_answers", (NpgsqlTypes.NpgsqlDbType)System.Data.DbType.AnsiString).Value = body.questions;
                res = Command.ExecuteNonQuery();
                sConn.Close();
            }
            return Ok(res);
        }

        [HttpPost("[action]")]
        // POST: GameContext/DiagnosisQuestionAnswer/
        // добавляет пару вопрос-ответ для диагноза
        public int DiagnosisQuestionAnswer(answers_questions_for_diagnosesString request/*int diagnosis_id, int question_id, int answer_id*/)
        {
            int res = -1;
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"INSERT INTO answers_questions_for_diagnoses (diagnosis_id, question_id, answer_id) 
                                    VALUES (@diagnosis_id, @question_id, @answer_id)"
                };
                NpgsqlParameter diagnosis_idParam = new NpgsqlParameter("@diagnosis_id", request.diagnosis_id);
                NpgsqlParameter question_idParam = new NpgsqlParameter("@question_id", request.question_id);
                NpgsqlParameter answer_idParam = new NpgsqlParameter("@answer_id", request.answer_id);
                Command.Parameters.Add(diagnosis_idParam);
                Command.Parameters.Add(question_idParam);
                Command.Parameters.Add(answer_idParam);
                res = Command.ExecuteNonQuery();
                sConn.Close();
            }
            return res;
        }

        [HttpDelete("[action]/{user_suggestion_id}")]
        // DELETE: GameContext/Suggestion/
        // удаляет предложение пользователя по идентификатору
        public void Suggestion(int user_suggestion_id)
        {
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"DELETE FROM user_suggestions WHERE id = @user_suggestion_id"
                };
                NpgsqlParameter user_suggestion_idParam = new NpgsqlParameter("@user_suggestion_id", user_suggestion_id);
                Command.Parameters.Add(user_suggestion_idParam);
                Command.ExecuteNonQuery();
                sConn.Close();
            }
        }

        [HttpDelete("[action]/{diagnosis_id}-{question_id}")]
        // DELETE:GameContext/AnswerOnQuestionDelete
        // удаляет ответ на вопрос для диагноза
        public void AnswerOnQuestionDelete(int diagnosis_id, int question_id)
        {
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"DELETE FROM answers_questions_for_diagnoses WHERE diagnosis_id = @diagnosis_id AND question_id = @question_id"
                };
                NpgsqlParameter diagnosis_idParam = new NpgsqlParameter("@diagnosis_id", diagnosis_id);
                NpgsqlParameter question_idParam = new NpgsqlParameter("@question_id", question_id);
                Command.Parameters.Add(diagnosis_idParam);
                Command.Parameters.Add(question_idParam);
                Command.ExecuteNonQuery();
                sConn.Close();
            }
        }

        [HttpPut("[action]/{diagnosis_id}-{question_id}-{new_question_id}-{new_answer_id}")]
        // PUT: GameContext/AnswerOnQuestionUpdate
        // изменяет ответ на вопрос для диагноза
        public void AnswerOnQuestionUpdate(int diagnosis_id, int question_id, int new_question_id, int new_answer_id)
        {
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"UPDATE answers_questions_for_diagnoses
                                    SET question_id = @new_question_id, answer_id = @new_answer_id
                                    WHERE diagnosis_id = @diagnosis_id AND question_id =@question_id"
                };
                NpgsqlParameter diagnosis_idParam = new NpgsqlParameter("@diagnosis_id", diagnosis_id);
                NpgsqlParameter question_idParam = new NpgsqlParameter("@question_id", question_id);
                NpgsqlParameter new_question_idParam = new NpgsqlParameter("@new_question_id", new_question_id);
                NpgsqlParameter new_answer_idParam = new NpgsqlParameter("@new_answer_id", new_answer_id);
                Command.Parameters.Add(diagnosis_idParam);
                Command.Parameters.Add(question_idParam);
                Command.Parameters.Add(new_question_idParam);
                Command.Parameters.Add(new_answer_idParam);
                Command.ExecuteNonQuery();
                sConn.Close();
            }
        }
    }

    public class Body
    {
        public int id { get; set; }
        public string simptoms { get; set; }
        public string visuals { get; set; }
        public string questions { get; set; }
    }

    public class Request
    {
        public int player_id { get; set; }
        public int right_diagnosis_id { get; set; }
        public int given_diagnosis_id { get; set; }
    }

    
}