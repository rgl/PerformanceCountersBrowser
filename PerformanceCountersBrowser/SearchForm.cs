using System;
using System.Linq;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace PerformanceCountersBrowser
{
    public partial class SearchForm : Form
    {
        private Searcher _searcher;
        private BackgroundWorker _indexerBackgroundWorker;

        public SearchForm()
        {
            InitializeComponent();
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            if (!CreateSearcher())
                return;

            // add the text into the combobox items.
            var queryText = searchComboBox.Text;
            var index = searchComboBox.Items.IndexOf(queryText);
            if (index >= 0)
                searchComboBox.Items.RemoveAt(index);
            searchComboBox.Items.Insert(0, queryText);

            // search...
            try
            {
                statusLabel.Text = "";

                var results = _searcher.Search(queryText);

                statusLabel.Text = string.Format("{0} Hits;  {1} Indexed Documents", results.TotalHits, results.TotalIndexedDocuments);

                listView.BeginUpdate();
                try
                {
                    listView.Items.Clear();

                    foreach (var d in results.Documents)
                    {
                        var listItem = listView.Items.Add(d.Category);
                        listItem.SubItems.Add(d.Name);
                        listItem.SubItems.Add(d.Help);
                        listItem.SubItems.Add(d.Score.ToString());
                    }

                    // resize the columns to fit the text content.
                    for (var i = 0; i < 2; ++i)
                    {
                        listView.Columns[i].Width = -2;
                        var a = listView.Columns[i].Width;

                        listView.Columns[i].Width = -1;
                        var b = listView.Columns[i].Width;

                        if (a > b)
                            listView.Columns[i].Width = a;
                    }
                }
                finally
                {
                    listView.EndUpdate();
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Error Searching", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void reIndexButton_Click(object sender, EventArgs e)
        {
            if (_indexerBackgroundWorker != null)
                return;

            if (_searcher != null)
                _searcher.Dispose();

            if (Directory.Exists("index"))
                Directory.Delete("index", true);

            reIndexButton.Enabled = false;

            _indexerBackgroundWorker = new BackgroundWorker
                                           {
                                               WorkerSupportsCancellation = true,
                                               WorkerReportsProgress = true,
                                           };
            _indexerBackgroundWorker.DoWork += _indexerBackgroundWorker_DoWork;
            _indexerBackgroundWorker.RunWorkerCompleted += _indexerBackgroundWorker_RunWorkerCompleted;

            var indexerProgressForm = new ProgressForm(_indexerBackgroundWorker) {Text = "Indexing..."};
            indexerProgressForm.Show(this);

            _indexerBackgroundWorker.RunWorkerAsync();
        }

        private void _indexerBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _indexerBackgroundWorker.Dispose();
            _indexerBackgroundWorker = null;
            reIndexButton.Enabled = true;
            CreateSearcher();
        }

        private static void _indexerBackgroundWorker_DoWork(object sender, DoWorkEventArgs workerEventArgs)
        {
            var worker = (BackgroundWorker) sender;

            // TODO maybe we should use an RAMDirectory to store the index on memory...);
            using (var indexer = new Indexer("index"))
            {
                var categories = PerformanceCounterCategory.GetCategories();

                foreach (var n in categories.Select((e,i)=>new{e,i}))
                {
                    var category = n.e;
                    var counters = category.CategoryType == PerformanceCounterCategoryType.SingleInstance ? category.GetCounters() : category.GetCounters("");

                    foreach (var counter in counters)
                    {
                        indexer.AddPerformanceCounter(counter);
                        counter.Dispose();
                    }

                    if (worker.CancellationPending)
                    {
                        workerEventArgs.Cancel = true;
                        return;
                    }

                    worker.ReportProgress(n.i * 100 / categories.Length);
                }
            }
        }

        private bool CreateSearcher()
        {
            if (_searcher != null)
            {
                _searcher.Dispose();
                _searcher = null;
            }

            try
            {
                _searcher = new Searcher("index");
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error opening the index (you might need to reindex it).", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public delegate void PerformanceCounterSelectedDelegate(string categoryName, string counterName);

        public event PerformanceCounterSelectedDelegate PerformanceCounterSelected;

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (PerformanceCounterSelected == null)
                return;

            if (listView.SelectedItems.Count == 0)
                return;

            var selectedItem = listView.SelectedItems[0];
            var categoryName = selectedItem.Text;
            var counterName = selectedItem.SubItems[1].Text;

            PerformanceCounterSelected(categoryName, counterName);
        }
    }
}
