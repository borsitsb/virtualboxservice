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

namespace VBoxWrapper.COMInterface {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using VBoxWrapper;
    using comVB = VirtualBox;
    using Microsoft.Win32;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class ComMachineProxyListBuilder : MachineProxyListBuilder, IDisposable {
        private dynamic _virtualBox;
        private TraceSource _log = Logging.ComInterfaceLog;

        public ComMachineProxyListBuilder() {
            initVirtualBox();
        }


        public virtual IEnumerable<IVirtualMachineProxy> buildMachineProxyList() {
            dynamic machines = _virtualBox.Machines;

            foreach (var machine in machines) {
                ComMachineProxy cmp = null;
                try {
                    cmp = new ComMachineProxy(machine);
                }
                catch (Exception anyEx) {
                    _log.TraceEvent(TraceEventType.Error, (int)Logging.ComInterfaceErrorIds.ComProxyInstantiationFailed, "Could not instantiate MachineProxy via COM-Interface", anyEx.ToString());
                    _log.TraceData(TraceEventType.Error, (int)Logging.ComInterfaceErrorIds.ComProxyInstantiationFailed, anyEx);
                }

                if (cmp != null) {
                    yield return cmp;
                }
            }
        }

        void initVirtualBox() {
            provisionComServer();

            _virtualBox = Activator.CreateInstance(Type.GetTypeFromProgID(VBoxComUtils.GetVirtualBoxProgID()));

            //Com-Server is now running, we can revert changes in registry
            unprovisionComServer();
        }

        /// <summary>
        /// Writes necessary registry keys, to get correct behaviour of COM Server, if it is called from a Windows Service.
        /// </summary>
        void provisionComServer() {
            RegistryKey AppIDKey = VBoxComUtils.GetVirtualBoxAppIDKey();
            AppIDKey.SetValue("LoadUserSettings", 0x0001, RegistryValueKind.DWord);
            AppIDKey.Close();
            _log.TraceEvent(TraceEventType.Verbose, (int)Logging.ComInterfaceErrorIds.ProvisionedComServer, "VirtualBox COM-Server provisioned via registry.");
        }

        void unprovisionComServer() {
            RegistryKey AppIDKey = VBoxComUtils.GetVirtualBoxAppIDKey();
            AppIDKey.DeleteValue("LoadUserSettings", false);
            _log.TraceEvent(TraceEventType.Verbose, (int)Logging.ComInterfaceErrorIds.UnprovisionedComServer, "VirtualBox COM-Server unprovisioned via registry.");
        }

        private bool disposed = false;
        public void Dispose() {
            if (!disposed) {
                Marshal.FinalReleaseComObject(_virtualBox); //May be used to tell RCW, that COM-Object is no more needed (RCW takes care of COM-Release, if not called)
                disposed = true;
            }
        }
    }
}

