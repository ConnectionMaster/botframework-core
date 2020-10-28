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

        private static readonly Regex UnsupportedSeparators = new Regex("(\\.|_)+");

        private readonly IConfiguration _configuration;

        public AdaptiveConfiguration(IConfiguration configuration)
        {
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        // SetValue function with below signature is required to bind to IMemory interface, which is required
        // to evaluate adaptive expressions.
#pragma warning disable CA1801 // Review unused parameters
        public void SetValue(string path, object value)
#pragma warning restore CA1801 // Review unused parameters
        {
            throw new InvalidOperationException("Assignment expressions are not supported.");
        }

        public bool TryGetValue(string path, out object value)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            string configurationPath = UnsupportedSeparators.Replace(path, SupportedSeparator);

            IConfigurationSection section = this._configuration.GetSection(configurationPath);
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
