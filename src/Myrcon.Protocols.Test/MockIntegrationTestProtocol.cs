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
using System;
using System.Collections.Generic;
using Potato.Net.Shared;
using Potato.Net.Shared.Actions;

namespace Myrcon.Protocols.Test {
    [ProtocolDeclaration(Type = "MockIntegrationTestProtocol", Name = "Mock Integration Test Protocol", Provider = "Myrcon")]
    public class MockIntegrationTestProtocol : IProtocol {

        public IClient WaitingClient { get; set; }
        public IProtocolState WaitingState { get; set; }
        public IProtocolSetup WaitingOptions { get; set; }
        public IProtocolType WaitingProtocolType { get; set; }

        public Func<IProtocolSetup, IProtocolSetupResult> OnSetupHandler { get; set; }
        public Func<INetworkAction, List<IPacket>> OnActionHandler { get; set; }
        public Func<IPacketWrapper, IPacket> OnSendHandler { get; set; }
        public Action OnAttemptConnectionHandler { get; set; }
        public Action OnShutdownHandler { get; set; }
        public Action OnSynchronizeHandler { get; set; }

        // IProtocol Implementation

        public IClient Client { get { return this.WaitingClient;  } }
        public IProtocolState State { get { return this.WaitingState; } }
        public IProtocolSetup Options { get { return this.WaitingOptions; } }
        public IProtocolType ProtocolType { get { return this.WaitingProtocolType; } }

        public string ProtocolsConfigDirectory { get; set; }
        public event Action<IProtocol, IProtocolEventArgs> ProtocolEvent;
        public event Action<IProtocol, IClientEventArgs> ClientEvent;

        public IProtocolSetupResult Setup(IProtocolSetup setup) {
            return this.OnSetupHandler != null ? this.OnSetupHandler(setup) : null;
        }

        public List<IPacket> Action(INetworkAction action) {
            return this.OnActionHandler != null ? this.OnActionHandler(action) : null;
        }

        public IPacket Send(IPacketWrapper packet) {
            return this.OnSendHandler != null ? this.OnSendHandler(packet) : null;
        }

        public void AttemptConnection() {
            if (this.OnAttemptConnectionHandler != null) {
                this.OnAttemptConnectionHandler();
            }
        }

        public void Shutdown() {
            if (this.OnShutdownHandler != null) {
                this.OnShutdownHandler();
            }
        }

        public void Synchronize() {
            if (this.OnSynchronizeHandler != null) {
                this.OnSynchronizeHandler();
            }
        }

        /// <summary>
        /// Mocks a call to the protocol event
        /// </summary>
        public void MockProtocolEvent(IProtocolEventArgs args) {
            if (this.ProtocolEvent != null) {
                this.ProtocolEvent(this, args);
            }
        }

        /// <summary>
        /// Mocks a call to the client event
        /// </summary>
        public void MockClientEvent(IClientEventArgs args) {
            if (this.ClientEvent != null) {
                this.ClientEvent(this, args);
            }
        }
    }
}
