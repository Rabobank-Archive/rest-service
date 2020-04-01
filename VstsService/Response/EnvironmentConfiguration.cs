namespace SecurePipelineScan.VstsService.Response
{
    public class EnvironmentConfiguration
    {
        public int Id { get; set; }
        public EnvironmentConfigurationType Type { get; set; }
        public Resource Resource { get; set; }
    }
}