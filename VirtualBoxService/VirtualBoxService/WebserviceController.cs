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
using System.Diagnostics;
using VBoxWrapper;
using System.IO;
using Microsoft.Win32;

namespace VirtualBoxService {
    class WebserviceController {
        Process _wsProcess;

        public void Start() {
            IVirtualBoxServiceConfig config = Config.GetConfig();

            _wsProcess = new Process();
            _wsProcess.StartInfo.Arguments = String.Format("-H {0}", config.WebServiceInterface);
            if (config.EnableTrace) {
                _wsProcess.StartInfo.Arguments += String.Format(" --logfile \"{0}\"", Path.Combine(Utils.GetApplicationPath(), "VirtualBoxWebService.log"));
            }

            _wsProcess.StartInfo.FileName = Path.Combine(getVboxDir(), "vboxwebsrv.exe");

            _wsProcess.Start();
        }

        string getVboxDir() {
            return VBoxWrapper.COMInterface.VBoxComUtils.GetVirtualBoxDir();
        }

        public void Stop() {
            try {
                _wsProcess.Kill();
                _wsProcess.Close();
            }
            catch (Exception) {
                
            }
        }
    }
}
