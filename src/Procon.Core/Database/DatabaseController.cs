﻿using System;
using System.Collections.Generic;
using System.Linq;
using Procon.Core.Shared;
using Procon.Core.Shared.Models;
using Procon.Core.Variables;
using Procon.Database;
using Procon.Database.Drivers;
using Procon.Database.Shared;

namespace Procon.Core.Database {

    /// <summary>
    /// Handles opening, managing and dispatching queries in databases
    /// </summary>
    public class DatabaseController : SharedController {

        /// <summary>
        /// The currently opened database drivers.
        /// </summary>
        public Dictionary<String, IDriver> OpenDrivers { get; set; }

        /// <summary>
        /// Manages the grouped variable names, listening for grouped changes.
        /// </summary>
        public GroupedVariableListener GroupedVariableListener { get; set; }

        /// <summary>
        /// List of drivers available for cloning and using.
        /// </summary>
        public List<IDriver> AvailableDrivers = new List<IDriver>() {
            new MySqlDriver(),
            new MongoDbDriver(),
            new SqLiteDriver()
        };

        /// <summary>
        /// Initializes default attributes
        /// </summary>
        public DatabaseController() : base() {
            this.OpenDrivers = new Dictionary<String, IDriver>();
            
            this.GroupedVariableListener = new GroupedVariableListener() {
                Variables = this.Variables,
                GroupsVariableName = CommonVariableNames.DatabaseConfigGroups.ToString(),
                ListeningVariablesNames = new List<String>() {
                    CommonVariableNames.DatabaseDriverName.ToString(),
                    CommonVariableNames.DatabaseHostname.ToString(),
                    CommonVariableNames.DatabasePort.ToString(),
                    CommonVariableNames.DatabaseUid.ToString(),
                    CommonVariableNames.DatabasePassword.ToString(),
                    CommonVariableNames.DatabaseMemory.ToString()
                }
            };

            this.AppendDispatchHandlers(new Dictionary<CommandAttribute, CommandDispatchHandler>() {
                {
                    new CommandAttribute() {
                        CommandType = CommandType.DatabaseQuery,
                        ParameterTypes = new List<CommandParameterType>() {
                            new CommandParameterType() {
                                Name = "query",
                                Type = typeof(IDatabaseObject)
                            }
                        }
                    },
                    new CommandDispatchHandler(this.Query)
                },
                {
                    new CommandAttribute() {
                        CommandType = CommandType.DatabaseQuery,
                        ParameterTypes = new List<CommandParameterType>() {
                            new CommandParameterType() {
                                Name = "query",
                                Type = typeof(IDatabaseObject),
                                IsList = true
                            }
                        }
                    },
                    new CommandDispatchHandler(this.Query)
                },
                {
                    new CommandAttribute() {
                        CommandType = CommandType.DatabaseQuery,
                        ParameterTypes = new List<CommandParameterType>() {
                            new CommandParameterType() {
                                Name = "driver",
                                Type = typeof(String)
                            },
                            new CommandParameterType() {
                                Name = "query",
                                Type = typeof(IDatabaseObject)
                            }
                        }
                    },
                    new CommandDispatchHandler(this.QueryDriver)
                },
                {
                    new CommandAttribute() {
                        CommandType = CommandType.DatabaseQuery,
                        ParameterTypes = new List<CommandParameterType>() {
                            new CommandParameterType() {
                                Name = "driver",
                                Type = typeof(String)
                            },
                            new CommandParameterType() {
                                Name = "query",
                                Type = typeof(IDatabaseObject),
                                IsList = true
                            }
                        }
                    },
                    new CommandDispatchHandler(this.QueryDriver)
                }
            });
        }

        /// <summary>
        /// Assign all current event handlers for all grouped options.
        /// 
        /// This will also setup the empty namespace group.
        /// </summary>
        protected void AssignEvents() {
            // Remove all current handlers, also clears the list in this.ListeningVariables
            this.UnassignEvents();
            
            this.GroupedVariableListener.AssignEvents();
            this.GroupedVariableListener.VariablesModified += GroupedVariableListenerOnVariablesModified;
        }

        /// <summary>
        /// Removes all current event handlers.
        /// </summary>
        protected void UnassignEvents() {
            this.GroupedVariableListener.VariablesModified -= GroupedVariableListenerOnVariablesModified;
            this.GroupedVariableListener.UnassignEvents();
        }

        /// <summary>
        /// Opens all of the database groups.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="databaseGroupNames"></param>
        private void GroupedVariableListenerOnVariablesModified(GroupedVariableListener sender, List<String> databaseGroupNames) {
            foreach (String databaseGroupName in databaseGroupNames) {
                IDriver driver = this.AvailableDrivers.FirstOrDefault(pool => String.Compare(pool.Name, this.Variables.Get<String>(VariableModel.NamespaceVariableName(databaseGroupName, CommonVariableNames.DatabaseDriverName)), StringComparison.InvariantCultureIgnoreCase) == 0);

                if (driver != null) {
                    if (this.OpenDrivers.ContainsKey(databaseGroupName) == false) {
                        driver = (IDriver)driver.Clone();

                        driver.Settings = new DriverSettings() {
                            Hostname = this.Variables.Get(VariableModel.NamespaceVariableName(databaseGroupName, CommonVariableNames.DatabaseHostname), String.Empty),
                            Port = this.Variables.Get<ushort>(VariableModel.NamespaceVariableName(databaseGroupName, CommonVariableNames.DatabasePort)),
                            Username = this.Variables.Get(VariableModel.NamespaceVariableName(databaseGroupName, CommonVariableNames.DatabaseUid), String.Empty),
                            Password = this.Variables.Get(VariableModel.NamespaceVariableName(databaseGroupName, CommonVariableNames.DatabasePassword), String.Empty),
                            Database = this.Variables.Get(VariableModel.NamespaceVariableName(databaseGroupName, CommonVariableNames.DatabaseName), String.Empty),
                            Memory = this.Variables.Get(VariableModel.NamespaceVariableName(databaseGroupName, CommonVariableNames.DatabaseMemory), false)
                        };

                        this.OpenDrivers.Add(databaseGroupName, driver);
                    }
                    else {
                        // Close it if it's already open.
                        this.OpenDrivers[databaseGroupName].Close();

                        this.OpenDrivers[databaseGroupName].Settings = new DriverSettings() {
                            Hostname = this.Variables.Get(VariableModel.NamespaceVariableName(databaseGroupName, CommonVariableNames.DatabaseHostname), String.Empty),
                            Port = this.Variables.Get<ushort>(VariableModel.NamespaceVariableName(databaseGroupName, CommonVariableNames.DatabasePort)),
                            Username = this.Variables.Get(VariableModel.NamespaceVariableName(databaseGroupName, CommonVariableNames.DatabaseUid), String.Empty),
                            Password = this.Variables.Get(VariableModel.NamespaceVariableName(databaseGroupName, CommonVariableNames.DatabasePassword), String.Empty),
                            Database = this.Variables.Get(VariableModel.NamespaceVariableName(databaseGroupName, CommonVariableNames.DatabaseName), String.Empty),
                            Memory = this.Variables.Get(VariableModel.NamespaceVariableName(databaseGroupName, CommonVariableNames.DatabaseMemory), false)
                        };
                    }
                }
            }
        }

        public override CoreController Execute() {
            this.GroupedVariableListener.Variables = this.Variables;

            this.AssignEvents();

            return base.Execute();
        }

        /// <summary>
        /// Executes the list of queries, returning the results of the queries.
        /// </summary>
        /// <param name="driver">The driver to execute the query on</param>
        /// <param name="queries">The queries to execute</param>
        /// <returns>The result of the commands containing the results of each query.</returns>
        protected CommandResultArgs ExecuteQueriesOnDriver(IDriver driver, List<IDatabaseObject> queries) {
            CommandResultArgs result = null;

            result = new CommandResultArgs() {
                Success = true,
                Status = CommandResultType.Success,
                Then = {
                    Queries = new List<IDatabaseObject>(queries)
                },
                Now = {
                    Queries = new List<IDatabaseObject>()
                }
            };

            foreach (IDatabaseObject query in queries) {
                // todo is this correct, or should it instead have a CollectionValue?
                result.Now.Queries.AddRange(driver.Query(query));
            }

            return result;
        }

        /// <summary>
        /// Runs a query on a specific driver by its name.
        /// </summary>
        /// <param name="databaseGroupName">The name of the database group to use</param>
        /// <param name="queries">The queries to execute on the matching driver</param>
        /// <returns>The result of the commands containing the results of each query.</returns>
        protected CommandResultArgs ExecuteQueriesOnGroupName(String databaseGroupName, List<IDatabaseObject> queries) {
            CommandResultArgs result = null;

            if (this.OpenDrivers.ContainsKey(databaseGroupName) == true) {
                result = this.ExecuteQueriesOnDriver(this.OpenDrivers[databaseGroupName], queries);
            }
            else {
                result = new CommandResultArgs() {
                    Message = String.Format(@"Database driver ""{0}"" is not supported.", databaseGroupName),
                    Status = CommandResultType.DoesNotExists,
                    Success = false
                };
            }

            return result;
        }

        protected CommandResultArgs ExecuteQueriesOnAllDrivers(List<IDatabaseObject> queries) {
            CommandResultArgs result = null;

            foreach (var databaseGroup in this.OpenDrivers) {
                result = this.ExecuteQueriesOnDriver(databaseGroup.Value, queries);
            }

            return result;
        }

        protected CommandResultArgs Query(Command command, Dictionary<String, CommandParameter> parameters) {
            return this.Security.DispatchPermissionsCheck(command, command.Name).Success == true ? this.ExecuteQueriesOnAllDrivers(parameters["query"].All<IDatabaseObject>()) : CommandResultArgs.InsufficientPermissions;
        }

        protected CommandResultArgs QueryDriver(Command command, Dictionary<String, CommandParameter> parameters) {
            return this.Security.DispatchPermissionsCheck(command, command.Name).Success == true ? this.ExecuteQueriesOnGroupName(parameters["driver"].First<String>(), parameters["query"].All<IDatabaseObject>()) : CommandResultArgs.InsufficientPermissions;
        }

        public override void Dispose() {
            this.UnassignEvents();
            this.GroupedVariableListener = null;

            foreach (var driver in this.OpenDrivers) {
                driver.Value.Close();
            }
            
            this.OpenDrivers.Clear();
            this.OpenDrivers = null;

            foreach (IDriver driver in this.AvailableDrivers) {
                driver.Close();
            }

            this.AvailableDrivers.Clear();
            this.AvailableDrivers = null;

            base.Dispose();
        }
    }
}
