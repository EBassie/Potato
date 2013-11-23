﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace Procon.Net.Protocols.Frostbite {
    public class FrostbiteClient : TcpClient<FrostbitePacket> {

        /// <summary>
        /// A list of packets currently sent to the server and awaiting a response
        /// </summary>
        protected Dictionary<int?, Packet> OutgoingPackets;

        /// <summary>
        /// A queue of packets to send to the server (waiting until the outgoing packets list is clear)
        /// </summary>
        protected Queue<Packet> QueuedPackets;

        /// <summary>
        /// Lock for processing new queue items
        /// </summary>
        protected readonly Object QueueUnqueuePacketLock = new Object();

        public FrostbiteClient(string hostname, ushort port) : base(hostname, port) {

            this.OutgoingPackets = new Dictionary<int?, Packet>();
            this.QueuedPackets = new Queue<Packet>();

            this.PacketSerializer = new FrostbitePacketSerializer();
        }

        /// <summary>
        /// Handles the queueing of outgoing packets so our communication with the Frostbite server is always atomic.
        /// 
        /// While the protocol is asynchronous we force synchronicity so our program flow and plugin extensions
        /// can have consistent and predictable execution of commands (they want to yell something to a player, then 
        /// kick the player)
        /// </summary>
        /// <param name="isSending">True if we are looking to send a packet</param>
        /// <param name="packet">The packet we are sending or have recieved</param>
        /// <param name="nextPacket">The next packet to be sent</param>
        /// <returns></returns>
        private bool QueueUnqueuePacket(bool isSending, Packet packet, out Packet nextPacket) {
            nextPacket = null;
            bool response = false;

            lock (this.QueueUnqueuePacketLock) {
                if (isSending == true) {
                    // If we have something that has been sent and is awaiting a response
                    if (this.OutgoingPackets.Count > 0) {
                        // Add the packet to our queue to be sent at a later time.
                        this.QueuedPackets.Enqueue(packet);

                        response = true;
                    }
                    // else - response = false
                }
                else {
                    // Else it's being called from recv and cpPacket holds the processed RequestPacket.

                    // Remove the packet 
                    if (packet != null && packet.RequestId != null) {
                        if (this.OutgoingPackets.ContainsKey(packet.RequestId) == true) {
                            this.OutgoingPackets.Remove(packet.RequestId);
                        }
                    }

                    if (this.QueuedPackets.Count > 0) {
                        nextPacket = this.QueuedPackets.Dequeue();

                        response = true;
                    }
                    else {
                        response = false;
                    }

                }
            }

            return response;
        }

        /// <summary>
        /// Validates that packets are not 'lost' after being sent. If this is the case then the connection is shutdown
        /// to then be rebooted at a later time.
        /// 
        /// If a packet exists in our outgoing "SentPackets"
        /// </summary>
        protected void RestartConnectionOnQueueFailure() {
            if (this.OutgoingPackets.Any(outgoingPacket => outgoingPacket.Value.Stamp < DateTime.Now.AddMinutes(-2)) == true) {
                this.OutgoingPackets.Clear();
                this.QueuedPackets.Clear();

                this.Shutdown(new Exception("Failed to hear response to packet within two minutes, forced shutdown."));
            }
        }

        public FrostbitePacket GetRequestPacket(FrostbitePacket recievedPacket) {

            Packet requestPacket = null;

            if (recievedPacket.RequestId != null && this.OutgoingPackets.ContainsKey(recievedPacket.RequestId) == true) {
                requestPacket = this.OutgoingPackets[recievedPacket.RequestId];
            }

            return requestPacket as FrostbitePacket;
        }

        protected override void OnPacketReceived(FrostbitePacket packet) {
            base.OnPacketReceived(packet);

            // Respond with "OK" to all server events.
            if (packet.Origin == PacketOrigin.Server && packet.Type == PacketType.Request) {
                base.Send(new FrostbitePacket(PacketOrigin.Server, PacketType.Response, packet.RequestId, FrostbitePacket.StringResponseOkay));
            }

            // Pop the next packet if a packet is waiting to be sent.
            Packet nextPacket = null;
            if (this.QueueUnqueuePacket(false, packet, out nextPacket) == true) {
                this.Send(nextPacket);
            }

            // Shutdown if we're just waiting for a response to an old packet.
            this.RestartConnectionOnQueueFailure();
        }

        public override void Send(Packet packet) {

            if (packet.RequestId == null) {
                packet.RequestId = this.AcquireSequenceNumber;
            }

            // QueueUnqueuePacket

            if (packet.Origin == PacketOrigin.Server && packet.Type == PacketType.Response) {
                // I don't think this will ever be encountered since OnPacketReceived calls the base.Send.
                base.Send(packet);
            }
            else {
                // Null return because we're not popping a packet, just checking to see if this one needs to be queued.
                Packet nullPacket = null;

                if (this.QueueUnqueuePacket(true, packet, out nullPacket) == false) {
                    // No need to queue, queue is empty.  Send away..
                    base.Send(packet);
                }

                // Shutdown if we're just waiting for a response to an old packet.
                this.RestartConnectionOnQueueFailure();
            }
        }

        protected override bool BeforePacketSend(Packet packet) {

            if (packet.RequestId != null && packet.Origin == PacketOrigin.Client && packet.Type == PacketType.Request && this.OutgoingPackets.ContainsKey(packet.RequestId) == false) {
                this.OutgoingPackets.Add(packet.RequestId, packet);
            }

            return base.BeforePacketSend(packet);
        }

        protected override void ShutdownConnection() {
            base.ShutdownConnection();

            this.OutgoingPackets.Clear();
            this.QueuedPackets.Clear();
        }
    }
}
