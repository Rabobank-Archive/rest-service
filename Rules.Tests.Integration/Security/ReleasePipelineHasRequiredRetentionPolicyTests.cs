using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using SecurePipelineScan.Rules.Security;
using SecurePipelineScan.VstsService;
using SecurePipelineScan.VstsService.Requests;
using Shouldly;
using Xunit;

namespace Rules.Tests.Integration.Security
{
    public class ReleasePipelineHasRequiredRetentionPolicyTests : IClassFixture<TestConfig>
    {
        private readonly TestConfig _config;
        private const string PipelineId = "1";
        private readonly Fixture _fixture = new Fixture {RepeatCount = 1};

        public ReleasePipelineHasRequiredRetentionPolicyTests(TestConfig config)
        {
            _config = config;
            _fixture.Customize(new AutoNSubstituteCustomization());
        }

        [Fact]
        [Trait("category", "integration")]
        public async Task EvaluateIntegrationTest()
        {
            //Arrange
            var client = new VstsRestClient(_config.Organization, _config.Token);
            var releasePipeline = await client.GetAsync(ReleaseManagement.Definition(_config.Project, PipelineId))
                .ConfigureAwait(false);

            //Act
            var rule = new ReleasePipelineHasRequiredRetentionPolicy(client);
            var result = await rule.EvaluateAsync(_config.Project, releasePipeline);

            //Assert
            result.ShouldBe(true);
        }

        [Fact]
        [Trait("category", "integration")]
        public async Task Reconcile()
        {
            //Arrange
            var client = new VstsRestClient(_config.Organization, _config.Token);

            //Act
            var rule = new ReleasePipelineHasRequiredRetentionPolicy(client) as IReconcile;
            await rule.ReconcileAsync(_config.Project, PipelineId);
        }
    }
}