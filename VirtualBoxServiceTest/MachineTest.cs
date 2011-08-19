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
using Moq;
using Moq.Protected;

namespace VirtualBoxServiceTest
{
    [TestClass()]
    public class MachineTest {

        Mock<ShutdownMethod> _mockShutdownMethod = new Mock<ShutdownMethod>();
        Mock<IVirtualMachineProxy> _mockMachineProxy = new Mock<IVirtualMachineProxy>();

        [TestMethod()]
        public void ShutdownMethodTest() {
            _mockShutdownMethod.SetupSet(x => x.VirtualMachineProxy = _mockMachineProxy.Object).Verifiable();
            Machine m = new Machine(_mockMachineProxy.Object); // TODO: Passenden Wert initialisieren
            m.ShutdownMethod = _mockShutdownMethod.Object;

            _mockShutdownMethod.VerifyAll();
        }

        [TestMethod()]
        public void SynchronousShutdownTest() {
            Machine m = new Machine(_mockMachineProxy.Object);
            m.ShutdownMethod = _mockShutdownMethod.Object;
            Mock<ProgressToken> pt = createGoodProgressToken();
            _mockShutdownMethod.Protected().Setup<ProgressToken>("OnShutdownAsync").Returns(pt.Object);

            m.Shutdown();
            pt.Verify(x => x.Wait());
        }

        [TestMethod()]
        public void SynchronousTimedoutShutdownTest() {
            Machine m = new Machine(_mockMachineProxy.Object);
            m.ShutdownMethod = _mockShutdownMethod.Object;
            Mock<ProgressToken> pt = createTimedoutProgressToken();
            _mockShutdownMethod.Protected().Setup<ProgressToken>("OnShutdownAsync").Returns(pt.Object);

            try {
                m.Shutdown();
                Assert.Fail("Shutdown() did not throw TimedOutException");
            }
            catch (TimeoutException) {
                pt.Verify(x => x.Wait());
            }
        }

        [TestMethod()]
        public void SynchronousFailedShutdownTest() {
            Machine m = new Machine(_mockMachineProxy.Object);
            m.ShutdownMethod = _mockShutdownMethod.Object;
            Mock<ProgressToken> pt = createFailedProgressToken();
            _mockShutdownMethod.Protected().Setup<ProgressToken>("OnShutdownAsync").Returns(pt.Object);

            try {
                m.Shutdown();
                Assert.Fail("Shutdown() did not throw TimedOutException");
            }
            catch (Exception ex) {
                pt.Verify(x => x.Wait());
                Assert.AreEqual("test", ex.Message);
            }
        }

        Mock<ProgressToken> createGoodProgressToken() {
            Mock<ProgressToken> mockedProgressToken = new Mock<ProgressToken>();
            mockedProgressToken.Setup(x => x.WaitTimedOut).Returns(false);
            mockedProgressToken.Setup(x => x.Finished).Returns(true);
            return mockedProgressToken;
        }

        Mock<ProgressToken> createTimedoutProgressToken() {
            Mock<ProgressToken> mockedProgressToken = new Mock<ProgressToken>();
            mockedProgressToken.Setup(x => x.WaitTimedOut).Returns(true);
            mockedProgressToken.Setup(x => x.Finished).Returns(false);
            return mockedProgressToken;
        }

        Mock<ProgressToken> createFailedProgressToken() {
            Mock<ProgressToken> mpt = new Mock<ProgressToken>();
            mpt.Setup(x => x.Failure).Returns(new InvalidProgramException("test"));
            mpt.Setup(x => x.Wait()).Throws(new InvalidProgramException("test"));
            return mpt;
        }
    }
}
