﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Procon.Core.Utils;

namespace Procon.Core {
    using Procon.Net.Protocols;
    using Procon.Net.Protocols.Daemon;

    [Serializable]
    public class Command : CommandAttribute {

        /// <summary>
        /// The scope that this commands execution should be limited to.
        /// </summary>
        public CommandScope Scope { get; set; }

        /// <summary>
        /// Where the command came from
        /// </summary>
        public CommandOrigin Origin { get; set; }

        /// <summary>
        /// The final result of this command.
        /// </summary>
        public CommandResultArgs Result { get; set; }

        /// <summary>
        /// The original request from a remote source.
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public DaemonPacket RemoteRequest { get; set; }

        /// <summary>
        /// The raw parameters to be passed into the executable command.
        /// </summary>
        public List<CommandParameter> Parameters { get; set; } 

        #region Executing User - Should we move this to it's own object?

        /// <summary>
        /// The username of the initiator
        /// </summary>
        public String Username { get; set; }

        /// <summary>
        /// The password of the user executing the command. Used to authenticate
        /// remote requests.
        /// </summary>
        /// <remarks>Will change to much more secure password authentication</remarks>
        public String PasswordPlainText { get; set; }

        /// <summary>
        /// The game type of the initiators player Uid
        /// </summary>
        public String GameType { get; set; }

        /// <summary>
        /// The uid of the player initiating the command
        /// </summary>
        public String Uid { get; set; }

        #endregion

        public Command() {
            this.Username = null;
            this.GameType = CommonGameType.None;
            this.Uid = null;

            this.Scope = new CommandScope();
        }

        /// <summary>
        /// Allows for essentially cloning a command, but then allows inline overrides of the 
        /// attributes.
        /// </summary>
        /// <param name="command"></param>
        public Command(Command command) {
            this.CommandType = command.CommandType;
            this.Name = command.Name;
            this.Username = command.Username;
            this.GameType = command.GameType;
            this.Uid = command.Uid;
            this.Origin = command.Origin;
            this.PasswordPlainText = command.PasswordPlainText;
            this.Scope = command.Scope;
        }

        /// <summary>
        /// The config only requires the name and parameters, everything else is ignored. We could just
        /// return the results of ToXElement() but we neaten it up a little bit just so the config
        /// isn't bloated with useless information.
        /// </summary>
        /// <returns></returns>
        public XElement ToConfigCommand() {
            XElement result = this.ToXElement();

            if (result != null) {
                XElement scope = result.Element("Scope");
                XElement origin = result.Element("Origin");
                XElement gameType = result.Element("GameType");

                if (scope != null) scope.Remove();
                if (origin != null) origin.Remove();
                if (gameType != null) gameType.Remove();
            }
            
            return result;
        }
    }
}