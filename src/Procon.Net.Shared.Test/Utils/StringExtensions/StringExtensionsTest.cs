using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Procon.Net.Shared.Utils;

namespace Procon.Net.Shared.Test.Utils.StringExtensions {

    /// <summary>
    /// Full of one-liner tests for the simple extension methods we have.
    /// </summary>
    [TestFixture]
    public class StringExtensionsTest {

        /// <summary>
        /// Tests calling random string with 20 calls won't produce a duplicate. If this simple test produces a
        /// collision then for our purposes 
        /// </summary>
        [Test]
        public void TestRandomStringSuccess() {
            List<string> generated = new List<string>();

            for (var count = 0; count < 20; count++) {
                generated.Add(Shared.Utils.StringExtensions.RandomString(30));
            }

            Assert.AreEqual(20, generated.Distinct().Count());
        }

        /// <summary>
        /// Tests that a string will be split into multiple strings for wrapping
        /// </summary>
        [Test]
        public void TestWordWrapSuccess() {
            List<string> wrapped = "Die Br�sel eines K�sekuchen verz�cken mich.".WordWrap(30);

            Assert.AreEqual(2, wrapped.Count);
            Assert.AreEqual("Die Br�sel eines K�sekuchen", wrapped.First());
            Assert.AreEqual("verz�cken mich.", wrapped.Last());
        }

        /// <summary>
        /// Tests that a string with diacritics is replaced.
        /// </summary>
        [Test]
        public void TestRemoveDiacriticsSuccess() {
            String removed = "Die Br�sel eines K�sekuchen verz�cken mich.".RemoveDiacritics();

            Assert.AreEqual("Die Brosel eines Kasekuchen verzucken mich.", removed);
        }

        /// <summary>
        /// Tests that a poxy leet speek text will be converted to something some-what legible.
        /// </summary>
        [Test]
        public void TestRemoveLeetSpeekSuccess() {
            String removed = "P]-[0gu3".RemoveLeetSpeek();

            Assert.AreEqual("PHOguE", removed);
        }

        /// <summary>
        /// Tests that a string with leet speek and diacritics will be stripped to a plain ASCII string
        /// </summary>
        [Test]
        public void TestStripSuccess() {
            String removed = "P]-[0gu3 Br�sel".Strip();

            Assert.AreEqual("PHOguE Brosel", removed);
        }

        /// <summary>
        /// Tests a directory with forward slashes will be sanitized.
        /// </summary>
        [Test]
        public void TestSanitizeDirectoryForwardSlashes() {
            String sanitized = @"c:/projects/something/test/".SanitizeDirectory();

            Assert.AreEqual("c_projects_something_test", sanitized);
        }

        /// <summary>
        /// Tests a directory/file name with back slashes will be sanitized.
        /// </summary>
        [Test]
        public void TestSanitizeDirectoryBackSlashes() {
            String sanitized = @"c:\projects\something\test\".SanitizeDirectory();

            Assert.AreEqual("c_projects_something_test", sanitized);
        }

        /// <summary>
        /// Tests a url is slugged, but this is a little off at the moment. I would expect "forum-myrcon-com"
        /// instead of the current result.
        /// </summary>
        [Test]
        public void TestUrlSlug() {
            String slugged = "http://forum.myrcon.com".UrlSlug();

            Assert.AreEqual("forummyrconcom", slugged);
        }
    }
}
