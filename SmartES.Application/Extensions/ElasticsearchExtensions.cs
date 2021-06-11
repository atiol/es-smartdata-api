using Nest;

namespace SmartES.Application.Extensions
{
    public static class ElasticsearchExtensions
    {
        public static IAnalysis AddSearchAnalyzer(this AnalysisDescriptor analysis)
        {
            return analysis.Analyzers(a => a
                .Custom("i_analyzer", c => c
                            .Tokenizer("standard")
                            .Filters("standard", "lowercase"))
                        .Custom("s_analyzer", s => s
                            .Tokenizer("standard")
                            .Filters("standard", "lowercase", "stop")))
                
                .Tokenizers(t => t
                        .EdgeNGram("i_analyzer", e => e
                            .MinGram(1)
                            .MaxGram(8)));
        }
    }
}
