using SourcePackages;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GitHubClient
{
    public interface IPackageMagic
    {
        IPackage ExtendedPackageInformation { get; set; }
        string Name { get; set; }
        string Version { get; set; }

        string UniqueName { get; set; }
        PackageType PackageType { get; set; }


    }
    public static class GitHubClient
    {
        private static System.Net.Http.HttpClient Client = new System.Net.Http.HttpClient();
        static GitHubClient()
        {
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            Client.DefaultRequestHeaders.Add("User-Agent", "northbricksClient");
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "3311d0c1f9bd3b79422a0ebe9733509a2200ff08");
            Client.BaseAddress = new Uri("https://api.github.com");

            IPackageMagic account = J.DeserializeObject<IPackageMagic>(json);

        }

        public static async Task ResolveAllGitHubProjects(List<IPackageMagic> packages)
        {
            int iCounter = 0;
            int iFoundLicenses = 0;
            int iFoundGitHubDescription = 0;
            await Task.Run(() =>
            {
                foreach (var item in packages.FindAll(o => o.ExtendedPackageInformation != null))
                {
                    iCounter++;
                    try
                    {
                        GitHubRepositoryResponse github = GetGitRepository(item).GetAwaiter().GetResult();

                        if (github != null)
                        {
                            iFoundGitHubDescription++;
                            item.GitHubRepository = github;
                            if (github.license != null && github.license.spdx_id != null)
                            {
                                iFoundLicenses++;
                                item.LicenseType = github.license.spdx_id;
                                //Utils.LogMessages($"Found github package description {item.PackageName}:{item.PackageVersion} with licensetype {item.LicenseType}");
                            }
                        }


                    }
                    catch (Exception ex)
                    {

                        //Utils.LogMessages(ex.Message);
                    }

                }
                //Utils.LogMessages($"Packages checked on github {iCounter} and found {iFoundLicenses} licenses and description of github repos {iFoundGitHubDescription}");
            });
        }
        public static async Task<GitHubRepositoryResponse> GetGitRepository(List<IPackageMagic> package)
        {
            GitHubRepositoryResponse objResponse = null;
            Uri newUri = null;
            Uri newUri2 = null;

            if (package.ExtendedPackageInformation.ProjectUrl != null && package.ExtendedPackageInformation.ProjectUrl.IsAbsoluteUri)
            {
                newUri = await FixGitHubUrl(package.ExtendedPackageInformation.ProjectUrl);
            }
            if (newUri == null && package.ExtendedPackageInformation.LicenseUrl != null && package.ExtendedPackageInformation.LicenseUrl.IsAbsoluteUri)
            {
                newUri2 = await FixGitHubUrl(package.ExtendedPackageInformation.LicenseUrl);
            }


            if (newUri != null)
            {
                objResponse = await GetGitHubRepository(newUri);
            }

            if (objResponse == null && newUri2 != null)
            {
                objResponse = await GetGitHubRepository(newUri2);
            }

            return objResponse;

        }

        public static async Task<Uri> FixGitHubUrl(Uri test)
        {

            if (!test.Host.Contains("github.com"))
            {
                return null;
            }
            string baseUrl = "https://api.github.com/repos";
            Uri newUrl = null;

            await Task.Run(() =>
            {
                string[] s = test.LocalPath.Split('/');

                int itemsAdded = 0;
                foreach (var item in s)
                {
                    if (item.Trim() != String.Empty)
                    {
                        itemsAdded++;
                        baseUrl += "/" + item;
                    }
                    if (itemsAdded == 2)
                    {
                        newUrl = new Uri(baseUrl);
                        break;
                    }

                }
            });
            return newUrl;
        }



        public static async Task<GitHubRepositoryResponse> GetGitHubRepository(Uri gitHubRepoEndpoint)
        {
            GitHubRepositoryResponse objResponse = null;

            var result = await Client.GetAsync($"{gitHubRepoEndpoint}");
            if (result.IsSuccessStatusCode)
            {
                string content = await result.Content.ReadAsStringAsync();
                objResponse = JsonConvert.DeserializeObject<GitHubRepositoryResponse>(content);
                Utils.LogMessages(objResponse, true);
            }

            return objResponse;
        }

    }
}
