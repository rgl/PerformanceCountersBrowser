using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

namespace PerformanceCountersBrowser
{
    class PerformanceCounterAnalyzer : StandardAnalyzer
    {
        public PerformanceCounterAnalyzer(Lucene.Net.Util.Version version)
            : base(version)
        {
        }

        public override TokenStream TokenStream(string fieldName, System.IO.TextReader reader)
        {
            var baseStream = base.TokenStream(fieldName, reader);
            return new PorterStemFilter(baseStream);
        }

        public override TokenStream ReusableTokenStream(string fieldName, System.IO.TextReader reader)
        {
            var baseStream = base.TokenStream(fieldName, reader);
            return new PorterStemFilter(baseStream);
        }
    }
}
