using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SourcePackages.packages;
using Newtonsoft.Json;
using System.IO;

namespace SourcePackages
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
            Console.WriteLine(FactoryPackages.GetPackages().Count);

            var json = JsonConvert.SerializeObject(FactoryPackages.GetPackages());
            using (StreamWriter file = File.CreateText(@"c:\temp\packagesGenerated.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, FactoryPackages.GetPackages());
            }
            foreach (var item in FactoryPackages.GetPackages().OrderBy(x => x.Name))
            {

                Console.WriteLine(item.PackageType + "|" + item.Name + " " + item.Version + "|" + item.UniqueName);
            }
        }

        static async Task RunAsync()
        {
            List<Task> myTasks = new List<Task>
            {
            NugetPackage.SearchForPackagesConfig(@"C:\git\raycare"),
            NugetPackage.SearchForAllPackageReferences(@"C:\git\raycare"),
            NpmPackage.LoopPackageJson(@"c:\git\raycare")
            };
            await Task.WhenAll(myTasks);
        }
    }
}
