using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Northbricks.SourceDependencies
{
    public static class OssIndexClient
    {
        private static System.Net.Http.HttpClient Client = new System.Net.Http.HttpClient();

        static OssIndexClient()
        {
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.Add("User-Agent", "packagemagicanalyzer");
            Client.BaseAddress = new Uri("https://ossindex.net");

        }


        public static async Task<List<OssIndexResponse>> CheckOSSPackage()
        {

            List<OssIndexResponse> objResponse = null;
            foreach (var package in FactoryPackages.GetPackages())
            {
                objResponse = new List<OssIndexResponse>();
                string packageManager = string.Empty;
                switch (package.PackageType)
                {
                    case PackageType.Npm:
                        packageManager = "npm";
                        break;
                    case PackageType.Nuget:
                        packageManager = "nuget";
                        break;
                    default:
                        break;
                }

                if (packageManager != String.Empty)
                {
                    objResponse = await CheckVulnerabilityOnPackage(package, packageManager);
                }
            }
            return objResponse;
        }

        public static async Task<List<OssIndexResponse>> CheckVulnerabilityOnPackage(PackageMagic package, string packageManager)
        {
            List<OssIndexResponse> objResponse = null;
            var result = await Client.GetAsync($"v2.0/package/{packageManager}/{package.Name}/{package.Version}");
            if (result.IsSuccessStatusCode)
            {
                string content = await result.Content.ReadAsStringAsync();
                objResponse = JsonConvert.DeserializeObject<List<OssIndexResponse>>(content);
                Console.WriteLine($"Checked vulnerabilitys from OSS Index -  {packageManager}/{package.Name}/{package.Version}");
                foreach (var item in objResponse)
                {
                    package.OssIndexResponse = item;
                    if (item.vulnerabilities != null)
                    {
                        Console.WriteLine($"*************************************************************");

                        Console.WriteLine($"Vulnerability found in package - {package.Name}:{package.Version}");
                        foreach (var vul in item.vulnerabilities)
                        {
                            Console.WriteLine($"Vulnerability found in package - {vul.title}");
                            Console.WriteLine($"Vulnerability found in package - {vul.description}");
                            Console.WriteLine($"Vulnerability found in package - {vul.resource}");
                        }
                        Console.WriteLine($"*************************************************************");
                    }
                }





            }
            else
            {
                Console.WriteLine($"OSS Index error statuscode {result.StatusCode}");
            }

            return objResponse;
        }
    }

}
