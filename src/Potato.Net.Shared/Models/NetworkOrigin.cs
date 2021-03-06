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

namespace Potato.Net.Shared.Models {
    /// <summary>
    /// The location of the event or data on the network
    /// </summary>
    [Serializable]
    public enum NetworkOrigin {
        /// <summary>
        /// No origin specified
        /// </summary>
        None,
        /// <summary>
        /// Message was generated by the server (Potato and the players in the server didn't say it)
        /// </summary>
        Server,
        /// <summary>
        /// Message was sent from a player in game
        /// </summary>
        Player,
        /// <summary>
        /// Sent from Potato and reflected back after confirmation (when available) of it being sent 
        /// </summary>
        Reflected
    }
}
