using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace FitnessClub.BLL.Helpers
{
    public static class ValidationHelper
    {
        private static readonly Regex NameRegex = new Regex(@"^[\p{L}\s'-]+$", RegexOptions.Compiled);
        private const int MaxNameLength = 50;
        private const int MinNameLength = 2;
        private const int MinUsernameLength = 3;
        private const int MaxUsernameLength = 20;
        private const int MinPasswordLength = 6;

        public static bool IsValidUsername(string? username)
        {
            return !string.IsNullOrWhiteSpace(username) && username.Length >= MinUsernameLength && username.Length <= MaxUsernameLength;
        }

        public static bool IsValidPassword(string? password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= MinPasswordLength;
        }

        public static bool IsValidName(string? name)
        {
            return !string.IsNullOrWhiteSpace(name) 
                   && name.Length >= MinNameLength 
                   && name.Length <= MaxNameLength 
                   && NameRegex.IsMatch(name);
        }
    }
}