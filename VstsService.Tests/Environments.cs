using System.Linq;
using System.Threading.Tasks;
using SecurePipelineScan.VstsService.Requests;
using Shouldly;
using Xunit;

namespace SecurePipelineScan.VstsService.Tests
{
    public class Environments : IClassFixture<TestConfig>
    {
        private readonly TestConfig _config;
        private readonly IVstsRestClient _client;

        public Environments(TestConfig config)
        {
            _config = config;
            _client = new VstsRestClient(_config.Organization, _config.Token);
        }

        [Fact]
        [Trait("category", "integration")]
        public void QueryEnvironments()
        {
            var environments = _client.Get(Requests.Environments.All(_config.Project));
            environments.ShouldNotBeEmpty();

            var environment = environments.First();
            environment.Id.ShouldNotBe(0);
            environment.Name.ShouldNotBeNull();
            environment.Project.ShouldNotBeNull();
            environment.Project.Id.ShouldNotBeNull();
        }

        [Fact]
        [Trait("category", "integration")]
        public void QueryEnvironmentConfig()
        {
            var environmentId = _client.Get(Requests.Environments.All(_config.Project)).First().Id;
            var configs = _client.Get(Requests.Environments.Configuration(_config.Project, environmentId));
            configs.ShouldNotBeEmpty();

            var config = configs.First();
            config.Id.ShouldNotBe(0);
            config.Resource.ShouldNotBeNull();
            config.Resource.Id.ShouldBe(environmentId);
            config.Type.ShouldNotBeNull();
            config.Type.Name.ShouldNotBeNull();
        }

        [Fact]
        [Trait("category", "integration")]
        public async Task QueryEnvironmentSecurityGroups()
        {
            var projectId = (await _client.GetAsync(Project.ProjectById(_config.Project)).ConfigureAwait(false)).Id;
            var environmentId = _client.Get(Requests.Environments.All(_config.Project)).First().Id;
            var groups = _client.Get(Requests.Environments.Security(projectId, environmentId));
            groups.ShouldNotBeEmpty();

            var group = groups.First();
            group.Identity.ShouldNotBeNull();
            group.Identity.DisplayName.ShouldNotBeNull();
            group.Identity.Id.ShouldNotBeNull();
            group.Role.ShouldNotBeNull();
            group.Role.Name.ShouldNotBeNull();
        }
    }
}