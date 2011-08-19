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
using System.IO;

namespace VBoxWrapper.COMInterface
{
	public class VBoxComUtils
	{
		public static RegistryKey GetVirtualBoxAppIDKey() {
			RegistryKey ClsidKey = GetVirtualBoxClsidKey();

			string virtualboxAppID = (string)ClsidKey.GetValue("AppID", string.Empty);
			ClsidKey.Close();

			RegistryKey AppIDKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\AppID\" + virtualboxAppID, true);
			return AppIDKey;
		}

		public static string GetVirtualBoxCLSID() {
			string curVersionProgID = GetVirtualBoxProgID();

			RegistryKey ProgIDKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\" + curVersionProgID + @"\CLSID");
			string virtualboxClsid = (string)ProgIDKey.GetValue("", string.Empty);
			ProgIDKey.Close();

			return virtualboxClsid;
		}

        public static RegistryKey GetVirtualBoxClsidKey() {
            string vbCLSID = GetVirtualBoxCLSID();

            //Attention: If running a x86-assembly on x64-windows, registry will be redirected to the WoW3264Node, but Virtualbox installs itself as x64!
            //On x86-windows there should be no problem, as Virtualbox installs itself as x86 on a x86 machine anyways and there is no redirection.
            if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess) {
                //Now we have to search in 64-bit registry from 32-bit process
                return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"Software\Classes\CLSID\" + vbCLSID);
            }
            else {
                return Registry.LocalMachine.OpenSubKey(@"Software\Classes\CLSID\" + vbCLSID);
            }
        }

		public static string GetVirtualBoxProgID() {
			RegistryKey versionlessProgIDKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\VirtualBox.VirtualBox\CurVer");
			string curVersionProgID = (string)versionlessProgIDKey.GetValue("", string.Empty);
			versionlessProgIDKey.Close();
			return curVersionProgID;
		}

        public static string GetVirtualBoxDir() {
            string entry = GetVirtualBoxComTypeLib();

            string dir = Directory.GetParent(entry).FullName;

            return dir;
        }

        public static string GetVirtualBoxComTypeLib() {
            RegistryKey clsidKey = GetVirtualBoxClsidKey();
            RegistryKey localServer32Key = clsidKey.OpenSubKey("LocalServer32", false);
            string entry = (string)localServer32Key.GetValue(null);

            if (entry != null) {
                entry = entry.TrimEnd('"');
                entry = entry.TrimStart('"');
            }
            else {
                throw new DirectoryNotFoundException("Local Installation of Virtualbox was not found.");
            }
            return entry;
        }
	}
}
