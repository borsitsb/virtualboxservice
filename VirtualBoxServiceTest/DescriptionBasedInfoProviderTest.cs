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
    public class DescriptionBasedInfoProviderTest {

        Mock<IVirtualMachineProxy> _mockedVirtualMachineProxy = new Mock<IVirtualMachineProxy>();

        [TestInitialize()]
        public void Setup() {
            _mockedVirtualMachineProxy.Setup(x => x.getDescription()).Returns("Wolf<!VirtualboxService--{\"Autostart\":\"true\", \"ShutdownType\":\"ACPIShutdown\", \"ACPIShutdownTimeout\":\"100\"}--/VirtualboxService>");
        }

        [TestMethod()]
        public void GetMachineProxyDescriptionFilterTest() {
            DescriptionBasedInfoProvider target = new DescriptionBasedInfoProvider(_mockedVirtualMachineProxy.Object);

            IVirtualMachineProxy filteredProxy = target.GetMachineProxyDescriptionFilter();

            Assert.AreEqual("Wolf", filteredProxy.getDescription());
        }

        [TestMethod()]
        public void getACPIShutdownTimeoutMilliSecondsTest() {
            DescriptionBasedInfoProvider ip = new DescriptionBasedInfoProvider(_mockedVirtualMachineProxy.Object);

            Assert.AreEqual(100, ip.getACPIShutdownTimeoutMilliSeconds());
        }

        [TestMethod()]
        public void getACPIShutdownTimeoutMilliSecondsDefaultTest() {
            _mockedVirtualMachineProxy.Setup(x => x.getDescription()).Returns("Wolf<!VirtualboxService--{\"Autostart\":\"true\", \"ShutdownType\":\"ACPIShutdown\"}--/VirtualboxService>");
            DescriptionBasedInfoProvider ip = new DescriptionBasedInfoProvider(_mockedVirtualMachineProxy.Object);

            Assert.AreEqual(10000, ip.getACPIShutdownTimeoutMilliSeconds());
        }

        
        [TestMethod()]
        public void getAutoBootTest() {
            DescriptionBasedInfoProvider ip = new DescriptionBasedInfoProvider(_mockedVirtualMachineProxy.Object);

            Assert.AreEqual(true, ip.getAutoBoot());
        }

        [TestMethod()]
        public void getAutoBootDefaultTest() {
            _mockedVirtualMachineProxy.Setup(x => x.getDescription()).Returns("Wolf<!VirtualboxService--{\"ShutdownType\":\"ACPIShutdown\"}--/VirtualboxService>");
            DescriptionBasedInfoProvider ip = new DescriptionBasedInfoProvider(_mockedVirtualMachineProxy.Object);

            Assert.AreEqual(false, ip.getAutoBoot());
        }

        [TestMethod()]
        public void getRealDescriptionTest() {
            _mockedVirtualMachineProxy.Setup(x => x.getDescription()).Returns("Wolf Huhn\n <!VirtualboxService--{\"Autostart\":\"true\", \"ShutdownType\":\"ACPIShutdown\", \"ACPIShutdownTimeout\":\"100\"}--/VirtualboxService>");
            DescriptionBasedInfoProvider ip = new DescriptionBasedInfoProvider(_mockedVirtualMachineProxy.Object);

            Assert.AreEqual("Wolf Huhn\n ", ip.getRealDescription());
        }

        [TestMethod()]
        public void getShutdownMethodTest() {
            DescriptionBasedInfoProvider ip = new DescriptionBasedInfoProvider(_mockedVirtualMachineProxy.Object);

            Assert.IsInstanceOfType(ip.getShutdownMethod(), typeof(ACPIShutdownMethod));


            _mockedVirtualMachineProxy.Setup(x => x.getDescription()).Returns("Wolf Huhn\n <!VirtualboxService--{\"Autostart\":\"true\", \"ACPIShutdownTimeout\":\"100\"}--/VirtualboxService>");

            Assert.IsInstanceOfType(ip.getShutdownMethod(), typeof(SaveStateMethod));


            _mockedVirtualMachineProxy.Setup(x => x.getDescription()).Returns(String.Empty);

            Assert.IsInstanceOfType(ip.getShutdownMethod(), typeof(SaveStateMethod));
        }
    }
}
