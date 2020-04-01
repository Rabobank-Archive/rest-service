namespace SecurePipelineScan.VstsService.Response
{
    public class EnvironmentSecurityGroup
    {
        public Identity Identity { get; set; }
        public EnvironmentSecurityRole Role { get; set; }
        public string Access { get; set; }
        public string AccessDisplayName { get; set; }
    }
}