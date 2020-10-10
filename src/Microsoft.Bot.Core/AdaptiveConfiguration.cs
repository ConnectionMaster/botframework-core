// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Core
{
    public class AdaptiveConfiguration
    {
        private const string SupportedSeparator = ":";

        static readonly Regex UnsupportedSeparators = new Regex("(\\.|_)+");

        private readonly IConfiguration configuration;

        public AdaptiveConfiguration(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void SetValue(string path, object value)
        {
            throw new InvalidOperationException("Assignment expressions are not supported.");
        }

        public bool TryGetValue(string path, out object value)
        {
            if (path == null) { throw new ArgumentNullException(nameof(path)); }

            string configurationPath = UnsupportedSeparators.Replace(path, SupportedSeparator);

            IConfigurationSection section = this.configuration.GetSection(configurationPath);
            if (section.Exists())
            {
                value = section.Value;
                return true;
            }

            value = null;
            return false;
        }

        public string Version()
        {
            return 1.ToString(CultureInfo.InvariantCulture);
        }
    }
}
