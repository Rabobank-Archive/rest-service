namespace SecurePipelineScan.VstsService.Response
{
    public class EnvironmentSecurityRole
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public int AllowPermissions { get; set; }
        public int DenyPermissions { get; set; }
        public string Identifier { get; set; }
        public string Description { get; set; }
        public string Scope { get; set; }
    }
}