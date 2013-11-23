﻿using System;
using System.Net;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Procon.Net {

    [Serializable]
    public abstract class Packet {

        /// <summary>
        /// When this packet was created
        /// </summary>
        public DateTime Stamp { get; set; }

        /// <summary>
        /// The origin of the packet. This is useful when the server sends back responses to packets, we
        /// can say the packet originiated from the client and this is the response.
        /// </summary>
        public PacketOrigin Origin { get; set; }

        /// <summary>
        /// If this is a response or not to a previous packet.
        /// </summary>
        public PacketType Type { get; set; }

        /// <summary>
        /// The sequence id for this command/event
        /// </summary>
        public int? RequestId { get; set; }

        [XmlIgnore, JsonIgnore]
        public IPEndPoint RemoteEndPoint { get; set; }

        protected Packet() {

        }

        // Used if we'll be using EncodePacket to send to the server.
        protected Packet(PacketOrigin origin, PacketType type) {
            this.Origin = origin;
            this.Type = type;
            this.RequestId = null;
            this.Stamp = DateTime.Now;
        }

        protected virtual void NullPacket() {
            this.Origin = PacketOrigin.Client;
            this.Type = PacketType.Request;
            this.RequestId = null;
        }

        public virtual string ToDebugString() {
            return base.ToString();
        }
    }
}
