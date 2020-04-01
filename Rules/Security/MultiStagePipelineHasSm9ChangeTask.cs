using SecurePipelineScan.VstsService.Response;
using SecurePipelineScan.VstsService;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace SecurePipelineScan.Rules.Security
{
    public class MultiStagePipelineHasSm9ChangeTask : IPipelineHasTaskRule, IMultiStagePipelineRule
    {
        private readonly YamlPipelineEvaluator _pipelineEvaluator;
        public MultiStagePipelineHasSm9ChangeTask(IVstsRestClient client) =>
            _pipelineEvaluator = new YamlPipelineEvaluator(client);

        public string TaskId => "d0c045b6-d01d-4d69-882a-c21b18a35472";
        public string TaskName => "SM9 - Create";
        public Dictionary<string, string> Inputs => new Dictionary<string, string>();

        [ExcludeFromCodeCoverage] string IRule.Description => 
            "Multi stage pipeline contains SM9 Create Change task";
        [ExcludeFromCodeCoverage] string IRule.Link => "https://confluence.dev.somecompany.nl/x/vDSgDw";

        public async Task<bool?> EvaluateAsync(Project project, BuildDefinition buildPipeline) =>
            await _pipelineEvaluator.EvaluateAsync(project, buildPipeline, this).ConfigureAwait(false);
    }
}