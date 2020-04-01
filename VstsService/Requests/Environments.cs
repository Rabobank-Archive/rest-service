using System.Collections.Generic;
using SecurePipelineScan.VstsService.Response;

namespace SecurePipelineScan.VstsService.Requests
{
    public static class Environments
    {
        public static IEnumerableRequest<EnvironmentYaml> All(string projectId) =>
            new VstsRequest<EnvironmentYaml>(
                $"{projectId}/_apis/distributedtask/environments", new Dictionary<string, object>
                {
                    { "api-version", "6.0-preview.1" }
                }).AsEnumerable();

        public static IEnumerableRequest<EnvironmentConfiguration> Configuration(string projectId, int environmentId) =>
            new VstsRequest<EnvironmentConfiguration>(
                $"{projectId}/_apis/pipelines/checks/configurations", new Dictionary<string, object>
                {
                    { "api-version", "6.0-preview.1" },
                    { "resourceType", "environment" },
                    { "resourceId", environmentId }
                }).AsEnumerable();

        public static IEnumerableRequest<EnvironmentSecurityGroup> Security(string projectId, int environmentId) =>
            new VstsRequest<EnvironmentSecurityGroup>(
                $"_apis/securityroles/scopes/distributedtask.environmentreferencerole/roleassignments/resources" +
                    $"/{projectId}_{environmentId}", new Dictionary<string, object>
                {
                    { "api-version", "6.0-preview.1" }
                }).AsEnumerable();
    }
}