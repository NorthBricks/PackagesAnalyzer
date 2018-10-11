using Newtonsoft.Json;
using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Northbricks.SourceDependencies
{
    public enum PackageType
    {
        Nuget,
        Npm
    }

    public interface IPackageMagicClient
    {
        //List<PackageProviders> Providers { get; set; }
    }
    public interface IPackageMagic : IPackageMagicClient
    {
        IPackage NugetExtendedPackageInformation { get; set; }
        OssIndexResponse OssIndexResponse { get; set; }

        string Name { get; set; }
        string Version { get; set; }
        SemanticVersion SemanticVersion { get; set; }
        string LicenseType { get; set; }
        string UniqueName { get; set; }
        PackageType PackageType { get; set; }




    }
    public class PackageMagic : IPackageMagic
    {
        public IPackage NugetExtendedPackageInformation { get; set; }
        public OssIndexResponse OssIndexResponse { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string LicenseType { get; set; }
        public string UniqueName { get; set; }
        public PackageType PackageType { get; set; }
        public SemanticVersion SemanticVersion { get; set; }
    }
    public static class FactoryPackages
    {
        static bool LoadedJsonFile = false;
        static List<PackageMagic> packages;
        static FactoryPackages()
        {
            packages = new List<PackageMagic>();
        }

        public static List<PackageMagic> GetPackages()
        {
            return packages;
        }

        public static void LoadPackagesJson()
        {
            List<IPackageMagic> newList = JsonConvert.DeserializeObject<List<IPackageMagic>>(File.ReadAllText(@"c:\temp\packagesGenerated.json"));
            LoadedJsonFile = true;
        }

        public static void AddPackage(PackageMagic package)
        {
            if (LoadedJsonFile)
            {
                return;
            }

            IPackageMagic checkIfExist = packages.Find(z => z.Name == package.Name && z.Version == package.Version);
            if (checkIfExist == null)
            {
                packages.Add(package);

            }
            else
            {
                //Console.WriteLine("Skipped - " + package.Name + " " + package.Version);
            }



        }

        public static void RemovePackage(PackageMagic package)
        {
            packages.Remove(package);
        }

    }

}
