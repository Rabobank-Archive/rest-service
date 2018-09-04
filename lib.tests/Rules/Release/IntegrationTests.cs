using System.Linq;
using System.Net;
using Shouldly;
using Xunit;

namespace lib.tests.Rules.Release
{
    public class IntegrationTests
    {
        [Fact]
        public void ReleaseWithApproval()
        {
            const string id = "616";
            var vsts = VstsClientFactory.Create();

            var release = vsts.Execute<Response.Release>(Requests.Release.Releases("TAS", id));
            release.ErrorMessage.ShouldBeNull();

            release.StatusCode.ShouldBe(HttpStatusCode.OK);
            release.Data.Id.ShouldBe(id);
            release.Data.Environments.ShouldNotBeEmpty();

            var env = release.Data.Environments.First();
            env.Id.ShouldNotBeNullOrEmpty();
            env.PreDeployApprovals.ShouldNotBeEmpty();
            env.DeploySteps.ShouldNotBeEmpty();

            var deploy = env.DeploySteps.First();
            deploy.RequestedFor.ShouldNotBeNull();
            deploy.RequestedFor.Id.ShouldNotBeNull();
            deploy.LastModifiedBy.ShouldNotBeNull();

            var predeploy = env.PreDeployApprovals.First();
            predeploy.Status.ShouldNotBeNullOrEmpty();
            predeploy.ApprovalType.ShouldNotBeNullOrEmpty();
            predeploy.IsAutomated.ShouldBe(false);
            predeploy.ApprovedBy.ShouldNotBeNull();
            predeploy.ApprovedBy.DisplayName.ShouldNotBeNullOrEmpty();
        }
    }
}