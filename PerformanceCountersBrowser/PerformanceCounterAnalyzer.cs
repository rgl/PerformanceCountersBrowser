using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace PerformanceCountersBrowser
{
    class PerformanceCounterAnalyzer : StandardAnalyzer
    {
        public PerformanceCounterAnalyzer(Version version)
            : base(version)
        {
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            var baseStream = base.TokenStream(fieldName, reader);
            return new PorterStemFilter(baseStream);
        }
    }
}
