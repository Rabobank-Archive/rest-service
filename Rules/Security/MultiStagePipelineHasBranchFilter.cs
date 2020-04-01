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
    public class MultiStagePipelineHasBranchFilter : IMultiStagePipelineRule
    {
        private const string Condition = "eq(variables['Build.SourceBranch'], 'refs/heads/master')";
        private readonly IVstsRestClient _client;
        private readonly IProductionItemsResolver _productionItemsResolver;

        public MultiStagePipelineHasBranchFilter(IVstsRestClient client, 
            IProductionItemsResolver productionItemsResolver)
        {
            _client = client;
            _productionItemsResolver = productionItemsResolver;
        }
        
        [ExcludeFromCodeCoverage] string IRule.Description =>
            "Production stage uses artifact from master branch (SOx)";
        [ExcludeFromCodeCoverage] string IRule.Link => "https://confluence.dev.somecompany.nl/x/tzSgDw";

        public async Task<bool?> EvaluateAsync(Response.Project project, 
            Response.BuildDefinition buildPipeline)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (buildPipeline == null)
                throw new ArgumentNullException(nameof(buildPipeline));

            var productionStageIds = await _productionItemsResolver.ResolveAsync(project.Id, buildPipeline.Id)
                .ConfigureAwait(false);

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

                foreach (JToken stage in stages)
                {
                    if (HasBranchFilter(stage) != true)
                        return false;
                }
                return true;
            }
            catch (FlurlHttpException e)
            {
                return e.Call.HttpStatus == HttpStatusCode.BadRequest ? false : (bool?)null;
            }
        }

        private static bool? HasBranchFilter(JToken stage) => 
            stage?["condition"]?.ToString().Contains(Condition, StringComparison.InvariantCultureIgnoreCase);

        private static IEnumerable<JToken> GetProductionStagesFromYaml(JToken yamlPipeline, 
                IEnumerable<string> stageIds) =>
            yamlPipeline.SelectTokens("stages[*]")
                .Where(s => stageIds.Any(i => i == s?["stage"]?.ToString()));
    }
}