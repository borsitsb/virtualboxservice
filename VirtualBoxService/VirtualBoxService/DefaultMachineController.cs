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

namespace VirtualBoxService
{
	using System;
	using System.Collections.Generic;
    using System.Linq;
	using System.Text;
	using VBoxWrapper;
    using MachineFilterDSL;
    using System.Diagnostics;

	public sealed class DefaultMachineController : MachineController
	{
        private class MachineOperationToken {
            private List<MachineOperation> _successfullyOperatedMachines = new List<MachineOperation>();

            public void SuccessfullyOperatedMachine(MachineOperation m) {
                _successfullyOperatedMachines.Add(m);
            }

            public MachineOperation GetMostRecent() {
                return _successfullyOperatedMachines.LastOrDefault();
            }

            public override string ToString() {
                string machines = String.Join("\n",  _successfullyOperatedMachines.Select(x => String.Format("{0} on {1} succeeded in {2}", x.OperationName, x.Machine.Name, x.TimeForOperation.ToString())));
                return machines;
            }
        }

        private class MachineOperation {
            public Machine Machine {get; private set;}
            public ProgressToken Token { get; private set; }
            public TimeSpan TimeForOperation {
                get {
                    return _timer.Elapsed;
                }
            }
            public string OperationName { get; private set; }
            public Logging.MachineControllerEventIds EventId { get; set; }
            private Stopwatch _timer;
            private TraceSource _log = Logging.MachineControllerLog;

            
            public MachineOperation(Machine m, ProgressToken t, string operationName, Logging.MachineControllerEventIds eventId) {
                Machine = m;
                Token = t;
                OperationName = operationName;
                EventId = eventId;
                _timer = new Stopwatch();
            }


            public void Execute(MachineOperationToken successfulOperatedMachines, Logging.MachineControllerEventIds onFailEventId) {
                _timer.Restart();
                try {
                    Token.Wait();
                    if (Token.WaitTimedOut) {
                        _log.TraceEvent(TraceEventType.Warning, (int)EventId, "{0} on machine \"{1}\" timed out.", OperationName, Machine.Name);
                    }
                    else {
                        successfulOperatedMachines.SuccessfullyOperatedMachine(this);
                    }
                }
                catch (Exception ex) {
                    _log.TraceEvent(TraceEventType.Error, (int)onFailEventId, "{0} on machine \"{1}\" failed because: {2}", OperationName, Machine.Name, ex.ToString());
                }
                finally {
                    _timer.Stop();
                }
            }
        }

        private class MachineProgressTokenTuple {
            public Machine Machine;
            public ProgressToken Token;

            public MachineProgressTokenTuple(ProgressToken t, Machine m) {
                Machine = m;
                Token = t;
            }
        }


        private VBoxWrapper.VBox _vbox;
        private TraceSource _log = Logging.MachineControllerLog;


        public DefaultMachineController(VBox vbox) {
            _vbox = vbox;
        }


        public override void Start() {
            _vbox.RefreshMachines();

            try {
                MachineOperationToken t = new MachineOperationToken();
                startSessionSavedMachines(t);
                startOtherMachines(t);
                _log.TraceEvent(TraceEventType.Information, (int)Logging.MachineControllerEventIds.MachineStartup, "The following machines were started:\n{0}", t);
            }
            finally {
                _vbox.DisposeMachines();
            }
        }

        public override void Shutdown() {
            _vbox.RefreshMachines();

            try {
                MachineOperationToken t = new MachineOperationToken();
                IEnumerable<MachineProgressTokenTuple> pendingShutdowns = beginShutdownOfACPIShutdownMachines(t);
                shutdownSaveStateShutdownMachines(t);
                awaitAcpiShutdowns(pendingShutdowns, t);
                hardOffRemaining(t);
                _log.TraceEvent(TraceEventType.Information, (int)Logging.MachineControllerEventIds.MachineStartup, "The following machines were shutdown:\n{0}", t);
            }
            finally {
                _vbox.DisposeMachines();
            }
        }


        void startSessionSavedMachines(MachineOperationToken token) {
            foreach (Machine m in _vbox.Machines.filterByState(MachineState.SessionSaved)) {
                operatMachine(() => m.StartAsync(), m, "Start from saved session", token, Logging.MachineControllerEventIds.MachineStartup, Logging.MachineControllerEventIds.MachineStartupFailed);
            }
        }

        void startOtherMachines(MachineOperationToken token) {
            foreach (Machine m in _vbox.Machines.filterAutobooting().filterByState(MachineState.Off)) {
                operatMachine(() => m.StartAsync(), m, "Boot", token, Logging.MachineControllerEventIds.MachineStartup, Logging.MachineControllerEventIds.MachineStartup);
            }
        }
                

        IList<MachineProgressTokenTuple> beginShutdownOfACPIShutdownMachines(MachineOperationToken token) {
            IEnumerable<Machine> machinesToShutdown = _vbox.Machines.filterAutobooting().filterByShutdownType<ACPIShutdownMethod>().filterByState(MachineState.Running);
            List<MachineProgressTokenTuple> acpiShutdownTokens = new List<MachineProgressTokenTuple>(machinesToShutdown.Count());
            int i = 0;
            foreach (Machine m in machinesToShutdown) {
                acpiShutdownTokens.Add(new MachineProgressTokenTuple(m.ShutdownAsync(),m));
                i++;
            }
            return acpiShutdownTokens;
        }

        void shutdownSaveStateShutdownMachines(MachineOperationToken token) {
            IEnumerable<Machine> saveStateMachines = _vbox.Machines.filterAutobooting().filterByShutdownType<SaveStateMethod>().filterByState(MachineState.Running);
            foreach (Machine m in saveStateMachines) {
                try {
                    _log.TraceEvent(TraceEventType.Verbose, (int)Logging.MachineControllerEventIds.BeginMachineSaveState, "Beginning savestate of machine {0}", m.Name);

                    operatMachine(() => m.ShutdownAsync(), m, "Save State", token, Logging.MachineControllerEventIds.MachineShutdown, Logging.MachineControllerEventIds.MachineShutdownFailed);
                    
                    _log.TraceEvent(TraceEventType.Verbose, (int)Logging.MachineControllerEventIds.EndMachineSaveState, "Finished savestate of machine {0}", m.Name);
                }
                catch (Exception ex) {
                    _log.TraceEvent(TraceEventType.Warning, (int)Logging.MachineControllerEventIds.MachineShutdownFailed, "State of Machine {0} could not be saved", m.Name);
                    _log.TraceData(TraceEventType.Warning, (int)Logging.MachineControllerEventIds.MachineShutdownFailed, ex);
                }
            }
        }

        /// <summary>
        /// Await ACPI-Shutdowns according to specified timeouts
        /// </summary>
        void awaitAcpiShutdowns(IEnumerable<MachineProgressTokenTuple> shutdownsInProgress, MachineOperationToken successfulOperations) {
            IEnumerable<MachineProgressTokenTuple> unfinishedAcpiShutdowns = shutdownsInProgress.Where(x => !x.Token.Finished);
            if (unfinishedAcpiShutdowns.Count() > 0) {
                foreach (var activeShutdown in unfinishedAcpiShutdowns) {
                    estimateAndAnnounceTimeForProgressToken(activeShutdown.Token, successfulOperations);
                    MachineOperation op = new MachineOperation(activeShutdown.Machine, activeShutdown.Token, "Await ACPI-Shutdown", Logging.MachineControllerEventIds.MachineShutdown);
                    op.Execute(successfulOperations, Logging.MachineControllerEventIds.MachineShutdownFailed);
                }

                foreach (var activeShutdown in unfinishedAcpiShutdowns) {
                    _log.TraceEvent(TraceEventType.Warning, (int)Logging.MachineControllerEventIds.MachineShutdownFailed, "Machine {0} took to long to properly shutdown after ACPI-Command", activeShutdown.Machine.Name);
                }
            }
        }

        /// <summary>
        /// Hard off remaining machines.
        /// </summary>
        void hardOffRemaining(MachineOperationToken successfulOperations) {
            IEnumerable<Machine> remainingMachines = _vbox.Machines.filterAutobooting().filterByState(MachineState.Running);
            foreach (Machine m in remainingMachines) {
                if (!(m.ShutdownMethod is HardOffMethod)) {
                    m.ShutdownMethod = new HardOffMethod();
                    _log.TraceEvent(TraceEventType.Warning, (int)Logging.MachineControllerEventIds.MachineShutdown, "Machine {0} has to be powered off hard, because the original shutdown-method failed.", m.Name);
                }

                operatMachine(() => m.ShutdownAsync(), m, "Hard off", successfulOperations, Logging.MachineControllerEventIds.MachineShutdown, Logging.MachineControllerEventIds.MachineShutdownFailed);
            }
        }


        void operatMachine(Func<ProgressToken> operation, Machine m, string operationDescription, MachineOperationToken successfulOperations, Logging.MachineControllerEventIds eventId, Logging.MachineControllerEventIds failEventId) {
            try {
                ProgressToken t = operation.Invoke();
                estimateAndAnnounceTimeForProgressToken(t, successfulOperations);
                MachineOperation op = new MachineOperation(m, t, operationDescription, eventId);
                op.Execute(successfulOperations, Logging.MachineControllerEventIds.MachineStartupFailed);
            }
            catch (Exception ex) {
                _log.TraceEvent(TraceEventType.Error, (int)failEventId, "{0} on machine {1} resulted in an error: {2}", operationDescription, m.Name, ex.ToString());
                //no rethrow, because we do not want to interrupt the other processes
            }
        }

        void estimateAndAnnounceTimeForProgressToken(ProgressToken t, MachineOperationToken successfulOperations) {
            try {
                TimeSpan remainingTime = t.EstimateRemainingTime();
                _log.TraceEvent(TraceEventType.Verbose, (int)Logging.MachineControllerEventIds.AnnounceRemainingTime, "Remaining time for current operation: {0}", remainingTime.ToString());
                announceEstimatedTime(remainingTime);
            }
            catch (NotSupportedException) {
                announceTimeOfLastOperationOrDefault(successfulOperations);
            }
            catch (Exception e) {
                _log.TraceEvent(TraceEventType.Warning, (int)Logging.MachineControllerEventIds.TimeEstimationProblem, "Could not estimate remaining time for Operation: {0}", e.ToString());
                announceTimeOfLastOperationOrDefault(successfulOperations);
            }
        }

        void announceTimeOfLastOperationOrDefault(MachineOperationToken successfulOperations) {
            TimeSpan time = TimeSpan.FromSeconds(15); //Default
            if (successfulOperations.GetMostRecent() != null) {
                time = successfulOperations.GetMostRecent().TimeForOperation;
            }

            announceEstimatedTime(time); //In case, the progresstoken does not support estimation of remaining time, use duration of last operation or a default as reference
        }

        void announceEstimatedTime(TimeSpan t) {
            try {
                RaiseAnnounceRemainingTime(new AnnounceRemainingTimeEventArgs(t));
            }
            catch (Exception e) {
                _log.TraceEvent(TraceEventType.Warning, (int)Logging.MachineControllerEventIds.TimeEstimationProblem, "Could not announce remaining time: {0}", e.ToString());
            }
        }
	}
}

namespace VirtualBoxService.MachineFilterDSL {
    using System.Collections.Generic;
    using VBoxWrapper;
    using System.Linq;
    
    public static class MachineFilterDSLClass {
        public static IEnumerable<Machine> filterByState(this IEnumerable<Machine> machines, MachineState state) {
            return machines.Where(x => x.State == state);
        }

        public static IEnumerable<Machine> filterByShutdownType<T>(this IEnumerable<Machine> machines) where T : ShutdownMethod {
            return machines.Where(x => x.ShutdownMethod is T);
        }

        public static IEnumerable<Machine> filterAutobooting(this IEnumerable<Machine> machines) {
            return machines.OfType<WrapperExtensions.ServiceAwareMachine>().Where(x => x.AutoBoot);
        }
    }
}

