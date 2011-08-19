namespace VirtualBoxService
{
	partial class ProjectInstaller
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Vom Komponenten-Designer generierter Code

		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent() {
            this.VirtualBoxServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.VirtualBoxServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // VirtualBoxServiceProcessInstaller
            // 
            this.VirtualBoxServiceProcessInstaller.Password = null;
            this.VirtualBoxServiceProcessInstaller.Username = null;
            // 
            // VirtualBoxServiceInstaller
            // 
            this.VirtualBoxServiceInstaller.Description = "This service is responsible for starting and shutdown of virtualmachines register" +
                "ed in Virtualbox.";
            this.VirtualBoxServiceInstaller.DisplayName = "VirtualBoxService";
            this.VirtualBoxServiceInstaller.ServiceName = "VirtualBoxService";
            this.VirtualBoxServiceInstaller.ServicesDependedOn = new string[] {
        "eventlog"};
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.VirtualBoxServiceProcessInstaller,
            this.VirtualBoxServiceInstaller});

		}

		#endregion

		private System.ServiceProcess.ServiceInstaller VirtualBoxServiceInstaller;
		public System.ServiceProcess.ServiceProcessInstaller VirtualBoxServiceProcessInstaller;
	}
}