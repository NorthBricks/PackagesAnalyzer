using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SourcePackages.packages;
namespace SourcePackages
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
            Console.WriteLine(FactoryPackages.GetPackages().Count);
            foreach (var item in FactoryPackages.GetPackages().OrderBy(x => x.Name))
            {

                Console.WriteLine(item.PackageType + "|" + item.Name + " " + item.Version + "|" + item.UniqueName);
            }
        }

        static async Task RunAsync()
        {
            List<Task> myTasks = new List<Task>
            {
            NugetPackage.SearchForPackagesConfig(@"C:\git\RayCare"),
            NugetPackage.SearchForAllPackageReferences(@"C:\git\RayCare"),
            NpmPackage.LoopPackageJson()
            };
            await Task.WhenAll(myTasks);
        }
    }
}
