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
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ServiceModel;
using System.Configuration.Install;
using System.Collections;
using System.IO;
using Microsoft.Win32;
using System.ServiceProcess;

namespace VirtualBoxService.InstallGui
{
	[ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
	class ElevatedService : IElevatedOperationsInterface
	{
        private class UserPassCombination {
            public string User;
            public string Pass;
        }

        private UserPassCombination _userPassCombination;
        
        ElevatedServiceHost.Hearbeat _heartbeat;
        
        public ElevatedService(ElevatedServiceHost.Hearbeat heartbeat) {
            _heartbeat = heartbeat;
        }

        public void InstallService(string user, string pass) {
            using (AssemblyInstaller installer = new AssemblyInstaller(Path.Combine(Utils.GetApplicationPath(), Utils.GetExecutableName()), null)) {
                installer.UseNewContext = true;
                Hashtable savedState = new Hashtable();

                UserPassCombination c = new UserPassCombination();
                c.User = user;
                c.Pass = pass;
                _userPassCombination = c;

                installer.BeforeInstall += new InstallEventHandler(installer_BeforeInstall);
                
                installer.Install(savedState);
                installer.Commit(savedState);

                installer.BeforeInstall -= installer_BeforeInstall;
            }
        }

        void installer_BeforeInstall(object sender, InstallEventArgs e) {
            AssemblyInstaller installer = (AssemblyInstaller)sender;
            setServiceProcessCredentials(_userPassCombination.User, _userPassCombination.Pass, installer.Installers);
        }

        private ServiceProcessInstaller getServiceProcessInstaller(InstallerCollection fromCollection) {
            foreach (var item in fromCollection) {
                if (item is ServiceProcessInstaller) {
                    return (ServiceProcessInstaller)item;
                }
                if (item is ProjectInstaller) {
                    return getServiceProcessInstaller(((ProjectInstaller)item).Installers);
                }
            }

            throw new InvalidOperationException("ServiceInstaller unavailiable. Assembly corrupted.");
        }

        private void setServiceProcessCredentials(string user, string pass, InstallerCollection installers) {
            ServiceProcessInstaller si = getServiceProcessInstaller(installers);
            si.Account = ServiceAccount.User;

            if (!user.Contains('\\')) {
                user = user.Insert(0, ".\\");
            }
            
            si.Username = user;
            si.Password = pass;
        }

		public void UninstallService() {
            using (AssemblyInstaller installer = new AssemblyInstaller(Path.Combine(Utils.GetApplicationPath(), Utils.GetExecutableName()), null)) {
                installer.UseNewContext = true;
                Hashtable table = new Hashtable();

                installer.Uninstall(table);
            }
		}

        public void StartService() {
            var vbox = InstallGuiMainForm.getVboxService();
            vbox.Start();
        }

        public void StopService() {
            var vbox = InstallGuiMainForm.getVboxService();
            vbox.Stop();
        }

        public void SetEnableTrace(bool value) {
            IVirtualBoxServiceConfig c = Config.GetConfig();
            c.EnableTrace = value;
        }

        public void SetEnableWebservice(bool value) {
            IVirtualBoxServiceConfig c = Config.GetConfig();
            c.EnableWebService = value;
        }

        public void SetWebserviceInterface(string value) {
            IVirtualBoxServiceConfig c = Config.GetConfig();
            c.WebServiceInterface = value;
        }

        public void Heartbeat() {
            _heartbeat.Beat();
        }
    }
}
