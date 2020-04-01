using SecurePipelineScan.Rules.Security;
using SecurePipelineScan.VstsService;
using SecurePipelineScan.VstsService.Requests;
using Response = SecurePipelineScan.VstsService.Response;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Rules.Tests.Integration.Security
{
    public class MultiStagePipelineHasRequiredRetentionPolicyTests : IClassFixture<TestConfig>
    {
        private readonly TestConfig _config;
        public MultiStagePipelineHasRequiredRetentionPolicyTests(TestConfig config) => _config = config;

        [Fact]
        [Trait("category", "integration")]
        public async Task EvaluateIntegrationTest()
        {
            var client = new VstsRestClient(_config.Organization, _config.Token);
            var project = await client.GetAsync(Project.ProjectById(_config.Project));
            var rule = new MultiStagePipelineHasRequiredRetentionPolicy(client);
            var result = await rule.EvaluateAsync(project, new Response.BuildDefinition())
                .ConfigureAwait(false);
            result.ShouldBe(true);
        }
    }
}