using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using SecurePipelineScan.VstsService.Requests;
using Shouldly;
using Xunit;

namespace SecurePipelineScan.VstsService.Tests
{
    [Trait("category", "integration")]
    public class UserEntitlementIntegrationTests : IClassFixture<TestConfig>
    {
        private readonly TestConfig _config;


        public UserEntitlementIntegrationTests(TestConfig config)
        {
            _config = config;
        }

        [Fact]
        public void TestUserEntitlement()
        {
            var client = new VstsRestClient(_config.Organization, _config.Token);
            var result = client
                .Get(MemberEntitlementManagement.UserEntitlements())
                .ToList();

            var first = result.First(x => x.LastAccessedDate != default);
            first.DateCreated.ShouldNotBe(default);
            first.Id.ShouldNotBe(default);

            var user = first.User;
            user.PrincipalName.ShouldNotBe(default);
            user.MailAddress.ShouldNotBe(default);
            user.DisplayName.ShouldNotBe(default);

            var msdn = result.First(x => x.AccessLevel.LicensingSource == "msdn").AccessLevel;
            msdn.Status.ShouldNotBeEmpty();
            msdn.LicenseDisplayName.ShouldNotBeEmpty();
            msdn.MsdnLicenseType.ShouldNotBe("none");
            msdn.AccountLicenseType.ShouldBe("none");

            var account = result.First(x => x.AccessLevel.LicensingSource == "account").AccessLevel;
            account.Status.ShouldNotBeEmpty();
            account.MsdnLicenseType.ShouldBe("none");
            account.AccountLicenseType.ShouldNotBe("none");
            account.MsdnLicenseType.ShouldNotBeEmpty();
        }

        [Fact]
        public void TestMultipleEntitlements_WhenResultIsMoreThanTake_ThenRemainderShouldFetchedInSubsequentRequest()
        {
            var client = new VstsRestClient(_config.Organization, _config.Token);
            var result = client.Get(MemberEntitlementManagement.UserEntitlements());
            result.Count().ShouldBeGreaterThan(20);
        }

        [Theory]
        [InlineData("stakeholder")]
        [InlineData("express")]
        public async Task TestUpdateLicense(string license)
        {
            var client = new VstsRestClient(_config.Organization, _config.Token);
            var entitlement = client
                .Get(MemberEntitlementManagement.UserEntitlements())
                .FirstOrDefault(e => e.User.MailAddress.Equals(_config.EntitlementUser));

            entitlement.AccessLevel.AccountLicenseType = license;

            var patchDocument = new JsonPatchDocument().Replace("/accessLevel", entitlement.AccessLevel);
            _ = await client.PatchAsync(MemberEntitlementManagement.PatchUserEntitlements(entitlement.Id), patchDocument);

            var result = client
                .Get(MemberEntitlementManagement.UserEntitlements())
                .FirstOrDefault(e => e.User.MailAddress.Equals(_config.EntitlementUser));

            Assert.Equal(license, result.AccessLevel.AccountLicenseType);
        }

        [Fact]
        public async Task TestUserEntitlementSummary()
        {
            var client = new VstsRestClient(_config.Organization, _config.Token);
            var result = await client.GetAsync(MemberEntitlementManagement.UserEntitlementSummary());
            result.Licenses.ShouldNotBeEmpty();

            var license = result.Licenses.First();
            license.LicenseName.ShouldNotBeEmpty();
            license.Assigned.ShouldNotBe(default);
        }
    }
}