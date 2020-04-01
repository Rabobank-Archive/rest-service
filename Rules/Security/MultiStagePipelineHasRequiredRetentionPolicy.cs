using SecurePipelineScan.VstsService;
using SecurePipelineScan.VstsService.Requests;
using Response = SecurePipelineScan.VstsService.Response;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace SecurePipelineScan.Rules.Security
{
    public class MultiStagePipelineHasRequiredRetentionPolicy : IMultiStagePipelineRule
    {
        private readonly IVstsRestClient _client;
        private const int RequiredRetentionDays = 450;

        public MultiStagePipelineHasRequiredRetentionPolicy(IVstsRestClient client) => _client = client;

        [ExcludeFromCodeCoverage] string IRule.Description =>
            "All pipeline runs are retained for at least 15 months (SOx)";
        [ExcludeFromCodeCoverage] string IRule.Link => "https://confluence.dev.somecompany.nl/x/sDSgDw";

        public async Task<bool?> EvaluateAsync(Response.Project project, Response.BuildDefinition buildPipeline)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            var retentionSettings = await _client.GetAsync(Builds.Retention(project.Id))
                .ConfigureAwait(false);
            return retentionSettings.PurgeRuns != null &&
                retentionSettings.PurgeRuns.Value >= RequiredRetentionDays;
        }
    }
}