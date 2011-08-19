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
using Microsoft.Win32;

namespace VirtualBoxService {
    class RegistryKeyConfig : IVirtualBoxServiceConfig {

        public const string BASE_KEY = @"SYSTEM\CurrentControlSet\services\VirtualBoxService\config";
        public const RegistryHive BASE_REGISTRY_HIVE = RegistryHive.LocalMachine;

        private RegistryKey _registryBaseKey;


        void openBaseKey(bool writable) {
            using (RegistryKey hive = RegistryKey.OpenBaseKey(BASE_REGISTRY_HIVE, RegistryView.Default)) {
                _registryBaseKey = hive.OpenSubKey(BASE_KEY, writable);
            }
        }

        void closeBaseKey() {
            if (_registryBaseKey != null) {
                _registryBaseKey.Close();
            }
        }

        bool parseBoolean(string input, bool defaultValue) {
            if (input != null) {
                try {
                    bool result = Convert.ToBoolean(input);
                    return result;
                }
                catch (FormatException) {
                    return defaultValue;
                }
            }
            else {
                return defaultValue;
            }
        }

        bool getBoolean(string key, bool defaultValue) {
            openBaseKey(false);
            try {
                if (_registryBaseKey != null) {
                    return parseBoolean((string)_registryBaseKey.GetValue(key, null), ENABLE_TRACE_DEFAULT);
                }
                else return defaultValue;
            }
            finally {
                closeBaseKey();
            }
        }

        void setBoolean(string key, bool value) {
            openBaseKey(true);
            try {
                _registryBaseKey.SetValue(key, value.ToString(), RegistryValueKind.String);
            }
            finally {
                closeBaseKey();
            }
        }

        string getString(string key, string defaultValue) {
            openBaseKey(false);
            try {
                if (_registryBaseKey != null) {
                    return (string)_registryBaseKey.GetValue(key, defaultValue);
                }
                else return defaultValue;
            }
            finally {
                closeBaseKey();
            }
        }

        void setString(string key, string value) {
            openBaseKey(true);
            try {
                _registryBaseKey.SetValue(key, value);
            }
            finally {
                closeBaseKey();
            }
        }


        const string ENABLE_TRACE_KEY = "EnableTrace";
        const bool ENABLE_TRACE_DEFAULT = false;
        public bool EnableTrace {
            get {
                return getBoolean(ENABLE_TRACE_KEY, ENABLE_TRACE_DEFAULT);
            }
            set {
                setBoolean(ENABLE_TRACE_KEY, value);
            }
        }

        const string ENABLE_WEB_SERVICE_KEY = "EnableWebService";
        const bool ENABLE_WEB_SERVICE_DEFAULT = false;
        public bool EnableWebService {
            get {
                return getBoolean(ENABLE_WEB_SERVICE_KEY, ENABLE_WEB_SERVICE_DEFAULT);
            }
            set {
                setBoolean(ENABLE_WEB_SERVICE_KEY, value);
            }
        }

        const string WEB_SERVICE_INTERFACE_KEY = "WebServiceInterface";
        const string WEB_SERVICE_INTERFACE_DEFAULT = "localhost";
        public string WebServiceInterface {
            get {
                return getString(WEB_SERVICE_INTERFACE_KEY, WEB_SERVICE_INTERFACE_DEFAULT);
            }
            set {
                setString(WEB_SERVICE_INTERFACE_KEY, value);
            }
        }

        public void Initialize() {
            using (RegistryKey hive = RegistryKey.OpenBaseKey(BASE_REGISTRY_HIVE, RegistryView.Default)) {
                hive.CreateSubKey(BASE_KEY);
            }
        }

        public void Clear() {
            using (RegistryKey k = RegistryKey.OpenBaseKey(BASE_REGISTRY_HIVE, RegistryView.Default)) {
                if (k != null) {
                    k.DeleteSubKey(BASE_KEY, false);
                }
            }
        }
    }
}
