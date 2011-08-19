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

using VBoxWrapper.COMInterface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VirtualBox;
using VBoxWrapper;
using Moq;
using System.Threading;

namespace VirtualBoxServiceTest
{
    /// <summary>
    /// <remarks>
    /// For this test to work, make sure the COM-Interop-Assemblies are not embedded, because otherwise MoQ will fail on recognizing types.
    /// More Information: http://msdn.microsoft.com/en-us/library/xwzy44e4.aspx
    /// </remarks>
    /// </summary>
    [TestClass()]
    public class COMMachineProxyTest {

        private Mock<IMachine> _mockedComMachine;



        [TestInitialize()]
        public void setup() {
            var mock = new Mock<IMachine>();
            mock.SetupGet(x => x.Name).Returns("testMachine");
            mock.SetupGet(x => x.Description).Returns("testDescription");
            mock.SetupGet(x => x.State).Returns(VirtualBox.MachineState.MachineState_PoweredOff);
            mock.SetupGet(x => x.Id).Returns("idOfTestMachine");
            _mockedComMachine = mock;
        }
        
        [TestMethod()]
        public void PropertyDelegationTest() {
            ComMachineProxy machineProxy = new ComMachineProxy(_mockedComMachine.Object);
            Assert.AreEqual("testMachine", machineProxy.getName());
            Assert.AreEqual("testDescription", machineProxy.getDescription());
            Assert.AreEqual("idOfTestMachine", machineProxy.getId());
        }

        [TestMethod()]
        public void MachineStateMappingTest() {
            ComMachineProxy machineProxy = new ComMachineProxy(_mockedComMachine.Object);
            
            Assert.AreEqual(VBoxWrapper.MachineState.Off, machineProxy.getState());
            
            _mockedComMachine.SetupGet(x => x.State).Returns(VirtualBox.MachineState.MachineState_Running);
            Assert.AreEqual(VBoxWrapper.MachineState.Running, machineProxy.getState());

            _mockedComMachine.SetupGet(x => x.State).Returns(VirtualBox.MachineState.MachineState_Paused);
            Assert.AreEqual(VBoxWrapper.MachineState.Running, machineProxy.getState());

            _mockedComMachine.SetupGet(x => x.State).Returns(VirtualBox.MachineState.MachineState_Saved);
            Assert.AreEqual(VBoxWrapper.MachineState.SessionSaved, machineProxy.getState());

            _mockedComMachine.SetupGet(x => x.State).Returns(VirtualBox.MachineState.MachineState_Saving);
            Assert.AreEqual(VBoxWrapper.MachineState.InTransition, machineProxy.getState());
        }

        [TestMethod()]
        public void StartMachineTest() {
            Mock<IProgress> progressMock = new Mock<IProgress>();
            int i = 0;
            progressMock.SetupGet(x => x.Completed).Returns(i).Callback(() => i = 1); //return 0 once, then 1
            
            _mockedComMachine.Setup(x => x.LaunchVMProcess(It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>())).Returns(progressMock.Object);

            
            ComMachineProxy machineProxy = new ComMachineProxy(_mockedComMachine.Object);

            ProgressToken t = machineProxy.Start();

            Assert.IsInstanceOfType(t, typeof(ComMachineProxy.IProgressProgressToken));

            t.Wait();
            progressMock.Verify(x => x.WaitForCompletion(It.IsAny<int>()));
        }

        [TestMethod()]
        public void StartMachineCancelTest() {
            Mock<IProgress> progressMock = new Mock<IProgress>();
            progressMock.SetupGet(x => x.Cancelable).Returns(1);
            _mockedComMachine.Setup(x => x.LaunchVMProcess(It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>())).Returns(progressMock.Object);


            ComMachineProxy machineProxy = new ComMachineProxy(_mockedComMachine.Object);
            ProgressToken t = machineProxy.Start();

            Assert.IsInstanceOfType(t, typeof(ComMachineProxy.IProgressProgressToken));

            t.Cancel();
            progressMock.Verify(x => x.Cancel());
        }

        //[TestMethod()] --> Can not test this, because there is no proper way to inject Mock-Session-Object
        public void ShutdownMachineTest() {
            ComMachineProxy machineProxy = new ComMachineProxy(_mockedComMachine.Object);
            
            machineProxy.Shutdown(ShutdownType.ACPI);

            _mockedComMachine.Verify(x => x.LockMachine(It.IsAny<Session>(), It.IsAny<LockType>()));
        }

        [TestMethod()]
        public void EstimateTimeProgressCompletedTest() {
            Mock<IProgress> progressMock = new Mock<IProgress>();
            int completed = 0;
            progressMock.SetupGet(x => x.Completed).Returns(() => completed).Callback(() => completed = 1);
            _mockedComMachine.Setup(x => x.LaunchVMProcess(It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>())).Returns(progressMock.Object);

            ComMachineProxy machineProxy = new ComMachineProxy(_mockedComMachine.Object);
            ProgressToken t = machineProxy.Start();
            TimeSpan test = t.EstimateRemainingTime();

            Assert.AreEqual(TimeSpan.Zero, test);
        }

        [TestMethod()]
        public void EstimateTimeProgressPercentReachedTest() {
            Mock<IProgress> progressMock = new Mock<IProgress>();
            int percent = 0;
            progressMock.SetupGet(x => x.Percent).Returns(() => (uint)percent).Callback(() => percent++);
            progressMock.SetupGet(x => x.TimeRemaining).Returns(10);
            _mockedComMachine.Setup(x => x.LaunchVMProcess(It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>())).Returns(progressMock.Object);

            ComMachineProxy machineProxy = new ComMachineProxy(_mockedComMachine.Object);
            ProgressToken t = machineProxy.Start();
            TimeSpan test = t.EstimateRemainingTime();

            Assert.AreEqual(10, test.TotalSeconds);
        }

        [TestMethod()]
        public void EstimateTimeProgressTimeoutTest() {
            Mock<IProgress> progressMock = new Mock<IProgress>();
            _mockedComMachine.Setup(x => x.LaunchVMProcess(It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>())).Returns(progressMock.Object);

            ComMachineProxy machineProxy = new ComMachineProxy(_mockedComMachine.Object);
            ProgressToken t = machineProxy.Start();

            try {
                TimeSpan test = t.EstimateRemainingTime();
                Assert.Fail("Timeout exception did not occur!");
            }
            catch (TimeoutException) {
                
            }
        }
    }
}
