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
using System.IO;

namespace VirtualBoxServiceTest
{
    [TestClass()]
    public class ParsedDescriptionTest {

        [TestMethod()]
        public void SimpleGoodDescription() {
            string d = "Foo, Bar, Baz! \n" +
                       "<!VirtualboxService--{\"Autostart\":\"true\", \"ShutdownType\":\"ACPIShutdown\", \"ACPIShutdownTimeout\":\"100\"}--/VirtualboxService>";

            ParsedDescription p = ParsedDescription.Parse(d);

            Assert.AreEqual(true, p.Autostart);
            Assert.AreEqual(ParsedDescription.ShutdownTypeEnum.ACPIShutdown, p.ShutdownType);
            Assert.AreEqual(100, p.ACPIShutdownTimeout);
        }

        [TestMethod()]
        public void IncompleteGoodDescription() {
            string d = "Foo, Bar, Baz! \n" +
                       "<!VirtualboxService--{\"Autostart\":\"true\", \"ShutdownType\":\"SaveState\"}--/VirtualboxService>";

            ParsedDescription p = ParsedDescription.Parse(d);

            Assert.AreEqual(true, p.Autostart);
            Assert.AreEqual(ParsedDescription.ShutdownTypeEnum.SaveState, p.ShutdownType);
            Assert.AreEqual(false, p.ACPIShutdownTimeout.HasValue);
        }

        [TestMethod()]
        public void WrongDelimiters() {
            string d = "Foo, Bar, Baz! \n" +
                       "<!VirtualboxSer--{'Autostart':'true', 'ShutdownType':'ACPIShutdown', 'ACPIShutdownTimeout':'100'}--/VirtualboxService>";

            ParsedDescription p = ParsedDescription.Parse(d);

            Assert.AreEqual(false, p.Autostart);
        }
    }
}
