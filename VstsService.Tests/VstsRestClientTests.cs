using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Testing;
using Newtonsoft.Json.Linq;
using SecurePipelineScan.VstsService.Requests;
using Shouldly;
using Xunit;

namespace SecurePipelineScan.VstsService.Tests
{
    public class VstsRestClientTests : IClassFixture<TestConfig>
    {
        private const string InvalidToken = "77p7fc7hpclqst4irzpwz452gkze75za7xkpbamkdy6lgtngjvcq";
        private readonly TestConfig _config;
        private readonly IVstsRestClient _vsts;

        public VstsRestClientTests(TestConfig config)
        {
            _config = config;
            _vsts = new VstsRestClient(config.Organization, config.Token);
        }

        [Fact]
        public async Task DeleteThrowsOnError()
        {
            var request = new VstsRequest<int>("/delete/some/data");

            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWith(status: 500);
                var client = new VstsRestClient("dummy", "pat");
                await Assert.ThrowsAsync<FlurlHttpException>(async () => await client.DeleteAsync(request));
            }
         }

        [Fact]
        public async Task PostThrowsOnError()
        {
            var request = new VstsRequest<int,int>("/get/some/data");

            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWith(status: 500);
                var client = new VstsRestClient("dummy", "pat");
                await Assert.ThrowsAsync<FlurlHttpException>(async () => await client.PostAsync(request, 3));
            }
        }
        
        [Fact]
        public async Task PutThrowsOnError()
        {
            var request = new VstsRequest<int,int>("/put/some/data");

            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWith(status: 500);
                var client = new VstsRestClient("dummy", "pat");
                await Assert.ThrowsAsync<FlurlHttpException>(async () => await client.PutAsync(request, 3));
            }
        }

        [Fact]
        public async Task GetThrowsOnError()
        {
            var request = new VstsRequest<int>("/get/some/data");

            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWith(status: 500);
                var client = new VstsRestClient("dummy", "pat");
                await Assert.ThrowsAsync<FlurlHttpException>(async () => await client.GetAsync(request));
            }
        }

        [Fact]
        public async Task GetJsonThrowsOnError()
        {
            var request = new VstsRequest<JObject>("/get/some/data");

            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWith(status: 500);
                var client = new VstsRestClient("dummy", "pat");
                await Assert.ThrowsAsync<FlurlHttpException>(async () => await client.GetAsync(request));
            }
        }

        [Fact]
        [Trait("category", "integration")]
        public async Task HtmlInsteadOfXmlShouldThrow()
        {
            var sut = new VstsRestClient("raboweb-test", InvalidToken);
            await Assert.ThrowsAsync<FlurlHttpException>(async () =>
            {
                await sut.GetAsync(Project.Projects());
            });
        }

        [Fact]
        [Trait("category", "integration")]
        public async Task RestRequestResultAsJsonObject()
        {
            var endpoints = await _vsts.GetAsync(Requests.ServiceEndpoint.Endpoints(_config.Project).AsJson());
            endpoints.SelectToken("value[?(@.data.subscriptionId == '45cfa52a-a2aa-4a18-8d3d-29896327b51d')]").ShouldNotBeNull();
        }

        
        [Fact]
        public async Task NotFoundIsNull()
        {
            (await _vsts.GetAsync(Requests.Builds.Build("TAS", "2342423"))).ShouldBeNull();
        }
    }
}