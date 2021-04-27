using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using VariableGroupUsage.Models;

namespace VariableGroupUsage.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _clientFactory;

        public HomeController(IConfiguration config, IHttpClientFactory clientFactory)
        {
            _config = config;
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> Index()
        {
            VariableGroups groups = await GetVariableGroupsAsync(_config.GetValue<string>("AzureDevOps:Organisation"), _config.GetValue<string>("AzureDevOps:ProjectName"));

            var viewModel = new HomeViewModel();
            viewModel.AllVariableGroups = groups.value.ToDictionary(x => x.id, x => x.name);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(HomeViewModel model)
        {

            VariableGroups groups = await GetVariableGroupsAsync(_config.GetValue<string>("AzureDevOps:Organisation"), _config.GetValue<string>("AzureDevOps:ProjectName"));
            model.AllVariableGroups = groups.value.ToDictionary(x => x.id, x => x.name);

            model.VariableGroupAppearsInBuildPipelines = await BuildsThatUseVariableGroupAsync(model.SelectedVariableGroupId);
            model.VariableGroupAppearsInReleasePipelines = await ReleasesThatUseVariableGroupAsync(model.SelectedVariableGroupId);

            return View(model);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region "Azure DevOps Code to refactor into an API controller..."

                private async Task<VariableGroups> GetVariableGroupsAsync(string organisation, string projectName)
                {
                    try
                    {
                        var jsonText = await ExecuteJsonAsync(string.Format("https://dev.azure.com/{0}/{1}/_apis/distributedtask/variablegroups", organisation, projectName));

                        VariableGroups vGroups = JsonConvert.DeserializeObject<VariableGroups>(jsonText);
                        return vGroups;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        throw;
                    }
                }

                private async Task<string> ExecuteJsonAsync(string url)
                {
                    var client = _clientFactory.CreateClient("DevOpsClient");

                    using (HttpResponseMessage response = await client.GetAsync(url))
                    {
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        return responseBody;
                    }
                }

                private async Task<Build> GetBuild(int id)
                {
                    var jsonText = await ExecuteJsonAsync(String.Format("https://dev.azure.com/{0}/{1}/_apis/build/Definitions/{2}", _config.GetValue<string>("AzureDevOps:Organisation"), _config.GetValue<string>("AzureDevOps:ProjectName"), id));

                    Build build = JsonConvert.DeserializeObject<Build>(jsonText);
                    return build;
                }
                private async Task<List<string>> BuildsThatUseVariableGroupAsync(int variableGroupId)
                {
                    List<string> builds = new List<string>();

                    BuildDefinitions buildDefinitions = await GetBuildDefinitionsAsync();
                    foreach (var buildDefinition in buildDefinitions.value)
                    {
                        Build build = await GetBuild(buildDefinition.id);
                        if (build.variableGroups != null)
                        {
                            foreach (VariableGroup variablegroup in build.variableGroups)
                            {
                                if (variablegroup.id == variableGroupId)
                                {
                                    builds.Add(build.name);
                                    break;
                                }
                            }
                        }
                    }

                    if (builds.Count > 0)
                    { return builds; }
                    else
                    { return null; }
                }

                public async Task<Release> GetRelease(int id)
                {
                    var jsonText = await ExecuteJsonAsync(String.Format("https://vsrm.dev.azure.com/{0}/{1}/_apis/Release/definitions/{2}", _config.GetValue<string>("AzureDevOps:Organisation"), _config.GetValue<string>("AzureDevOps:ProjectName"), id));

                    Release release = JsonConvert.DeserializeObject<Release>(jsonText);
                    return release;
                }

                private async Task<ReleaseDefinitions> GetReleaseDefinitionsAsync()
                {
                        var jsonText = await ExecuteJsonAsync(String.Format("https://vsrm.dev.azure.com/{0}/{1}/_apis/release/definitions", _config.GetValue<string>("AzureDevOps:Organisation"), _config.GetValue<string>("AzureDevOps:ProjectName")));

                        ReleaseDefinitions rDef = JsonConvert.DeserializeObject<ReleaseDefinitions>(jsonText);
                        return rDef;
                }

                private async Task<BuildDefinitions> GetBuildDefinitionsAsync()
                {
                    try
                    {
                        var jsonText = await ExecuteJsonAsync(String.Format("https://dev.azure.com/{0}/{1}/_apis/build/definitions", _config.GetValue<string>("AzureDevOps:Organisation"), _config.GetValue<string>("AzureDevOps:ProjectName")));

                        BuildDefinitions buildDefinitions = JsonConvert.DeserializeObject<BuildDefinitions>(jsonText);
                        return buildDefinitions;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        throw;
                    }
                }
                private async Task<List<string>> ReleasesThatUseVariableGroupAsync(int variableGroupId)
                {
                    List<string> releases = new List<string>();

                    ReleaseDefinitions releaseDefinitions = await GetReleaseDefinitionsAsync();
                    foreach (var releaseDefinition in releaseDefinitions.value)
                    {
                        Release release = await GetRelease(releaseDefinition.id);
                        if (release.variableGroups != null) 
                        {
                            foreach (var variablegroupId in release.variableGroups)
                            {
                                if (variablegroupId == variableGroupId)
                                {
                                    releases.Add(release.name);
                                    break;
                                }
                            }
                        }

                        foreach (VariableGroupUsage.Models.Environment environment in release.environments)
                        {
                            foreach (int variableGroup in environment.variableGroups)
                            {
                                if (variableGroup == variableGroupId)
                                {
                                    releases.Add(String.Format("{0} ({1})", release.name, environment.name));
                                    break;
                                }
                            }
                        }
                    }

                    if (releases.Count > 0)
                        {return releases;}
                    else
                        {return null;}
                }
        
        #endregion

    }
}
