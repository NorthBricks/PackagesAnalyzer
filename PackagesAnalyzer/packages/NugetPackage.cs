using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Northbricks.PackagesAnalyzer
{
    public class NugetPackage : PackageMagic
    {
        static string patternPackagesConfig = "*.csproj";
        public NugetPackage()
        {
            //Providers.Add(PackageProviders.GitHub);
            //Providers.Add(PackageProviders.OssIndex);
        }

        private static async Task SearchNugetPackageReferences(string file)
        {
            await Task.Run(() =>
            {
                XmlDocument xDoc = new XmlDocument();

                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    xDoc.Load(fs);

                    // Load Xml

                    var nodes = xDoc.GetElementsByTagName("PackageReference");

                    foreach (XmlNode node in nodes)
                    {
                        if (node.Attributes != null && (node.Attributes["Version"] != null || node.Attributes["version"] != null))
                        {
                            string packageVersion;
                            if (node.Attributes["Version"] == null)
                            {
                                packageVersion = node.Attributes["version"].Value;
                            }
                            else
                            {
                                packageVersion = node.Attributes["Version"].Value;
                            }
                            PackageMagic package = new NugetPackage { Name = node.Attributes["Include"].Value, Version = packageVersion, UniqueName = "PackageReference", PackageType = PackageType.Nuget };
                            FactoryPackages.AddPackage(package);
                        }
                        else
                        {
                            //Console.WriteLine(file + "No version found in PackageReference");
                        }

                    }



                }
            });

        }
        public static async Task SearchForAllPackageReferences(string projectDirectory)
        {
            string[] csProjFiles = Directory.GetFiles(projectDirectory, patternPackagesConfig, SearchOption.AllDirectories);
            Console.WriteLine($"Searching for Package References in {projectDirectory} (filter {patternPackagesConfig}");
            Console.WriteLine($"Found {csProjFiles.Count()} package references {patternPackagesConfig}");
            foreach (var csProjFile in csProjFiles)
            {
                await SearchNugetPackageReferences(csProjFile);
            }

        }
        public static async Task SearchForPackagesConfig(string projectDirectory)
        {
            Console.WriteLine($"Searching for Packages Config in {projectDirectory}");
            await Task.Run(() =>
            {
                string[] packagesConfig = Directory.GetFiles(projectDirectory, "packages.config", SearchOption.AllDirectories);
                Console.WriteLine($"Found {packagesConfig.Count()} packages.config files");
                foreach (var packConfig in packagesConfig)
                {
                    var directoryName = Path.GetDirectoryName(packConfig);
                    string foundCsProjFile = "";
                    if (directoryName != null)
                    {
                        string[] proj = Directory.GetFiles(directoryName, patternPackagesConfig, SearchOption.TopDirectoryOnly);

                        if (proj.Length > 0)
                        {
                            foundCsProjFile = proj[0];
                        }
                        else
                        {
                            foundCsProjFile = "No csProj file found " + packConfig;
                        }
                    }


                    var file = new PackageReferenceFile(packConfig);
                    foreach (PackageReference packageReference in file.GetPackageReferences())
                    {
                        PackageMagic package = new NugetPackage { Name = packageReference.Id, Version = packageReference.Version.ToNormalizedString(), UniqueName = "PackageConfig", PackageType = PackageType.Nuget };
                        FactoryPackages.AddPackage(package);
                    }
                }
            });
        }

        public static async Task GetNugetPackageInformation()
        {
            IPackage pack;
            IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");

            await Task.Run(() =>
            {
                foreach (var item in FactoryPackages.GetPackages().FindAll(o => o.PackageType == PackageType.Nuget))
                {
                    pack = repo.FindPackage(item.Name, SemanticVersion.Parse(item.Version));
                    item.NugetExtendedPackageInformation = pack;
                    Console.WriteLine("Get Nuget Extended Package Information on " + item.Name + " " + item.Version);
                }
            });

        }


    }

}
