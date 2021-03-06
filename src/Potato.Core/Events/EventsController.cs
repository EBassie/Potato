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
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Potato.Core.Shared;
using Potato.Core.Shared.Events;
using Potato.Core.Shared.Models;
using Potato.Net.Shared;
using Potato.Service.Shared;

namespace Potato.Core.Events {
    /// <summary>
    /// Logs events, keeping them in memory until a specific time occurs that will write all
    /// events older than a time period to disk.
    /// </summary>
    public class EventsController : CoreController, ISharedReferenceAccess {
        /// <summary>
        /// List of events for history
        /// </summary>
        public List<IGenericEvent> LoggedEvents { get; protected set; }

        /// <summary>
        /// Lock used when fetching a new event Id. I hate that this was originally copied from Potato.Net without looking =\
        /// </summary>
        protected readonly Object AcquireEventIdLock = new object();

        /// <summary>
        /// Aquires an event id
        /// </summary>
        protected ulong AcquireEventId {
            get {
                lock (this.AcquireEventIdLock) {
                    return ++this._mEventId;
                }
            }
        }
        private ulong _mEventId;

        /// <summary>
        /// Fired when an event has been logged.
        /// </summary>
        public event EventLoggedHandler EventLogged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event that has been logged</param>
        public delegate void EventLoggedHandler(Object sender, IGenericEvent e);

        public SharedReferences Shared { get; private set; }

        protected List<String> DefaultEventsLogIgnoredNames = new List<String>() {
            ProtocolEventType.ProtocolPlayerlistUpdated.ToString(),
            ProtocolEventType.ProtocolSettingsUpdated.ToString()
        };

        /// <summary>
        /// Initializes default attributes and sets up command dispatching
        /// </summary>
        public EventsController() : base() {
            this.Shared = new SharedReferences();
            this.LoggedEvents = new List<IGenericEvent>();

            this.CommandDispatchers.AddRange(new List<ICommandDispatch>() {
                new CommandDispatch() {
                    CommandType = CommandType.EventsFetchAfterEventId,
                    ParameterTypes = new List<CommandParameterType>() {
                        new CommandParameterType() {
                            Name = "eventId",
                            Type = typeof(ulong)
                        }
                    },
                    Handler = this.EventsFetchAfterEventId
                },
                new CommandDispatch() {
                    CommandType = CommandType.EventsLog,
                    ParameterTypes = new List<CommandParameterType>() {
                        new CommandParameterType() {
                            Name = "event",
                            Type = typeof(IGenericEvent)
                        }
                    },
                    Handler = this.EventsLog
                }
            });
        }

        protected virtual void OnEventLogged(IGenericEvent e) {
            EventLoggedHandler handler = this.EventLogged;

            if (handler != null) {
                handler(this, e);
            }
        }

        public override void Dispose() {
            this.WriteEventsList(this.LoggedEvents);
            this.LoggedEvents.Clear();
            this.LoggedEvents = null;

            this.EventLogged = null;

            base.Dispose();
        }

        /// <summary>
        /// Log an item to the events list
        /// </summary>
        /// <param name="item"></param>
        public void Log(IGenericEvent item) {
            // Can be null after disposal.
            if (this.LoggedEvents != null) {
                item.Id = this.AcquireEventId;

                lock (this.LoggedEvents) {
                    this.LoggedEvents.Add(item);
                }

                this.OnEventLogged(item);
            }
        }

        /// <summary>
        /// Builds the full path to the events log file on a given stamp.
        /// </summary>
        /// <param name="stamp">The date time to build the directory from. Will ignore the minutes and seconds.</param>
        /// <returns>The path to the file to log events</returns>
        public String EventsLogFileName(DateTime stamp) {
            String directory = Path.Combine(Defines.LogsDirectory.FullName, stamp.ToString("yyyy-MM-dd"));

            if (Directory.Exists(directory) == false) {
                Directory.CreateDirectory(directory);
            }

            return Path.Combine(directory, String.Format("events_{0}_to_{1}.json", stamp.ToString("HH_00_00"), stamp.AddHours(1.0D).ToString("HH_00_00")));
        }

        /// <summary>
        /// Writes the selected events to a file.
        /// </summary>
        /// <param name="events">The events to write.</param>
        protected bool WriteEventsList(List<IGenericEvent> events) {
            // Assume everything was successful
            bool saved = true;

            if (this.Shared.Variables.Get(CommonVariableNames.WriteLogEventsToFile, false) == true) {
                foreach (var eventHourlyGroup in events.GroupBy(e => new DateTime(e.Stamp.Year, e.Stamp.Month, e.Stamp.Day, e.Stamp.Hour, 0, 0))) {
                    String logFileName = this.EventsLogFileName(eventHourlyGroup.Key);

                    try {
                        using (TextWriter writer = new StreamWriter(logFileName, true)) {
                            var serializer = new JsonSerializer {
                                NullValueHandling = NullValueHandling.Ignore,
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            };

                            foreach (var @event in eventHourlyGroup) {
                                serializer.Serialize(writer, @event);
                                writer.WriteLine(",");
                            }
                        }
                    }
                    catch {
                        saved = false;
                    }
                }
            }

            return saved;
        }

        /// <summary>
        /// Write all events older than a specified amount of seconds to a file,
        /// freeing up memory.
        /// </summary>
        public void WriteEvents() {
            this.WriteEvents(DateTime.Now);
        }

        /// <summary>
        /// Write all events older than a specified amount of seconds to a file,
        /// freeing up memory.
        /// </summary>
        /// <param name="now">The current time to use for all calculations in this method</param>
        public void WriteEvents(DateTime now) {

            // Events can be null after disposal.
            if (this.LoggedEvents != null) {

                List<IGenericEvent> flushEvents = null;

                DateTime before = now - TimeSpan.FromSeconds(this.Shared.Variables.Get(CommonVariableNames.MaximumEventsTimeSeconds, 30));

                lock (this.LoggedEvents) {
                    // All events are appended to the Events list, so we
                    // remove all events until we find one that isn't old enough.
                    // Provided the event is not ignored (don't write ignored events)
                    flushEvents = this.LoggedEvents.Where(e => e.Stamp < before).Where(e => this.Shared.Variables.Get(CommonVariableNames.EventsLogIgnoredNames, this.DefaultEventsLogIgnoredNames).Contains(e.Name) == false).ToList();
                }

                // Don't hold up other threads attempting to log an event.
                this.WriteEventsList(flushEvents);

                // Now remove all old events. This differs from the events we wrote to disk, as we may
                // have ignored some of the events but we still need to get rid of old events.
                lock (this.LoggedEvents) {
                    var flushed = this.LoggedEvents.Where(e => e.Stamp < before).ToList();
                    flushed.ForEach(e => {
                        e.Dispose();
                        this.LoggedEvents.Remove(e);
                    });
                }
            }
        }

        /// <summary>
        /// Fetches all events after a passed id, as well as after a certain date.
        /// </summary>
        public ICommandResult EventsFetchAfterEventId(ICommand command, Dictionary<String, ICommandParameter> parameters) {
            ICommandResult result = null;

            ulong eventId = parameters["eventId"].First<ulong>();

            if (this.Shared.Security.DispatchPermissionsCheck(command, command.Name).Success == true) {
                List<IGenericEvent> events = null;

                lock (this.LoggedEvents) {
                    events = this.LoggedEvents.Where(e => e.Stamp > DateTime.Now - TimeSpan.FromSeconds(this.Shared.Variables.Get(CommonVariableNames.MaximumEventsTimeSeconds, 300)))
                                              .Where(e => e.Id > eventId)
                                              .OrderBy(e => e.Id)
                                              .ToList();
                }

                result = new CommandResult() {
                    Success = true,
                    CommandResultType = CommandResultType.Success,
                    Message = String.Format(@"Fetched {0} event(s)", events.Count),
                    Now = {
                        Events = events
                    }
                };
            }
            else {
                result = CommandResult.InsufficientPermissions;
            }

            return result;
        }

        /// <summary>
        /// Logs a new event
        /// </summary>
        public ICommandResult EventsLog(ICommand command, Dictionary<String, ICommandParameter> parameters) {
            ICommandResult result = null;

            IGenericEvent @event = parameters["event"].First<IGenericEvent>();

            if (this.Shared.Security.DispatchPermissionsCheck(command, command.Name).Success == true) {
                this.Log(@event);

                result = new CommandResult() {
                    Success = true,
                    CommandResultType = CommandResultType.Success,
                    Now = {
                        Events = new List<IGenericEvent>() {
                            @event
                        }
                    }
                };
            }
            else {
                result = CommandResult.InsufficientPermissions;
            }

            return result;
        }
    }
}
