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

namespace VirtualBoxService.WrapperExtensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using VBoxWrapper;
    using System.IO;
    using System.Diagnostics;

	public class DescriptionBasedInfoProvider : IServiceInfoProvider
	{
        const bool DEFAULT_AUTOBOOT = false;
        private readonly ShutdownMethod DEFAULT_SHUTDOWNMETHOD = new SaveStateMethod();
        const int DEFAULT_ACPI_SHUTDOWN_TIMEOUT = 10000;

        private class MachineProxyDescriptionFilter : IVirtualMachineProxy {
            private IVirtualMachineProxy _innerProxy;
            private DescriptionBasedInfoProvider _innerDescriptionInfoProvider;


            public MachineProxyDescriptionFilter(IVirtualMachineProxy innerProxy, DescriptionBasedInfoProvider innerDescriptionInfoProvider) {
                _innerProxy = innerProxy;
                _innerDescriptionInfoProvider = innerDescriptionInfoProvider;
            }


            public string getDescription() {
                return _innerDescriptionInfoProvider.getRealDescription();
            }

            public string getId() {
                return _innerProxy.getId();
            }

            public string getName() {
                return _innerProxy.getName();
            }

            public MachineState getState() {
                return _innerProxy.getState();
            }

            public ProgressToken Shutdown(ShutdownType type) {
                return _innerProxy.Shutdown(type);
            }

            public ProgressToken Start() {
                return _innerProxy.Start();
            }

            public void Dispose() {
                _innerProxy.Dispose();
            }
        }


        private IVirtualMachineProxy _proxy;
        private IVirtualMachineProxy _filteredProxy;
        private TraceSource _log = VirtualBoxService.Logging.MachineControllerLog;

        public DescriptionBasedInfoProvider(IVirtualMachineProxy proxy) {
            _proxy = proxy;
            _filteredProxy = new MachineProxyDescriptionFilter(proxy, this);
        }


        public IVirtualMachineProxy GetMachineProxyDescriptionFilter() {
            return _filteredProxy;
        }

		public virtual bool getAutoBoot()
		{
            ParsedDescription p = parseDescription();
            if (p.Autostart.HasValue) {
                return p.Autostart.Value;
            }
            else {
                return DEFAULT_AUTOBOOT;
            }
		}

		public virtual ShutdownMethod getShutdownMethod()
		{
            ParsedDescription p = parseDescription();
            if (p.ShutdownType.HasValue) {
                switch (parseDescription().ShutdownType.Value) {
                    case ParsedDescription.ShutdownTypeEnum.SaveState:
                        return new SaveStateMethod();
                    case ParsedDescription.ShutdownTypeEnum.ACPIShutdown:
                        return new ServiceAwareACPIShutdownMethod(this);
                    case ParsedDescription.ShutdownTypeEnum.HardOff:
                        return new HardOffMethod();
                    default:
                        throw new InvalidOperationException("ShutdownMethod could not be matched");
                }
            }
            else {
                return DEFAULT_SHUTDOWNMETHOD;
            }
		}

		public virtual int getACPIShutdownTimeoutMilliSeconds()
		{
            ParsedDescription p = parseDescription();
            if (p.ACPIShutdownTimeout.HasValue) {
                return p.ACPIShutdownTimeout.Value;
            }
            else {
                return DEFAULT_ACPI_SHUTDOWN_TIMEOUT;
            }
		}

        public virtual string getRealDescription() {
            ParsedDescription p = parseDescription();
            return p.Description;
        }

        private ParsedDescription parseDescription() {
            try {
                return ParsedDescription.Parse(_proxy.getDescription());
            }
            catch (InvalidDataException) {
                //_log.TraceEvent(TraceEventType.Verbose, (int)VirtualBoxService.Logging.MachineControllerEventIds.DescriptionNotParsable, "Description could not be parsed: {0} \n\nUsing Defaults.", ida.ToString());
                return new ParsedDescription();
            }
            catch (Exception ex) {
                _log.TraceEvent(TraceEventType.Verbose, (int)VirtualBoxService.Logging.MachineControllerEventIds.DescriptionNotParsable, "Description could not be parsed: {0} \n\nUsing Defaults.", ex.ToString());
                return new ParsedDescription();
            }
        }

	}
}

