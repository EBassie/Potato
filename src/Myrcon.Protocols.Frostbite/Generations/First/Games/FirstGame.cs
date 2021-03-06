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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Potato.Net.Shared;
using Potato.Net.Shared.Actions;
using Potato.Net.Shared.Models;

namespace Myrcon.Protocols.Frostbite.Generations.First.Games {
    /// <summary>
    /// The base battlefield game that handles methods used by the majority of implementations.
    /// Some protocol types may require further specialization, but the most common function
    /// will always be placed here.
    /// </summary>
    public class FirstGame : FrostbiteGame {

        public void ServerOnLevelLoadedDispatchHandler(IPacketWrapper request, IPacketWrapper response) {
            if (request.Packet.Words.Count >= 5) {
                int currentRound = 0, totalRounds = 0;

                if (int.TryParse(request.Packet.Words[3], out currentRound) == true && int.TryParse(request.Packet.Words[4], out totalRounds) == true) {
                    this.UpdateSettingsMap(request.Packet.Words[1], request.Packet.Words[2]);
                    this.UpdateSettingsRound(currentRound, totalRounds);
                }
            }
        }

        /// <summary>
        /// Handles the player.onKill event sent from the server.
        /// This method holds for Battlefield 3/4, but may need an override in bfbc2.
        /// </summary>
        public override void PlayerOnKillDispatchHandler(IPacketWrapper request, IPacketWrapper response) {

            if (request.Packet.Words.Count >= 5) {
                bool headshot = false;

                if (bool.TryParse(request.Packet.Words[4], out headshot) == true) {
                    ItemModel item = this.State.Items.Select(i => i.Value).FirstOrDefault(i => i.Name == Regex.Replace(request.Packet.Words[3], @"[^\w\/_-]+", "")) ?? new ItemModel() { Name = request.Packet.Words[3] };

                    var killer = this.State.Players.Select(p => p.Value).FirstOrDefault(p => p.Name == request.Packet.Words[1]);
                    var victim = this.State.Players.Select(p => p.Value).FirstOrDefault(p => p.Name == request.Packet.Words[2]);

                    if (killer != null && victim != null) {
                        // Assign the item to the player, overwriting everything else attached to this killer.
                        killer.Inventory.Now.Items.Clear();
                        killer.Inventory.Now.Items.Add(item);

                        victim.Deaths++;

                        // If this wasn't an inside job.
                        if (killer.Uid != victim.Uid) {
                            killer.Kills++;
                        }

                        var difference = new ProtocolStateDifference() {
                            Modified = {
                                Players = new ConcurrentDictionary<String, PlayerModel>()
                            }
                        };

                        difference.Modified.Players.AddOrUpdate(killer.Uid, id => killer, (id, model) => killer);
                        difference.Modified.Players.AddOrUpdate(victim.Uid, id => victim, (id, model) => victim);

                        this.ApplyProtocolStateDifference(difference);

                        // We've updated the state, now fetch the players from the state with all of the statistics information attached.
                        PlayerModel stateKiller = null;
                        PlayerModel stateVictim = null;

                        this.State.Players.TryGetValue(killer.Uid, out stateKiller);
                        this.State.Players.TryGetValue(victim.Uid, out stateVictim);

                        this.OnProtocolEvent(
                            ProtocolEventType.ProtocolPlayerKill,
                            difference,
                            new ProtocolEventData() {
                                Kills = new List<KillModel>() {
                                    new KillModel() {
                                        Scope = {
                                            Players = new List<PlayerModel>() {
                                                stateVictim
                                            },
                                            Items = new List<ItemModel>() {
                                                item
                                            },
                                            HumanHitLocations = new List<HumanHitLocation>() {
                                                headshot == true ? FrostbiteGame.Headshot : FrostbiteGame.Bodyshot
                                            }
                                        },
                                        Now = {
                                            Players = new List<PlayerModel>() {
                                                stateKiller
                                            }
                                        }
                                    }
                                }
                            }
                        );
                    }
                }
            }
        }

        protected override List<IPacketWrapper> ActionMap(INetworkAction action) {
            List<IPacketWrapper> wrappers = new List<IPacketWrapper>();

            // If it's centered around a list of maps (adding, removing etc)
            if (action.Now.Maps != null && action.Now.Maps.Count > 0) {
                foreach (MapModel map in action.Now.Maps) {
                    var closureMap = map;

                    if (action.ActionType == NetworkActionType.NetworkMapAppend) {
                        // mapList.add <map: string> <gamemode: string> <rounds: integer> [index: integer]
                        wrappers.Add(this.CreatePacket("mapList.add \"{0}\" \"{1}\" {2}", map.Name, closureMap.GameMode.Name, map.Rounds));

                        wrappers.Add(this.CreatePacket("mapList.save"));

                        wrappers.Add(this.CreatePacket("mapList.list"));
                    }
                    // Added by Imisnew2 - You should check this phogue!
                    else if (action.ActionType == NetworkActionType.NetworkMapChangeMode) {
                        if (map.GameMode != null) {
                            wrappers.Add(this.CreatePacket("admin.setPlaylist \"{0}\"", map.GameMode.Name));
                        }
                    }
                    else if (action.ActionType == NetworkActionType.NetworkMapInsert) {
                        // mapList.add <map: string> <gamemode: string> <rounds: integer> [index: integer]
                        wrappers.Add(this.CreatePacket("mapList.add \"{0}\" \"{1}\" {2} {3}", map.Name, closureMap.GameMode.Name, map.Rounds, map.Index));

                        wrappers.Add(this.CreatePacket("mapList.save"));

                        wrappers.Add(this.CreatePacket("mapList.list"));
                    }
                    else if (action.ActionType == NetworkActionType.NetworkMapRemove) {
                        var matchingMaps = this.State.Maps.Where(m => m.Value.Name == closureMap.Name).OrderByDescending(m => m.Value.Index);

                        wrappers.AddRange(matchingMaps.Select(match => this.CreatePacket("mapList.remove {0}", match.Value.Index)));

                        wrappers.Add(this.CreatePacket("mapList.save"));

                        wrappers.Add(this.CreatePacket("mapList.list"));
                    }
                    else if (action.ActionType == NetworkActionType.NetworkMapRemoveIndex) {
                        wrappers.Add(this.CreatePacket("mapList.remove {0}", map.Index));

                        wrappers.Add(this.CreatePacket("mapList.list"));
                    }
                    else if (action.ActionType == NetworkActionType.NetworkMapNextIndex) {
                        wrappers.Add(this.CreatePacket("mapList.setNextMapIndex {0}", map.Index));
                    }
                }
            }
            else {
                // The action does not need a map to function
                if (action.ActionType == NetworkActionType.NetworkMapRestart || action.ActionType == NetworkActionType.NetworkMapRoundRestart) {
                    wrappers.Add(this.CreatePacket("mapList.restartRound"));
                }
                else if (action.ActionType == NetworkActionType.NetworkMapNext || action.ActionType == NetworkActionType.NetworkMapRoundNext) {
                    wrappers.Add(this.CreatePacket("mapList.runNextRound"));
                }
                else if (action.ActionType == NetworkActionType.NetworkMapClear) {
                    wrappers.Add(this.CreatePacket("mapList.clear"));

                    wrappers.Add(this.CreatePacket("mapList.save"));
                }
            }

            return wrappers;
        }
    }
}
