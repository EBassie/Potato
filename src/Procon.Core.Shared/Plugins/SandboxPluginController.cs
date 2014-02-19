using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Procon.Core.Shared.Events;
using Procon.Net.Shared;

namespace Procon.Core.Shared.Plugins {
    /// <summary>
    /// Controller to run on the plugin side of the AppDomain
    /// </summary>
    public class SandboxPluginController : CoreController, ISandboxPluginController {
        /// <summary>
        /// A list of loaded plugins. This can be considered a pool of plugins that
        /// exist, but not necessarily plugins that should recieve events and commands.
        /// </summary>
        protected ConcurrentDictionary<Guid, IPluginController> LoadedPlugins = new ConcurrentDictionary<Guid, IPluginController>();

        /// <summary>
        /// A list of plugins that are enabled which should recieve events and commands.
        /// </summary>
        protected ConcurrentDictionary<Guid, IPluginController> EnabledPlugins = new ConcurrentDictionary<Guid, IPluginController>();

        /// <summary>
        /// Creates an instance of a type in an assembly.
        /// </summary>
        /// <param name="assemblyFile"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public IPluginController Create(String assemblyFile, String typeName) {

            IPluginController loadedPlugin = (IPluginController)Activator.CreateInstanceFrom(
                assemblyFile,
                typeName,
                false,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
                null,
                null,
                null,
                null
            ).Unwrap();

            this.LoadedPlugins.TryAdd(loadedPlugin.PluginGuid, loadedPlugin);

            loadedPlugin.Execute();

            return loadedPlugin;
        }

        /// <summary>
        /// Enables a plugin by it's guid, allowing it to accept events and commands.
        /// </summary>
        /// <param name="pluginGuid"></param>
        /// <returns>Returns true if the plugin was disabled and is now enabled. False will be returned if the plugin does not exist or was already enabled to begin with.</returns>
        public bool TryEnablePlugin(Guid pluginGuid) {
            bool wasEnabled = false;

            // If we have the plugin and the plugin isn't already in the dictionary.
            if (this.LoadedPlugins.ContainsKey(pluginGuid) == true && this.EnabledPlugins.ContainsKey(pluginGuid) == false) {
                IPluginController plugin = this.LoadedPlugins[pluginGuid];

                this.EnabledPlugins.TryAdd(plugin.PluginGuid, plugin);

                plugin.BubbleObjects.Add(this);

                //plugin.PluginCallback = new List<IExecutableBase>() {
                //    this
                //};

                plugin.GenericEvent(new GenericEvent() {
                    GenericEventType = GenericEventType.PluginsEnabled
                });

                wasEnabled = true;
            }

            return wasEnabled;
        }

        /// <summary>
        /// Disables a plugin, denying it events and commands.
        /// </summary>
        /// <param name="pluginGuid"></param>
        /// <returns>Returns true if the plugin was enabled and is now disabled. False will be returned if the plugin does not exist or wasn't enabled to begin with.</returns>
        public bool TryDisablePlugin(Guid pluginGuid) {
            bool wasDisabled = false;

            IPluginController plugin;

            if (this.EnabledPlugins.TryRemove(pluginGuid, out plugin) == true) {
                plugin.BubbleObjects.Clear();

                plugin.GenericEvent(new GenericEvent() {
                    GenericEventType = GenericEventType.PluginsDisabled
                });

                wasDisabled = true;
            }

            return wasDisabled;
        }

        public void GameEvent(List<IProtocolEventArgs> items) {
            foreach (var item in items) {
                foreach (var enabledPlugin in this.EnabledPlugins) {
                    enabledPlugin.Value.GameEvent(item);
                }
            }
        }

        public void ClientEvent(List<IClientEventArgs> items) {
            foreach (var item in items) {
                foreach (var enabledPlugin in this.EnabledPlugins) {
                    enabledPlugin.Value.ClientEvent(item);
                }
            }
        }

        /// <summary>
        /// Check if a plugin is marked as enabled or not
        /// </summary>
        /// <param name="pluginGuid"></param>
        /// <returns>True if a plugin is enabled, false otherwise.</returns>
        public bool IsPluginEnabled(Guid pluginGuid) {
            return this.EnabledPlugins.ContainsKey(pluginGuid);
        }

        /// <summary>
        /// Shutdown all of the plugins, firing a 
        /// </summary>
        public void Shutdown() {
            foreach (var plugin in this.LoadedPlugins) {
                plugin.Value.GenericEvent(new GenericEvent() {
                    GenericEventType = GenericEventType.PluginsUnloading
                });

                plugin.Value.Dispose();
            }
        }

        /// <summary>
        /// Enforce an origin of plugin before we bubble any commands.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected override IList<ICoreController> BubbleExecutableObjects(ICommand command) {
            command.Origin = CommandOrigin.Plugin;

            return this.BubbleObjects ?? new List<ICoreController>();
        }

        protected override IList<ICoreController> TunnelExecutableObjects(ICommand command) {
            List<ICoreController> list = new List<ICoreController>();

            if (command.ScopeModel != null && command.ScopeModel.PluginGuid != Guid.Empty) {
                IPluginController enabledPlugin;

                // Get the enabled plugin if it exists.
                if (this.EnabledPlugins.TryGetValue(command.ScopeModel.PluginGuid, out enabledPlugin) == true) {
                    list.Add(enabledPlugin);
                }
                // else the plugin isn't enabled
                // todo potentially alter command result to say the plugin isn't enabled.
            }
            else {
                // Add all of the enabled plugins.
                list.AddRange(this.EnabledPlugins.Values);
            }

            return list;
        }
    }
}