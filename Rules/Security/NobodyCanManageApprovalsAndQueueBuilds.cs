using Flurl.Http;
using Newtonsoft.Json.Linq;
using SecurePipelineScan.VstsService;
using SecurePipelineScan.VstsService.Requests;
using Response = SecurePipelineScan.VstsService.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SecurePipelineScan.Rules.Security
{
    public class NobodyCanManageApprovalsAndQueueBuilds : IMultiStagePipelineRule
    {
        private const string PeoGroupName = "Production Environment Owners";
        private readonly IVstsRestClient _client;
        private readonly IProductionItemsResolver _productionItemsResolver;

        public NobodyCanManageApprovalsAndQueueBuilds(IVstsRestClient client,
            IProductionItemsResolver productionItemsResolver)
        {
            _client = client;
            _productionItemsResolver = productionItemsResolver;
        }

        [ExcludeFromCodeCoverage] string IRule.Description => 
            "Nobody can both manage approvals and queue builds (SOx)";
        [ExcludeFromCodeCoverage] string IRule.Link => "https://confluence.dev.somecompany.nl/x/rjSgDw";

        public async Task<bool?> EvaluateAsync(Response.Project project, 
            Response.BuildDefinition buildPipeline)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (buildPipeline == null)
                throw new ArgumentNullException(nameof(buildPipeline));

            var productionStageIds = await _productionItemsResolver.ResolveAsync(project.Id, buildPipeline.Id)
                .ConfigureAwait(false);
            var projectEnvironments = _client.Get(Environments.All(project.Id));
            
            try
            {
                var response = await _client.PostAsync(YamlPipeline.Parse(project.Id, buildPipeline.Id),
                    new YamlPipeline.YamlPipelineRequest()).ConfigureAwait(false);
                if (response?.FinalYaml == null)
                    return false;
                var yamlPipeline = YamlPipelineEvaluator.ConvertYamlToJson(response?.FinalYaml);

                var stages = GetProductionStagesFromYaml(yamlPipeline, productionStageIds);
                if (!stages.Any())
                    return null;

                var environmentNames = GetProductionEnvironments(stages);
                var environmentIds = environmentNames.Select(e => projectEnvironments.Single(p => p.Name == e).Id);
                return environmentIds.All(e => HasCorrectPermissions(project.Name,
                    _client.Get(Environments.Security(project.Id, e))));
            }
            catch (FlurlHttpException e)
            {
                return e.Call.HttpStatus == HttpStatusCode.BadRequest ? false : (bool?)null;
            }
        }

        private static IEnumerable<JToken> GetProductionStagesFromYaml(JToken yamlPipeline,
                IEnumerable<string> stageIds) =>
            yamlPipeline.SelectTokens("stages[*]")
                .Where(s => stageIds.Any(i => i == s?["stage"]?.ToString()));

        private static IEnumerable<string> GetProductionEnvironments(IEnumerable<JToken> stages) =>
            stages.SelectMany(s => s.SelectTokens("jobs[*]"))
                .Where(j => j?["environment"] != null)
                .Select(j => j?["environment"]?["name"]?.ToString());

        private static bool HasCorrectPermissions(string projectName, 
                IEnumerable<Response.EnvironmentSecurityGroup> groups) =>
            !groups.Any(g => g.Role.Name == "Administrator" && 
                g.Identity.DisplayName != $"[{projectName}]\\{PeoGroupName}");
    }
}