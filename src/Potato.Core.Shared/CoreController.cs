#region Copyright
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
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json.Linq;
using Potato.Core.Shared.Serialization;

namespace Potato.Core.Shared {
    /// <summary>
    /// Handles command routing and config handling
    /// </summary>
    [Serializable]
    public abstract class CoreController : MarshalByRefObject, INotifyPropertyChanged, ICloneable, ICoreController {
        /// <summary>
        /// List of dispatch attributes to the method to call, provided the parameter list matches.
        /// </summary>
        public List<ICommandDispatch> CommandDispatchers { get; set; } 

        /// <summary>
        /// All objects to tunnel downwards
        /// </summary>
        public List<ICoreController> TunnelObjects { get; set; }

        /// <summary>
        /// All objects to bubble upwards
        /// </summary>
        public List<ICoreController> BubbleObjects { get; set; }

        protected CoreController() : base() {
            this.CommandDispatchers = new List<ICommandDispatch>();
            this.TunnelObjects = new List<ICoreController>();
            this.BubbleObjects = new List<ICoreController>();
        }

        /// <summary>
        /// Fired after the disposal method has been executed on this object.
        /// </summary>
        [field: NonSerialized]
        public event EventHandler Disposed;

        protected virtual void OnDisposed() {
            EventHandler handler = Disposed;

            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Allows for an optional child implementation.
        /// </summary>
        public virtual void Dispose() {
            this.OnDisposed();

            if (this.BubbleObjects != null) this.BubbleObjects.Clear();
            this.BubbleObjects = null;

            if (this.TunnelObjects != null) this.TunnelObjects.Clear();
            this.TunnelObjects = null;

            this.Disposed = null;
        }

        /// <summary>
        /// Allows for an optional child implementation.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="password"></param>
        public virtual void WriteConfig(IConfig config, string password = null) { }

        public virtual void Poke() {
            
        }

        /// <summary>
        /// Loads the specified configuration file.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="password">The password to decrypt the config commands with</param>
        /// <returns></returns>
        public virtual ICoreController Execute(IConfig config, String password = null) {
            this.TunnelObjects = this.TunnelObjects ?? new List<ICoreController>();
            this.BubbleObjects = this.BubbleObjects ?? new List<ICoreController>();

            this.Execute(new Command() {
                Origin = CommandOrigin.Local
            }, config, password);

            return this;
        }

        /// <summary>
        /// Called after the constructor is called
        /// </summary>
        /// <returns></returns>
        public virtual ICoreController Execute() {
            this.TunnelObjects = this.TunnelObjects ?? new List<ICoreController>();
            this.BubbleObjects = this.BubbleObjects ?? new List<ICoreController>();

            return this;
        }

        /// <summary>
        /// Finds the commands specified in the config file and invokes them with the specified attributes.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="config"></param>
        /// <param name="password">The password to decrypt encrypted config commands</param>
        protected void Execute(ICommand command, IConfig config, String password = null) {
            if (config != null && config.Root != null) {

                foreach (var loadedConfigCommand in config.RootOf(this.GetType()).Children<JObject>().Select(item => item.ToObject<ConfigCommand>(JsonSerialization.Minimal)).Where(item => item != null)) {
                    // only attempt a decrypt if we've been given a password. If we don't have a password, we won't bother
                    // and the command will have a null Command and be skipped anyway.
                    if (password != null) {
                        loadedConfigCommand.Decrypt(password);
                    }

                    var loadedCommand = loadedConfigCommand.Command;

                    if (loadedCommand != null && loadedCommand.Name != null) {
                        command.ParseCommandType(loadedCommand.Name);
                        command.Parameters = loadedCommand.Parameters;
                        command.Scope = loadedCommand.Scope;

                        this.Tunnel(command);
                    }
                }
            }
        }

        private ICommandResult Run(CommandAttributeType attributeType, ICommand command, CommandResultType maintainStatus) {

            // Loop through all matching commands with the identical name and type
            foreach (var dispatch in this.CommandDispatchers.Where(dispatch => dispatch.CanDispatch(attributeType, command))) {
                
                // Check if we can build a parameter list.
                Dictionary<String, ICommandParameter> parameters = dispatch.BuildParameterDictionary(command.Parameters);

                if (parameters != null) {
                    command.Result = dispatch.Handler(command, parameters);

                    // Our status has changed, break our loop.
                    if (command.Result.CommandResultType != maintainStatus) {
                        break;
                    }
                }
            }

            return command.Result;
        }

        public virtual ICommandResult PropogatePreview(ICommand command, CommandDirection direction) {
            command.Result = this.Run(CommandAttributeType.Preview, command, CommandResultType.Continue);

            if (command.Result.CommandResultType == CommandResultType.Continue) {
                IList<ICoreController> propogationList = direction == CommandDirection.Tunnel ? this.TunnelExecutableObjects(command) : this.BubbleExecutableObjects(command);

                for (int offset = 0; propogationList != null && offset < propogationList.Count && command.Result.CommandResultType == CommandResultType.Continue; offset++) {
                    if (propogationList[offset] != null) {
                        command.Result = propogationList[offset].PropogatePreview(command, direction);
                    }
                }
            }

            return command.Result;
        }

        public virtual ICommandResult PropogateHandler(ICommand command, CommandDirection direction) {
            command.Result = this.Run(CommandAttributeType.Handler, command, CommandResultType.Continue);

            if (command.Result.CommandResultType == CommandResultType.Continue) {
                IList<ICoreController> propogationList = direction == CommandDirection.Tunnel ? this.TunnelExecutableObjects(command) : this.BubbleExecutableObjects(command);

                for (int offset = 0; propogationList != null && offset < propogationList.Count && command.Result.CommandResultType == CommandResultType.Continue; offset++) {
                    if (propogationList[offset] != null) {
                        command.Result = propogationList[offset].PropogateHandler(command, direction);
                    }
                }
            }

            return command.Result;
        }

        public virtual ICommandResult PropogateExecuted(ICommand command, CommandDirection direction) {
            command.Result = this.Run(CommandAttributeType.Executed, command, command.Result.CommandResultType);

            IList<ICoreController> propogationList = direction == CommandDirection.Tunnel ? this.TunnelExecutableObjects(command) : this.BubbleExecutableObjects(command);

            if (propogationList != null) {
                foreach (CoreController executable in propogationList) {
                    if (executable != null) {
                        command.Result = executable.PropogateExecuted(command, direction);
                    }
                }
            }

            return command.Result;
        }

        /// <summary>
        /// Executes a command against this object, provided the command attribute matches as well as the types of each parameter.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual ICommandResult Tunnel(ICommand command) {
            // Setup the initial command result.
            command.Result = new CommandResult() {
                Success = true,
                CommandResultType = CommandResultType.Continue
            };

            command.Result = this.PropogatePreview(command, CommandDirection.Tunnel);

            if (command.Result.CommandResultType == CommandResultType.Continue) {
                command.Result = this.PropogateHandler(command, CommandDirection.Tunnel);

                command.Result = this.PropogateExecuted(command, CommandDirection.Tunnel);
            }
            // If the preview stole the command and executed it, let everyone know it has been executed.
            else if (command.Result.CommandResultType == CommandResultType.Success) {
                command.Result = this.PropogateExecuted(command, CommandDirection.Tunnel);
            }

            return command.Result;
        }

        /// <summary>
        /// Execute a command, then bubble it if the dispatch fails or remains as continuing
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual ICommandResult Bubble(ICommand command) {
            // Setup the initial command result.
            command.Result = new CommandResult() {
                Success = true,
                CommandResultType = CommandResultType.Continue
            };

            command.Result = this.PropogatePreview(command, CommandDirection.Bubble);

            if (command.Result.CommandResultType == CommandResultType.Continue) {
                command.Result = this.PropogateHandler(command, CommandDirection.Bubble);

                command.Result = this.PropogateExecuted(command, CommandDirection.Bubble);
            }
            // If the preview stole the command and executed it, let everyone know it has been executed.
            else if (command.Result.CommandResultType == CommandResultType.Success) {
                command.Result = this.PropogateExecuted(command, CommandDirection.Bubble);
            }

            return command.Result;
        }

        protected virtual IList<ICoreController> TunnelExecutableObjects(ICommand command) {
            return this.TunnelObjects;
        }

        protected virtual IList<ICoreController> BubbleExecutableObjects(ICommand command) {
            return this.BubbleObjects;
        } 

        public object Clone() {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// Event for whenever a property is modified on this executable object
        /// </summary>
        /// <remarks>I think this is only used for variables, which I would like to move specifically to
        /// the variables controlle. There is no need for other variables to use this functionality.</remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="property"></param>
        protected void OnPropertyChanged(Object sender, String property) {
            var handler = this.PropertyChanged;
            if (handler != null) {
                handler(sender, new PropertyChangedEventArgs(property));
            }
        }
    }
}
