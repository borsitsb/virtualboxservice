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
using System.ServiceProcess;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using VirtualBoxService.InstallGui;

namespace VirtualBoxService
{
	static class Program
	{
		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>
		static void Main(string[] args) {
			if (args.Length > 0 && args.Contains("--IPCServer")) {
                runElevatedIpcServer();
			}
			else if (Environment.UserInteractive) {
                runInstallGui();
			}
			else {
                runService();
			}
		}

        static void runElevatedIpcServer() {
            ElevatedServiceHost.RunServer();
        }

        static void runInstallGui() {
            Application.EnableVisualStyles();
            Application.Run(new InstallGui.InstallGuiMainForm());
        }

        static void runService() {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
				{ 
					new Service() 
				};
            ServiceBase.Run(ServicesToRun);
        }
	}
}
