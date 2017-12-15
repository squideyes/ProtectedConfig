// Copyright 2017 Louis S.Berman.
//
// This file is part of ProtectedConfig.
//
// ProtectedConfig is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation, either version 3 of the License, 
// or (at your option) any later version.
//
// ProtectedConfig is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with ProtectedConfig.  If not, see <http://www.gnu.org/licenses/>.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtectedConfig;
using System.IO;

namespace UnitTests
{
    [TestClass]
    public class ConfigManagerTests
    {
        private const string CODE = "CODE";

        [TestMethod]
        public void NullCanBeSetAndRetrieved()
        {
            using (var stream = new MemoryStream())
            {
                var source = new ConfigManager()
                    .Set<string>("CODE", null)
                    .Save(stream);

                stream.Position = 0;

                var target = new ConfigManager()
                    .Load(stream);

                Assert.IsNull(target.Get<string>(CODE));
            }
        }

        [TestMethod]
        public void LastSetWins()
        {
            const string NEWCODE = "XYZ987";

            var cm = new ConfigManager()
                .Set("CODE", "ABC123")
                .Set("CODE", NEWCODE);

            Assert.AreEqual(cm.Get<string>(CODE), NEWCODE);
        }

        [DataTestMethod]
        [ExpectedException(typeof(ConfigException))]
        [DataRow(" a")]
        [DataRow("a ")]
        [DataRow(" a ")]
        [DataRow(" ")]
        [DataRow("")]
        [DataRow(null)]
        public void BadSetKeyThrowsError(string key)
        {
            var cm = new ConfigManager()
                .Set(key, "ABC123");
        }

        [DataTestMethod]
        [ExpectedException(typeof(ConfigException))]
        [DataRow(" a")]
        [DataRow("a ")]
        [DataRow(" a ")]
        [DataRow(" ")]
        [DataRow("")]
        [DataRow(null)]
        public void BadGetKeyThrowsError(string key)
        {
            var value = new ConfigManager()
                .Get<string>(key);
        }

        [TestMethod]
        public void WithCurrentUserScopeReturnsSelf()
        {
            var cm = new ConfigManager();

            Assert.AreSame(cm, cm.WithCurrentUserScope());
        }

        [TestMethod]
        public void WithEntropyReturnsSelf()
        {
            var cm = new ConfigManager();

            Assert.AreSame(cm, cm.WithEntropy(new byte[] { 1, 2, 3 }));
        }

        [TestMethod]
        public void SetReturnsSelf()
        {
            var cm = new ConfigManager();

            Assert.AreSame(cm, cm.Set("CODE", "ABC123"));
        }

        [TestMethod]
        public void SaveToStreamReturnsSelf()
        {
            var cm = new ConfigManager()
                .Set(CODE, "ABC123");

            using (var stream = new MemoryStream())
                Assert.AreSame(cm, cm.Save(stream));
        }

        [TestMethod]
        public void LoadToStreamReturnsSelf()
        {
            using (var stream = new MemoryStream())
            {
                var source = new ConfigManager()
                    .Set(CODE, "ABC123")
                    .Save(stream);

                stream.Position = 0;

                var target = new ConfigManager();

                Assert.AreSame(target, target.Load(stream));
            }
        }

        [TestMethod]
        public void SaveThenLoadRoundtrips()
        {
            using (var stream = new MemoryStream())
            {
                var source = new ConfigManager()
                    .Set(CODE, "ABC123")
                    .Save(stream);

                stream.Position = 0;

                var target = new ConfigManager()
                    .Load(stream);

                Assert.AreEqual(source.Get<string>(CODE), target.Get<string>(CODE));
            }
        }

        [DataTestMethod]
        [DataRow("Password", "ABC123")]
        [DataRow("Guid", "H3I46109-B317-4FFD-8CA4-98E8975D6771")]
        [DataRow("ConnString", "Server=server;Database=db;Uid=uid;Pwd=pwd;")]
        private void GetEqualsSet(string key, object value)
        {
            var result = new ConfigManager()
                .Set(key, value)
                .Get<string>(key);

            Assert.AreEqual(result, value);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigException))]
        public void NullStreamThrowsErrorOnLoad()
        {
            new ConfigManager()
                .Load((Stream)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigException))]
        public void NullStreamThrowsErrorOnSave()
        {
            new ConfigManager()
                .Set(CODE, "ABC123")
                .Save((Stream)null);
        }

        [TestMethod]
        public void GetIgnoresKeyCase()
        {
            const string VALUE = "ABC123";

            var result = new ConfigManager()
                .Set("CODE", VALUE)
                .Get<string>("cOdE");

            Assert.AreEqual(result, VALUE);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigException))]
        public void SaveWithEntropyThenLoadWithoutFails()
        {
            var entropy = new byte[] { 1, 2, 3, 4, 5 };

            using (var stream = new MemoryStream())
            {
                var source = new ConfigManager()
                    .WithEntropy(entropy)
                    .Set(CODE, "ABC123")
                    .Save(stream);

                stream.Position = 0;

                var target = new ConfigManager()
                    .Load(stream);
            }
        }

        [TestMethod]
        public void SaveAndLoadWithEntropyWorks()
        {
            var entropy = new byte[] { 1, 2, 3, 4, 5 };

            using (var stream = new MemoryStream())
            {
                var source = new ConfigManager()
                    .WithEntropy(entropy)
                    .Set(CODE, "ABC123")
                    .Save(stream);

                stream.Position = 0;

                var target = new ConfigManager()
                    .WithEntropy(entropy)
                    .Load(stream);

                Assert.AreEqual(source.Get<string>(CODE), target.Get<string>(CODE));
            }
        }
    }
}