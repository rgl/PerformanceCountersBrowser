using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace PerformanceCountersBrowser
{
    class Searcher : IDisposable
    {
        private readonly QueryParser _parser;
        private readonly IndexSearcher _searcher;

        public class Results
        {
            public int TotalHits { get; private set; }
            public int TotalIndexedDocuments { get; private set; }
            public float MaxScore { get; private set; }
            public ResultDocument[] Documents { get; private set; }

            public Results(int totalHits, float maxScore, ResultDocument[] documents, int totalIndexedDocuments)
            {
                TotalHits = totalHits;
                MaxScore = maxScore;
                Documents = documents;
                TotalIndexedDocuments = totalIndexedDocuments;
            }
        }

        public class ResultDocument
        {
            public int Id { get; private set; }
            public float Score { get; private set; }
            public string Category { get; private set; }
            public string Name { get; private set; }
            public string Help { get; private set; }
            public Document Document { get; private set; }

            public ResultDocument(int id, float score, Document document)
            {
                Id = id;
                Score = score;
                Document = document;
                Category = document.Get("category");
                Name = document.Get("name");
                Help = document.Get("help");
            }
        }

        public Searcher(string indexPath)
        {
            var directory = FSDirectory.Open(new DirectoryInfo(indexPath));
            var analyzer = Indexer.CreateAnalyzer();
            _parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_CURRENT, "help", analyzer);
            _searcher = new IndexSearcher(directory, true);
        }

        public Results Search(string queryText)
        {
            var totalIndexedDocuments = _searcher.GetIndexReader().NumDocs();

            if (string.IsNullOrEmpty(queryText))
                return new Results(0, 0, new ResultDocument[0], totalIndexedDocuments);

            var query = _parser.Parse(queryText);
            var parsedQueryText = query.ToString();

            if (string.IsNullOrEmpty(parsedQueryText))
                return new Results(0, 0, new ResultDocument[0], totalIndexedDocuments);

            var mergedQueryText = string.Format(
                "({0}) OR ({1}) OR ({2})",
                parsedQueryText,
                parsedQueryText.Replace("help:", "name:"),
                parsedQueryText.Replace("help:", "category:")
            );

            query = _parser.Parse(mergedQueryText);

            var topDocuments = _searcher.Search(query, 20000);

            var documents = new List<ResultDocument>();
            foreach (var scoreDoc in topDocuments.scoreDocs) {
    			var document = _searcher.Doc(scoreDoc.doc);
    			documents.Add(new ResultDocument(scoreDoc.doc, scoreDoc.score, document));
    		}

            var results = new Results(topDocuments.totalHits, topDocuments.GetMaxScore(), documents.ToArray(), totalIndexedDocuments);
            return results;
        }

        public void Dispose()
        {
            _searcher.Close();
        }
    }
}
