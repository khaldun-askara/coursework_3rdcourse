using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace foreversickWebAppPSQL
{
    [Serializable]
    public class DiagnosisList
    {
        public List<Diagnosis> diagnoses { get; set; }
        public DiagnosisList() { diagnoses = new List<Diagnosis>(); }
        public DiagnosisList(List<Diagnosis> diagnoses)
        {
            this.diagnoses = diagnoses;
        }
        public void Add(Diagnosis diagnosis)
        {
            diagnoses.Add(diagnosis);
        }
    }
    [Serializable]
    public class Diagnosis
    {
        public int id { get; set; }
        public string diagnosis_text { get; set; }
        public string mcb_code { get; set; }
        public Diagnosis() { }
        public Diagnosis(int id, string diagnosis_text, string mcb_code)
        {
            this.id = id;
            this.diagnosis_text = diagnosis_text;
            this.mcb_code = mcb_code;
        }
    }
}
