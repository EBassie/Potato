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
using System.Linq;
using NUnit.Framework;
using Procon.Core.Events;
using Procon.Core.Shared;
using Procon.Core.Shared.Models;

namespace Procon.Core.Test.CoreInstance {
    [TestFixture]
    public class TestCommandInstanceServiceMergePackage {
        [SetUp]
        public void Initialize() {
            SharedReferences.Setup();
        }

        /// <summary>
        /// Tests that attempting the command without any users in the security controller will
        /// result in insufficient permissions
        /// </summary>
        [Test]
        public void TestResultInsufficientPermissions() {
            InstanceController instance = new InstanceController();

            ICommandResult result = instance.Tunnel(CommandBuilder.InstanceServiceMergePackage("localhost", "id").SetOrigin(CommandOrigin.Remote).SetAuthentication(new CommandAuthenticationModel() {
                Username = "Phogue"
            }));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandResultType.InsufficientPermissions, result.CommandResultType);

            instance.Dispose();
        }

        /// <summary>
        /// Tests that passing in an empty uri will result in an invalid parameter status
        /// </summary>
        [Test]
        public void TestResultInvalidParameterUri() {
            InstanceController instance = new InstanceController();

            ICommandResult result = instance.Tunnel(CommandBuilder.InstanceServiceMergePackage("", "id").SetOrigin(CommandOrigin.Local));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandResultType.InvalidParameter, result.CommandResultType);

            instance.Dispose();
        }

        /// <summary>
        /// Tests that passing in an empty packageId will result in an invalid parameter status
        /// </summary>
        [Test]
        public void TestResultInvalidParameterPackageId() {
            InstanceController instance = new InstanceController();

            ICommandResult result = instance.Tunnel(CommandBuilder.InstanceServiceMergePackage("localhost", "").SetOrigin(CommandOrigin.Local));

            Assert.IsFalse(result.Success);
            Assert.AreEqual(CommandResultType.InvalidParameter, result.CommandResultType);

            instance.Dispose();
        }

        /// <summary>
        /// Tests that with permissions the result will be a success.
        /// </summary>
        [Test]
        public void TestResultSuccess() {
            InstanceController instance = new InstanceController();

            ICommandResult result = instance.Tunnel(CommandBuilder.InstanceServiceMergePackage("localhost", "id").SetOrigin(CommandOrigin.Local));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(CommandResultType.Success, result.CommandResultType);

            instance.Dispose();
        }

        /// <summary>
        /// Tests that a service message is set when successfully executing a merge package command.
        /// </summary>
        [Test]
        public void TestMessageLogged() {
            InstanceController instance = new InstanceController();

            instance.Tunnel(CommandBuilder.InstanceServiceMergePackage("localhost", "id").SetOrigin(CommandOrigin.Local));

            Assert.IsNotNull(instance.ServiceMessage);
            Assert.AreEqual("merge", instance.ServiceMessage.Name);
            Assert.AreEqual("localhost", instance.ServiceMessage.Arguments["uri"]);
            Assert.AreEqual("id", instance.ServiceMessage.Arguments["packageid"]);

            instance.Dispose();
        }

        /// <summary>
        /// Tests that an event is logged for a restart when successfully executing a restart command.
        /// </summary>
        [Test]
        public void TestEventLogged() {
            EventsController events = new EventsController();
            InstanceController instance = new InstanceController {
                Shared = {
                    Events = events
                }
            };

            instance.Tunnel(CommandBuilder.InstanceServiceMergePackage("localhost", "id").SetOrigin(CommandOrigin.Local));

            Assert.IsNotEmpty(events.LoggedEvents);
            Assert.AreEqual("InstanceServiceMergePackage", events.LoggedEvents.First().Name);

            instance.Dispose();
        }
    }
}
