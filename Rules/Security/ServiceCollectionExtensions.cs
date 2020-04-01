using Microsoft.Extensions.DependencyInjection;

namespace SecurePipelineScan.Rules.Security
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultRules(this IServiceCollection collection)
        {
            return collection
                .AddGlobalPermissions()
                .AddRepositoryRules()
                .AddBuildRules()
                .AddReleaseRules()
                .AddMultiStageRules();
        }

        public static IServiceCollection AddGlobalPermissions(this IServiceCollection collection) =>
            collection
                .AddSingleton<IProjectRule, NobodyCanDeleteTheTeamProject>()
                .AddSingleton<IProjectRule, ShouldBlockPlainTextCredentialsInPipelines>();

        public static IServiceCollection AddRepositoryRules(this IServiceCollection collection) =>
            collection
                .AddSingleton<IRepositoryRule, NobodyCanDeleteTheRepository>()
                .AddSingleton<IRepositoryRule, ReleaseBranchesProtectedByPolicies>()
                .AddSingleton<IRepositoryRule, NobodyCanBypassPolicies>();

        public static IServiceCollection AddBuildRules(this IServiceCollection collection) =>
            collection
                .AddSingleton<IBuildPipelineRule, NobodyCanDeleteBuilds>()
                .AddSingleton<IBuildPipelineRule, ArtifactIsStoredSecure>()
                .AddSingleton<IBuildPipelineRule, BuildPipelineHasSonarqubeTask>()
                .AddSingleton<IBuildPipelineRule, BuildPipelineHasFortifyTask>()
                .AddSingleton<IBuildPipelineRule, BuildPipelineHasNexusIqTask>()
                .AddSingleton<IBuildPipelineRule, BuildPipelineHasCredScanTask>();

        public static IServiceCollection AddReleaseRules(this IServiceCollection collection) =>
            collection
                .AddSingleton<IReleasePipelineRule, NobodyCanDeleteReleases>()
                .AddSingleton<IReleasePipelineRule, NobodyCanManageApprovalsAndCreateReleases>()
                .AddSingleton<IReleasePipelineRule, ReleasePipelineHasRequiredRetentionPolicy>()
                .AddSingleton<IReleasePipelineRule, ReleasePipelineUsesBuildArtifact>()
                .AddSingleton<IReleasePipelineRule, ReleasePipelineHasBranchFilter>()
                .AddSingleton<IReleasePipelineRule, ReleasePipelineContainsApproval>()
                .AddSingleton<IReleasePipelineRule, ReleasePipelineHasSm9ChangeTask>()
                .AddSingleton<IReleasePipelineRule, ReleasePipelineHasDeploymentMethod>();

        public static IServiceCollection AddMultiStageRules(this IServiceCollection collection) =>
            collection
                .AddSingleton<IMultiStagePipelineRule, MultiStagePipelineContainsApproval>()
                .AddSingleton<IMultiStagePipelineRule, MultiStagePipelineHasBranchFilter>()
                .AddSingleton<IMultiStagePipelineRule, MultiStagePipelineHasRequiredRetentionPolicy>()
                .AddSingleton<IMultiStagePipelineRule, MultiStagePipelineHasSm9ChangeTask>()
                .AddSingleton<IMultiStagePipelineRule, NobodyCanManageApprovalsAndQueueBuilds>();
    }
}