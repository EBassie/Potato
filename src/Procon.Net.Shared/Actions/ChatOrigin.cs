﻿using System;

namespace Procon.Net.Shared.Actions {
    [Serializable]
    public enum ChatOrigin {
        None,
        /// <summary>
        /// Message was generated by the server (procon and the players in the server didn't say it)
        /// </summary>
        Server,
        /// <summary>
        /// Message was sent from a player in game
        /// </summary>
        Player,
        /// <summary>
        /// Sent from procon and reflected back after confirmation (when available) of it being sent 
        /// </summary>
        Reflected
    }
}