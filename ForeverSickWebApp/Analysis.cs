using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ForeverSickWebApp
{
    [Serializable]
    public class AnalysisList
    {
        public List<Analysis> analyses { get; set; }
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
        public int id { get; set; }
        public string analysis_text { get; set; }
        public bool is_enum { get; set; }

        public Analysis() { }

        public Analysis(int id, string analysis_text, bool is_enum)
        {
            this.id = id;
            this.analysis_text = analysis_text;
            this.is_enum = is_enum;
        }
    }
}
