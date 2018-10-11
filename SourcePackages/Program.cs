using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.IO;

namespace Northbricks.SourceDependencies
{
    class Program
    {
        static string pathToSearch = @"c:\git\rtest";
        static void Main(string[] args)
        {
            //FactoryPackages.LoadPackagesJson();
            RunAsync().GetAwaiter().GetResult();
            Console.WriteLine(FactoryPackages.GetPackages().Count);
            var obj = JsonConvert.SerializeObject(FactoryPackages.GetPackages(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            File.WriteAllText(@"c:\temp\packagesGenerated.json", obj);

            foreach (var item in FactoryPackages.GetPackages().OrderBy(x => x.Name))
            {

                Console.WriteLine(item.PackageType + "|" + item.Name + " " + item.Version + "|" + item.UniqueName);
            }
        }

        static async Task RunAsync()
        {
            List<Task> myTasks = new List<Task>
            {
            NugetPackage.SearchForPackagesConfig(pathToSearch),
            NugetPackage.SearchForAllPackageReferences(pathToSearch),
            NpmPackage.LoopPackageJson(pathToSearch)

            };
            await Task.WhenAll(myTasks);
            await OssIndexClient.CheckOSSPackage();
            await NugetPackage.GetNugetPackageInformation();

            //await NpmPackage.RunNpmViewCheckLicense();
            //await NpmPackage.RunNpmView();

        }
    }
}
