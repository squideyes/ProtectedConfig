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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace ProtectedConfig
{
    public class ConfigManager
    {
        private Dictionary<string, object> keyValues = new Dictionary<string, object>();
        private DataProtectionScope scope = DataProtectionScope.LocalMachine;
        private byte[] entropy = null;

        private readonly IFileSystem fileSystem;

        public ConfigManager(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public ConfigManager()
            : this(new FileSystem())
        {
        }

        public ConfigManager WithEntropy(byte[] entropy)
        {
            this.entropy = entropy;

            return this;
        }

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

        private Exception GetConfigFileError(string fileName, string verb, Exception error) =>
            new ConfigException($"The \"{fileName}\" configuration file cannot be {verb}!", error);

        public ConfigManager Save(string fileName)
        {
            if (!fileName.IsFileName(false))
                throw new ConfigException($"The \"{fileName}\" filename is invalid!");

            try
            {
                using (var stream = fileSystem.File.OpenWrite(fileName))
                    return Save(stream);
            }
            catch (Exception error)
            {
                throw GetConfigFileError(fileName, "saved", error);
            }
        }

        public ConfigManager Save(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanWrite)
                throw new Exception("The stream does not support writing!");

            var bytes = Encoding.ASCII.GetBytes(
                JsonConvert.SerializeObject(keyValues, Formatting.None));

            EncryptToStream(bytes, stream);

            return this;
        }

        public ConfigManager Load(string fileName)
        {
            if (!fileName.IsFileName(false))
                throw new ConfigException($"The \"{fileName}\" filename is invalid!");

            try
            {
                using (var stream = fileSystem.File.OpenRead(fileName))
                    return Load(stream);
            }
            catch (Exception error)
            {
                throw GetConfigFileError(fileName, "loaded", error);
            }
        }

        public ConfigManager Load(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
                throw new Exception("The stream does not support reading!");

            var json = Encoding.ASCII.GetString(DecryptFromStream(stream));

            keyValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            return this;
        }

        private void EncryptToStream(byte[] bytes, Stream stream)
        {
            int length = 0;

            var data = ProtectedData.Protect(bytes, entropy, scope);

            if (stream.CanWrite && data != null)
            {
                stream.Write(data, 0, data.Length);

                length = data.Length;
            }
        }

        private byte[] DecryptFromStream(Stream stream)
        {
            var reader = new BinaryReader(stream);

            var input = reader.ReadToEnd();

            return ProtectedData.Unprotect(input, entropy, scope);
        }
    }
}
