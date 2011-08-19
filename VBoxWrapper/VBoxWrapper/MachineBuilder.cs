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

	public class MachineBuilder
	{
        public virtual MachineProxyListBuilder MachineListBuilder {	get; set; }

		public virtual IEnumerable<Machine> buildMachineList()
		{
            IEnumerable<IVirtualMachineProxy> proxies = MachineListBuilder.buildMachineProxyList();

            foreach (IVirtualMachineProxy proxy in proxies) {
                yield return buildMachine(proxy);
            }
		}

		public virtual Machine buildMachine(IVirtualMachineProxy proxy)
		{
            Machine m = new Machine(proxy);
            m.ShutdownMethod = ShutdownMethodFactory.createShutdownMethod();
            return m;
		}

	}
}

