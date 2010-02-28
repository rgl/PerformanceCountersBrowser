using System;
using System.Diagnostics;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace PerformanceCountersBrowser
{
    class Indexer : IDisposable
    {
        private readonly IndexWriter _writer;

        public static Analyzer CreateAnalyzer()
        {
            // create the standard analyzer. this analyzer is a compound of:
            //  - StandardTokenizer
            //		- Maximum of 255 characters per token (StandardAnalyzer.DEFAULT_MAX_TOKEN_LENGTH)
            //      - This should be a good tokenizer for most European-language documents:
            //      	- Splits words at punctuation characters, removing punctuation. However, a 
            //      	  dot that's not followed by whitespace is considered part of a token.
            //      	- Splits words at hyphens, unless there's a number in the token, in which case
            //      	- the whole token is interpreted as a product number and is not split.
            //      	- Recognizes email addresses and internet hostnames as one token.
            //  - StandardFilter
            //		- normalizes the StandardTokenizer output by:
            //			- removing 's
            //			- removing dots
            //  - LowerCaseFilter
            //  - StopFilter
            //  	- Default English stop words removal (StopAnalyzer.ENGLISH_STOP_WORDS_SET)
            //  - PorterStemFilter
            // NB: This analyzer is only used when user uses the AddDocument overload
            //     that does not receive an Analyzer type. (like we do in the AddPerformanceCounter
            //     method bellow).
            var analyzer = new PerformanceCounterAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT);
            return analyzer;
        }

        public Indexer(string indexPath)
        {
            var directory = FSDirectory.Open(new DirectoryInfo(indexPath));
            var defaultAnalyzer = CreateAnalyzer();
            _writer = new IndexWriter(directory, defaultAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            //_writer.SetInfoStream(Console.Error); // TODO cast Console.Error into a StreamWriter...
        }

        public void AddPerformanceCounter(PerformanceCounter counter)
        {
            var doc = new Document();

            var documentTypeField = new Field("_type", "counter", Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO);
            documentTypeField.SetOmitTermFreqAndPositions(true);
            doc.Add(documentTypeField);

            var categoryField = new Field("category", counter.CategoryName, Field.Store.YES, Field.Index.ANALYZED);
            categoryField.SetBoost(1.5f);
            doc.Add(categoryField);

            var nameField = new Field("name", counter.CounterName, Field.Store.YES, Field.Index.ANALYZED);
            nameField.SetBoost(2);
            doc.Add(nameField);

            var typeField = new Field("type", counter.CounterType.ToString(), Field.Store.YES, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO);
            typeField.SetOmitTermFreqAndPositions(true);
            doc.Add(typeField);

            doc.Add(new Field("help", counter.CounterHelp, Field.Store.YES, Field.Index.ANALYZED));

            _writer.AddDocument(doc);
        }

        public void Dispose()
        {
            _writer.Optimize();
            _writer.Close();
        }
    }
}
