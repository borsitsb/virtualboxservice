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

namespace VBoxWrapper
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public class ACPIShutdownMethod : ShutdownMethod
	{
		public virtual int ACPIShutdownTimeoutMilliSeconds
		{
			get;
			set;
		}



        protected override ProgressToken OnShutdownAsync() {
            return new ACPITimeoutProgressToken(VirtualMachineProxy.Shutdown(ShutdownType.ACPI), ACPIShutdownTimeoutMilliSeconds);
        }


        private class ACPITimeoutProgressToken : ProgressToken {
            private ProgressToken _innerToken;
            private int _timeoutMillis;
            private DateTime _shutdownInitiatedAt = DateTime.Now;

            public override bool Finished {
                get { return _innerToken.Finished; }
            }


            public ACPITimeoutProgressToken(ProgressToken innerToken, int timeoutMillis) {
                _innerToken = innerToken;
                _timeoutMillis = timeoutMillis;
            }


            public override void Wait() {
                WaitTimedOut = false;
                double remainingMillis = getRemainingTime().TotalMilliseconds;
                if (remainingMillis > 0) {
                    Wait((int)remainingMillis);
                }
                else {
                    WaitTimedOut = true;
                }
            }

            public override void Cancel() {
                _innerToken.Cancel();
            }

            public override void Wait(int timeout) {
                _innerToken.Wait(timeout);
                WaitTimedOut = _innerToken.WaitTimedOut;
            }

            public override TimeSpan EstimateRemainingTime() {
                return getRemainingTime();
            }


            private TimeSpan getAlreadyWaited() {
                TimeSpan alreadyWaited = DateTime.Now - _shutdownInitiatedAt;
                return alreadyWaited;
            }

            private TimeSpan getRemainingTime() {
                return TimeSpan.FromMilliseconds(_timeoutMillis - getAlreadyWaited().TotalMilliseconds);
            }
        }
    }
}

