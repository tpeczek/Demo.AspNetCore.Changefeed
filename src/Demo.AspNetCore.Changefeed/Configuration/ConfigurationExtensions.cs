using System;
using Microsoft.Extensions.Configuration;

namespace Demo.AspNetCore.Changefeed.Configuration
{
    internal static class ConfigurationExtensions
    {
        private const string CHANGEFEED_TYPE_CONFIGURATION_KEY = "ChangefeedService";

        public static ChangefeedServices GetChangefeedService(this IConfiguration configuration)
        {
            string changefeedServiceConfigurationValue = configuration.GetValue(CHANGEFEED_TYPE_CONFIGURATION_KEY, String.Empty);

            if (!Enum.TryParse(changefeedServiceConfigurationValue, true, out ChangefeedServices changefeedService))
            {
                throw new NotSupportedException($"Not supported changefeed type.");
            }

            return changefeedService;
        }
    }
}
