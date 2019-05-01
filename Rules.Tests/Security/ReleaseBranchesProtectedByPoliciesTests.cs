using System;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NSubstitute;
using SecurePipelineScan.Rules.Security;
using SecurePipelineScan.VstsService;
using SecurePipelineScan.VstsService.Requests;
using SecurePipelineScan.VstsService.Response;
using Shouldly;
using Xunit;
using Repository = SecurePipelineScan.VstsService.Response.Repository;

namespace SecurePipelineScan.Rules.Tests.Security
{
    public class ReleaseBranchesProtectedByPoliciesTests : IClassFixture<TestConfig>
    {
        private readonly TestConfig _config;
        private const string RepositoryId = "3167b64e-c72b-4c55-84eb-986ac62d0dec";
        private readonly Fixture _fixture = new Fixture { RepeatCount = 1 };
        private readonly IVstsRestClient _client = Substitute.For<IVstsRestClient>();

        public ReleaseBranchesProtectedByPoliciesTests(TestConfig config)
        {
            _config = config;
            _fixture.Customize(new AutoNSubstituteCustomization());
        }

        [Fact]
        public void EvaluateIntegrationTest()
        {
            var client = new VstsRestClient(_config.Organization, _config.Token);
            var projectId = client.Get(VstsService.Requests.Project.Properties(_config.Project)).Id;

            var rule = new NobodyCanDeleteTheRepository(client);
            rule.Evaluate(projectId, RepositoryId);
        }
        
        [Fact]
        public void EvaluateShouldReturnTrueForRepoHasCorrectPolicies()
        {
            //Arrange
            CustomizeScope(_fixture, RepositoryId);
            CustomizeMinimumNumberOfReviewersPolicy(_fixture);
            CustomizePolicySettings(_fixture);

            SetupClient(_client, _fixture);

            //Act
            var rule = new ReleaseBranchesProtectedByPolicies(_client);
            var evaluatedRule = rule.Evaluate(_config.Project, RepositoryId);

            //Assert
            evaluatedRule.ShouldBeTrue();
        }

        [Fact]
        public void EvaluateShouldReturnFalseForRepoNotMatchingPolicies()
        {
            //Arrange
            SetupClient(_client, _fixture);

            //Act
            var rule = new ReleaseBranchesProtectedByPolicies(_client);
            var evaluatedRule = rule.Evaluate(_config.Project, RepositoryId);

            //Assert
            evaluatedRule.ShouldBeFalse();
        }

        [Fact]
        public void EvaluateShouldReturnFalseWhenMinimumApproverCountIsLessThan2()
        {
            //Arrange
            CustomizeScope(_fixture, RepositoryId);
            CustomizeMinimumNumberOfReviewersPolicy(_fixture, true);
            CustomizePolicySettings(_fixture, minimumApproverCount: 1);

            SetupClient(_client, _fixture);

            //Act
            var rule = new ReleaseBranchesProtectedByPolicies(_client);
            var evaluatedRule = rule.Evaluate(_config.Project, RepositoryId);

            //Assert
            evaluatedRule.ShouldBeFalse();
        }

        [Fact]
        public void EvaluateShouldReturnFalseWhenPolicyIsNotEnabled()
        {
            //Arrange
            CustomizeScope(_fixture, RepositoryId);
            CustomizeMinimumNumberOfReviewersPolicy(_fixture, false);
            CustomizePolicySettings(_fixture);

            SetupClient(_client, _fixture);

            //Act
            var rule = new ReleaseBranchesProtectedByPolicies(_client);
            var evaluatedRule = rule.Evaluate(_config.Project, RepositoryId);

            //Assert
            evaluatedRule.ShouldBeFalse();
        }

        [Fact]
        public void EvaluateShouldReturnFalseWhenThereAreNoCorrectPoliciesForMasterBranch()
        {
            //Arrange
            CustomizeScope(_fixture, refName: "ref/heads/not-master");
            CustomizeMinimumNumberOfReviewersPolicy(_fixture);
            CustomizePolicySettings(_fixture);

            SetupClient(_client, _fixture);

            //Act
            var rule = new ReleaseBranchesProtectedByPolicies(_client);
            var evaluatedRule = rule.Evaluate(_config.Project, RepositoryId);

            //Assert
            evaluatedRule.ShouldBeFalse();
        }

        private void CustomizeScope(IFixture fixture, 
            string id = null,
            string refName = "refs/heads/master")
        {
            fixture.Customize<Scope>(ctx => ctx
                .With(r => r.RepositoryId, new Guid(id ?? RepositoryId))
                .With(r => r.RefName, refName));
        }

        private static void CustomizePolicySettings(IFixture fixture, 
            int minimumApproverCount = 2,
            bool resetOnSourcePush = true, 
            bool creatorVoteCounts = true)
        {
            fixture.Customize<MinimumNumberOfReviewersPolicySettings>(ctx => ctx
                .With(r => r.MinimumApproverCount, minimumApproverCount)
                .With(r => r.ResetOnSourcePush, resetOnSourcePush)
                .With(r => r.CreatorVoteCounts, creatorVoteCounts));
        }

        private static void CustomizeMinimumNumberOfReviewersPolicy(IFixture fixture, bool enabled = true)
        {
            fixture.Customize<MinimumNumberOfReviewersPolicy>(ctx => ctx
                .With(r => r.IsEnabled, enabled));
        }


        private static void SetupClient(IVstsRestClient client, IFixture fixture)
        {
            client
                .Get(Arg.Any<IVstsRestRequest<Multiple<MinimumNumberOfReviewersPolicy>>>())
                .Returns(fixture.Create<Multiple<MinimumNumberOfReviewersPolicy>>());
        }
    }
}