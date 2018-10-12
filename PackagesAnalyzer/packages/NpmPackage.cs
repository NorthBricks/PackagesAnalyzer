using Newtonsoft.Json.Linq;
using NuGet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northbricks.PackagesAnalyzer
{
    public class NpmPackage : PackageMagic
    {
        public static string NpmRegistry { get; set; }

        public static readonly string NpmPath;
        static NpmPackage()
        {
            NpmPath = FindNpmPath("npm.cmd");
            //Providers.Add(PackageProviders.GitHub);
            //Providers.Add(PackageProviders.OssIndex);

        }

        public static async Task LoopPackageJson(string packageJsonPath)
        {
            Console.WriteLine($"Search for package.json in path {packageJsonPath}");
            string[] packageJson = Directory.GetFiles(packageJsonPath, "package.json", SearchOption.AllDirectories);
            Console.WriteLine($"Found {packageJson.Count()} package.json (skipping package.json in directory with name node_modules");
            await Task.Run(() =>
            {
                foreach (var item in packageJson)
                {
                    if (!item.Contains("node_modules"))
                    {
                        Console.WriteLine($"Read json {item}");
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
                    FactoryPackages.AddPackage(new NugetPackage { Name = p.Name, Version = p.Value.ToString(), UniqueName = "devDependencies", PackageType = PackageType.Npm });
                }
            }
            IList<JToken> jsonDep = o1["dependencies"];
            if (jsonDep != null)
            {
                foreach (var jToken in jsonDep)
                {
                    var p = (JProperty)jToken;
                    FactoryPackages.AddPackage(new NugetPackage { Name = p.Name, Version = p.Value.ToString(), UniqueName = "dependencies", PackageType = PackageType.Npm });
                }


            }
        }

        public static string FindNpmPath(string npmCmd)
        {
            Console.WriteLine($"Searching for NPM path");
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
            Console.WriteLine($"NPM path found {Path.GetFullPath(npmCmd)}");
            return Path.GetFullPath(npmCmd);
        }
        public static async Task RunNpmViewCheckLicense()
        {
            JObject output = null;
            string license = String.Empty;
            await Task.Run(() =>
            {
                foreach (var package in FactoryPackages.GetPackages().FindAll(o => o.PackageType == PackageType.Npm))
                {
                    try
                    {
                        var packageToCheck = $"{package.Name}@{package.Version}";
                        //return output;
                        var psiNpmRunDist = new ProcessStartInfo
                        {
                            FileName = NpmPath,
                            Arguments = $"view {packageToCheck}",
                            RedirectStandardInput = true,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            StandardOutputEncoding = Encoding.UTF8
                        };

                        var pNpmRunDist = Process.Start(psiNpmRunDist);


                        if (pNpmRunDist != null)
                        {
                            try
                            {
                                output = JObject.Parse(pNpmRunDist.StandardOutput.ReadToEnd());
                            }
                            catch (Exception)
                            {
                                output = null;
                            }

                            pNpmRunDist.StandardInput.WriteLine("npm run view & exit");
                            pNpmRunDist.WaitForExit();
                        }

                    }
                    catch (Exception e)
                    {
                        output = null;

                    }

                    try
                    {
                        JToken licenseToken = output.GetValue("license");
                        if (licenseToken != null)
                        {
                            license = licenseToken.ToString();
                        }

                        if (license == null)
                        {
                            foreach (var item in output)
                            {
                                if (item.Key.ToLower() == "licenses")
                                {
                                    foreach (JObject val in item.Value.Children())
                                    {
                                        foreach (var d in val)
                                        {
                                            if (d.Key == "type")
                                            {
                                                license = d.Value.ToString();
                                                break;
                                            }

                                        }
                                    }

                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {

                    }

                    if (license != null)
                    {
                        Console.WriteLine("Npm licensetype " + license);
                        package.LicenseType = license;
                    }

                }
            });
            //return license;
        }

        public static async Task<JObject> RunNpmView()
        {
            JObject output = null;
            try
            {
                foreach (var package in FactoryPackages.GetPackages().FindAll(o => o.PackageType == PackageType.Npm))
                {
                    var packageToCheck = $"{package.Name}@{package.Version}";

                    //return output;
                    var psiNpmRunDist = new ProcessStartInfo
                    {
                        FileName = NpmPath,
                        Arguments = $"view {packageToCheck}",
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
                                output = JObject.Parse(pNpmRunDist.StandardOutput.ReadToEnd());
                            }
                            catch (Exception)
                            {
                                output = null;
                            }

                            pNpmRunDist.StandardInput.WriteLine("npm run view & exit");
                            pNpmRunDist.WaitForExit();
                        }
                    });
                }
            }
            catch (Exception e)
            {
                output = null;

            }
            try
            {
                dynamic outputDynamic = output;
                var license = output.GetValue("license");

                //var licenses = output.GetValue("licenses");
                //if (licenses != null)
                //{
                if (license == null)
                {
                    foreach (var item in output)
                    {
                        if (item.Key.ToLower() == "licenses")
                        {
                            foreach (JObject val in item.Value.Children())
                            {
                                foreach (var d in val)
                                {
                                    Console.WriteLine($"{d.Key} {d.Value.ToString()}");
                                }
                            }

                        }
                    }
                }
                //    Utils.LogMessages(licenses.First.ToString());
                //}
            }
            catch (Exception ex)
            {


            }

            return output;
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
