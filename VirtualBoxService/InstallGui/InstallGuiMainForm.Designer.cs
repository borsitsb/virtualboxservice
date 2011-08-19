namespace VirtualBoxService.InstallGui
{
	partial class InstallGuiMainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.InstallButton = new System.Windows.Forms.Button();
            this.UninstallButton = new System.Windows.Forms.Button();
            this.StartStopServiceButton = new System.Windows.Forms.Button();
            this.serviceStatusRefreshTimer = new System.Windows.Forms.Timer(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ApplySettingsButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbWebserviceInterface = new System.Windows.Forms.ComboBox();
            this.chkEnableWebservice = new System.Windows.Forms.CheckBox();
            this.chkEnableTrace = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // InstallButton
            // 
            this.InstallButton.Location = new System.Drawing.Point(353, 34);
            this.InstallButton.Name = "InstallButton";
            this.InstallButton.Size = new System.Drawing.Size(85, 23);
            this.InstallButton.TabIndex = 0;
            this.InstallButton.Text = "Install";
            this.InstallButton.UseVisualStyleBackColor = true;
            this.InstallButton.Click += new System.EventHandler(this.InstallButton_Click);
            // 
            // UninstallButton
            // 
            this.UninstallButton.Location = new System.Drawing.Point(353, 63);
            this.UninstallButton.Name = "UninstallButton";
            this.UninstallButton.Size = new System.Drawing.Size(85, 23);
            this.UninstallButton.TabIndex = 1;
            this.UninstallButton.Text = "Uninstall";
            this.UninstallButton.UseVisualStyleBackColor = true;
            this.UninstallButton.Click += new System.EventHandler(this.UninstallButton_Click);
            // 
            // StartStopServiceButton
            // 
            this.StartStopServiceButton.Location = new System.Drawing.Point(353, 93);
            this.StartStopServiceButton.Name = "StartStopServiceButton";
            this.StartStopServiceButton.Size = new System.Drawing.Size(85, 23);
            this.StartStopServiceButton.TabIndex = 2;
            this.StartStopServiceButton.Text = "Start Service";
            this.StartStopServiceButton.UseVisualStyleBackColor = true;
            this.StartStopServiceButton.Click += new System.EventHandler(this.StartStopServiceButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ApplySettingsButton);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cmbWebserviceInterface);
            this.groupBox1.Controls.Add(this.chkEnableWebservice);
            this.groupBox1.Controls.Add(this.chkEnableTrace);
            this.groupBox1.Location = new System.Drawing.Point(12, 177);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(426, 72);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // ApplySettingsButton
            // 
            this.ApplySettingsButton.Location = new System.Drawing.Point(328, 39);
            this.ApplySettingsButton.Name = "ApplySettingsButton";
            this.ApplySettingsButton.Size = new System.Drawing.Size(92, 21);
            this.ApplySettingsButton.TabIndex = 9;
            this.ApplySettingsButton.Text = "Apply Settings";
            this.ApplySettingsButton.UseVisualStyleBackColor = true;
            this.ApplySettingsButton.Click += new System.EventHandler(this.ApplySettingsButton_Click_1);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(152, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Webservice Interface:";
            // 
            // cmbWebserviceInterface
            // 
            this.cmbWebserviceInterface.FormattingEnabled = true;
            this.cmbWebserviceInterface.Location = new System.Drawing.Point(155, 40);
            this.cmbWebserviceInterface.Name = "cmbWebserviceInterface";
            this.cmbWebserviceInterface.Size = new System.Drawing.Size(160, 21);
            this.cmbWebserviceInterface.TabIndex = 7;
            // 
            // chkEnableWebservice
            // 
            this.chkEnableWebservice.AutoSize = true;
            this.chkEnableWebservice.Location = new System.Drawing.Point(6, 42);
            this.chkEnableWebservice.Name = "chkEnableWebservice";
            this.chkEnableWebservice.Size = new System.Drawing.Size(119, 17);
            this.chkEnableWebservice.TabIndex = 6;
            this.chkEnableWebservice.Text = "Enable Webservice";
            this.chkEnableWebservice.UseVisualStyleBackColor = true;
            // 
            // chkEnableTrace
            // 
            this.chkEnableTrace.AutoSize = true;
            this.chkEnableTrace.Location = new System.Drawing.Point(6, 19);
            this.chkEnableTrace.Name = "chkEnableTrace";
            this.chkEnableTrace.Size = new System.Drawing.Size(90, 17);
            this.chkEnableTrace.TabIndex = 5;
            this.chkEnableTrace.Text = "Enable Trace";
            this.chkEnableTrace.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 16);
            this.label2.TabIndex = 7;
            this.label2.Text = "Instructions";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(15, 36);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(312, 135);
            this.richTextBox1.TabIndex = 8;
            this.richTextBox1.Text = "";
            // 
            // InstallGuiMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 258);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.StartStopServiceButton);
            this.Controls.Add(this.UninstallButton);
            this.Controls.Add(this.InstallButton);
            this.Name = "InstallGuiMainForm";
            this.Text = "VirtualBoxService Installer";
            this.Load += new System.EventHandler(this.InstallGuiMainForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button InstallButton;
		private System.Windows.Forms.Button UninstallButton;
        private System.Windows.Forms.Button StartStopServiceButton;
        private System.Windows.Forms.Timer serviceStatusRefreshTimer;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbWebserviceInterface;
        private System.Windows.Forms.CheckBox chkEnableWebservice;
        private System.Windows.Forms.CheckBox chkEnableTrace;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button ApplySettingsButton;
        private System.Windows.Forms.RichTextBox richTextBox1;
	}
}