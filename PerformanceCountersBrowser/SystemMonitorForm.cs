using System.Windows.Forms;
using SystemMonitor;

namespace PerformanceCountersBrowser
{
    public partial class SystemMonitorForm : Form
    {
        public SystemMonitorForm()
        {
            InitializeComponent();
        }

        private void SystemMonitorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        // eg. AddCounter(@"\Processor(0)\% Processor Time")
        public ICounterItem AddCounter(string counterPath)
        {
            ICounterItem counterItem;
            systemMonitor.AddCounter(counterPath, out counterItem);
            return counterItem;
        }

        public void RemoveCounter(ICounterItem counterItem)
        {
            systemMonitor.DeleteCounter(counterItem);
        }
    }
}
