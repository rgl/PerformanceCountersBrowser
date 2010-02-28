using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace PerformanceCountersBrowser
{
    public partial class SearchForm : Form
    {
        private Searcher _searcher;

        public SearchForm()
        {
            InitializeComponent();

            CreateSearcher();
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
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
            _searcher.Dispose();

            if (Directory.Exists("index"))
                Directory.Delete("index", true);

            CreateIndex();
            CreateSearcher();
        }

        private void CreateSearcher()
        {
            if (_searcher != null)
                _searcher.Dispose();
            _searcher = new Searcher("index");
        }

        private static void CreateIndex()
        {
            // TODO maybe we should use an RAMDirectory to store the index on memory...
            using (var indexer = new Indexer("index"))
            {
                var categories = PerformanceCounterCategory.GetCategories();

                foreach (var category in categories)
                {
                    var counters = category.CategoryType == PerformanceCounterCategoryType.SingleInstance ? category.GetCounters() : category.GetCounters("");

                    foreach (var counter in counters)
                    {
                        indexer.AddPerformanceCounter(counter);
                        counter.Dispose();
                    }
                }
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
