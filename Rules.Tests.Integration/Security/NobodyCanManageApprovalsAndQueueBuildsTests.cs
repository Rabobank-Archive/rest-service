using NSubstitute;
using SecurePipelineScan.Rules.Security;
using SecurePipelineScan.VstsService;
using SecurePipelineScan.VstsService.Requests;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Rules.Tests.Integration.Security
{
    public class NobodyCanManageApprovalsAndQueueBuildsTests : IClassFixture<TestConfig>
    {
        private readonly TestConfig _config;
        public NobodyCanManageApprovalsAndQueueBuildsTests(TestConfig config) => _config = config;

        [Fact]
        [Trait("category", "integration")]
        public async Task EvaluateIntegrationTest()
        {
            var client = new VstsRestClient(_config.Organization, _config.Token);
            var project = await client.GetAsync(Project.ProjectById(_config.Project));
            var buildPipeline = await client.GetAsync(Builds.BuildDefinition(project.Id, "312"))
                .ConfigureAwait(false);

            var productionItemsResolver = Substitute.For<IProductionItemsResolver>();
            productionItemsResolver.ResolveAsync(_config.Project, buildPipeline.Id)
                .Returns(new[] { "Production" });

            var rule = new NobodyCanManageApprovalsAndQueueBuilds(client, productionItemsResolver);
            var result = await rule.EvaluateAsync(project, buildPipeline).ConfigureAwait(false);
            result.ShouldBe(true);
        }
    }
}