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
using System.ServiceProcess;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace VirtualBoxService
{
    class ServiceBasePreshutdownEnabler
    {
        private static class NativeMethods {
            [DllImport("advapi32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseServiceHandle(
                IntPtr hSCObject);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ChangeServiceConfig2(
                SafeServiceHandle hService,
                int dwInfoLevel,
                ref SERVICE_PRESHUTDOWN_INFO lpInfo);

            [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern SafeServiceHandle OpenSCManager(
                string machineName,
                string databaseName,
                SCM_ACCESS dwAccess);

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern SafeServiceHandle OpenService(
                SafeServiceHandle hSCManager,
                string lpServiceName,
                SERVICE_ACCESS dwDesiredAccess);

            [Flags]
            public enum SERVICE_ACCESS : uint {
                STANDARD_RIGHTS_REQUIRED = 0xF0000,
                SERVICE_QUERY_CONFIG = 0x00001,
                SERVICE_CHANGE_CONFIG = 0x00002,
                SERVICE_QUERY_STATUS = 0x00004,
                SERVICE_ENUMERATE_DEPENDENTS = 0x00008,
                SERVICE_START = 0x00010,
                SERVICE_STOP = 0x00020,
                SERVICE_PAUSE_CONTINUE = 0x00040,
                SERVICE_INTERROGATE = 0x00080,
                SERVICE_USER_DEFINED_CONTROL = 0x00100,
                SERVICE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
                                  SERVICE_QUERY_CONFIG |
                                  SERVICE_CHANGE_CONFIG |
                                  SERVICE_QUERY_STATUS |
                                  SERVICE_ENUMERATE_DEPENDENTS |
                                  SERVICE_START |
                                  SERVICE_STOP |
                                  SERVICE_PAUSE_CONTINUE |
                                  SERVICE_INTERROGATE |
                                  SERVICE_USER_DEFINED_CONTROL)
            }

            [Flags]
            public enum SCM_ACCESS : uint {
                SC_MANAGER_CREATE_SERVICE = 0x00002,
                SC_MANAGER_CONNECT = 0x00001,
                SC_MANAGER_ENUMERATE_SERVICE = 0x00004,
                SC_MANAGER_QUERY_LOCK_STATUS = 0x00010,
                SC_MANAGER_LOCK = 0x00008,
                SC_MANAGER_MODIFY_BOOT_CONFIG = 0x00020,
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SERVICE_PRESHUTDOWN_INFO {
                public UInt32 dwPreshutdownTimeout;
            }
        }

        private class SafeServiceHandle : SafeHandle {
            //PrePrepareMethod, because methods can be run in critical finalizer in Constrained Execution Region (CER)
            public SafeServiceHandle() : base(IntPtr.Zero, true) { }
            public override bool IsInvalid {
                [PrePrepareMethod]
                get { return (handle == IntPtr.Zero); }
            }
            [PrePrepareMethod]
            protected override bool ReleaseHandle() {
                return NativeMethods.CloseServiceHandle(handle);
            }
        }

        private class ServiceManager : IDisposable {

            private SafeServiceHandle _handle;
            
            public ServiceManager() {
                _handle = NativeMethods.OpenSCManager(null, null, NativeMethods.SCM_ACCESS.SC_MANAGER_ENUMERATE_SERVICE);
                if (_handle.IsInvalid) {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
            }

            public ServiceConfig OpenService(string serviceName) {
                SafeServiceHandle serviceHandle = NativeMethods.OpenService(_handle, serviceName, NativeMethods.SERVICE_ACCESS.SERVICE_CHANGE_CONFIG);
                if (serviceHandle.IsInvalid) {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
                return new ServiceConfig(serviceHandle);
            }

            public void Dispose() {
                _handle.Dispose();
            }
        }

        private class ServiceConfig : IDisposable {

            const int SERVICE_CONFIG_PRESHUTDOWN_INFO = 7;

            private SafeServiceHandle _handle;

            public ServiceConfig(SafeServiceHandle handle) {
                _handle = handle;
            }

            public void SetPreshutdownTimeout(int milliseconds) {
                if (milliseconds <= 0) {
                    throw new ArgumentOutOfRangeException("PreshutdownTimeout must be a positive value.");
                }
                
                NativeMethods.SERVICE_PRESHUTDOWN_INFO info = new NativeMethods.SERVICE_PRESHUTDOWN_INFO();
                info.dwPreshutdownTimeout = (uint)milliseconds;
                
                bool ret = NativeMethods.ChangeServiceConfig2(_handle, SERVICE_CONFIG_PRESHUTDOWN_INFO, ref info);
                if (!ret) {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
            }

            public void Dispose() {
                _handle.Dispose();
            }
        }


        
        private ServiceBase _serviceBase;
        private TraceSource _log = Logging.ServiceInterfaceLog;
        private int _timeoutMilliseconds;


        public ServiceBasePreshutdownEnabler(ServiceBase b)
        {
            _serviceBase = b;
        }

        public void EnablePreshutdownAccepted()
        {
            int controlsAccepted = getControlsAccepted();
            controlsAccepted |= 0x100;
            setControlsAccepted(controlsAccepted);

            Debug.Assert((getControlsAccepted() & 0x100) == 1);

           log("Enabled Preshutdown Accepted");
        }

        public void DisablePreshutdownAccepted()
        {
            int controlsAccepted = getControlsAccepted();
            controlsAccepted &= -257;
            setControlsAccepted(controlsAccepted);

            Debug.Assert((getControlsAccepted() & 0x100) == 0);

            log("Disabled Preshutdown Accepted");
        }

        public void RequestAdditionalTime(int timeInMilliseconds) {
            if (timeInMilliseconds < 0) {
                timeInMilliseconds = 1000;
            }
            _timeoutMilliseconds += timeInMilliseconds;

            using (ServiceManager scm = new ServiceManager()) {
                using (ServiceConfig sc = scm.OpenService(_serviceBase.ServiceName)) {
                    sc.SetPreshutdownTimeout(_timeoutMilliseconds);
                    log(String.Format("Set PreshutdownTimeout to {0}ms", _timeoutMilliseconds));
                }
            }
        }

        int getControlsAccepted()
        {
            /*FieldInfo statusField = typeof(ServiceBase).GetField("status", BindingFlags.NonPublic | BindingFlags.Instance);
            object statusStructure = statusField.GetValue(_serviceBase);
            FieldInfo controlsAcceptedField = statusField.FieldType.GetField("controlsAccepted");
            int controlsAccepted = (int)controlsAcceptedField.GetValue(statusStructure);*/

            FieldInfo acceptedControls = typeof(ServiceBase).GetField("acceptedCommands", BindingFlags.NonPublic | BindingFlags.Instance);
            int controlsAccepted = (int)acceptedControls.GetValue(_serviceBase);
            return controlsAccepted;
        }

        void setControlsAccepted(int value)
        {
            /*FieldInfo statusField = typeof(ServiceBase).GetField("status", BindingFlags.NonPublic | BindingFlags.Instance);
            object statusStructure = statusField.GetValue(_serviceBase);
            FieldInfo controlsAcceptedField = statusField.FieldType.GetField("controlsAccepted");*/

            FieldInfo acceptedControls = typeof(ServiceBase).GetField("acceptedCommands", BindingFlags.NonPublic | BindingFlags.Instance);
            acceptedControls.SetValue(_serviceBase, value);
        }

        void log(string msg) {
            Logging.ServiceInterfaceLog.TraceEvent(TraceEventType.Verbose, (int)Logging.ServiceInterfaceEventIds.PreshutdownManipulated, msg);
        }
    }
}
