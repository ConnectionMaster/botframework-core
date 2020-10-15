// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Core
{
    public class LuisConfig
    {
        public string Name { get; set; }

        public string DefaultLanguage { get; set; }

#pragma warning disable CA2227 // Collection properties should be read onl
        public List<string> Models { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        public string AuthoringKey { get; set; }

        public bool Dialogs { get; set; }

        public string Environment { get; set; }

        public bool Autodelete { get; set; }

        public string AuthoringRegion { get; set; }

        public string Folder { get; set; }

        public bool Help { get; set; }

        public bool Force { get; set; }

        public string Config { get; set; }

        public string EndpointKeys { get; set; }
    }
}
