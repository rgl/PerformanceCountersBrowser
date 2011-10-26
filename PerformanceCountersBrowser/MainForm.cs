using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace PerformanceCountersBrowser
{
    public partial class MainForm : Form
    {
        private SearchForm _searchForm;
        private SystemMonitorForm _systemMonitorForm;

        public MainForm()
        {
            InitializeComponent();
        }

        private class CategoryInfo
        {
            public string Name { get; set; }
            public string Help { get; set; }
            public PerformanceCounterCategoryType Type { get; set; }
        }

        private class CounterInfo
        {
            public CategoryInfo CategoryInfo { get; set; }
            public string Name { get; set; }
            public string Help { get; set; }
            public PerformanceCounterType Type { get; set; }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var categories = PerformanceCounterCategory.GetCategories().OrderBy(c => c.CategoryName);

            treeView.BeginUpdate();
            try
            {
                foreach (var category in categories)
                {
                    var node = new TreeNode(category.CategoryName)
                    {
                        Tag = new CategoryInfo
                        {
                            Name = category.CategoryName,
                            Help = category.CategoryHelp,
                            Type = category.CategoryType
                        }
                    };
                    node.Nodes.Add("Loading...");
                    treeView.Nodes.Add(node);
                }
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            helpTextBox.Text = "";
            instancesListBox.Items.Clear();

            var categoryInfo = e.Node.Tag as CategoryInfo;
            if (categoryInfo != null)
            {
                helpTextBox.Text = string.Format(
                    "{0}\r\n\r\nCategory Type={1}",
                    categoryInfo.Help,
                    categoryInfo.Type
                );

                if (categoryInfo.Type == PerformanceCounterCategoryType.MultiInstance)
                {
                    var category = new PerformanceCounterCategory(categoryInfo.Name);
                    var instanceNames = category.GetInstanceNames().OrderBy(s => s).ToArray();
                    instancesListBox.Items.AddRange(instanceNames);
                }
                return;
            }

            var counterInfo = e.Node.Tag as CounterInfo;
            if (counterInfo != null)
            {
                helpTextBox.Text = string.Format(
                    "{0}\r\n\r\nCounter Type={1}",
                    counterInfo.Help,
                    counterInfo.Type
                );

                categoryInfo = counterInfo.CategoryInfo;
                if (categoryInfo.Type == PerformanceCounterCategoryType.MultiInstance)
                {
                    var category = new PerformanceCounterCategory(categoryInfo.Name);
                    var instanceNames = category.GetInstanceNames().OrderBy(s => s).ToArray();
                    instancesListBox.Items.AddRange(instanceNames);
                }
                return;
            }
        }

        private void treeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            LoadCounters(e.Node);
        }

        private void LoadCounters(TreeNode categoryNode)
        {
            var firstNode = categoryNode.Nodes[0];

            // if the node childs are already loaded, bail.
            if (firstNode.Tag != null || categoryNode.Nodes.Count > 1)
                return;

            var categoryInfo = categoryNode.Tag as CategoryInfo;
            if (categoryInfo != null)
            {
                // TODO load the category counters...
                var category = new PerformanceCounterCategory(categoryInfo.Name);

                treeView.BeginUpdate();
                try
                {
                    categoryNode.Nodes.Clear();

                    if (category.CategoryType == PerformanceCounterCategoryType.SingleInstance)
                    {
                        var counters = category.GetCounters().OrderBy(c => c.CounterName);
                        foreach (var counter in counters)
                        {
                            var node = new TreeNode(counter.CounterName)
                                       {
                                           Tag = new CounterInfo
                                                 {
                                                     CategoryInfo = categoryInfo,
                                                     Name = counter.CounterName,
                                                     Help = counter.CounterHelp,
                                                     Type = counter.CounterType
                                                 }
                                       };

                            categoryNode.Nodes.Add(node);

                            counter.Dispose();
                        }
                    }
                    else if (category.CategoryType == PerformanceCounterCategoryType.MultiInstance)
                    {
                        var instanceNames = category.GetInstanceNames();

                        LoadPerformanceCountersFromInstance(categoryNode, categoryInfo, category, "");

                        /*foreach (var instanceName in instanceNames)
                        {
                            LoadPerformanceCountersFromInstance(e.Node, categoryInfo, category, instanceName);
                        }*/
                    }
                    else
                    {
                        categoryNode.Nodes.Add("Unknown category type...");
                    }
                }
                finally
                {
                    treeView.EndUpdate();
                }
            }
        }

        private static void LoadPerformanceCountersFromInstance(TreeNode parentNode, CategoryInfo info, PerformanceCounterCategory category, string instanceName)
        {
            var counters = category.GetCounters(instanceName).OrderBy(c => c.CounterName);

            foreach (var counter in counters)
            {
                try
                {
                    var node = new TreeNode(counter.CounterName)//string.Format("{0} -- {1}", counter.CounterName, instanceName))
                    {
                        // TODO use an CounterInstanceInfo instead...
                        Tag = new CounterInfo
                        {
                            CategoryInfo = info,
                            Name = counter.CounterName,
                            Help = counter.CounterHelp,
                            Type = counter.CounterType
                        }
                    };
                    parentNode.Nodes.Add(node);
                    
                }
                catch (Exception e)
                {
                    // NB: sometimes, the CounterType property getter raises an
                    // InvalidOperationException:
                    //
                    //  The Counter layout for the Category specified is invalid, a
                    //  counter of the type:  AverageCount64, AverageTimer32,
                    //  CounterMultiTimer, CounterMultiTimerInverse,
                    //  CounterMultiTimer100Ns, CounterMultiTimer100NsInverse,
                    //  RawFraction, or SampleFraction has to be immediately
                    //  followed by any of the base counter types: AverageBase,
                    //  CounterMultiBase, RawBase or SampleBase.
                    //
                    // So, I'm going to ignore the counter altogether.
                    // TODO log this.
                }
                counter.Dispose();
            }
        }

        private void searchToolStripButton_Click(object sender, EventArgs e)
        {
            if (_searchForm == null)
            {
                _searchForm = new SearchForm();
                _searchForm.VisibleChanged += (a, b) => searchToolStripButton.Checked = _searchForm.Visible;
                _searchForm.PerformanceCounterSelected += _searchForm_PerformanceCounterSelected;
            }

            if (searchToolStripButton.Checked)
                _searchForm.Show(this);
            else
                _searchForm.Hide();
        }

        private void systemMonitorToolStripButton_Click(object sender, EventArgs e)
        {
            if (_systemMonitorForm == null)
            {
                _systemMonitorForm = new SystemMonitorForm();
                _systemMonitorForm.VisibleChanged += (a, b) => systemMonitorToolStripButton.Checked = _systemMonitorForm.Visible;
            }

            if (systemMonitorToolStripButton.Checked)
                _systemMonitorForm.Show(this);
            else
                _systemMonitorForm.Hide();
        }

        void _searchForm_PerformanceCounterSelected(string categoryName, string counterName)
        {
            var categoryNode = treeView.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == categoryName);
            if (categoryNode == null)
                return;

            for (var i = 0; i < 2; ++i)
            {
                var counterNode = categoryNode.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == counterName);
                if (counterNode == null)
                {
                    LoadCounters(categoryNode);
                }
                else
                {
                    treeView.SelectedNode = counterNode;
                    break;
                }
            }
        }

        private void treeView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            var node = treeView.GetNodeAt(e.X, e.Y);
            if (node != null)
            {
                treeView.SelectedNode = node;
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView.SelectedNode;
            
            if (node.Tag is CategoryInfo)
            {
                var categoryInfo = (CategoryInfo)node.Tag;

                Clipboard.SetText(string.Format(
                    "{0}\r\n{1}\r\n{2}\r\n",
                    categoryInfo.Type,
                    categoryInfo.Name,
                    categoryInfo.Help
                ));
            }
            else if (node.Tag is CounterInfo)
            {
                var counterInfo = (CounterInfo)node.Tag;

                Clipboard.SetText(string.Format(
                    "{0}\r\n{1}\r\n{2}\r\n{3}\r\n",
                    counterInfo.Type,
                    counterInfo.CategoryInfo.Name,
                    counterInfo.Name,
                    counterInfo.Help
                ));
            }
        }

        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (_systemMonitorForm == null || !_systemMonitorForm.Visible)
                return;

            var node = e.Node;

            if (node.Tag is CounterInfo)
            {
                var counterInfo = (CounterInfo)node.Tag;

                string counterPath;

                if (counterInfo.CategoryInfo.Type == PerformanceCounterCategoryType.SingleInstance)
                {
                    counterPath = string.Format(@"\{0}\{1}", counterInfo.CategoryInfo.Name, counterInfo.Name);
                }
                else
                {
                    if (instancesListBox.Items.Count == 0)
                        return;
                    // TODO add all instances...
                    counterPath = string.Format(@"\{0}({2})\{1}", counterInfo.CategoryInfo.Name, counterInfo.Name, instancesListBox.Items[0]);
                }

                try
                {
                    _systemMonitorForm.AddCounter(counterPath);
                }
                catch (Exception error)
                {
                    MessageBox.Show(this, string.Format("Failed to add counter with path {0}: {1}", counterPath, error), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
