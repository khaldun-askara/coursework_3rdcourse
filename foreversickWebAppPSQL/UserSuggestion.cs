using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace foreversickWebAppPSQL
{
    [Serializable]
    public class UserSuggestionList
    {
        public List<UserSuggestion> userSuggestions { get; set; }
        public UserSuggestionList() { userSuggestions = new List<UserSuggestion>(); }
        public UserSuggestionList(List<UserSuggestion> userSuggestions)
        {
            this.userSuggestions = userSuggestions;
        }
        public void Add(UserSuggestion userSuggestion)
        {
            userSuggestions.Add(userSuggestion);
        }
    }
    [Serializable]
    public class UserSuggestion
    {
        public UserSuggestion()
        {
        }

        public UserSuggestion(int id, int diagnosis_id, string symptoms, string visible_signs, string questions_and_answers)
        {
            this.id = id;
            this.diagnosis_id = diagnosis_id;
            this.symptoms = symptoms;
            this.visible_signs = visible_signs;
            this.questions_and_answers = questions_and_answers;
        }

        public int id { get; set; }
        public int diagnosis_id { get; set; }
        public string symptoms { get; set; }
        public string visible_signs { get; set; }
        public string questions_and_answers { get; set; }


    }
}
