// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using System.Collections.Generic;

namespace Weather.Gov.Models
{
    public partial class PointJsonLd
    {
        [Newtonsoft.Json.JsonProperty("@context", Required = Newtonsoft.Json.Required.Always)]
        new public object[] Context { get; set; }

        [Newtonsoft.Json.JsonProperty("geometry", Required = Newtonsoft.Json.Required.AllowNull)]
        new public GeoJsonGeometry Geometry { get; set; }

        private IDictionary<string, object> _properties = new Dictionary<string, object>();

    }

    public partial class GeoJsonFeature
    {
        [Newtonsoft.Json.JsonProperty("@context", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public object[] Context { get; set; }


        [Newtonsoft.Json.JsonProperty("geometry", Required = Newtonsoft.Json.Required.AllowNull)]
        public GeoJsonGeometry Geometry { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class GridpointGeoCollectionJson : GeoJsonFeatureCollection
    {
        [Newtonsoft.Json.JsonProperty("properties", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Gridpoint Properties { get; set; }
    }

    public partial class GeoJsonFeatureCollection
    {
        [Newtonsoft.Json.JsonProperty("@context", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public object[] Context { get; set; }
    }
}
