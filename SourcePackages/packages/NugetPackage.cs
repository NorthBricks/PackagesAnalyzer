using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SourcePackages.packages
{
    public class NugetPackage : IPackageMagic
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public PackageType PackageType { get; set; }
        public string UniqueName { get; set; }

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
                            FactoryPackages.AddPackage(new NugetPackage { Name = node.Attributes["Include"].Value, Version = packageVersion, UniqueName = "PackageReference", PackageType = PackageType.Nuget });
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

            string[] csProjFiles = Directory.GetFiles(projectDirectory, "*.csproj", SearchOption.AllDirectories);
            foreach (var csProjFile in csProjFiles)
            {
                await SearchNugetPackageReferences(csProjFile);
            }

        }
        public static async Task SearchForPackagesConfig(string projectDirectory)
        {
            await Task.Run(() =>
            {
                string[] packagesConfig = Directory.GetFiles(projectDirectory, "packages.config", SearchOption.AllDirectories);

                foreach (var packConfig in packagesConfig)
                {
                    //NugetPackages p = new NugetPackages();
                    var directoryName = Path.GetDirectoryName(packConfig);
                    string foundCsProjFile = "";
                    if (directoryName != null)
                    {
                        string[] proj = Directory.GetFiles(directoryName, "*.csproj", SearchOption.TopDirectoryOnly);

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
                        FactoryPackages.AddPackage(new NugetPackage { Name = packageReference.Id, Version = packageReference.Version.ToNormalizedString(), UniqueName = "PackageConfig", PackageType = PackageType.Nuget });
                        //Utils.AddToPackageInformation(new PackageInformation { PackageName = packageReference.Id, PackageVersion = packageReference.Version.ToNormalizedString(), PackageDescription = foundCsProjFile, OriginOfPackage = PackageInformation.Origin.PackageConfig, CsProjFile = foundCsProjFile }).GetAwaiter().GetResult();

                    }
                }
            });
        }
    }



    //[XmlRoot(ElementName = "package")]
    //public class Package
    //{
    //    [XmlAttribute(AttributeName = "id")]
    //    public string Id { get; set; }
    //    [XmlAttribute(AttributeName = "targetFramework")]
    //    public string TargetFramework { get; set; }
    //    [XmlAttribute(AttributeName = "version")]
    //    public string Version { get; set; }
    //}

    //[XmlRoot(ElementName = "packages")]
    //public class Packages
    //{
    //    [XmlElement(ElementName = "package")]
    //    public List<Package> Package { get; set; }
    //}
}
