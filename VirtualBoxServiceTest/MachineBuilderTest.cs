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

using VBoxWrapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Moq;

namespace VirtualBoxServiceTest
{
    [TestClass()]
    public class MachineBuilderTest {

        MachineBuilder _builder;
        Mock<MachineProxyListBuilder> _proxyListBuilderMock;


        [TestInitialize()]
        public void setup() {
            _builder = new MachineBuilder();
            _proxyListBuilderMock = new Mock<MachineProxyListBuilder>();
        }


        [TestMethod()]
        public void buildMachineTest() {
            Mock<IVirtualMachineProxy> _proxyMock = new Mock<IVirtualMachineProxy>();
            
            Machine m = _builder.buildMachine(_proxyMock.Object);


            m.StartAsync();
            _proxyMock.Verify(x => x.Start());

            m.ShutdownAsync();
            _proxyMock.Verify(x => x.Shutdown(ShutdownType.SaveState));
        }

        [TestMethod()]
        public void buildMachineListTest() {
            List<Mock<IVirtualMachineProxy>> proxyMocks = new List<Mock<IVirtualMachineProxy>>();
            Mock<IVirtualMachineProxy> p1 = new Mock<IVirtualMachineProxy>();
            Mock<IVirtualMachineProxy> p2 = new Mock<IVirtualMachineProxy>();
            proxyMocks.Add(p1);
            proxyMocks.Add(p2);

            p1.Setup(x => x.getName()).Returns("proxy1");
            p2.Setup(x => x.getName()).Returns("proxy2");

            _proxyListBuilderMock.Setup(x => x.buildMachineProxyList()).Returns(proxyMocks.Select(x => x.Object));
            _builder.MachineListBuilder = _proxyListBuilderMock.Object;

            List<Machine> result = _builder.buildMachineList().ToList();

            Assert.IsTrue(result.Any(x => x.Name.Equals("proxy1")));
            Assert.IsTrue(result.Any(x => x.Name.Equals("proxy2")));
        }
    }
}
