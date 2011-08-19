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
 * along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
 * 
 **/

namespace VirtualBoxService
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
    using System.Diagnostics;

	public static class Logging
	{
		public static TraceSource MachineControllerLog
		{
            get {
                return getSource("VirtualBoxService.MachineController");
            }
		}

		public static TraceSource ServiceInterfaceLog
		{
            get {
                return getSource("VirtualBoxService.ServiceInterface");
            }
		}


        static IList<TraceSource> _traceSources = new List<TraceSource>();

		private static TraceSource getSource(string name)
		{
            lock (_traceSources) {
                IDictionary<string, TraceSource> dict = _traceSources.ToDictionary(x => x.Name);
                
                if (!dict.ContainsKey(name)) {
                    _traceSources.Add(new TraceSource(name));
                    dict = _traceSources.ToDictionary(x => x.Name);
                }

                return dict[name];
            }
		}



        internal enum MachineControllerEventIds {
            MachineStartupFailed = 20401,
            MachineStartup = 20201,
            
            MachineShutdown = 20202,
            MachineShutdownFailed = 20402,
            BeginMachineSaveState = 20203,
            EndMachineSaveState = 20213,
            BeginMachineAcpiShutdown = 20204,
            EndMachineAcpiShutdown = 20214,
            BeginMachineHardOff = 20205,
            EndMachineHardOff = 20215,

            DescriptionNotParsable = 20403,
            
            TimeEstimationProblem = 20302,
            AnnounceRemainingTime = 20230,
        }

        internal enum ServiceInterfaceEventIds {
            PreshutdownManipulated = 10201,
            PreshutdownCommandReceived = 10202,
        }
	}
}

