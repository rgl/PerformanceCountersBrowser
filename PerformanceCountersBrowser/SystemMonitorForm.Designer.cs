namespace PerformanceCountersBrowser
{
    partial class SystemMonitorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SystemMonitorForm));
            this.systemMonitor = new AxSystemMonitor.AxSystemMonitor();
            ((System.ComponentModel.ISupportInitialize)(this.systemMonitor)).BeginInit();
            this.SuspendLayout();
            // 
            // systemMonitor
            // 
            this.systemMonitor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.systemMonitor.Enabled = true;
            this.systemMonitor.Location = new System.Drawing.Point(0, 0);
            this.systemMonitor.Name = "systemMonitor";
            this.systemMonitor.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("systemMonitor.OcxState")));
            this.systemMonitor.Size = new System.Drawing.Size(772, 516);
            this.systemMonitor.TabIndex = 0;
            // 
            // SystemMonitorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(772, 516);
            this.Controls.Add(this.systemMonitor);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "SystemMonitorForm";
            this.ShowInTaskbar = false;
            this.Text = "System Monitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SystemMonitorForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.systemMonitor)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxSystemMonitor.AxSystemMonitor systemMonitor;
    }
}