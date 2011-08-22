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
    public class ConfigInstaller : Installer {

        public override void Install(System.Collections.IDictionary stateSaver) {
            try {
                Config.GetConfig().Initialize();
                this.Context.LogMessage("Config initialized");
            }
            finally {
                base.Install(stateSaver);
            }
        }

        public override void Rollback(System.Collections.IDictionary savedState) {
            try {
                clearConfig();
            }
            finally {
                base.Rollback(savedState);
            }
        }

        public override void Uninstall(System.Collections.IDictionary savedState) {
            try {
                clearConfig();
            }
            finally {
               base.Uninstall(savedState);
            }
        }

        private void clearConfig() {
            Config.GetConfig().Clear();
            this.Context.LogMessage("Config cleared");
        }

        
    }
}
