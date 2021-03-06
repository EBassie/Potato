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
using Potato.Core.Shared;
using Potato.Core.Shared.Events;
using Potato.Core.Shared.Models;
using Potato.Service.Shared;

namespace Potato.Core.Variables {
    /// <summary>
    /// Stores variables and handles commands to edit/fetch a variable
    /// </summary>
    public class VariableController : CoreController, ISharedReferenceAccess {

        /// <summary>
        /// Anything in this list is volatile and will not be saved on
        /// exit.
        /// </summary>
        public ConcurrentDictionary<String, VariableModel> VolatileVariables { get; set; }
        
        /// <summary>
        /// Anything in this list will be saved to the config
        /// </summary>
        public ConcurrentDictionary<String, VariableModel> ArchiveVariables { get; set; }

        /// <summary>
        /// Anything in this list will be saved to the config, but saved as a volatile set. The variable
        /// value will only last until the instance is restarted and the config consumed.
        /// </summary>
        public ConcurrentDictionary<String, VariableModel> FlashVariables { get; set; }

        public SharedReferences Shared { get; private set; }

        /// <summary>
        /// Initializes the variable controller with default values and sets up command handlers.
        /// </summary>
        public VariableController() : base() {
            this.Shared = new SharedReferences();
            this.VolatileVariables = new ConcurrentDictionary<String, VariableModel>();
            this.ArchiveVariables = new ConcurrentDictionary<String, VariableModel>();
            this.FlashVariables = new ConcurrentDictionary<String, VariableModel>();

            this.CommandDispatchers.AddRange(new List<ICommandDispatch>() {
                new CommandDispatch() {
                    CommandType = CommandType.VariablesSet,
                    ParameterTypes = new List<CommandParameterType>() {
                        new CommandParameterType() {
                            Name = "name",
                            Type = typeof(String)
                        },
                        new CommandParameterType() {
                            Name = "value",
                            Type = typeof(String),
                            IsList = true
                        }
                    },
                    Handler = this.CommandSetCollection
                },
                new CommandDispatch() {
                    CommandType = CommandType.VariablesSet,
                    ParameterTypes = new List<CommandParameterType>() {
                        new CommandParameterType() {
                            Name = "name",
                            Type = typeof(String)
                        },
                        new CommandParameterType() {
                            Name = "value",
                            Type = typeof(String)
                        }
                    },
                    Handler = this.CommandSetSingular
                },
                new CommandDispatch() {
                    CommandType = CommandType.VariablesSetA,
                    ParameterTypes = new List<CommandParameterType>() {
                        new CommandParameterType() {
                            Name = "name",
                            Type = typeof(String)
                        },
                        new CommandParameterType() {
                            Name = "value",
                            Type = typeof(String),
                            IsList = true
                        }
                    },
                    Handler = this.CommandSetACollection
                },
                new CommandDispatch() {
                    CommandType = CommandType.VariablesSetA,
                    ParameterTypes = new List<CommandParameterType>() {
                        new CommandParameterType() {
                            Name = "name",
                            Type = typeof(String)
                        },
                        new CommandParameterType() {
                            Name = "value",
                            Type = typeof(String)
                        }
                    },
                    Handler = this.CommandSetASingular
                },
                new CommandDispatch() {
                    CommandType = CommandType.VariablesSetF,
                    ParameterTypes = new List<CommandParameterType>() {
                        new CommandParameterType() {
                            Name = "name",
                            Type = typeof(String)
                        },
                        new CommandParameterType() {
                            Name = "value",
                            Type = typeof(String),
                            IsList = true
                        }
                    },
                    Handler = this.CommandSetFCollection
                },
                new CommandDispatch() {
                    CommandType = CommandType.VariablesSetF,
                    ParameterTypes = new List<CommandParameterType>() {
                        new CommandParameterType() {
                            Name = "name",
                            Type = typeof(String)
                        },
                        new CommandParameterType() {
                            Name = "value",
                            Type = typeof(String)
                        }
                    },
                    Handler = this.CommandSetFSingular
                },
                new CommandDispatch() {
                    CommandType = CommandType.VariablesGet,
                    ParameterTypes = new List<CommandParameterType>() {
                        new CommandParameterType() {
                            Name = "name",
                            Type = typeof(String)
                        }
                    },
                    Handler = this.CommandGet
                }
            });
        }

        // @todo this should be moved to something like "InstanceVariableController" or something.
        // It's otherwise too specialized for something that could be used in plugins.
        private void SetupDefaultVariables() {
            this.Set(new Command() {
                Origin = CommandOrigin.Local
            }, CommonVariableNames.TextCommandPublicPrefix, "!");

            this.Set(new Command() {
                Origin = CommandOrigin.Local
            }, CommonVariableNames.TextCommandProtectedPrefix, "#");

            this.Set(new Command() {
                Origin = CommandOrigin.Local
            }, CommonVariableNames.TextCommandPrivatePrefix, "@");

            this.Set(new Command() {
                Origin = CommandOrigin.Local
            }, CommonVariableNames.DatabaseMaximumSelectedRows, 20);

            var sourceRepositoryUri = this.Variable(CommonVariableNames.PackagesDefaultSourceRepositoryUri);
            sourceRepositoryUri.Value = Defines.PackagesDefaultSourceRepositoryUri;
            sourceRepositoryUri.Readonly = true;
        }

        /// <summary>
        /// Begins the execution of this variable controller.
        /// Assigns events and loads the config for this file.
        /// </summary>
        public override ICoreController Execute() {
            this.AssignEvents();

            this.SetupDefaultVariables();

            return base.Execute();
        }

        /// <summary>
        /// Information about this object is handled via it's parent interface.
        /// </summary>
        public override void Dispose() {
            foreach (var variable in this.VolatileVariables) {
                variable.Value.Dispose();
            }

            foreach (var archiveVariable in this.ArchiveVariables) {
                archiveVariable.Value.Dispose();
            }

            this.VolatileVariables.Clear();
            this.VolatileVariables = null;

            this.ArchiveVariables.Clear();
            this.ArchiveVariables = null;
        }

        /// <summary>
        /// Does nothing.  Information about this object is handled via it's parent interface.
        /// </summary>
        public override void WriteConfig(IConfig config, String password = null) {
            // Use the .Value.Name to maintain the case
            foreach (var archiveVariable in this.ArchiveVariables) {
                // Don't save empty values, even empty values in a list.
                var values = archiveVariable.Value.ToList<String>().Where(item => item.Length > 0).ToList();

                if (values.Count > 0) {
                    config.Append(CommandBuilder.VariablesSetA(archiveVariable.Value.Name, values).ToConfigCommand());
                }
            }

            foreach (var flashVariable in this.FlashVariables) {
                // Don't save empty values, even empty values in a list.
                var values = flashVariable.Value.ToList<String>().Where(item => item.Length > 0).ToList();

                if (values.Count > 0) {
                    config.Append(CommandBuilder.VariablesSet(flashVariable.Value.Name, values).ToConfigCommand());
                }
            }
        }

        protected void AssignEvents() {
            
        }

        /// <summary>
        /// Fetches a variable by name
        /// </summary>
        /// <param name="name">The name of the variable object</param>
        /// <returns>The variable, if available. False otherwise.</returns>
        public VariableModel Variable(String name) {
            return this.VolatileVariables.GetOrAdd(name.ToLowerInvariant(), s => new VariableModel() {
                Name = name
            });
        }

        /// <summary>
        /// Alias of Variable(String)
        /// </summary>
        /// <param name="name">The name of the variable object</param>
        /// <returns>The variable, if available. False otherwise.</returns>
        public VariableModel Variable(CommonVariableNames name) {
            return this.Variable(name.ToString());
        }

        /// <summary>
        /// Supports '-' and '--' arguments.
        /// </summary>
        /// <param name="arguments">A list of arguments to pass.</param>
        public void ParseArguments(List<String> arguments) {
            for (int offset = 0; offset < arguments.Count; offset++) {
                String argument = arguments[offset];

                // if the argument is a switch.
                if (argument[0] == '-') {
                    // Trims any hyphens from the start of the argument. Allows for "-argument" and "--argument"
                    argument = argument.TrimStart('-');

                    VariableModel variable = null;

                    // Does another argument exist?
                    if (offset + 1 < arguments.Count && arguments[offset + 1][0] != '-') {
                        // No, the next string is not an argument switch. It's the value of the
                        // argument.
                        variable = this.Set(new Command() { Origin = CommandOrigin.Local }, argument, arguments[offset + 1]).Now.Variables.FirstOrDefault();
                    }
                    else {
                        // Set to "true"
                        variable = this.Set(new Command() { Origin = CommandOrigin.Local }, argument, true).Now.Variables.FirstOrDefault();
                    }

                    if (variable != null) {
                        variable.Readonly = true;
                    }
                }
            }
        }

        protected ICommandResult CommandSetCollection(ICommand command, Dictionary<String, ICommandParameter> parameters) {
            String name = parameters["name"].First<String>();
            List<String> value = parameters["value"].All<String>();

            return this.Set(command, name, value);
        }

        protected ICommandResult CommandSetSingular(ICommand command, Dictionary<String, ICommandParameter> parameters) {
            String name = parameters["name"].First<String>();
            String value = parameters["value"].First<String>();

            return this.Set(command, name, value);
        }

        protected ICommandResult CommandSetACollection(ICommand command, Dictionary<String, ICommandParameter> parameters) {
            String name = parameters["name"].First<String>();
            List<String> value = parameters["value"].All<String>();

            return this.SetA(command, name, value);
        }

        protected ICommandResult CommandSetASingular(ICommand command, Dictionary<String, ICommandParameter> parameters) {
            String name = parameters["name"].First<String>();
            String value = parameters["value"].First<String>();

            return this.SetA(command, name, value);
        }

        protected ICommandResult CommandSetFCollection(ICommand command, Dictionary<String, ICommandParameter> parameters) {
            String name = parameters["name"].First<String>();
            List<String> value = parameters["value"].All<String>();

            return this.SetF(command, name, value);
        }

        protected ICommandResult CommandSetFSingular(ICommand command, Dictionary<String, ICommandParameter> parameters) {
            String name = parameters["name"].First<String>();
            String value = parameters["value"].First<String>();

            return this.SetF(command, name, value);
        }

        protected ICommandResult CommandGet(ICommand command, Dictionary<String, ICommandParameter> parameters) {
            String name = parameters["name"].First<String>();

            return this.Get(command, name);
        }

        /// <summary>
        /// This will first set the value, then set the value in the archived list
        /// which will be saved to the config
        /// </summary>
        /// <param name="command">Details of the commands origin</param>
        /// <param name="name">The unique name of the variable to set</param>
        /// <param name="value">The value of the variable</param>
        /// <returns></returns>
        public ICommandResult Set(ICommand command, String name, Object value) {
            ICommandResult result = null;

            if (command.Origin == CommandOrigin.Local || this.Shared.Security.DispatchPermissionsCheck(command, command.Name).Success == true) {
                if (name.Length > 0) {
                    VariableModel variable = this.Variable(name);

                    if (variable.Readonly == false) {
                        variable.Value = value;

                        result = new CommandResult() {
                            Success = true,
                            CommandResultType = CommandResultType.Success,
                            Message = String.Format(@"Successfully set value of variable name ""{0}"" to ""{1}"".", variable.Name, variable.Value),
                            Now = new CommandData() {
                                Variables = new List<VariableModel>() {
                                    variable
                                }
                            }
                        };

                        if (this.Shared.Events != null) {
                            this.Shared.Events.Log(GenericEvent.ConvertToGenericEvent(result, GenericEventType.VariablesSet));
                        }
                    }
                    else {
                        // Variable set to read only and cannot be modified.
                        result = new CommandResult() {
                            Success = false,
                            CommandResultType = CommandResultType.Failed,
                            Message = String.Format(@"Variable name ""{0}"" is set to read-only.", variable.Name)
                        };
                    }
                }
                else {
                    result = new CommandResult() {
                        Success = false,
                        CommandResultType = CommandResultType.InvalidParameter,
                        Message = "A variable name must not be zero length"
                    };
                }
            }
            else {
                result = CommandResult.InsufficientPermissions;
            }

            return result;
        }

        /// <summary>
        /// This will first set the value, then set the value in the archived list
        /// which will be saved to the config
        /// </summary>
        /// <param name="command">Details of the commands origin</param>
        /// <param name="name">The unique name of the variable to set</param>
        /// <param name="value">The value of the variable</param>
        /// <returns></returns>
        public ICommandResult Set(ICommand command, CommonVariableNames name, Object value) {
            return this.Set(command, name.ToString(), value);
        }

        /// <summary>
        /// This will first set the value, then set the value in the archived list
        /// which will be saved to the config
        /// </summary>
        /// <param name="command">Details of the commands origin</param>
        /// <param name="name">The unique name of the variable to set</param>
        /// <param name="value">The value of the variable</param>
        /// <returns></returns>
        public ICommandResult SetA(ICommand command, String name, Object value) {
            ICommandResult result = null;

            if (command.Origin == CommandOrigin.Local || this.Shared.Security.DispatchPermissionsCheck(command, command.Name).Success == true) {
                ICommandResult volatileSetResult = this.Set(command, name, value);

                if (volatileSetResult.Success == true) {
                    // All good.
                    var variable = volatileSetResult.Now.Variables.First();

                    // Upsert he archive variable
                    this.ArchiveVariables.AddOrUpdate(variable.Name.ToLowerInvariant(), s => variable, (s, model) => variable);

                    // Remove the flash value (so archive + flash are not being saved)
                    VariableModel removed;
                    this.FlashVariables.TryRemove(variable.Name.ToLowerInvariant(), out removed);

                    result = new CommandResult() {
                        Success = true,
                        CommandResultType = CommandResultType.Success,
                        Message = String.Format(@"Successfully set value of variable name ""{0}"" to ""{1}"".", variable.Name, variable.Value),
                        Now = new CommandData() {
                            Variables = new List<VariableModel>() {
                                variable
                            }
                        }
                    };

                    if (this.Shared.Events != null) {
                        this.Shared.Events.Log(GenericEvent.ConvertToGenericEvent(result, GenericEventType.VariablesSetA));
                    }
                }
                else {
                    // Bubble the error.
                    result = volatileSetResult;
                }
            }
            else {
                result = CommandResult.InsufficientPermissions;
            }

            return result;
        }

        /// <summary>
        /// This will first set the value, then set the value in the flash list
        /// which will be saved to the config
        /// </summary>
        /// <param name="command">Details of the commands origin</param>
        /// <param name="name">The unique name of the variable to set</param>
        /// <param name="value">The value of the variable</param>
        /// <returns></returns>
        public ICommandResult SetF(ICommand command, String name, Object value) {
            ICommandResult result = null;

            if (command.Origin == CommandOrigin.Local || this.Shared.Security.DispatchPermissionsCheck(command, command.Name).Success == true) {
                ICommandResult volatileSetResult = this.Set(command, name, value);

                if (volatileSetResult.Success == true) {
                    // All good.
                    var variable = volatileSetResult.Now.Variables.First();

                    this.FlashVariables.AddOrUpdate(variable.Name.ToLowerInvariant(), s => variable, (s, model) => variable);

                    // Remove the archived value (so archive + flash are not being saved)
                    VariableModel removed;
                    this.ArchiveVariables.TryRemove(variable.Name.ToLowerInvariant(), out removed);

                    result = new CommandResult() {
                        Success = true,
                        CommandResultType = CommandResultType.Success,
                        Message = String.Format(@"Successfully set value of variable name ""{0}"" to ""{1}"".", variable.Name, variable.Value),
                        Now = new CommandData() {
                            Variables = new List<VariableModel>() {
                                variable
                            }
                        }
                    };

                    if (this.Shared.Events != null) {
                        this.Shared.Events.Log(GenericEvent.ConvertToGenericEvent(result, GenericEventType.VariablesSetF));
                    }
                }
                else {
                    // Bubble the error.
                    result = volatileSetResult;
                }
            }
            else {
                result = CommandResult.InsufficientPermissions;
            }

            return result;
        }

        /// <summary>
        /// Gets and converts a value given a name
        /// </summary>
        /// <typeparam name="T">The type of value to return</typeparam>
        /// <param name="name">The unique name of the variable to fetch</param>
        /// <param name="defaultValue"></param>
        /// <returns>The converted value of the variable with the specified name</returns>
        public T Get<T>(String name, T defaultValue = default(T)) {
            T result = defaultValue;

            VariableModel variable = this.Variable(name);

            result = variable.ToType(defaultValue);

            return result;
        }

        /// <summary>
        /// Gets and converts a value given a name
        /// </summary>
        /// <typeparam name="T">The type of value to return</typeparam>
        /// <param name="name">The unique name of the variable to fetch</param>
        /// <param name="defaultValue"></param>
        /// <returns>The converted value of the variable with the specified kenamey</returns>
        public T Get<T>(CommonVariableNames name, T defaultValue = default(T)) {
            return this.Get(name.ToString(), defaultValue);
        }

        /// <summary>
        /// Gets a raw value given a knameey, returned as a Object
        /// </summary>
        /// <param name="command">Details of the commands origin</param>
        /// <param name="name">The unique name of the variable to fetch</param>
        /// <param name="defaultValue"></param>
        /// <returns>The raw object with no conversion</returns>
        public ICommandResult Get(ICommand command, String name, Object defaultValue = null) {
            ICommandResult result = null;

            if (command.Origin == CommandOrigin.Local || this.Shared.Security.DispatchPermissionsCheck(command, command.Name).Success == true) {
                if (name.Length > 0) {
                    VariableModel variable = this.Variable(name);

                    result = new CommandResult() {
                        Success = true,
                        CommandResultType = CommandResultType.Success,
                        Message = String.Format(@"Value of variable with name ""{0}"" is ""{1}"".", variable.Name, variable.Value),
                        Now = new CommandData() {
                            Variables = new List<VariableModel>() {
                                variable
                            }
                        }
                    };
                }
                else {
                    result = new CommandResult() {
                        Success = false,
                        CommandResultType = CommandResultType.InvalidParameter,
                        Message = "A variable name must not be zero length"
                    };
                }
            }
            else {
                result = CommandResult.InsufficientPermissions;
            }

            return result;
        }
    }
}
