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
using System.Configuration.Install;
using System.ComponentModel;

namespace VirtualBoxService.InstallGui {
    
    [RunInstaller(true)]
    public class VirtualBoxInteropAssemblyInstaller : Installer {

        public override void Install(System.Collections.IDictionary stateSaver) {
            try {
                VirtualBoxComInteropAssemblyBuilder iaBuilder = new VirtualBoxComInteropAssemblyBuilder();
                iaBuilder.DeleteTypeLib(Utils.GetApplicationPath());
                iaBuilder.BuildLib(Utils.GetApplicationPath());

                this.Context.LogMessage("Created Interop-Assembly.");
            }
            catch (Exception ex) {
                this.Context.LogMessage("Creation of Interop-Assembly failed. Is Virtualbox installed?");
                throw new InstallException("Creation of Interop-Assembly failed. Is Virtualbox installed?", ex);
            }
            finally {
                base.Install(stateSaver);
            }
        }

        public override void Rollback(System.Collections.IDictionary savedState) {
            try {
                deleteLib();
            }
            finally {
                base.Rollback(savedState);
            }
        }

        public override void Uninstall(System.Collections.IDictionary savedState) {
            try {
                deleteLib();
            }
            finally {
                base.Uninstall(savedState);
            }
        }

        private void deleteLib() {
            VirtualBoxComInteropAssemblyBuilder iaBuilder = new VirtualBoxComInteropAssemblyBuilder();
            iaBuilder.DeleteTypeLib(Utils.GetApplicationPath());

            this.Context.LogMessage("Deleted Interop-Assembly");
        }
    }
}
