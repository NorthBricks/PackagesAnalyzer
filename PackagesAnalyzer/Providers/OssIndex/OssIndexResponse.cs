using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Northbricks.PackagesAnalyzer
{
    public class Ids
    {
        public string CVE { get; set; }
    }

    public class Vulnerability
    {
        public UInt64 id { get; set; }
        public string resource { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public List<string> versions { get; set; }
        public List<string> references { get; set; }
        public string cve { get; set; }
        public long published { get; set; }
        public long updated { get; set; }
        public Ids ids { get; set; }
    }

    public class OssIndexResponse
    {
        public UInt64 id { get; set; }
        public string pm { get; set; }
        public string name { get; set; }

        public string version { get; set; }

        [JsonProperty("vulnerability-total")]
        public int vulnerabilitytotal { get; set; }
        [JsonProperty("vulnerability-matches")]
        public int vulnerabilitymatches { get; set; }
        public List<Vulnerability> vulnerabilities { get; set; }
    }
}
