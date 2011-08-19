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
using Moq.Protected;

namespace VirtualBoxServiceTest
{
    [TestClass()]
    public class ServiceAwareMachineTest {

        Mock<IVirtualMachineProxy> _machineProxyMock = new Mock<IVirtualMachineProxy>();
        Mock<IServiceInfoProvider> _infoProviderMock = new Mock<IServiceInfoProvider>();
        Mock<ShutdownMethod> _shutdownMethodMock = new Mock<ShutdownMethod>();

        [TestMethod()]
        public void DefaultShutdownMethodTest() {
            _infoProviderMock.Setup(x => x.getShutdownMethod()).Returns(_shutdownMethodMock.Object);
            _shutdownMethodMock.SetupSet(x => x.VirtualMachineProxy = _machineProxyMock.Object).Verifiable();
            ServiceAwareMachine m = new ServiceAwareMachine(_machineProxyMock.Object);
            m.ServiceInfoProvider = _infoProviderMock.Object;

            Assert.AreEqual(_shutdownMethodMock.Object, m.ShutdownMethod);
            _shutdownMethodMock.VerifyAll();
        }

        [TestMethod()]
        public void UserDefinedShutdownMethodTest() {
            _infoProviderMock.Setup(x => x.getShutdownMethod()).Returns(_shutdownMethodMock.Object);
            Mock<ShutdownMethod> userDefinedShutdownMethod = new Mock<ShutdownMethod>();
            userDefinedShutdownMethod.SetupSet(x => x.VirtualMachineProxy = _machineProxyMock.Object).Verifiable();
            userDefinedShutdownMethod.Protected().Setup<ProgressToken>("OnShutdownAsync");

            ServiceAwareMachine m = new ServiceAwareMachine(_machineProxyMock.Object);
            m.ServiceInfoProvider = _infoProviderMock.Object;
            m.ShutdownMethod = userDefinedShutdownMethod.Object;
            m.ShutdownAsync();

            Assert.AreEqual(userDefinedShutdownMethod.Object, m.ShutdownMethod);
            userDefinedShutdownMethod.VerifyAll();
        }
    }
}
