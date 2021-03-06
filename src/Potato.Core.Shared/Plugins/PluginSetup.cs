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

namespace Potato.Core.Shared.Plugins {
    /// <summary>
    /// Handles passing simple variables from Potato AppDomain to the plugin AppDomain
    /// </summary>
    [Serializable]
    public class PluginSetup : IPluginSetup {
        public String ConnectionGuid { get; set; }
        public String ConfigDirectoryPath { get; set; }
        public String LogDirectoryPath { get; set; }
    }
}
