﻿// Copyright 2011 Geoffrey 'Phogue' Green
// 
// http://www.phogue.net
//  
// This file is part of Procon 2.
// 
// Procon 2 is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Procon 2 is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Procon 2.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Procon.Net.Protocols.CallOfDuty {
    using Procon.Net.Utils.HTTP;
    using Procon.Net.Protocols.CallOfDuty.Objects;

    public class CallOfDutyLogfile {

        private static readonly Regex Entry = new Regex("^(?<time>[0-9]+) (?<text>(say|sayteam|k).+)[\r]*?$", RegexOptions.IgnoreCase | RegexOptions.Compiled); // | RegexOptions.Multiline 

        private static readonly Dictionary<Regex, Type> EntryTypes = new Dictionary<Regex, Type>() {
            { new Regex(@"^.*?(?<Command>say|sayteam);(?<GUID>[0-9]+?);(?<ID>[0-9]+?);(?<Name>.+?);\cU?(?<Text>.+)[\r]*?$", RegexOptions.IgnoreCase | RegexOptions.Compiled), typeof(CallOfDutyChat) },
            { new Regex(@"^.*?(?<Command>K);(?<V_GUID>[0-9]*?);(?<V_ID>[0-9]*?);(?<V_TeamName>[a-zA-Z]*);(?<V_Name>.*);(?<K_GUID>[0-9]*?);(?<K_ID>[0-9]*?);(?<K_TeamName>[a-zA-Z]*);(?<K_Name>.*);(?<Weapon>[a-zA-Z0-9_]*);(?<Damage>[0-9]*?);(?<DamageType>[a-zA-Z_]*);(?<HitLocation>[a-zA-Z_]*)[\r]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled), typeof(CallOfDutyKill) }
        };

        #region Events

        public delegate void EmptyParameterHandler(CallOfDutyLogfile sender);
        public event EmptyParameterHandler BeginParse;
        public event EmptyParameterHandler EndParse;

        public delegate void ChatEntryHandler(CallOfDutyLogfile sender, DateTime eventTime, CallOfDutyChat chat);
        public event ChatEntryHandler ChatEntry;

        public delegate void KillEntryHandler(CallOfDutyLogfile sender, DateTime eventTime, CallOfDutyKill kill);
        public event KillEntryHandler KillEntry;

        #endregion

        private Request LogRequest { get; set; }

        public short Interval { get; set; }
        public DateTime LatestEvent { get; private set; }

        public string LogAddress { get; set; }

        /*
        public string LogAddress {
            get {
                if (this.LogRequest != null) {
                    return this.LogRequest.FileName;
                }
                else {
                    return null;
                }
            }
            set {
                if (this.LogRequest == null) {
                    this.LogRequest = new Request(value) { DownloadRate = false };
                    this.LogRequest.RequestComplete += new Request.RequestEventDelegate(LogRequest_RequestComplete);
                    this.LogRequest.RequestError += new Request.RequestEventDelegate(LogRequest_RequestError);
                }
            }
        }
        */
        public CallOfDutyLogfile() {
            this.Interval = 10;
            //this.LatestEvent = DateTime.Now.AddSeconds(-1.0D * this.Interval);
        }

        private void Parse(DateTime entryTime, string text) {

            foreach (KeyValuePair<Regex, Type> command in CallOfDutyLogfile.EntryTypes) {

                Match matchedCommand = command.Key.Match(text);

                if (matchedCommand.Success == true) {
                    ICallOfDutyObject newObject = ((ICallOfDutyObject)Activator.CreateInstance(command.Value)).Parse(matchedCommand);

                    if (newObject is CallOfDutyChat && this.ChatEntry != null) {
                        this.ChatEntry(this, entryTime, (CallOfDutyChat)newObject);
                    }
                    else if (newObject is CallOfDutyKill && this.KillEntry != null) {
                        this.KillEntry(this, entryTime, (CallOfDutyKill)newObject);
                    }

                    this.LatestEvent = entryTime;
                }
            }
        }

        private bool SkipNextRequest { get; set; }

        private void LogRequest_RequestError(Request sender) {
            sender.RequestComplete -= new Request.RequestEventDelegate(LogRequest_RequestComplete);
            sender.RequestError -= new Request.RequestEventDelegate(LogRequest_RequestError);

            this.SkipNextRequest = true;
        }

        private List<string> m_previousEntries = new List<string>();

        private void LogRequest_RequestComplete(Request sender) {

            if (this.BeginParse != null) {
                this.BeginParse(this);
            }

            //string debugLogoutput = string.Format("doutput/text-{0:yyyy-MM-dd_hh-mm-ss-tt}.txt", DateTime.Now);

            string logData = Encoding.ASCII.GetString(sender.CompleteFileData);//.Replace("\r", "");

            int newData = 0;

            foreach (string line in logData.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)) {
            //foreach (Match entry in CallOfDutyLogfile.Entry.Matches(logData)) {

                Match entry = CallOfDutyLogfile.Entry.Match(line);
                if (entry.Success == true) {
                    DateTime entryTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(long.Parse(entry.Groups["time"].Value));

                    if (entryTime.Ticks > this.LatestEvent.Ticks || (entryTime.Ticks == this.LatestEvent.Ticks && this.m_previousEntries.Contains(line) == false)) {
                        // Because we might have been cut off during a log entry made in the same second.

                        this.Parse(entryTime, entry.Groups["text"].Value);
                        newData += entry.Value.Length;

                        if (entryTime.Ticks != this.LatestEvent.Ticks) {
                            this.m_previousEntries.Clear();
                        }
                        this.m_previousEntries.Add(line);

                        //System.IO.File.AppendAllText(debugLogoutput, String.Format("PASS: {0}\r\n", line));

                    }
                    //else {
                    //    System.IO.File.AppendAllText(debugLogoutput, String.Format("TIME: {0}\r\n", line));
                    //}
                    
                }
                //else {
                //    System.IO.File.AppendAllText(debugLogoutput, String.Format("FAIL: {0}\r\n", line));
                //}
            }

            sender.RequestComplete -= new Request.RequestEventDelegate(LogRequest_RequestComplete);
            sender.RequestError -= new Request.RequestEventDelegate(LogRequest_RequestError);

            if (this.EndParse != null) {
                this.EndParse(this);
            }
        }

        public void Fetch() {
            if (this.SkipNextRequest == false) {
                this.LogRequest = new Request(this.LogAddress) {
                    DownloadRate = false,
                    Range = -2048,//-8192, // TODO: Test on busy server.
                    Referrer = "Procon 2.0"
                };

                this.LogRequest.RequestComplete += new Request.RequestEventDelegate(LogRequest_RequestComplete);
                this.LogRequest.RequestError += new Request.RequestEventDelegate(LogRequest_RequestError);
                this.LogRequest.BeginRequest();
            }

            this.SkipNextRequest = false;
        }

    }
}
