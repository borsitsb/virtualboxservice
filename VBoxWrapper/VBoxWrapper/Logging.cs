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

namespace VBoxWrapper
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
    using System.Diagnostics;

	public static class Logging
	{
		public static TraceSource MachineLog
		{
            get {
                return getSource("VBoxWrapper.Machine");
            }
		}

		public static TraceSource ComInterfaceLog
		{
            get {
                return getSource("VBoxWrapper.ComInterface");
            }
		}

		public static TraceSource MachineBuildingLog
		{
            get {
                return getSource("VBoxWrapper.MachineBuilding");
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


        internal enum ComInterfaceErrorIds {
            ComProxyInstantiationFailed = 40401,
            ProvisionedComServer = 40201,
            UnprovisionedComServer = 40202,
            IProgressWaitTimedOutSet = 40203,
            MachineEventListenerRegistered = 40204,
            MachineEventListenerDeregistered = 40214,
        }
	}
}

