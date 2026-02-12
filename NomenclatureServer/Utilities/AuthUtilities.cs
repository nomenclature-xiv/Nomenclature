using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NomenclatureServer.Utilities
{
    /// <summary>
    ///     Handles the generation of secrets to be used in the plugin
    /// </summary>
    public static class AuthUtilities
    {
        // Const
        private const string SecretCharSet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        private const int SecretLength = 64;

        private const string FriendCharSet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int FriendLength = 14;

        public static string GenerateSecret() => Generate(SecretCharSet, SecretLength);
        public static string GenerateFriendCode() => Generate(FriendCharSet, FriendLength);

        /// <summary>
        ///     Generates a new secret following established criteria
        /// </summary>
        private static string Generate(string charset, int length)
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);

            var secret = new char[length];
            for (var i = 0; i < length; i++)
                secret[i] = charset[bytes[i] % charset.Length];

            return new string(secret);
        }
    }
}
