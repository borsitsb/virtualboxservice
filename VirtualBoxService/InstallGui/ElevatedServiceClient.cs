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
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Runtime.InteropServices;

namespace VirtualBoxService.InstallGui
{
	class ElevatedServiceClient : IDisposable
	{
		Process _elevatedProcess;
		

		public IElevatedOperationsInterface Interface {
			get;
			private set;
		}


		public ElevatedServiceClient() {
			_elevatedProcess = new Process();
			_elevatedProcess.StartInfo.FileName = Path.Combine(Utils.GetApplicationPath(), Utils.GetExecutableName());
			_elevatedProcess.StartInfo.Arguments = "--IPCServer";
			_elevatedProcess.StartInfo.Verb = "runas";

			_elevatedProcess.Start();

			Thread.Sleep(1000); //Give IPCHandler time to load

			Interface = ChannelFactory<IElevatedOperationsInterface>.CreateChannel(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/myPipe/elevatedServices"));
            Interface.Heartbeat();
		}

        bool _disposed;
		public void Dispose() {
            if (!_disposed) {
                if (!_elevatedProcess.HasExited) {
                    _elevatedProcess.Kill();
                }
                _disposed = true;   
            }
		}
	}
}
