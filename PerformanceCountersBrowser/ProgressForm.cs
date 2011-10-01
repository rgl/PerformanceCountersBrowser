using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace PerformanceCountersBrowser
{
    public partial class ProgressForm : Form
    {
        private readonly BackgroundWorker _backgroundWorker;

        public ProgressForm(BackgroundWorker backgroundWorker)
        {
            InitializeComponent();

            _backgroundWorker = backgroundWorker;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
        }

        private void ProgressForm_Load(object sender, EventArgs e)
        {
            // NB this is needed because the StartPosition = CenterParent does
            //    not work with Form.Show ... only with ShowDialog ... bummer!
            CenterToParent();
        }

        private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Dispose();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            CancelBackgroundWorker();
        }

        private void ProgressForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = _backgroundWorker.IsBusy;
            CancelBackgroundWorker();
        }

        private void CancelBackgroundWorker()
        {
            _backgroundWorker.CancelAsync();
        }
    }
}
