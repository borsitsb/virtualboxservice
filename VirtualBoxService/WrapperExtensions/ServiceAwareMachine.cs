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

namespace VirtualBoxService.WrapperExtensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using VBoxWrapper;

	public class ServiceAwareMachine : Machine
	{
		public virtual bool AutoBoot
		{
            get {
                return ServiceInfoProvider.getAutoBoot();
            }
		}

		public override ShutdownMethod ShutdownMethod
		{
            get {
                if (_shutdownMethod == null) {
                    ShutdownMethod m = ServiceInfoProvider.getShutdownMethod();
                    m.VirtualMachineProxy = this._proxy;
                    return m;
                }
                else {
                    return _shutdownMethod;
                }
            }
            set {
                if (value != null) {
                    value.VirtualMachineProxy = this._proxy;
                    _shutdownMethod = value;
                }
                else {
                    _shutdownMethod = null;
                }
            }
		}

        public virtual IServiceInfoProvider ServiceInfoProvider {
            get;
            set;
        }


        internal ServiceAwareMachine(IVirtualMachineProxy proxy) : base(proxy) { }

	}
}

