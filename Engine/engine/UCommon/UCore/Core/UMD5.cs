using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace UEngine
{
    public static class UMD5
    {
        // Hash an input file and return the hash as
        // a 32 character hexadecimal string.
        public static string GetFileMD5Hash(string filePath)
        {
            byte[] bytes = UFileAccessor.ReadBinaryFile(filePath);
            return GetMD5Hash(bytes);
        }

        // Hash an input string and return the hash as
        // a 32 character hexadecimal string.
        public static string GetMD5Hash(string input)
        {
            return GetMD5Hash(Encoding.Default.GetBytes(input));
        }

        // Hash an input bytes and return the hash as
        // a 32 character hexadecimal string.
        public static string GetMD5Hash(byte[] input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(input);

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a file.
        public static bool VerifyFileMD5Hash(string filePath, string hash)
        {
            byte[] bytes = UFileAccessor.ReadBinaryFile(filePath);
            return VerifyMD5Hash(bytes, hash);
        }

        // Verify a hash against a string.
        public static bool VerifyMD5Hash(string input, string hash)
        {
            return VerifyMD5Hash(Encoding.Default.GetBytes(input), hash);
        }

        // Verify a hash against a bytes.
        public static bool VerifyMD5Hash(byte[] input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMD5Hash(input);

            // Create a StringComparer an comare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
