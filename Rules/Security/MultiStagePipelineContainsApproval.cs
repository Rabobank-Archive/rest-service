using Flurl.Http;
using Newtonsoft.Json.Linq;
using SecurePipelineScan.VstsService;
using SecurePipelineScan.VstsService.Requests;
using Response = SecurePipelineScan.VstsService.Response;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SecurePipelineScan.Rules.Security
{
    public class MultiStagePipelineContainsApproval : IMultiStagePipelineRule
    {
        private readonly IVstsRestClient _client;
        public MultiStagePipelineContainsApproval(IVstsRestClient client) => _client = client;

        [ExcludeFromCodeCoverage] string IRule.Description => 
            "Multi stage pipeline contains 4-eyes approval (SOx)";
        [ExcludeFromCodeCoverage] string IRule.Link => "https://confluence.dev.somecompany.nl/x/sjSgDw";

        public async Task<bool?> EvaluateAsync(Response.Project project, Response.BuildDefinition buildPipeline)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (buildPipeline == null)
                throw new ArgumentNullException(nameof(buildPipeline));

            var projectEnvironments = _client.Get(Environments.All(project.Id));

            try
            {
                var response = await _client.PostAsync(YamlPipeline.Parse(project.Id, buildPipeline.Id),
                    new YamlPipeline.YamlPipelineRequest()).ConfigureAwait(false);
                if (response?.FinalYaml == null)
                    return false;
                var yamlPipeline = YamlPipelineEvaluator.ConvertYamlToJson(response?.FinalYaml);
                
                var environments = GetEnvironmentsFromYaml(yamlPipeline);
                foreach (string environment in environments)
                {
                    if (HasApproval(project, projectEnvironments, environment))
                        return true;
                }
                return false;

            }
            catch (FlurlHttpException e)
            {
                return e.Call.HttpStatus == HttpStatusCode.BadRequest ? false : (bool?)null;
            }
        }

        private bool HasApproval(Response.Project project, 
            IEnumerable<Response.EnvironmentYaml> projectEnvironments, string environment)
        {
            var environmentId = projectEnvironments.Single(e => e.Name == environment).Id;
            var config = _client.Get(Environments.Configuration(project.Id, environmentId));
            return config.Any(c => c.Type.Name == "Approval");
        }

        private static IEnumerable<string> GetEnvironmentsFromYaml(JToken yamlPipeline) =>
            yamlPipeline.SelectTokens("stages[*].jobs[*]")
                .Where(j => j?["environment"] != null)
                .Select(j => j?["environment"]?["name"]?.ToString());
    }
}