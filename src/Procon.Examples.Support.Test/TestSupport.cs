﻿using System.Linq;
using NUnit.Framework;
using Procon.Core.Connections.Plugins;
using Procon.Core.Shared;
using Procon.Net.Shared;
using Procon.Net.Shared.Models;
using Procon.Net.Shared.Truths;

namespace Procon.Examples.Support.Test {
    [TestFixture]
    public class TestSupport {

        /// <summary>
        /// Test for you to debug.
        /// Set a breakpoint within Procon.Examples.Support.TestSupportToKillPlayersUsingBranchBuilder
        /// </summary>
        [Test]
        public void TestSupportUsingBranchBuilderTrue() {
            // Create a new plugin controller to load up the test plugin
            CorePluginController plugins = new CorePluginController().Execute() as CorePluginController;

            // Enable the single plugin that was loaded, otherwise it won't recieve any tunneled
            // commands or events.
            plugins.Tunnel(new Command() {
                Origin = CommandOrigin.Local,
                CommandType = CommandType.PluginsEnable,
                Scope = {
                    PluginGuid = plugins.LoadedPlugins.First().PluginGuid
                }
            });

            // Every game event will update the gamestate within the plugin.
            plugins.PluginFactory.GameEvent(new GameEventArgs() {
                GameEventType = GameEventType.GameSettingsUpdated,
                // This would generally be a persistant object that Procon updates with all known information.
                GameState = new GameState() {
                    Support = Tree.Union(
                        BranchBuilder.ProtocolCanKillPlayer(),
                        BranchBuilder.ProtocolKnowsWhenPlayerKillPlayer(),
                        BranchBuilder.ProtocolKnowsWhenPlayerChatToEveryone(),
                        BranchBuilder.ProtocolKnowsWhenPlayerChatToGroup()
                    )
                }
            });

            CommandResultArgs result = plugins.Tunnel(new Command() {
                Name = "TestSupportToKillPlayersUsingBranchBuilder",
                // We're cheating a little bit here and just saying the command came from
                // "local" as in it was generated by Procon itself.
                Origin = CommandOrigin.Local
            });

            Assert.AreEqual("True", result.Message);
        }

        /// <summary>
        /// Test for you to debug.
        /// Set a breakpoint within Procon.Examples.Support.TestSupportToKillPlayersUsingBranchBuilder
        /// </summary>
        [Test]
        public void TestSupportUsingBranchBuilderFalse() {
            // Create a new plugin controller to load up the test plugin
            CorePluginController plugins = new CorePluginController().Execute() as CorePluginController;

            // Enable the single plugin that was loaded, otherwise it won't recieve any tunneled
            // commands or events.
            plugins.Tunnel(new Command() {
                Origin = CommandOrigin.Local,
                CommandType = CommandType.PluginsEnable,
                Scope = {
                    PluginGuid = plugins.LoadedPlugins.First().PluginGuid
                }
            });

            // Every game event will update the gamestate within the plugin.
            plugins.PluginFactory.GameEvent(new GameEventArgs() {
                GameEventType = GameEventType.GameSettingsUpdated,
                // This would generally be a persistant object that Procon updates with all known information.
                GameState = new GameState() {
                    Support = Tree.Union(
                        // BranchBuilder.ProtocolCanKillPlayer(),
                        BranchBuilder.ProtocolKnowsWhenPlayerKillPlayer(),
                        BranchBuilder.ProtocolKnowsWhenPlayerChatToEveryone(),
                        BranchBuilder.ProtocolKnowsWhenPlayerChatToGroup()
                    )
                }
            });

            CommandResultArgs result = plugins.Tunnel(new Command() {
                Name = "TestSupportToKillPlayersUsingBranchBuilder",
                // We're cheating a little bit here and just saying the command came from
                // "local" as in it was generated by Procon itself.
                Origin = CommandOrigin.Local
            });

            Assert.AreEqual("False", result.Message);
        }

        /// <summary>
        /// Test for you to debug.
        /// Set a breakpoint within Procon.Examples.Support.TestSupportCustomBuildAndTest
        /// </summary>
        [Test]
        public void TestSupportUsingTestSupportCustomBuildAndTestTrue() {
            // Create a new plugin controller to load up the test plugin
            CorePluginController plugins = new CorePluginController().Execute() as CorePluginController;

            // Enable the single plugin that was loaded, otherwise it won't recieve any tunneled
            // commands or events.
            plugins.Tunnel(new Command() {
                Origin = CommandOrigin.Local,
                CommandType = CommandType.PluginsEnable,
                Scope = {
                    PluginGuid = plugins.LoadedPlugins.First().PluginGuid
                }
            });

            // Every game event will update the gamestate within the plugin.
            plugins.PluginFactory.GameEvent(new GameEventArgs() {
                GameEventType = GameEventType.GameSettingsUpdated,
                // This would generally be a persistant object that Procon updates with all known information.
                GameState = new GameState() {
                    Support = Tree.Union(
                        BranchBuilder.ProtocolCanKillPlayer(),
                        BranchBuilder.ProtocolKnowsWhenPlayerKillPlayer(),
                        BranchBuilder.ProtocolKnowsWhenPlayerChatToEveryone(),
                        BranchBuilder.ProtocolKnowsWhenPlayerChatToGroup()
                    )
                }
            });

            CommandResultArgs result = plugins.Tunnel(new Command() {
                Name = "TestSupportCustomBuildAndTest",
                // We're cheating a little bit here and just saying the command came from
                // "local" as in it was generated by Procon itself.
                Origin = CommandOrigin.Local
            });

            Assert.AreEqual("True", result.Message);
        }
    }
}
