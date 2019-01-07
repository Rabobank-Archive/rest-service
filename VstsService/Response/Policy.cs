﻿using Newtonsoft.Json;
using SecurePipelineScan.VstsService.Converters;

namespace SecurePipelineScan.VstsService.Response
{
    /* cannot use the [JsonConverter(typeof(PolicyConverter))] here because it
        it results in a stackoverflow when trying to deserialize the derived types
    */
    public class Policy
    {
        public string Id { get; set; }

        public bool? IsEnabled { get; set; }
        public bool? IsBlocking { get; set; }
        public bool? IsDeleted { get; set; }
    }
}