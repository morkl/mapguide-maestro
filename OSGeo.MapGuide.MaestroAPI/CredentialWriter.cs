﻿#region Disclaimer / License

// Copyright (C) 2012, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

#endregion Disclaimer / License

using OSGeo.MapGuide.MaestroAPI;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using ObjCommon = OSGeo.MapGuide.ObjectModels.Common;

namespace OSGeo.MapGuide.ObjectModels.FeatureSource
{
    /// <summary>
    /// Extension methods for feature sources
    /// </summary>
    public static class FeatureSourceCredentialExtensions
    {
        /// <summary>
        /// Sets the encrypted credentials for this Feature Source, the credentials are referenced with the %MG_USERNAME%
        /// and %MG_PASSWORD% placeholder tokens in the Feature Source content.
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="conn"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public static void SetEncryptedCredentials(this IFeatureSource fs, IServerConnection conn, string username, string password)
        {
            Check.ArgumentNotNull(fs, nameof(fs));
            if (string.IsNullOrEmpty(fs.ResourceID))
                throw new ArgumentException(Strings.ErrorNoResourceIdAttached);
            using (var stream = CredentialWriter.Write(username, password))
            {
                conn.ResourceService.SetResourceData(fs.ResourceID, StringConstants.MgUserCredentialsResourceData, ObjCommon.ResourceDataType.String, stream);
            }
        }

        /// <summary>
        /// Gets the encrypted username referenced by the %MG_USERNAME% placeholder token in the Feature Source content
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static string GetEncryptedUsername(this IFeatureSource fs, IServerConnection conn)
        {
            Check.ArgumentNotNull(fs, nameof(fs));
            var resData = conn.ResourceService.EnumerateResourceData(fs.ResourceID);
            foreach (var rd in resData.ResourceData)
            {
                if (rd.Name.ToUpper() == StringConstants.MgUserCredentialsResourceData)
                {
                    using (var sr = new StreamReader(conn.ResourceService.GetResourceData(fs.ResourceID, StringConstants.MgUserCredentialsResourceData)))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Utility class to create encrypted Feature Source credentials
    /// </summary>
    internal static class CredentialWriter
    {
        //NOTE: This is a verbatim copy of MgCryptographyUtil in MgDev\Common\Security to the best I can do in .net
        //Only the encryption bits are implemented. Maestro has no need to decrypt MG_USER_CREDENTIALS

        //I'm sure this particular key isn't meant to be made public, but being able to correctly
        //write MG_USER_CREDENTIALS trumps this concern. Besides, if this key were to be truly private, it wouldn't be publicly visible
        //in the source code of a publicly accessible repository now would it?
        private const string MG_CRYPTOGRAPHY_PRIVATE_KEY = "WutsokeedbA"; //NOXLATE

        private static readonly char[] MG_CRYPTOGRAPHY_DEC_CHARS = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }; //NOXLATE
        private static readonly char[] MG_CRYPTOGRAPHY_HEX_CHARS = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' }; //NOXLATE
        private const int MG_CRYPTOGRAPHY_MAGIC_NUMBER_1 = 42; //NOXLATE
        private const int MG_CRYPTOGRAPHY_MAGIC_NUMBER_2 = 3;
        private const int MG_CRYPTOGRAPHY_MIN_COLUMN_NUMBER = 5;

        private const int MIN_CIPHER_TEXT_LENGTH = 34;
        private const int MIN_KEY_LENGTH = 14;
        private const int MAX_KEY_LENGTH = 32;
        private const string STRING_DELIMITER = "\v"; //NOXLATE
        private const string RESERVED_CHARACTERS_STRINGS = "\v\f"; //NOXLATE
        private const string RESERVED_CHARACTERS_CREDENTIALS = "\t\r\n\v\f"; //NOXLATE

        /// <summary>
        /// Encrypts the specified credentials. For a feature source that uses %MG_USERNAME% and %MG_PASSWORD% placeholder tokens to
        /// properly use these encrypted credentials, the encrypted string must be set as the MG_USER_CREDENTIALS resource data item
        /// for that feature source
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>A <see cref="T:System.IO.Stream"/> containing the encrypted credentials</returns>
        public static Stream Write(string username, string password)
        {
            string credentials;
            EncryptStrings(username, password, out credentials, RESERVED_CHARACTERS_CREDENTIALS);
            return new MemoryStream(ASCIIEncoding.Default.GetBytes(credentials));
        }

        private static void EncryptStrings(string plainText1, string plainText2, out string cipherText, string reservedCharacters)
        {
            var reservedChars = reservedCharacters.ToCharArray();
            if (plainText1.IndexOfAny(reservedChars) >= 0)
            {
                throw new ArgumentException(string.Format(Strings.ErrorArgContainsReservedCharacters, "plainText1")); //NOXLATE
            }
            if (plainText2.IndexOfAny(reservedChars) >= 0)
            {
                throw new ArgumentException(string.Format(Strings.ErrorArgContainsReservedCharacters, "plainText2")); //NOXLATE
            }

            string publicKey;
            GenerateCryptographKey(out publicKey);

            string tmpStr1, tmpStr2;
            CombineStrings(plainText1, plainText2, out tmpStr1);
            EncryptStringWithKey(tmpStr1, out tmpStr2, publicKey);

            CombineStrings(tmpStr2, publicKey, out tmpStr1);
            EncryptStringWithKey(tmpStr1, out tmpStr2, MG_CRYPTOGRAPHY_PRIVATE_KEY);

            EncryptStringByTransposition(tmpStr2, out cipherText);
        }

        private static void EncryptStringByTransposition(string inStr, out string outStr)
        {
            string tmpStr;
            int inStrLength = inStr.Length;

            int numOfColumn = MG_CRYPTOGRAPHY_MIN_COLUMN_NUMBER;
            EncryptStringByTransposition(inStr, out tmpStr, numOfColumn);

            numOfColumn += inStrLength % 6;
            EncryptStringByTransposition(tmpStr, out outStr, numOfColumn);
            Debug.Assert(inStrLength == outStr.Length);
        }

        private static void EncryptStringByTransposition(string inStr, out string outStr, int numOfColumn)
        {
            int inStrLen = inStr.Length;
            int numOfRow = (int)Math.Ceiling((double)inStrLen / (double)numOfColumn);

            StringBuilder sb = new StringBuilder();

            for (int currColumn = 0; currColumn < numOfColumn; ++currColumn)
            {
                for (int currRow = 0; currRow < numOfRow; ++currRow)
                {
                    int inIdx = currColumn + currRow * numOfColumn;

                    if (inIdx < inStrLen)
                    {
                        sb.Append(inStr[inIdx]);
                    }
                }
            }

            outStr = sb.ToString();
        }

        private static void GenerateCryptographKey(out string publicKey)
        {
            DateTime dt = DateTime.UtcNow;
            publicKey = dt.ToString("yyyymmddHHmmss"); //NOXLATE
        }

        private static void CombineStrings(string str1, string str2, out string outStr)
        {
            outStr = str1;
            outStr += STRING_DELIMITER;
            outStr += str2;
        }

        private static void EncryptStringWithKey(string inStr, out string outStr, string key)
        {
            char prevChar = Convert.ToChar(MG_CRYPTOGRAPHY_MAGIC_NUMBER_1);
            char currChar;
            int keyIdx = 0;
            int keyLen = key.Length;
            int outStrLen = inStr.Length;
            StringBuilder tmpStr = new StringBuilder();

            for (int i = 0; i < outStrLen; ++i)
            {
                currChar = inStr[i];
                char c = Convert.ToChar(currChar ^ key[keyIdx] ^ prevChar ^ ((i / MG_CRYPTOGRAPHY_MAGIC_NUMBER_2) % 255));
                tmpStr.Append(c);
                prevChar = currChar;

                ++keyIdx;

                if (keyIdx >= keyLen)
                {
                    keyIdx = 0;
                }
            }

            BinToHex(tmpStr.ToString(), out outStr);
            Debug.Assert((inStr.Length * 2) == outStr.Length);
        }

        private static void BinToHex(string binStr, out string hexStr)
        {
            int binStrLen = binStr.Length;

            hexStr = string.Empty;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < binStrLen; ++i)
            {
                int num = binStr[i];

                for (int j = 1; j >= 0; --j)
                {
                    char c = MG_CRYPTOGRAPHY_HEX_CHARS[(num >> j * 4) & 0xF];
                    sb.Append(c);
                }
            }

            hexStr = sb.ToString();
        }
    }
}