﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Procon.Net.Shared.Truths;
using Procon.Net.Shared.Truths.Agents;
using Procon.Net.Shared.Truths.Goals;
using Procon.Net.Shared.Truths.Streams;
namespace Procon.Net.Shared.Test.Truths {
    [TestFixture]
    public class TestTruthsMerge {
        /// <summary>
        /// Tests the root identical node will be merged properly
        /// </summary>
        [Test]
        public void TestMergeTwoBranchesWithIdenticalRootNode() {
            Tree tree = Tree.Union(
                BranchBuilder.ProtocolCanKillPlayer(),
                BranchBuilder.ProtocolKnowsWhenPlayerKillPlayer()
            );

            Assert.AreEqual(1, tree.Count);
            Assert.IsTrue(tree.BuildAndTest(new ProtocolAgent(), new CanFlow(), new KillGoal(), new PlayerAgent()));
            Assert.IsTrue(tree.BuildAndTest(new ProtocolAgent(), new KnowsWhenFlow(), new PlayerAgent(), new KillGoal(), new PlayerAgent()));
        }

        /// <summary>
        /// Tests that two trees can be combined, then tested with the the built branch.
        /// </summary>
        [Test]
        public void TestMergeTwoBranchesWithIdenticalRootNodeTestWithBuiltBranch() {
            Tree tree = Tree.Union(
                BranchBuilder.ProtocolCanKillPlayer(),
                BranchBuilder.ProtocolKnowsWhenPlayerKillPlayer()
            );

            Assert.IsTrue(tree.Test(BranchBuilder.ProtocolCanKillPlayer()));
        }
    }
}