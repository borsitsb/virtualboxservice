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
    using System.Diagnostics;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Reflection;

    public class ComMachineProxy : IVirtualMachineProxy, IDisposable {
        private comVB.IMachine _comMachine;


        public ComMachineProxy(dynamic comMachine) {
            _comMachine = comMachine;
        }


        public virtual string getDescription() {
            return _comMachine.Description;
        }

        public virtual string getId() {
            return _comMachine.Id;
        }

        public virtual string getName() {
            return _comMachine.Name;
        }

        public virtual MachineState getState() {
            if (_reversedComStateMapping.ContainsKey(_comMachine.State)) {
                return _reversedComStateMapping[_comMachine.State][0]; //assume we have only ONE Wrapper-State for MANY Com-States
            }
            else {
                return MachineState.InTransition;
            }
        }


        public virtual ProgressToken Start() {
            Trace.Assert(getState() != VBoxWrapper.MachineState.Running);

            comVB.Session session = new comVB.Session();
            comVB.IProgress progress = _comMachine.LaunchVMProcess(session, "headless", "");

            return new IProgressProgressToken(progress);
        }

        public virtual ProgressToken Shutdown(ShutdownType type) {
            Trace.Assert(getState() == VBoxWrapper.MachineState.Running);

            comVB.Session session = acquireClientLock();

            switch (type) {
                case ShutdownType.ACPI:
                    return acpiShutdown(session);
                case ShutdownType.HardOff:
                    return hardOff(session);
                case ShutdownType.SaveState:
                    return saveState(session);
                default:
                    throw new InvalidProgramException("ShutdownType unknown -> Normally never reached...");
            }
        }

        private ProgressToken acpiShutdown(comVB.Session session) {
            session.Console.PowerButton();

            return new MachineEventProgressToken(session.Console, x => comVB.MachineState.MachineState_PoweredOff.Equals(x.State));
        }

        private ProgressToken hardOff(comVB.Session session) {
            comVB.IProgress progress = session.Console.PowerDown();

            return new IProgressProgressToken(progress);
        }

        private ProgressToken saveState(comVB.Session session) {
            comVB.IProgress progress = session.Console.SaveState();

            return new IProgressProgressToken(progress);
        }


        private comVB.Session acquireClientLock() {
            comVB.Session session = new comVB.Session(); //possible bug: Class can only be created, if running in according platform (Virtualbox running x64 => Service has to run also as x64!)
            _comMachine.LockMachine(session, comVB.LockType.LockType_Shared);
            return session;
        }


        private bool disposed = false;
        public void Dispose() {
            if (!disposed) {
                Marshal.FinalReleaseComObject(_comMachine); //Should be called to release COM-References early, but is not neccessary (RCW takes care of this)
                disposed = true;
            }
        }


        static Dictionary<MachineState, comVB.MachineState[]> COM_STATE_TO_WRAPPER_STATE_MAPPING = new Dictionary<MachineState, comVB.MachineState[]> {
					{ MachineState.Off, new comVB.MachineState[] { 
                        comVB.MachineState.MachineState_PoweredOff,
                        comVB.MachineState.MachineState_Aborted}},
					{ MachineState.Running, new comVB.MachineState[] {
                        comVB.MachineState.MachineState_Running,
                        comVB.MachineState.MachineState_Paused}},
                    { MachineState.SessionSaved, new comVB.MachineState[] {
                        comVB.MachineState.MachineState_Saved}}
        };

        static Dictionary<comVB.MachineState, MachineState[]> _reversedComStateMapping = reverseDictionary<MachineState, comVB.MachineState>(COM_STATE_TO_WRAPPER_STATE_MAPPING);

        static Dictionary<TValue, TKey[]> reverseDictionary<TKey, TValue>(Dictionary<TKey, TValue[]> input) {
            var newDict = new Dictionary<TValue, TKey[]>();

            var newKeys = input.Values.SelectMany(v => v).Distinct();
            foreach (var nk in newKeys) {
                var vals = input.Keys.Where(k => input[k].Contains(nk));
                newDict.Add(nk, vals.ToArray());
            }

            return newDict;
        }


        public class IProgressProgressToken : ProgressToken {
            const int DEFAULT_TIMEOUT_MILLISECONDS = 10000;
            const int SPIN_WAIT_CYCLE_DURATION_MILLISECONDS = 500;
            private comVB.IProgress _progress;
            private TraceSource _log = Logging.ComInterfaceLog;

            public override bool Finished {
                get { 
                    return Convert.ToBoolean(_progress.Completed);
                }
            }

            public override Exception Failure {
                get {
                    if (_progress.Completed == 1 && _progress.ResultCode != 0) {
                        string description;
                        _progress.ErrorInfo.GetDescription(out description);
                        return new COMException(description);
                    }
                    else return null;
                }
            }


            public IProgressProgressToken(comVB.IProgress progressToken) {
                _progress = progressToken;
            }


            public override void Wait() {
                Wait(DEFAULT_TIMEOUT_MILLISECONDS);
            }

            public override void Wait(int timeout) {
                _progress.WaitForCompletion(timeout);
                if (_progress.Completed != 1) {
                    WaitTimedOut = true;
                }
                throwPossibleException();
            }

            public override void Cancel() {
                if (_progress.Cancelable == 1) {
                    _progress.Cancel();
                }
            }

            public override TimeSpan EstimateRemainingTime() {
                try {
                    waitForProgressAt(5); //needed for Virtualbox to calculate a meaningful remaining time
                }
                catch (TimeoutException ex) {
                    throw new TimeoutException("Could not estimate remaining time within timeout", ex);
                }

                if (_progress.Completed != 1) {
                    TimeSpan t = TimeSpan.FromSeconds(_progress.TimeRemaining);
                    
                    //sometimes IProgress.TimeRemaining returns a negative value (wtf?!)
                    if (t.CompareTo(TimeSpan.Zero) < 0) {
                        return t.Negate();
                    }
                    else {
                        return t;
                    }
                }
                else return TimeSpan.Zero;
            }

            private void waitForProgressAt(int percent) {
                int timeout = DEFAULT_TIMEOUT_MILLISECONDS;
                while (_progress.Completed != 1 && _progress.Percent < percent && timeout >= 0) {
                    Thread.Sleep(SPIN_WAIT_CYCLE_DURATION_MILLISECONDS);
                    timeout -= SPIN_WAIT_CYCLE_DURATION_MILLISECONDS;
                }

                if (timeout < 0) {
                    throw new TimeoutException("Could not wait for progress to reach " + percent + "% before timeout.");
                }
            }
        }

        public class MachineEventProgressToken : ProgressToken {

            const int DEFAULT_TIMEOUT_MILLISECONDS = 10000;

            [ComVisible(true)] //this one took me hours... (needed for dynamic invocation of RegisterListener())
            public class MachineStateChangedEventListener : comVB.IEventListener {
                Action _callback;
                Func<comVB.IStateChangedEvent, bool> _eventSelector;
                ManualResetEvent _waitHandle;

                public WaitHandle WaitHandle {
                    get { return _waitHandle; }
                }

                public static comVB.VBoxEventType[] Events = createEventList();

                private static comVB.VBoxEventType[] createEventList() {
                    comVB.VBoxEventType[] list = new comVB.VBoxEventType[1];
                    list[0] = comVB.VBoxEventType.VBoxEventType_OnStateChanged;
                    return list;
                }

                public MachineStateChangedEventListener(Action callback, Func<comVB.IStateChangedEvent, bool> eventSelector) : this(eventSelector) {
                    _callback = callback;
                }

                public MachineStateChangedEventListener(Func<comVB.IStateChangedEvent, bool> eventSelector) {
                    _eventSelector = eventSelector;
                    _waitHandle = new ManualResetEvent(false);
                }


                public void HandleEvent(comVB.IEvent aEvent) {
                    if (aEvent.Type == comVB.VBoxEventType.VBoxEventType_OnStateChanged) {

                        comVB.IStateChangedEvent machineStateChanged = (comVB.IStateChangedEvent)aEvent;
                        if (_eventSelector.Invoke(machineStateChanged)) {

                            if (_callback != null) {
                                _callback.Invoke();
                            }

                            _waitHandle.Set();
                        }
                    }
                }
            }

            private bool _finished;
            private comVB.IConsole _console;
            private MachineStateChangedEventListener _listener;
            private bool _canceled = false;
            private TraceSource _log = Logging.ComInterfaceLog;

            public override bool Finished {
                get { return _finished; }
            }



            public MachineEventProgressToken(comVB.IConsole console, Func<comVB.IStateChangedEvent, bool> eventSelector) {
                _console = console;
                _listener = new MachineStateChangedEventListener(() => onListenerFired(),  eventSelector);
                registerListener();
            }

            

            public override void Cancel() {
                //Impossible to cancel because we can just wait for the event to come true...
                unregisterListener();
                _canceled = true;
            }

            public override void Wait() {
                Wait(DEFAULT_TIMEOUT_MILLISECONDS);
            }

            public override void Wait(int timeout) {
                if (!_canceled) {
                    WaitTimedOut = !_listener.WaitHandle.WaitOne(timeout);
                }
                else {
                    throw new InvalidOperationException("This Token was already canceled!");
                }
            }

            public override TimeSpan EstimateRemainingTime() {
                throw new NotSupportedException("Impossible to estimate remaining time.");
            }



            void registerListener() {
                dynamic eventSource = _console.EventSource; //use dynamic here because of interface change between v4.0 and v4.1
                
                if (getVersionAsInt() >= 410) {
                    Array x = (Array)MachineStateChangedEventListener.Events;
                    eventSource.RegisterListener(_listener, x, 1);
                }
                else {
                    eventSource.RegisterListener(_listener, MachineStateChangedEventListener.Events, 1);
                }
                
                _log.TraceEvent(TraceEventType.Verbose, (int)Logging.ComInterfaceErrorIds.MachineEventListenerRegistered, "MachineStateChangedEventListener registered at COM-Server.");
            }

            void unregisterListener() {
                _console.EventSource.UnregisterListener(_listener);
                _log.TraceEvent(TraceEventType.Verbose, (int)Logging.ComInterfaceErrorIds.MachineEventListenerDeregistered, "MachineStateChangedEventListener unregistered from COM-Server.");
            }

            void onListenerFired() {
                _finished = true;
                unregisterListener();
            }

            int getVersionAsInt() {
                string version = _console.Machine.Parent.Version;
                string[] versionSplit = version.Split('.');
                int versionInt = Convert.ToInt32(versionSplit[0]) * 100 + Convert.ToInt32(versionSplit[1]) * 10 + Convert.ToInt32(versionSplit[2]);
                return versionInt;
            }

            /* -> CCW should prevent stale reference in EventDispatcher of COM-Server
            ~MachineEventProgressToken() {
                unregisterListener();
            }*/
        }
    }
}

