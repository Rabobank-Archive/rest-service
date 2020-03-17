using System;
using System.Buffers.Text;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace SecurePipelineScan.VstsService.Tests
{
    public class TestConfig
    {
        public TestConfig()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.user.json", true)
                .AddEnvironmentVariables()
                .Build();
                
            configuration.Bind(this);
        }

        public string Token { get; set; }
        public string Project { get; set; }
        public string Organization { get; set; }
        public string ExpectedAgentPoolName { get; set; } = "Default";
        public string ReleaseDefinitionId { get; set; } = "1";
        public string ReleaseDefinitionName { get; set; } = "AZDO-COMPLIANCY-FUNCTION (Green)";
        public int AgentPoolId { get; set; } = 1;
        public string BuildId { get; set; } = "20997";
        public string BuildDefinitionId { get; set; } = "2";
        public string RepositoryId { get; set; } = "6435e3f0-15b7-4302-814d-4ab586e61f8b";
        public string GitItemFilePath { get; set; } = "/azure-pipelines.yml";
        public string EntitlementUser { get; set; } = Encoding.UTF8.GetString(Convert.FromBase64String("UmljaGFyZC5PcHJpbnNAcmFib2Jhbmsubmw=")); // some obfuscation to hide the e-mail address
    }
}