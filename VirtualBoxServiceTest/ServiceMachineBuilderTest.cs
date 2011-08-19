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

using VirtualBoxService.WrapperExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VBoxWrapper;
using Moq;

namespace VirtualBoxServiceTest
{
    [TestClass()]
    public class ServiceMachineBuilderTest {

        Mock<IVirtualMachineProxy> _mockMachineProxy = new Mock<IVirtualMachineProxy>();
        Mock<MachineProxyListBuilder> _mockMachineProxyListBuilder = new Mock<MachineProxyListBuilder>();

        [TestMethod()]
        public void buildMachineTest() {
            _mockMachineProxy.Setup(x => x.Start()).Returns(new Mock<ProgressToken>().Object).Verifiable();

            ServiceMachineBuilder smb = new ServiceMachineBuilder();
            smb.MachineListBuilder = _mockMachineProxyListBuilder.Object;

            Machine m = smb.buildMachine(_mockMachineProxy.Object);
            ProgressToken t = m.StartAsync();
            _mockMachineProxy.VerifyAll();

            Assert.IsInstanceOfType(((ServiceAwareMachine)m).ServiceInfoProvider, typeof(DescriptionBasedInfoProvider));
        }
    }
}
