﻿using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Procon.Net.Shared.Sandbox;

namespace Procon.Net.Shared.Test.TestShared.TestSandboxProtocolController {
    [TestFixture]
    public class TestCreate {
        private ProtocolAssemblyMetadata Meta { get; set; }

        [SetUp]
        public void LoadMeta() {
            this.Meta = new ProtocolAssemblyMetadata() {
                Directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory),
                Assembly = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Myrcon.Protocols.Test.dll")),
                Meta = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Myrcon.Protocols.Test.json")),
                Name = "Myrcon.Protocols.Test"
            };

            this.Meta.Load();
        }

        /// <summary>
        /// Tests that loading an assembly, creating a protocol from a type etc will not fail
        /// if correct information is passed through
        /// </summary>
        [Test]
        public void TestSuccess() {
            var controller = new SandboxProtocolController();

            controller.Create(this.Meta.Assembly.FullName, this.Meta.ProtocolTypes.FirstOrDefault(type => type.Type == "MyrconTestProtocol8"));

            Assert.IsNotNull(controller.SandboxedProtocol);
        }

        /// <summary>
        /// Passes in a fake path to load
        /// </summary>
        [Test]
        public void TestFailureWhenAssemblyDoesNotExist() {
            var controller = new SandboxProtocolController();

            controller.Create("Protocol.dll", this.Meta.ProtocolTypes.FirstOrDefault(type => type.Type == "MyrconTestProtocol8"));

            Assert.IsNull(controller.SandboxedProtocol);
        }

        /// <summary>
        /// Passes in an unknown provider
        /// </summary>
        [Test]
        public void TestFailureWhenProtocolProviderDoesNotExist() {
            var controller = new SandboxProtocolController();

            controller.Create(this.Meta.Assembly.FullName, new ProtocolType() {
                Provider = "Fake",
                Type = "MyrconTestProtocol8"
            });

            Assert.IsNull(controller.SandboxedProtocol);
        }

        /// <summary>
        /// Passes in an unknown provider
        /// </summary>
        [Test]
        public void TestFailureWhenProtocolTypeDoesNotExist() {
            var controller = new SandboxProtocolController();

            controller.Create(this.Meta.Assembly.FullName, new ProtocolType() {
                Provider = "Myrcon",
                Type = "Fake"
            });

            Assert.IsNull(controller.SandboxedProtocol);
        }
    }
}
