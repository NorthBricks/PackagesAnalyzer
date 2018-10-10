using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourcePackages.packages
{
    public class NpmPackage
    {
        public static string NpmRegistry { get; set; }
        public static readonly string NpmPath;
        static NpmPackage()
        {
            NpmPath = FindNpmPath("npm.cmd");

        }

        public static async Task LoopPackageJson(string packageJsonPath = @"C:\git\RayCare\src\adapters\RayCare.Web.UI\package.json")
        {
            string[] packageJson = Directory.GetFiles(@"c:\git\RayCare", "package.json", SearchOption.AllDirectories);
            await Task.Run(() =>
            {
                foreach (var item in packageJson)
                {
                    if (!item.Contains("node_modules"))
                    {
                        ReadPackageJsonFile(item);
                    }
                    else
                    {
                        //Skips this for the moment - here we can get all in node_nodules also
                        //ReadPackageJsonFile(item);
                    }

                }
            });
        }

        private static void ReadPackageJsonFile(string item)
        {
            dynamic o1 = JObject.Parse(File.ReadAllText(item));
            IList<JToken> jsonDevDep = o1["devDependencies"];
            if (jsonDevDep != null)
            {
                foreach (var jToken in jsonDevDep)
                {
                    var p = (JProperty)jToken;
                    //var license = await RunNpmViewCheckLicense(p.Name, p.Value.ToString());
                    //await Utils.AddToPackageInformation(new PackageInformation { PackageName = p.Name, PackageVersion = p.Value.ToString(), PackageDescription = "", OriginOfPackage = PackageInformation.Origin.Npm, FeedRegistry = NpmRegistry });
                    FactoryPackages.AddPackage(new NugetPackage { Name = p.Name, Version = p.Value.ToString(), UniqueName = "devDependencies", PackageType = PackageType.Npm });
                }
            }
            IList<JToken> jsonDep = o1["dependencies"];
            if (jsonDep != null)
            {
                foreach (var jToken in jsonDep)
                {
                    var p = (JProperty)jToken;
                    //var license = await RunNpmViewCheckLicense(p.Name, p.Value.ToString());
                    //await Utils.AddToPackageInformation(new PackageInformation { PackageName = p.Name, PackageVersion = p.Value.ToString(), PackageDescription = "", OriginOfPackage = PackageInformation.Origin.Npm, FeedRegistry = NpmRegistry });
                    FactoryPackages.AddPackage(new NugetPackage { Name = p.Name, Version = p.Value.ToString(), UniqueName = "dependencies", PackageType = PackageType.Npm });
                }


            }
        }

        public static string FindNpmPath(string npmCmd)
        {
            npmCmd = Environment.ExpandEnvironmentVariables(npmCmd);
            if (!File.Exists(npmCmd))
            {
                if (Path.GetDirectoryName(npmCmd) == string.Empty)
                    foreach (var test in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
                    {
                        var path = test.Trim();
                        if (!string.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, npmCmd)))
                            return Path.GetFullPath(path);
                    }

                throw new FileNotFoundException(new FileNotFoundException().Message, npmCmd);
            }

            return Path.GetFullPath(npmCmd);
        }

        public static async Task<string> CheckNpmRegistry()
        {
            string output = null;
            try
            {
                //return output;
                var psiNpmRunDist = new ProcessStartInfo
                {
                    FileName = NpmPath,
                    Arguments = $"config get registry",
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8
                };
                await Task.Run(() =>
                {
                    var pNpmRunDist = Process.Start(psiNpmRunDist);


                    if (pNpmRunDist != null)
                    {
                        try
                        {
                            output = pNpmRunDist.StandardOutput.ReadToEnd();
                        }
                        catch (Exception)
                        {
                            output = null;
                        }

                        pNpmRunDist.StandardInput.WriteLine("npm config get registry & exit");
                        pNpmRunDist.WaitForExit();
                    }
                });
            }
            catch (Exception e)
            {
                output = null;

            }
            NpmRegistry = output;
            return output;
        }
    }
}
