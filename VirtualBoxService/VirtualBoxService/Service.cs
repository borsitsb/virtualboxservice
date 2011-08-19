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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.IO;

namespace VirtualBoxService
{

	public partial class Service : ServiceBase
	{
        MachineController _mc;
        WebserviceController _wsc;
        ServiceBasePreshutdownEnabler _preShutdownEnabler;
        TraceSource _log = Logging.ServiceInterfaceLog;
		
		public Service() {
			InitializeComponent();

            configureLogging();
            configurePreshutdown();
		}

        VBoxWrapper.VBox configureVBoxWrapper() {
            VBoxWrapper.VBox vbox = new VBoxWrapper.VBox();
            VBoxWrapper.MachineBuilder machineBuilder = new WrapperExtensions.ServiceMachineBuilder();
            machineBuilder.MachineListBuilder = new VBoxWrapper.COMInterface.ComMachineProxyListBuilder();
            vbox.MachineBuilder = machineBuilder;
            return vbox;
        }

        void configureLogging() {
            EventLogTraceListener eventLogListener = new EventLogTraceListener(this.EventLog);
            eventLogListener.Filter = new EventTypeFilter(SourceLevels.Information | SourceLevels.Critical | SourceLevels.Error | SourceLevels.Warning);

            EventLogTraceListener eventLogListenerError = new EventLogTraceListener(this.EventLog);
            eventLogListenerError.Filter = new EventTypeFilter(SourceLevels.Critical | SourceLevels.Error | SourceLevels.Warning);

            Logging.MachineControllerLog.Listeners.Add(eventLogListener);
            Logging.ServiceInterfaceLog.Listeners.Add(eventLogListenerError);
            VBoxWrapper.Logging.ComInterfaceLog.Listeners.Add(eventLogListenerError);
            VBoxWrapper.Logging.MachineBuildingLog.Listeners.Add(eventLogListenerError);
            VBoxWrapper.Logging.MachineLog.Listeners.Add(eventLogListenerError);

            if (Config.GetConfig().EnableTrace) {
                TextWriterTraceListener textFileListener = new TextWriterListenerWithTime(Path.Combine(Utils.GetApplicationPath(), @"VirtualBoxService.log"), "textFileLog");
                StreamWriter sw = textFileListener.Writer as StreamWriter;
                if (sw != null) sw.AutoFlush = true;

                List<TraceSource> sources = new List<TraceSource> {
                VBoxWrapper.Logging.ComInterfaceLog,
                VBoxWrapper.Logging.MachineBuildingLog,
                VBoxWrapper.Logging.MachineLog,
                Logging.MachineControllerLog,
                Logging.ServiceInterfaceLog,
            };

                foreach (var s in sources) {
                    s.Switch.Level = SourceLevels.All;
                    s.Listeners.Add(textFileListener);
                }
            }
        }

        public class TextWriterListenerWithTime : TextWriterTraceListener {
            public override void WriteLine(string message) {
                base.Write("[" + DateTime.Now.ToString() + "]");
                base.Write(" ");
                base.WriteLine(message);
            }

            public TextWriterListenerWithTime(string file, string name) : base(file, name) { }
        }

        void configurePreshutdown() {
            Version versionWinVistaSp1 = new Version(6, 0, 6001);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= versionWinVistaSp1) {
                _preShutdownEnabler = new ServiceBasePreshutdownEnabler(this);
                _preShutdownEnabler.EnablePreshutdownAccepted();
            }
        }


        [Conditional("DEBUG")]
        void waitForDebugger() {
            this.RequestAdditionalTime(60000);
            while (!Debugger.IsAttached) {
				Thread.Sleep(2000);
			}
        }

        [Conditional("DEBUG")]
        public void Start() {
            OnStart(new string[] {"wolf"});
        }


        protected override void OnStart(string[] args) {
            waitForDebugger();

            startMachineController();

            startWebServiceIfNeeded();
		}

        private void startMachineController() {
            _mc = new DefaultMachineController(configureVBoxWrapper());

            _mc.AnnounceRemainingTime += new EventHandler<MachineController.AnnounceRemainingTimeEventArgs>(requestAdditionalTime);
            try {
                _mc.Start();
            }
            finally {
                _mc.AnnounceRemainingTime -= requestAdditionalTime;
            }
        }

        private void startWebServiceIfNeeded() {
            if (Config.GetConfig().EnableWebService) {
                _wsc = new WebserviceController();
                _wsc.Start();
            }
        }


		protected override void OnStop() {
            stopMachineController();

            stopWebServiceIfNeeded();
		}

        private void stopWebServiceIfNeeded() {
            if (Config.GetConfig().EnableWebService) {
                _wsc.Stop(); 
            }
        }

        private void stopMachineController() {
            _mc.AnnounceRemainingTime += new EventHandler<MachineController.AnnounceRemainingTimeEventArgs>(requestAdditionalTime);
            try {
                _mc.Shutdown();
            }
            finally {
                _mc.AnnounceRemainingTime -= requestAdditionalTime;
            }
        }

        /// <summary>
        /// Called on XP
        /// </summary>
		protected override void OnShutdown() {
            this.Stop();
		}

        /// <summary>
        /// Called if Preshutdownsupport is enabled
        /// </summary>
        /// <param name="command"></param>
		protected override void OnCustomCommand(int command) {
			if (command == 0xF) {
				_log.TraceEvent(TraceEventType.Verbose, (int)Logging.ServiceInterfaceEventIds.PreshutdownCommandReceived, "Preshutdown-Command received.");
                
                _mc.AnnounceRemainingTime += new EventHandler<MachineController.AnnounceRemainingTimeEventArgs>(requestPreshutdownTime);
                try {
                    this.Stop();
                }
                finally {
                    _mc.AnnounceRemainingTime -= requestPreshutdownTime;
                }
			}
		}

        private void requestAdditionalTime(object sender, MachineController.AnnounceRemainingTimeEventArgs args) {
            this.RequestAdditionalTime((int)args.RemainingTime.TotalMilliseconds);
        }

        private void requestPreshutdownTime(object sender, MachineController.AnnounceRemainingTimeEventArgs args) {
            _preShutdownEnabler.RequestAdditionalTime((int)args.RemainingTime.TotalMilliseconds);
        }
	}
}
