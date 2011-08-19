/* 
 * Copyright 2011 Felix Rüttiger
 * 
 * This file is part of VirtualBoxService.
 *
 * VirtualBoxService is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * VirtualBoxService is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with VirtualBoxService.  If not, see <http://www.gnu.org/licenses/>.
 * 
 **/

using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration.Install;
using System.IO;
using System.ServiceProcess;
using System.Security;
using System.Diagnostics;
using System.ServiceModel;
using Microsoft.Win32;
using System.Net;

namespace VirtualBoxService.InstallGui
{
	public partial class InstallGuiMainForm : Form
	{
		public InstallGuiMainForm() {
			InitializeComponent();
		}


		void InstallGuiMainForm_Load(object sender, EventArgs e) {
			setGuiElementsAccordingServiceState();
            setGuiElementsFromConfig();
            setInstructions();
            setIps();

            serviceStatusRefreshTimer.Tick += new EventHandler(serviceStatusRefreshTimer_Tick);
            serviceStatusRefreshTimer.Interval = 3500;
            serviceStatusRefreshTimer.Enabled = true;
		}

        void serviceStatusRefreshTimer_Tick(object sender, EventArgs e) {
            if (getVboxService() != null && (getVboxService().Status == ServiceControllerStatus.StartPending || getVboxService().Status == ServiceControllerStatus.StopPending)) {
                lockGuiElements();
            }
            else {
                setGuiElementsAccordingServiceState();
            }
        }

		void setGuiElementsAccordingServiceState() {
			ServiceController vboxService = getVboxService();
			
			if (vboxService == null) {
				InstallButton.Enabled = true;
				UninstallButton.Enabled = false;
				ApplySettingsButton.Enabled = false;
				StartStopServiceButton.Enabled = false;
				StartStopServiceButton.Text = "Start Service";
				return;
			}

			if (vboxService.Status == ServiceControllerStatus.Stopped) {
				InstallButton.Enabled = false;
				UninstallButton.Enabled = true;
				InstallButton.Enabled = false;
				ApplySettingsButton.Enabled = true;
				StartStopServiceButton.Enabled = true;
				StartStopServiceButton.Text = "Start Service";
			}

			if (vboxService.Status == ServiceControllerStatus.Running) {
				InstallButton.Enabled = false;
				UninstallButton.Enabled = false;
				ApplySettingsButton.Enabled = true;
				StartStopServiceButton.Enabled = true;
				StartStopServiceButton.Text = "Stop Service";
				return;
			}
		}

        void setGuiElementsFromConfig() {
            IVirtualBoxServiceConfig c = Config.GetConfig();
            chkEnableTrace.Checked = c.EnableTrace;
            chkEnableWebservice.Checked = c.EnableWebService;
            cmbWebserviceInterface.SelectedText = c.WebServiceInterface;
            if (cmbWebserviceInterface.Items.Contains(c.WebServiceInterface)) {
                cmbWebserviceInterface.SelectedItem = c.WebServiceInterface;    
            }
            else {
                cmbWebserviceInterface.Text = c.WebServiceInterface;
            }
        }

        void lockGuiElements() {
            InstallButton.Enabled = false;
            UninstallButton.Enabled = false;
            ApplySettingsButton.Enabled = false;
            StartStopServiceButton.Enabled = false;
            StartStopServiceButton.Text = "Pending...";
            return;
        }

        void setIps() {
            cmbWebserviceInterface.Items.Add("localhost");
            
            String strHostName = Dns.GetHostName();

            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
            foreach (IPAddress ipaddress in iphostentry.AddressList) {
                cmbWebserviceInterface.Items.Add(ipaddress.ToString());
            }
        }

        void setInstructions() {
            MemoryStream m = new MemoryStream(VirtualBoxService.Properties.Resources.instructions);
            richTextBox1.LoadFile(m, RichTextBoxStreamType.RichText);
        }




		public static ServiceController getVboxService() {
			ServiceController[] services = ServiceController.GetServices();
			ServiceController vboxService = services.FirstOrDefault(x => x.ServiceName == "VirtualBoxService");
			return vboxService;
		}

		private void InstallButton_Click(object sender, EventArgs e) {
            ServiceLogonInformationDialog dlg = new ServiceLogonInformationDialog();
            switch (dlg.ShowDialog(this)) {
                case DialogResult.Cancel:
                    return;
                case DialogResult.OK:
                    break;
                default:
                    return;
            } 

			try {
				using (ElevatedServiceClient c = new ElevatedServiceClient()) {
					c.Interface.InstallService(dlg.User, dlg.Pass);
				}
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
				//throw;
                return;
			}

			setGuiElementsAccordingServiceState();
		}

		private void UninstallButton_Click(object sender, EventArgs e) {
			try {
				using (ElevatedServiceClient c = new ElevatedServiceClient()) {
					c.Interface.UninstallService();
				}
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
				//throw;
                return;
			}

			setGuiElementsAccordingServiceState();
		}

		private void StartStopServiceButton_Click(object sender, EventArgs e) {
			ServiceController vboxService = getVboxService();
			if (vboxService.Status == ServiceControllerStatus.Stopped) {
                using (ElevatedServiceClient c = new ElevatedServiceClient()) {
                    c.Interface.StartService();
                }
			}
			else {
                using (ElevatedServiceClient c = new ElevatedServiceClient()) {
                    c.Interface.StopService();
                }
			}

			lockGuiElements();
		}

        private void ApplySettingsButton_Click_1(object sender, EventArgs e) {
            try {
                using (ElevatedServiceClient c = new ElevatedServiceClient()) {
                    c.Interface.SetEnableTrace(chkEnableTrace.Checked);
                    c.Interface.SetEnableWebservice(chkEnableWebservice.Checked);
                    c.Interface.SetWebserviceInterface(cmbWebserviceInterface.Text);
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
                //throw;
                return;
            }

            setGuiElementsFromConfig();
        }
	}
}
