using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace FitnessClub.BLL.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsValidUsername(string? username)
        {
            return !string.IsNullOrWhiteSpace(username) && username.Length >= 3 && username.Length <= 20;
        }

        public static bool IsValidPassword(string? password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
        }

        public static bool IsValidName(string? name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Length >= 2 && name.Length <= 50 && !name.Any(char.IsDigit);
        }

        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}