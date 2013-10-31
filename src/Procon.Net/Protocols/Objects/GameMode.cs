﻿using System;

namespace Procon.Net.Protocols.Objects {

    [Serializable]
    public sealed class GameMode : NetworkObject {

        /// <summary>
        /// This game mode's name as it is used via Rcon.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// This game mode's human-readable name.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// This game mode's number of teams, not including spectator/neutral.
        /// </summary>
        public int TeamCount { get; set; }

        /// <summary>
        /// List of groups to default a player to when moving them and such.
        /// </summary>
        public GroupingList DefaultGroups { get; set; }

        public GameMode() {
            this.Name = String.Empty;
            this.FriendlyName = String.Empty;
            this.TeamCount = 0;
        }
    }
}
