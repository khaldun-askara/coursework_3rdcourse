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
        //string Host => configuration["ConnectionStrings:Host"];
        //int Port => int.Parse(configuration["ConnectionStrings:Port"]);
        //string Database => configuration["ConnectionStrings:Host"];
        //string Usermane => configuration["ConnectionStrings:Username"];
        //string Password => configuration["ConnectionStrings:Password"];

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
        string sConnStr => configuration["ConnectionStrings:REMOTEDB"];
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
            substring = Regex.Replace(substring.ToLower(), @"^[a-zа-яё]", m => m.Value.ToUpper());

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
                                    (mkb_name LIKE '%' || @upper_substring || '%' OR mkb_name LIKE '%' || @lower_substring || '%')"
                };
                //Command.Parameters.Add("@upper_substring", (NpgsqlTypes.NpgsqlDbType)System.Data.DbType.AnsiString).Value = substring;
                //Command.Parameters.Add("@lower_substring", (NpgsqlTypes.NpgsqlDbType)System.Data.DbType.AnsiString).Value = substring.ToLower();
                NpgsqlParameter upper_substring = new NpgsqlParameter("@upper_substring", substring);
                NpgsqlParameter lower_substring = new NpgsqlParameter("@lower_substring", substring.ToLower());
                Command.Parameters.Add(upper_substring);
                Command.Parameters.Add(lower_substring);
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
        // НЕ РАБОТАЕТ
        public string DiagnosesСategory()
        {
            DiagnosisList diagnosisList = new DiagnosisList();
            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                NpgsqlCommand Command = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT id as parentid,
                                           rec_code as parent_rec_code,
                                           mkb_code,
                                           mkb_name,
                                           id_parent
                                    FROM diagnoses WHERE
                                        (SELECT COUNT(*) FROM diagnoses WHERE rec_code LIKE RTRIM(parent_rec_code)||'%' AND
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
        //[HttpPost("[action]")]
        //// POST: GameContext/Question
        //// добавляет вариант вопроса
        //public int Question(Question question)
        //{
        //    int res = -1;
        //    using (var sConn = new NpgsqlConnection(sConnStr))
        //    {
        //        sConn.Open();
        //        NpgsqlCommand Command = new NpgsqlCommand
        //        {
        //            Connection = sConn,
        //            CommandText = @"INSERT INTO player_questions (name)
        //                            VALUES (@question)"
        //        };
        //        Command.Parameters.Add("@question", System.Data.DbType.AnsiString).Value = question.question_text;
        //        res = Command.ExecuteNonQuery();
        //        sConn.Close();
        //    }
        //    return res;
        //}
        //[HttpPost("[action]")]
        //// POST: GameContext/Answer
        //// добавляет вариант ответа
        //public int Answer(Answer answer)
        //{
        //    int res = -1;
        //    using (var sConn = new NpgsqlConnection(sConnStr))
        //    {
        //        sConn.Open();
        //        NpgsqlCommand Command = new NpgsqlCommand
        //        {
        //            Connection = sConn,
        //            CommandText = @"INSERT INTO patient_answers (name)
        //                            VALUES (@answer)"
        //        };
        //        Command.Parameters.Add("@answer", System.Data.DbType.AnsiString).Value = answer.answer_text;
        //        res = Command.ExecuteNonQuery();
        //        sConn.Close();
        //    }
        //    return res;
        //}
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

        [HttpGet("[action]")]
        // GET: GameContext/HMMM
        // ПРОСТО ТЫКАЮСЬ
        public string HMMM()
        {
            string answer = "no answer:(";
            using (var sConn = new NpgsqlConnection(sConnStr2))
            {
                sConn.Open();
                var sCommand = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT *
                                FROM roles",
                };
                using (var sqlReader = sCommand.ExecuteReader())
                    if (sqlReader.Read())
                        answer = sqlReader.GetString(1);
            }
            return answer;
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