namespace SecurePipelineScan.VstsService.Response
{
    public class ProjectRetentionSetting
    {
        public RetentionSetting PurgeArtifacts { get; set; }
        public RetentionSetting PurgePullRequestRuns { get; set; }
        public RetentionSetting PurgeRuns { get; set; }
        public RetentionSetting RetainRunsPerProtectedBranch { get; set; }
    }
}