using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Koala.Core
{
    public static class RandomGeneratorExtensions
    {
        public static string GetUniqueId<T>(this T type, bool? prependType = default, string separate = default)
        {
            return (type == null ? typeof(T).Name : type.GetType().Name).GetUniqueId(prependType, separate);
        }

        public static string GetUniqueId(this string value, bool? prependValue = default, string separate = default)
        {
            var append = string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : prependValue ?? true ? $"{value.Trim()}{separate ?? ":"}" : $"{separate ?? ":"}{value.Trim()}";

            return (prependValue ?? true)
                ? $"{append}{Guid.NewGuid().ToString().Replace("-", string.Empty)}"
                : $"{Guid.NewGuid().ToString().Replace("-", string.Empty)}{append}"
                    .ToLower()
                    .Trim();
        }

        public static string GetRandomName<T>(this T type, bool? prependType = default, string separate = default)
        {
            return (type == null ? typeof(T).Name : type.GetType().Name).GetRandomName(prependType, separate);
        }

        public static string GetRandomName(this string value, bool? prependValue = default, string separate = default)
        {
            var append = string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : prependValue ?? true ? $"{value.Trim()}{separate ?? ":"}" : $"{separate ?? ":"}{value.Trim()}";

            return (prependValue ?? true)
                ? $"{append}{append.GenerateRandom()}"
                : $"{append.GenerateRandom()}{append}"
                    .ToLower()
                    .Trim();
        }

        public static string GenerateRandom(this string input, int length = 7)
        {
            char[] stringChars = new char[length];
            byte[] randomBytes = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = GenerateRandomCharacters[randomBytes[i] % GenerateRandomCharacters.Length];
            }

            return new string(stringChars);
        }

        public static string GenerateUniqueString(this string input, int length = 32)
        {
            var stringChars = new char[length];
            var randomBytes = new byte[length];

            var dateTimeCharStack = new Stack<char>(DateTime.Now.ToString(DateTimeStringFormat));

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            var alternate = false;

            for (var i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = alternate
                    ? dateTimeCharStack.Count > 0
                        ? dateTimeCharStack.Pop()
                        : GenerateUniqueStringCharacters[randomBytes[i] % GenerateUniqueStringCharacters.Length]
                    : GenerateUniqueStringCharacters[randomBytes[i] % GenerateUniqueStringCharacters.Length];

                alternate = !alternate;
            }

            return new string(stringChars);
        }

        public static string NormalizeStringSpaces(this string input, bool? replaceSlashN = default, bool? replaceSlashR = default)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var trimmedInput = input.Trim();

            if (replaceSlashN ?? true) trimmedInput = trimmedInput.Replace("\n", "");
            if (replaceSlashR ?? true) trimmedInput = trimmedInput.Replace("\r", "");

            var currSpaceString = string.Empty;
            var processedInput = string.Empty;

            foreach (char c in trimmedInput)
            {
                if (c != ' ')
                {
                    if (currSpaceString.Length > 0)
                    {
                        processedInput = $"{processedInput}{" "}";
                        currSpaceString = string.Empty;
                    }

                    processedInput = $"{processedInput}{c}";
                }

                if (c == ' ')
                {
                    currSpaceString = $"{currSpaceString}{c}";
                }
            }

            return processedInput;
        }

        private const string DateTimeStringFormat = "yyMMddHHmmssfff";
        private const string GenerateRandomCharacters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string GenerateUniqueStringCharacters = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz";
    }
}
