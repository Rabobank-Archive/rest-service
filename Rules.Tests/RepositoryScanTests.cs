﻿using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NSubstitute;
using RestSharp;
using Rules.Reports;
using SecurePipelineScan.Rules.Reports;
using SecurePipelineScan.VstsService;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Response = SecurePipelineScan.VstsService.Response;

namespace SecurePipelineScan.Rules.Tests
{
    public class RepositoryScanTests : IClassFixture<TestConfig>
    {
        private readonly ITestOutputHelper output;
        private readonly TestConfig config;

        public RepositoryScanTests(ITestOutputHelper output, TestConfig config)
        {
            this.output = output;
            this.config = config;
        }

        [Fact]
        [Trait("category", "integration")]
        public void IntegrationTestOnScan()
        {
            var organization = config.Organization;
            string token = config.Token;

            var client = new VstsRestClient(organization, token);
            var scan = new RepositoryScan(client);
            scan.Execute(config.Project).ToList().ForEach(x => output.WriteLine($"Repository: {x.Repository}, Result: {x.HasRequiredReviewerPolicy}"));
        }

        [Fact]
        public void ReportsProgress()
        {
            var fixture = new Fixture();
            fixture.Customize(new AutoNSubstituteCustomization());

            fixture.Customize<RestResponse<Response.Multiple<Response.MinimumNumberOfReviewersPolicy>>>(
            e => e.Without(x => x.ErrorMessage));

            var minimumNumberOfReviewersPolicy = fixture.Create<RestResponse<Response.Multiple<Response.MinimumNumberOfReviewersPolicy>>>();

            var repos = fixture.Create<RestResponse<Response.Multiple<Response.Repository>>>();

            var client = Substitute.For<IVstsRestClient>();
            client
                .Execute(Arg.Any<IVstsRestRequest<Response.Multiple<Response.MinimumNumberOfReviewersPolicy>>>())
                .Returns(minimumNumberOfReviewersPolicy);

            client
                .Execute(Arg.Any<IVstsRestRequest<Response.Multiple<Response.Repository>>>())
                .Returns(repos);

            var scan = new RepositoryScan(client);
            scan.Execute("dummy").ShouldNotBeEmpty();
        }
    }
}