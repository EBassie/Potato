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
using System.Linq;
using System.Reflection;

namespace Potato.Service.Shared {
    /// <summary>
    /// The proxy to be loaded in the service appdomain
    /// </summary>
    public sealed class ServiceLoaderProxy : MarshalByRefObject, IServiceLoaderProxy {
        /// <summary>
        /// The proxy to the Potato.Core.Instance object.
        /// </summary>
        public IService Service { get; set; }

        public override object InitializeLifetimeService() {
            return null;
        }

        /// <summary>
        /// Creates the Potato instance in the Potato instance appdomain
        /// </summary>
        public void Create() {
            this.Service = (IService)Activator.CreateInstanceFrom(
                Defines.SearchPaths(Defines.PotatoCoreDll, new List<String> {
                    Defines.BaseDirectory.FullName,
                    Defines.PackageMyrconPotatoCoreLibNet40.FullName,
                    Defines.PackageMyrconPotatoSharedLibNet40.FullName
                }).First(),
                Defines.TypePotatoCorePotatoController,
                false,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
                null,
                null,
                null,
                null
            ).Unwrap();
        }

        public void Start() {
            if (this.Service != null) this.Service.Start();
        }

        public void WriteConfig() {
            if (this.Service != null) this.Service.WriteConfig();
        }

        public void Dispose() {
            if (this.Service != null) this.Service.Dispose();
        }

        public void ParseCommandLineArguments(List<string> arguments) {
            if (this.Service != null) this.Service.ParseCommandLineArguments(arguments);
        }

        public ServiceMessage PollService() {
            ServiceMessage message = null;

            if (this.Service != null) {
                var polled = this.Service.PollService();

                // Clone the message so we have no proxy to the other side.
                message = new ServiceMessage() {
                    Name = polled.Name,
                    Arguments = polled.Arguments,
                    Stamp = polled.Stamp
                };
            }

            return message;
        }

        public ServiceMessage ExecuteMessage(ServiceMessage message) {
            ServiceMessage result = null;

            if (this.Service != null) {
                var polled = this.Service.ExecuteMessage(message);

                // Clone the message so we have no proxy to the other side.
                result = new ServiceMessage() {
                    Name = polled.Name,
                    Arguments = polled.Arguments,
                    Stamp = polled.Stamp
                };
            }

            return result;
        }
    }
}