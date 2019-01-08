using SecurePipelineScan.Rules.Reports;

namespace Rules.Reports
{
    public class SecurityReport
    {
        public string Project { get; set; }
        
        public bool ApplicationGroupContainsProductionEnvironmentOwner { get; set; }
        public bool ProjectAdminGroupOnlyContainsRabobankProjectAdminGroup { get; set; }
        
        public RepositoryRights RepositoryRightsProjectAdmin { get; set; }
        
        public BuildRights BuildRightsProjectAdmin { get; set; }
        public BuildRights BuildRightsBuildAdmin { get; set; }
        public BuildRights BuildDefinitionsRightsProjectAdmin { get; set; }
        public BuildRights BuildDefinitionsRightsBuildAdmin { get; set; }
        
        public ReleaseRights ReleaseRightsRaboProjectAdmin { get; set; }
        public ReleaseRights ReleaseRightsContributor { get; set; }
        public ReleaseRightsProductionEnvOwner ReleaseRightsProductionEnvOwner { get; set; }

        public bool ProjectIsSecure => ApplicationGroupContainsProductionEnvironmentOwner &&
                                       ProjectAdminGroupOnlyContainsRabobankProjectAdminGroup &&
                                       BuildRightsBuildAdmin.BuildRightsIsSecure &&
                                       BuildRightsProjectAdmin.BuildRightsIsSecure &&
                                       BuildDefinitionsRightsProjectAdmin.BuildRightsIsSecure &&
                                       BuildDefinitionsRightsBuildAdmin.BuildRightsIsSecure &&
                                       RepositoryRightsProjectAdmin.RepositoryRightsIsSecure &&
                                       ReleaseRightsRaboProjectAdmin.ReleaseRightsIsSecure &&
                                       ReleaseRightsContributor.ReleaseRightsIsSecure &&
                                       ReleaseRightsProductionEnvOwner.ReleaseRightsProductEnvOwnerIsSecure
        ;

    }
}