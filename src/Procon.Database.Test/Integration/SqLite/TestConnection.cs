﻿#region Copyright
// Copyright 2014 Myrcon Pty. Ltd.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using NUnit.Framework;
using Procon.Database.Drivers;

namespace Procon.Database.Test.Integration.SqLite {
    [TestFixture]
    public class TestConnection {
        /// <summary>
        /// Tests we can load up an in-memory SQLite database
        /// </summary>
        [Test]
        public void TestBasicConnection() {
            IDriver driver = new SqLiteDriver() {
                Settings = new DriverSettings() {
                    Memory = true
                }
            };

            Assert.IsTrue(driver.Connect());

            driver.Close();
        }
    }
}
