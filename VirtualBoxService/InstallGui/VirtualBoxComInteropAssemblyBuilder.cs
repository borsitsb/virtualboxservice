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
using System.Runtime.InteropServices;
using System.Reflection.Emit;
using System.IO;

namespace VirtualBoxService.InstallGui {
    class VirtualBoxComInteropAssemblyBuilder {
        private static class NativeMethods {
            [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
            public static extern void LoadTypeLibEx(String strTypeLibName, RegKind regKind,
                [MarshalAs(UnmanagedType.Interface)] out Object typeLib);

            public enum RegKind {
                RegKind_Default = 0,
                RegKind_Register = 1,
                RegKind_None = 2
            }
        }

        private class ConversionEventHandler : ITypeLibImporterNotifySink {
            
            public void ReportEvent(ImporterEventKind eventKind, int eventCode, string eventMsg) {
                return;
            }

            public System.Reflection.Assembly ResolveRef(object typeLib) {
                throw new NotImplementedException("Virtualbox-Typelibrary references some other libraries. This is not implemented.");
            }
        }

        object _typeLibInMemory;

        public void BuildLib(string targetFolder) {
            NativeMethods.LoadTypeLibEx(VBoxWrapper.COMInterface.VBoxComUtils.GetVirtualBoxComTypeLib(), NativeMethods.RegKind.RegKind_None, out _typeLibInMemory);
            
            if (_typeLibInMemory == null) {
                throw new DllNotFoundException("Could not load Virtualbox-Typelibrary.");
            }

            TypeLibConverter converter = new TypeLibConverter();
            ConversionEventHandler handler = new ConversionEventHandler();

            AssemblyBuilder asm = converter.ConvertTypeLibToAssembly(_typeLibInMemory, "Interop.VirtualBox.dll", TypeLibImporterFlags.SafeArrayAsSystemArray, handler, null, null, "VirtualBox", null); //using assembly name "VirtualBox" and SafeArrayAsSystemArray to be compatible to VisualStudio-Generated Interop-Assembly
            asm.Save("Interop.VirtualBox.dll");
        }

        public void DeleteTypeLib(string targetFolder) {
            string typeLibPath = Path.Combine(targetFolder, "Interop.VirtualBox.dll");
            if (File.Exists(typeLibPath)) {
                File.Delete(typeLibPath);
            }
        }
    }
}
