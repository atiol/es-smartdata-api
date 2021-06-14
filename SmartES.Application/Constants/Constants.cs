using System;
using System.Collections.Generic;
using System.Text;

namespace SmartES.Application.Constants
{
    public static class ElasticsearchConstants
    {
        public static readonly string PropertyIndex = "property";
        public static readonly string MgmtIndex = "mgmt";
        public static readonly string IndexAnalyzer = "smartdata_auto";
        public static readonly string SearchAnalyzer = "smartdata_auto_search";
        public static readonly string EnglishStopwords = "_english_";
        public static readonly string EnglishStemmer = "english_stemmer";
        public static readonly string EnglishLanguage = "english";
        public static readonly string EnglishStopTokenFilter = "english_stop";
        public static readonly string StandardTokenizer = "standard";
        public static readonly string Lowercase = "lowercase";

        public static readonly int MaximumPageSize = 45;
    }
}
