using Microsoft.Extensions.Configuration;
using System;

namespace FitnessClub.BLL
{
    public static class AppSettings
    {
        private static IConfiguration? _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private static T GetRequiredConfigValue<T>(string key) where T : IParsable<T>
        {
            var value = _configuration?[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Configuration value for '{key}' is missing or empty.");
            }
            if (T.TryParse(value, null, out var result))
            {
                return result;
            }
            throw new InvalidOperationException($"Configuration value for '{key}' ('{value}') could not be parsed as {typeof(T).Name}.");
        }

         private static T GetRequiredConfigValue<T>(string key, Func<string, T> parser)
        {
            var value = _configuration?[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Configuration value for '{key}' is missing or empty.");
            }
            try
            {
                return parser(value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Configuration value for '{key}' ('{value}') could not be parsed as {typeof(T).Name}.", ex);
            }
        }

        public static class MembershipSettings
        {
            public static decimal SingleClubPrice => GetRequiredConfigValue<decimal>("MembershipSettings:SingleClubPrice", decimal.Parse);
            public static decimal NetworkPrice => GetRequiredConfigValue<decimal>("MembershipSettings:NetworkPrice", decimal.Parse);
            public static int SingleClubDurationDays => GetRequiredConfigValue<int>("MembershipSettings:SingleClubDurationDays", int.Parse);
            public static int NetworkDurationDays => GetRequiredConfigValue<int>("MembershipSettings:NetworkDurationDays", int.Parse);
        }

        public static class ClassSettings
        {
            
            public static int DefaultCapacity => GetRequiredConfigValue<int>("ClassSettings:DefaultCapacity", int.Parse);
            public static int YogaCapacity => GetRequiredConfigValue<int>("ClassSettings:YogaCapacity", int.Parse);
            public static int PilatesCapacity => GetRequiredConfigValue<int>("ClassSettings:PilatesCapacity", int.Parse);
            public static int BoxingCapacity => GetRequiredConfigValue<int>("ClassSettings:BoxingCapacity", int.Parse);
            public static int StretchingCapacity => GetRequiredConfigValue<int>("ClassSettings:StretchingCapacity", int.Parse);
            public static int FitnessCapacity => GetRequiredConfigValue<int>("ClassSettings:FitnessCapacity", int.Parse);
            public static int CrossfitCapacity => GetRequiredConfigValue<int>("ClassSettings:CrossfitCapacity", int.Parse);
        }

        public static class BookingSettings
        {
            public static int MaxBookingsPerUser => GetRequiredConfigValue<int>("BookingSettings:MaxBookingsPerUser", int.Parse);
            public static int BookingWindowDays => GetRequiredConfigValue<int>("BookingSettings:BookingWindowDays", int.Parse);
        }

        public static class ClassTypes
        {
            public static readonly string[] AllTypes = new[]
            {
                "Йога",
                "Пілатес",
                "Бокс",
                "Розтяжка",
                "Фітнес",
                "Кросфіт"
            };
        }
    }
}