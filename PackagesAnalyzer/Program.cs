using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.IO;
using System.Configuration;

namespace Northbricks.PackagesAnalyzer
{
    class Program
    {
        public static string PathToSearch { get; private set; }

        public static string SaveJsonPath { get; private set; }

        static void Main(string[] args)
        {
            PathToSearch = ConfigurationManager.AppSettings["SourceCodePath"];
            SaveJsonPath = ConfigurationManager.AppSettings["SaveJsonPath"];
            Console.WriteLine($"Searching directory: {PathToSearch}");
            Console.WriteLine($"Saving result to: {SaveJsonPath}");

            RunAsync().GetAwaiter().GetResult();
            Console.WriteLine($"Found {FactoryPackages.GetPackages().Count} packages");

            //var obj = JsonConvert.SerializeObject(FactoryPackages.GetPackages(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            //File.WriteAllText(SaveJsonPath, obj);
            SavePackagesToDisc();
            foreach (var item in FactoryPackages.GetPackages().OrderBy(x => x.Name))
            {
                Console.WriteLine(item.PackageType + "|" + item.Name + " " + item.Version + "|" + item.UniqueName);
            }
        }
        private static void SavePackagesToDisc()
        {
            if (!Directory.Exists("testresults"))
            {
                Directory.CreateDirectory("testresults");
            }
            using (StreamWriter file = File.CreateText(@"testresults\allPackages.json"))
            {
                Console.WriteLine($"Saving result to json {file.ToString()}");
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, FactoryPackages.GetPackages());
                //If this is used by Azure DevOps or TFS it will upload result to release
                Console.WriteLine($"##vso[task.uploadfile]{Path.GetFullPath("testresults/allPackages.json")}");
            }
        }
        static async Task RunAsync()
        {
            List<Task> firstTasks = new List<Task>
            {
            NugetPackage.SearchForPackagesConfig(PathToSearch),
            NugetPackage.SearchForAllPackageReferences(PathToSearch),
            NpmPackage.LoopPackageJson(PathToSearch)

            };
            await Task.WhenAll(firstTasks);

            List<Task> secondTasks = new List<Task>
            {
            OssIndexClient.CheckOSSPackage(),
            NugetPackage.GetNugetPackageInformation()
        };
            await Task.WhenAll(secondTasks);

            //await NpmPackage.RunNpmViewCheckLicense();
            //await NpmPackage.RunNpmView();

        }
    }
}
