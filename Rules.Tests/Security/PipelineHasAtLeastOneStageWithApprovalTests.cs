using AutoFixture;
using NSubstitute;
using SecurePipelineScan.Rules.Security;
using SecurePipelineScan.VstsService;
using SecurePipelineScan.VstsService.Response;
using Shouldly;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace SecurePipelineScan.Rules.Tests.Security
{
    public class PipelineHasAtLeastOneStageWithApprovalTests : IClassFixture<TestConfig>
    {
        private readonly TestConfig _config;
        private readonly IVstsRestClient _client = Substitute.For<IVstsRestClient>();

        public PipelineHasAtLeastOneStageWithApprovalTests(TestConfig config)
        {
            _config = config;
        }

        [Fact]
        public async Task EvaluateIntegrationTest()
        {
            //Arrange
            var client = new VstsRestClient(_config.Organization, _config.Token);

            //Act
            var rule = new PipelineHasAtLeastOneStageWithApproval(client);
            (await rule.EvaluateAsync(_config.Project, "1")).ShouldBeTrue();
        }

        [Theory]
        [InlineData(false,true)]
        [InlineData(true,false)]
        public async Task GivenReleaseCreatorCanBeApprover_ShouldEvaluate(bool releaseCreatorCanBeApprover, bool compliant)
        {
            //Arrange
            var fixture = new Fixture();
            fixture.Customize<ApprovalOptions>(ctx =>
                ctx.With(a => a.ReleaseCreatorCanBeApprover, releaseCreatorCanBeApprover));
            var def = fixture.Create<ReleaseDefinition>();

            _client
                .GetAsync(Arg.Any<IVstsRequest<ReleaseDefinition>>())
                .Returns(def);
            
            //Act
            var rule = new PipelineHasAtLeastOneStageWithApproval(_client);
            var result = await rule.EvaluateAsync(_config.Project, "1");
            
            //Assert
            result.ShouldBe(compliant);

        }

        [Fact]
        public async Task GivenNoApprovers_ShouldBeNonCompliant()
        {
            //Arrange
            var fixture = new Fixture();
            fixture.Customize<Approval>(ctx =>
                ctx.With(a => a.Approver, null));
            var def = fixture.Create<ReleaseDefinition>();

            _client
                .GetAsync(Arg.Any<IVstsRequest<ReleaseDefinition>>())
                .Returns(def);
            
            //Act
            var rule = new PipelineHasAtLeastOneStageWithApproval(_client);
            var result = await rule.EvaluateAsync(_config.Project, "1");
            
            //Assert
            result.ShouldBe(false);
        }

        [Fact]
        public async Task GivenNoApprovalOptions_ShouldBeNonCompliant()
        {
            //Arrange
            var fixture = new Fixture();
            fixture.Customize<PreDeployApprovals>(ctx =>
                ctx.With(a => a.ApprovalOptions, null));
            var def = fixture.Create<ReleaseDefinition>();

            _client
                .GetAsync(Arg.Any<IVstsRequest<ReleaseDefinition>>())
                .Returns(def);
            
            //Act
            var rule = new PipelineHasAtLeastOneStageWithApproval(_client);
            var result = await rule.EvaluateAsync(_config.Project, "1");
            
            //Assert
            result.ShouldBe(false);
        }
    }
}