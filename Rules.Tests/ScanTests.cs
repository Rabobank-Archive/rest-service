﻿
using System;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NSubstitute;
using RestSharp;
using SecurePipelineScan.Rules.Release;
using SecurePipelineScan.VstsService;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using Response = SecurePipelineScan.VstsService.Response;
using SecurePipelineScan.Rules.Reports;

namespace SecurePipelineScan.Rules.Tests
{
    /// <summary>
    /// This is a test
    /// </summary>
    public class ScanTests : IClassFixture<TestConfig>
    {
        private readonly ITestOutputHelper output;

        public ScanTests(ITestOutputHelper output, TestConfig config)
        {
            this.output = output;
            Config = config;
        }

        public TestConfig Config { get; }

        [Fact]
        [Trait("category", "integration")]
        public void GetAllRules()
        {
            var organization = Config.Organization;
            string token = Config.Token;

            var client = new VstsRestClient(organization, token);
            var scan = new Scan(client, x => output.WriteLine($"{x.Request.Definition.Name}: {(x as ReleaseReport)?.Result}"));
            scan.Execute(Config.Project);
        }

        [Fact]
        [Trait("category", "integration")]
        public void Test714()
        {
            var client = new VstsRestClient(Config.Organization, Config.Token);
            var rule = new FourEyesOnAllBuildArtefacts();

            var release = client.Execute(new VstsRestRequest<Response.Release>("https://raboweb.vsrm.visualstudio.com/f64ffdfa-0c4e-40d9-980d-bb8479366fc5/_apis/Release/releases/741", Method.GET)).ThrowOnError();
            rule.GetResult(release.Data, 1915).ShouldBeTrue();
        }

        [Fact]
        public void ThrowsOnErrorWhenServiceEndpointsFails()
        {
            var client = Substitute.For<IVstsRestClient>();
            client.Execute(Arg.Any<IVstsRestRequest<Response.Multiple<Response.ServiceEndpoint>>>()).Returns(new RestResponse<Response.Multiple<Response.ServiceEndpoint>>
            {
                ErrorMessage = "fail"
            });

            var scan = new Scan(client, _ => { });
            var ex = Assert.Throws<Exception>(() => scan.Execute("dummy"));
            ex.Message.ShouldBe("fail");
        }

        [Fact]
        public void ThrowsOnErrorWhenServiceEndpointHistoryFails()
        {
            var client = Substitute.For<IVstsRestClient>();

            client.Execute(Arg.Any<IVstsRestRequest<Response.Multiple<Response.ServiceEndpoint>>>()).Returns(new RestResponse<Response.Multiple<Response.ServiceEndpoint>>
            {
                Data = new Response.Multiple<Response.ServiceEndpoint>
                {
                    Value = new[] {
                        new Response.ServiceEndpoint {
                        }
                    }
                }
            });

            client.Execute(Arg.Any<IVstsRestRequest<Response.Multiple<Response.ServiceEndpointHistory>>>()).Returns(new RestResponse<Response.Multiple<Response.ServiceEndpointHistory>>
            {
                ErrorMessage = "fail"
            });

            var scan = new Scan(client, _ => { });
            var ex = Assert.Throws<Exception>(() => scan.Execute("dummy"));
            ex.Message.ShouldBe("fail");
        }

        [Fact]
        public void ThrowsOnErrorWhenReleaseFails()
        {
            var fixture = new Fixture();
            fixture.Customize(new AutoNSubstituteCustomization());

            fixture.Customize<RestResponse<Response.Multiple<Response.ServiceEndpoint>>>(
                e => e.Without(x => x.ErrorMessage));

            fixture.Customize<RestResponse<Response.Multiple<Response.ServiceEndpointHistory>>>(
                e => e.Without(x => x.ErrorMessage));

            fixture.Customize<Response.ServiceEndpointHistoryData>(
                e => e.With(x => x.PlanType, "Release"));

            var endpoints = fixture.Create<RestResponse<Response.Multiple<Response.ServiceEndpoint>>>();
            var history = fixture.Create<RestResponse<Response.Multiple<Response.ServiceEndpointHistory>>>();
            var release = fixture.Build<RestResponse<Response.Release>>().With(
                e => e.ErrorMessage, "fail").Create();

            var client = Substitute.For<IVstsRestClient>();
            client
                .Execute(Arg.Any<IVstsRestRequest<Response.Multiple<Response.ServiceEndpoint>>>())
                .Returns(endpoints);

            client
                .Execute(Arg.Any<IVstsRestRequest<Response.Multiple<Response.ServiceEndpointHistory>>>())
                .Returns(history);

            client
                .Execute(Arg.Any<IVstsRestRequest<Response.Release>>())
                .Returns(release);


            var scan = new Scan(client, _ => { });
            var ex = Assert.Throws<Exception>(() => scan.Execute("dummy"));
            ex.Message.ShouldBe("fail");
        }

        [Fact]
        public void ReportsProgress()
        {
            var fixture = new Fixture();
            fixture.Customize(new AutoNSubstituteCustomization());

            fixture.Customize<RestResponse<Response.Multiple<Response.ServiceEndpoint>>>(
                e => e.Without(x => x.ErrorMessage));

            fixture.Customize<RestResponse<Response.Multiple<Response.ServiceEndpointHistory>>>(
                e => e.Without(x => x.ErrorMessage));

            fixture.Customize<RestResponse<Response.Release>>(
                e => e.Without(x => x.ErrorMessage));

            fixture.Customize<Response.ServiceEndpointHistoryData>(
                e => e.With(x => x.PlanType, "Release"));

            var endpoints = fixture.Create<RestResponse<Response.Multiple<Response.ServiceEndpoint>>>();
            var history = fixture.Create<RestResponse<Response.Multiple<Response.ServiceEndpointHistory>>>();
            var release = fixture.Create<RestResponse<Response.Release>>();

            var client = Substitute.For<IVstsRestClient>();
            client
                .Execute(Arg.Any<IVstsRestRequest<Response.Multiple<Response.ServiceEndpoint>>>())
                .Returns(endpoints);

            client
                .Execute(Arg.Any<IVstsRestRequest<Response.Multiple<Response.ServiceEndpointHistory>>>())
                .Returns(history);

            client
                .Execute(Arg.Any<IVstsRestRequest<Response.Release>>())
                .Returns(release);


            var progress = Substitute.For<Action<ScanReport>>();
            var scan = new Scan(client, progress);
            scan.Execute("dummy");

            progress.Received().Invoke(Arg.Any<ScanReport>());
        }
    }
}
