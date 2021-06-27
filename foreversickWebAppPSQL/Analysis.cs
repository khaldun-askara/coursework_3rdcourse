﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace foreversickWebAppPSQL
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

    [Serializable]
    public class NumericalIndicatorList
    {
        public List<NumericalIndicator> numericalIndicators { get; set; }
        public NumericalIndicatorList() { numericalIndicators = new List<NumericalIndicator>(); }
        public NumericalIndicatorList(List<NumericalIndicator> numericalIndicators)
        {
            this.numericalIndicators = numericalIndicators;
        }
        public void Add(NumericalIndicator indicator)
        {
            numericalIndicators.Add(indicator);
        }
    }
    [Serializable]
    public class NumericalIndicator
    {
        public int indicator_id { get; set; }
        public string name { get; set; }
        public double min_value { get; set; }
        public double max_value { get; set; }

        public double normal_min { get; set; }
        public double normal_max { get; set; }
        public string units_name { get; set; }
        public int accuracy { get; set; }

        public NumericalIndicator() { }

        public NumericalIndicator(int indicator_id, string name, double min_value, double max_value, double normal_min, double normal_max, string units_name, int accuracy)
        {
            this.indicator_id = indicator_id;
            this.name = name;
            this.min_value = min_value;
            this.max_value = max_value;
            this.normal_min = normal_min;
            this.normal_max = normal_max;
            this.units_name = units_name;
            this.accuracy = accuracy;
        }
    }

    public class NumericalIndicatorInDiagnosis
    {
        public int diagnosis_id { get; set; }
        public NumericalIndicator indicator { get; set; }
        public double value_min { get; set; }
        public double value_max { get; set; }
        public NumericalIndicatorInDiagnosis(){}
        public NumericalIndicatorInDiagnosis(int diagnosis_id, NumericalIndicator indicator, double value_min, double value_max)
        {
            this.diagnosis_id = diagnosis_id;
            this.indicator = indicator;
            this.value_min = value_min;
            this.value_max = value_max;
        }
    }

    public class NumericalIndicatorInDiagnosisList
    {
        public List<NumericalIndicatorInDiagnosis> numericalIndicators { get; set; }
        public NumericalIndicatorInDiagnosisList() { numericalIndicators = new List<NumericalIndicatorInDiagnosis>(); }
        public NumericalIndicatorInDiagnosisList(List<NumericalIndicatorInDiagnosis> numericalIndicators)
        {
            this.numericalIndicators = numericalIndicators;
        }
        public void Add(NumericalIndicatorInDiagnosis indicator)
        {
            numericalIndicators.Add(indicator);
        }
    }

    public class num_indicator_in_diagnosis
    {
        public int diagnosis_id { get; set; }
        public int indicator_id { get; set; }
        public double value_min { get; set; }
        public double value_max { get; set; }
    }

    [Serializable]
    public class EnumeratedIndicator
    {
        public int indicator_id { get; set; }
        public string name { get; set; }
        public EnumeratedIndicator() { }

        public EnumeratedIndicator(int indicator_id, string name)
        {
            this.indicator_id = indicator_id;
            this.name = name;
        }
    }

    [Serializable]
    public class EnumeratedIndicatorList
    {
        public List<EnumeratedIndicator> enumeratedIndicators { get; set; }
        public EnumeratedIndicatorList() { enumeratedIndicators = new List<EnumeratedIndicator>(); }
        public EnumeratedIndicatorList(List<EnumeratedIndicator> enumeratedIndicators)
        {
            this.enumeratedIndicators = enumeratedIndicators;
        }
        public void Add(EnumeratedIndicator indicator)
        {
            enumeratedIndicators.Add(indicator);
        }
    }

    [Serializable]
    public class EnumeratedValue
    {
        public int value_id { get; set; }
        public string name { get; set; }
        public EnumeratedValue() { }

        public EnumeratedValue(int value_id, string name)
        {
            this.value_id = value_id;
            this.name = name;
        }
    }

    [Serializable]
    public class EnumeratedValueList
    {
        public List<EnumeratedValue> enumeratedValues { get; set; }
        public EnumeratedValueList() { enumeratedValues = new List<EnumeratedValue>(); }
        public EnumeratedValueList(List<EnumeratedValue> enumeratedValues)
        {
            this.enumeratedValues = enumeratedValues;
        }
        public void Add(EnumeratedValue indicator)
        {
            enumeratedValues.Add(indicator);
        }
    }

    [Serializable]
    public class EnumeratedIndicatorInDiagnosis
    {
        public int diagnosis_id { get; set; }
        public EnumeratedIndicator indicator { get; set; }
        public EnumeratedValue value { get; set; }
        public EnumeratedIndicatorInDiagnosis() { }
        public EnumeratedIndicatorInDiagnosis(int diagnosis_id, EnumeratedIndicator indicator, EnumeratedValue value)
        {
            this.diagnosis_id = diagnosis_id;
            this.indicator = indicator;
            this.value = value;
        }
    }

    [Serializable]
    public class EnumeratedIndicatorInDiagnosisList
    {
        public List<EnumeratedIndicatorInDiagnosis> enumeratedIndicators { get; set; }
        public EnumeratedIndicatorInDiagnosisList() { enumeratedIndicators = new List<EnumeratedIndicatorInDiagnosis>(); }
        public EnumeratedIndicatorInDiagnosisList(List<EnumeratedIndicatorInDiagnosis> enumeratedIndicators)
        {
            this.enumeratedIndicators = enumeratedIndicators;
        }
        public void Add(EnumeratedIndicatorInDiagnosis indicator)
        {
            enumeratedIndicators.Add(indicator);
        }
    }

    public class enum_indicator_in_diagnosis
    {
        public int diagnosis_id { get; set; }
        public int indicator_id { get; set; }
        public int value_id { get; set; }
    }
}
