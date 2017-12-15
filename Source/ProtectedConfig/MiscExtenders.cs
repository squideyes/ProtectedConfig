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
using System.IO;

namespace ProtectedConfig
{
    public static class MiscExtenders
    {
        public static bool IsKey(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (value != value.Trim())
                return false;

            return true;
        }

        public static byte[] ReadToEnd(this BinaryReader reader)
        {
            const int BUFFERSIZE = 4096;

            using (var ms = new MemoryStream())
            {
                var buffer = new byte[BUFFERSIZE];

                int count;

                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);

                return ms.ToArray();
            }
        }

        public static bool IsFileName(this string value, bool mustBeRooted = true)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            try
            {
                var fileInfo = new FileInfo(value);

                if (!mustBeRooted)
                    return true;
                else
                    return Path.IsPathRooted(value);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (PathTooLongException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }
    }
}
