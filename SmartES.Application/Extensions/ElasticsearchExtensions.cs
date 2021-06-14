using Nest;
using SmartES.Application.Constants;
using SmartES.Application.Models.Mgmt;
using SmartES.Application.Models.Property;

namespace SmartES.Application.Extensions
{
    public static class ElasticsearchExtensions
    {
        public static IAnalysis ConfigureAnalyzer(this AnalysisDescriptor analysis)
        {
            return analysis
                .TokenFilters(tk => tk
                    .Stop(ElasticsearchConstants.EnglishStopTokenFilter, es => es
                        .StopWords(ElasticsearchConstants.EnglishStopwords))
                    .Stemmer(ElasticsearchConstants.EnglishStemmer, st => st
                        .Language(ElasticsearchConstants.EnglishLanguage)))
                .Analyzers(a => a
                    .Custom(ElasticsearchConstants.IndexAnalyzer, c => c
                        .Tokenizer(ElasticsearchConstants.StandardTokenizer)
                        .Filters(ElasticsearchConstants.Lowercase, ElasticsearchConstants.EnglishStemmer))
                    .Custom(ElasticsearchConstants.SearchAnalyzer, s => s
                        .Tokenizer(ElasticsearchConstants.StandardTokenizer)
                        .Filters(ElasticsearchConstants.Lowercase, 
                            ElasticsearchConstants.EnglishStopTokenFilter, ElasticsearchConstants.EnglishStemmer)))
                    .Tokenizers(t => t
                        .EdgeNGram(ElasticsearchConstants.IndexAnalyzer, e => e
                            .MinGram(2)
                            .MaxGram(10)
                            .TokenChars(TokenChar.Letter, TokenChar.Digit)));
        }

        public static ITypeMapping AddPropertyMapping(this TypeMappingDescriptor<PropertyDetailsModel> mapping)
        {
            return mapping
                .Properties(p => p
                    .Number(i => i
                        .Name(n => n.PropertyId)
                        .Type(NumberType.Integer))
                    .SearchAsYouType(sayt => sayt
                        .Name(n => n.Name)
                        .Analyzer(ElasticsearchConstants.IndexAnalyzer)
                        .SearchAnalyzer(ElasticsearchConstants.SearchAnalyzer))
                    .SearchAsYouType(sayt => sayt
                        .Name(n => n.FormerName)
                        .Analyzer(ElasticsearchConstants.IndexAnalyzer)
                        .SearchAnalyzer(ElasticsearchConstants.SearchAnalyzer))
                    .Keyword(kw => kw
                        .Name(n => n.StreetAddress))
                    .Keyword(kw => kw
                        .Name(n => n.Market))
                    .Keyword(kw => kw
                        .Name(n => n.State))
                    .Keyword(kw => kw
                        .Name(n => n.City))
                    .Number(fr => fr
                        .Name(n => n.Lat)
                        .Index(false)
                        .Type(NumberType.Float))
                    .Number(fr => fr
                        .Name(n => n.Lng)
                        .Index(false)
                        .Type(NumberType.Float))
               );
        }

        public static ITypeMapping AddMgmtMapping(this TypeMappingDescriptor<MgmtDetailsModel> mapping)
        {
            return mapping
                .Properties(p => p
                    .Number(lr => lr
                        .Name(n => n.MgmtId)
                        .Type(NumberType.Integer))
                    .SearchAsYouType(sayt => sayt
                        .Name(n => n.Name)
                        .Analyzer(ElasticsearchConstants.IndexAnalyzer)
                        .SearchAnalyzer(ElasticsearchConstants.SearchAnalyzer))
                    .Keyword(kw => kw
                        .Name(n => n.Market))
                    .Keyword(kw => kw
                        .Name(n => n.State))
                );
        }
    }
}
