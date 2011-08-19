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
using System.ServiceModel;
using System.Diagnostics;
using System.Threading;

namespace VirtualBoxService.InstallGui
{
	class ElevatedServiceHost
	{
        public class Hearbeat {
            bool _set;

            public void Beat() {
                lock (this) {
                    _set = true;
                }
            }

            public bool IsSet() {
                lock (this) {
                    return _set;
                }
            }

            public void Reset() {
                lock (this) {
                    _set = false;
                }
            }
        }

        static Hearbeat _heartbeat;

		static public void RunServer() {
            
            
            _heartbeat = new Hearbeat();
            ElevatedService service = new ElevatedService(_heartbeat);
            
            ServiceHost sh = new ServiceHost(service, new Uri("net.pipe://localhost/myPipe"));
			NetNamedPipeBinding np = new NetNamedPipeBinding();
			sh.AddServiceEndpoint(typeof(IElevatedOperationsInterface), np, "elevatedServices");
			sh.Open();

            

            _heartbeat.Beat(); //give parent time to connect
            waitForDebugger();
            while (isParentProcessAlive()) {
                _heartbeat.Reset();
                Thread.Sleep(60000);
            }

            sh.Close();
		}

        static bool isParentProcessAlive() {
            return _heartbeat.IsSet();
        }

        [Conditional("DEBUG")]
        static void waitForDebugger() {
            while (!Debugger.IsAttached) {
                Thread.Sleep(2000);
            }
        }
	}
}
