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

	public class Machine : IDisposable
	{
        protected IVirtualMachineProxy _proxy;


		public virtual MachineState State
		{
            get { return _proxy.getState(); }
		}

		public virtual string Name
		{
            get { return _proxy.getName(); }
		}

		public virtual string Id
		{
            get { return _proxy.getId(); }
		}

		public virtual string Description
		{
            get { return _proxy.getDescription(); }
		}

        protected ShutdownMethod _shutdownMethod;
        public virtual VBoxWrapper.ShutdownMethod ShutdownMethod
		{
            get { return _shutdownMethod; }
            set {
                value.VirtualMachineProxy = _proxy;
                _shutdownMethod = value;
            }
		}


        protected internal Machine(IVirtualMachineProxy machineProxy) {
            _proxy = machineProxy;
        }


		public virtual void Start()
		{
			ProgressToken p = _proxy.Start();
            p.Wait();

            if (p.WaitTimedOut) {
                createTimeOutException("Start");
            }
		}

		public virtual void Shutdown()
		{
            ProgressToken p = ShutdownMethod.ShutdownAsync();
            p.Wait();

            if (p.WaitTimedOut) {
                createTimeOutException("Shutdown");
            }
		}

        private void createTimeOutException(string operationName) {
            throw new TimeoutException("Operation \""+ operationName + "\" for Machine \"" + Name + "\" timed out.");
        }


		public virtual ProgressToken StartAsync()
		{
			return _proxy.Start();
		}

		public virtual ProgressToken ShutdownAsync()
		{
            return ShutdownMethod.ShutdownAsync();
		}

        public virtual void Dispose() {
            _proxy.Dispose();
        }
	}
}

