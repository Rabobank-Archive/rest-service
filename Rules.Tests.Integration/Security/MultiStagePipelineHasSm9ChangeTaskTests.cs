using System.Threading.Tasks;
using SecurePipelineScan.Rules.Security;
using SecurePipelineScan.VstsService;
using SecurePipelineScan.VstsService.Requests;
using Shouldly;
using Xunit;

namespace Rules.Tests.Integration.Security
{
    public class MultiStagePipelineHasSm9ChangeTaskTests : IClassFixture<TestConfig>
    {
        private readonly TestConfig _config;
        public MultiStagePipelineHasSm9ChangeTaskTests(TestConfig config) => _config = config;

        [Fact]
        [Trait("category", "integration")]
        public async Task EvaluateIntegrationTest()
        {
            var client = new VstsRestClient(_config.Organization, _config.Token);
            var project = await client.GetAsync(Project.ProjectById(_config.Project));
            var buildPipeline = await client.GetAsync(Builds.BuildDefinition(project.Id, "312"))
                .ConfigureAwait(false);
            var rule = new MultiStagePipelineHasSm9ChangeTask(client);
            var result = await rule.EvaluateAsync(project, buildPipeline).ConfigureAwait(false);
            result.ShouldBe(true);
        }
    }
}