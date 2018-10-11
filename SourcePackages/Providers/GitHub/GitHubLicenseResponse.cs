using System.Collections.Generic;

namespace GitHubClient
{

    public class GitHubLicenseResponse
    {
        public string key { get; set; }
        public string name { get; set; }
        public string spdx_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string description { get; set; }
        public string implementation { get; set; }
        public List<string> permissions { get; set; }
        public List<string> conditions { get; set; }
        public List<string> limitations { get; set; }
        public string body { get; set; }
        public bool featured { get; set; }
    }
}
