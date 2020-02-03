using System.Threading.Tasks;
using SecurePipelineScan.VstsService;
using SecurePipelineScan.VstsService.Requests;

namespace SecurePipelineScan.Rules.Permissions
{
    internal class ForRepository : ForAll, IFor
    {
        private readonly IVstsRestClient _client;
        private readonly string _projectId;
        private readonly string _itemId;

        public ForRepository(IVstsRestClient client, string projectId, string itemId) : base(client, projectId)
        {
            _client = client;
            _projectId = projectId;
            _itemId = itemId;
        }

        public Task<VstsService.Response.ApplicationGroups> IdentitiesAsync() =>
            _client.GetAsync(ApplicationGroup.ExplicitIdentitiesRepos(_projectId, SecurityNamespaceIds.GitRepositories, _itemId));

        public Task<VstsService.Response.PermissionsSetId> PermissionSetAsync(VstsService.Response.ApplicationGroup identity) =>
            _client.GetAsync(VstsService.Requests.Permissions.PermissionsGroupRepository(_projectId, SecurityNamespaceIds.GitRepositories,identity.TeamFoundationId, _itemId));
    }
}