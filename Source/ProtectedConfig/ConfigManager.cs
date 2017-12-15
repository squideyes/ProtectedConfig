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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace ProtectedConfig
{
    public class ConfigManager
    {
        private Dictionary<string, object> keyValues = new Dictionary<string, object>();
        private DataProtectionScope scope = DataProtectionScope.LocalMachine;
        private byte[] entropy = null;

        public ConfigManager WithCurrentUserScope()
        {
            scope = DataProtectionScope.CurrentUser;

            return this;
        }

        public ConfigManager Set<T>(string key, T value)
        {
            if (!key.IsKey())
                throw new ConfigException($"The \"{key}\" key is invalid!");

            keyValues[key.ToUpper()] = value;

            return this;
        }

        public T Get<T>(string key)
        {
            if (!key.IsKey())
                throw new ConfigException($"The \"{key}\" key is invalid!");

            key = key.ToUpper();

            return keyValues.ContainsKey(key) ? (T)keyValues[key] : default(T);
        }

        public ConfigManager Load(Stream stream)
        {
            if (stream == null)
                throw new ConfigException("The \"stream\" parameter is null.");

            try
            {
                var reader = new BinaryReader(stream);

                var bytes = ProtectedData.Unprotect(
                    reader.ReadToEnd(), entropy, scope);

                var formatter = new BinaryFormatter();

                using (var ms = new MemoryStream(bytes))
                    keyValues = (Dictionary<string, object>)formatter.Deserialize(ms);

                return this;
            }
            catch (Exception error)
            {
                throw new ConfigException(
                    "The config data could not be deserialized!", error);
            }
        }

        public ConfigManager Load(string fileName)
        {
            if (!fileName.IsFileName())
                throw new ConfigException($"The \"{fileName}\" filename is invalid!");

            if (!File.Exists(fileName))
                return this;

            try
            {
                var bytes = ProtectedData.Unprotect(
                    File.ReadAllBytes(fileName), entropy, scope);

                using (var stream = new MemoryStream(bytes))
                    return Load(stream);
            }
            catch (Exception error)
            {
                throw GetConfigError(fileName, "loaded", error);
            }
        }

        public ConfigManager Save(Stream stream)
        {
            if (stream == null)
                throw new ConfigException("The \"stream\" parameter is null.");

            if (keyValues.Count == 0)
                return this;

            try
            {
                var formatter = new BinaryFormatter();

                using (var ms = new MemoryStream())
                {
                    formatter.Serialize(ms, keyValues);

                    var data = ProtectedData.Protect(
                        ms.ToArray(), entropy, scope);

                    stream.Write(data, 0, data.Length);
                }

                return this;
            }
            catch (Exception error)
            {
                throw new ConfigException(
                    "The config data could not be serialized!", error);
            }
        }

        public ConfigManager Save(string fileName)
        {
            if (!fileName.IsFileName())
                throw new ConfigException($"The \"{fileName}\" filename is invalid!");

            try
            {
                using (var stream = File.OpenWrite(fileName))
                    Save(stream);

                return this;
            }
            catch (Exception error)
            {
                throw GetConfigError(fileName, "saved", error);
            }
        }

        public ConfigManager WithEntropy(byte[] value)
        {
            if (value != null || value.Length == 0)
                value = null;

            entropy = value;

            return this;
        }

        private Exception GetConfigError(string fileName, string verb, Exception error)
        {
            return new ConfigException(
                $"The \"{fileName}\" configuration file cannot be {verb}!", error);
        }
    }
}
