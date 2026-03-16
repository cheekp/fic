using System.Net;
using Fic.Contracts;
using Fic.Platform.Web.Components.Layout;
using Fic.Platform.Web.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Fic.Platform.Web.Tests;

public sealed class MerchantAuthBoundaryTests
{
    private const string BaseUri = "https://demo.fic.test/";
    private const string FallbackLogoUrl = "data:image/svg+xml;base64,ZmFrZQ==";

    [Fact]
    public async Task MerchantWorkspace_RedirectsUnauthenticatedUser_ToLogin()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();
        var workspace = await CreateMerchantAsync(state);

        var response = await client.GetAsync($"/portal/merchant/{workspace.Merchant.MerchantId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/account/login?returnUrl=", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoginEndpoint_SignsMerchantIntoTheirWorkspace()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();
        var workspace = await CreateMerchantAsync(state);
        state.ConfigureMerchantAccess(workspace.Merchant.MerchantId, "very-secure-password");

        var response = await client.PostAsync(
            "/account/session/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = workspace.Merchant.ContactEmail,
                ["password"] = "very-secure-password",
                ["returnUrl"] = $"/portal/merchant/{workspace.Merchant.MerchantId}"
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"/portal/merchant/{workspace.Merchant.MerchantId}", response.Headers.Location?.OriginalString);

        var workspaceResponse = await client.GetAsync($"/portal/merchant/{workspace.Merchant.MerchantId}");

        if (workspaceResponse.StatusCode == HttpStatusCode.Redirect)
        {
            var location = workspaceResponse.Headers.Location?.OriginalString;
            Assert.NotNull(location);
            Assert.Contains($"/portal/merchant/{workspace.Merchant.MerchantId}", location, StringComparison.Ordinal);
            Assert.DoesNotContain("/account/login", location, StringComparison.Ordinal);
        }
        else
        {
            Assert.Equal(HttpStatusCode.OK, workspaceResponse.StatusCode);
            Assert.NotEqual("/account/login", workspaceResponse.RequestMessage?.RequestUri?.AbsolutePath);
        }
    }

    [Fact]
    public async Task MerchantWorkspace_RedirectsToAccessDenied_WhenSignedInMerchantDoesNotOwnRoute()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();
        var firstWorkspace = await CreateMerchantAsync(state);
        var secondWorkspace = await state.CreateMerchantAsync(
            "Beam Me Up",
            "Bath",
            "BA1 1AA",
            "beam@shop.test",
            logoUpload: null,
            fallbackLogoUrl: FallbackLogoUrl,
            primaryColor: "#1f3731",
            accentColor: "#f4c15d",
            baseUri: BaseUri);

        state.ConfigureMerchantAccess(firstWorkspace.Merchant.MerchantId, "very-secure-password");
        state.ConfigureMerchantAccess(secondWorkspace.Merchant.MerchantId, "another-secure-password");

        await client.PostAsync(
            "/account/session/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = firstWorkspace.Merchant.ContactEmail,
                ["password"] = "very-secure-password"
            }));

        var response = await client.GetAsync($"/portal/merchant/{secondWorkspace.Merchant.MerchantId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/account/access-denied", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Logout_ClearsMerchantSession()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();
        var workspace = await CreateMerchantAsync(state);
        state.ConfigureMerchantAccess(workspace.Merchant.MerchantId, "very-secure-password");

        await client.PostAsync(
            "/account/session/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = workspace.Merchant.ContactEmail,
                ["password"] = "very-secure-password"
            }));

        var logout = await client.GetAsync("/account/logout");
        var afterLogout = await client.GetAsync($"/portal/merchant/{workspace.Merchant.MerchantId}");

        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        Assert.Equal("/", logout.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.Redirect, afterLogout.StatusCode);
        Assert.StartsWith("/account/login?returnUrl=", afterLogout.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CompleteSignupEndpoint_RejectsUnsupportedPlan()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();
        var workspace = await CreateMerchantAsync(state);

        var response = await client.PostAsync(
            "/account/session/complete-signup",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["merchantId"] = workspace.Merchant.MerchantId.ToString("D"),
                ["plan"] = "enterprise",
                ["password"] = "very-secure-password",
                ["confirmPassword"] = "very-secure-password"
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"/portal/signup/plan/{workspace.Merchant.MerchantId:D}?error=unsupported-plan", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task CompleteSignupEndpoint_AllowsStarterPlanWithoutBillingCheckbox()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();
        var workspace = await CreateMerchantAsync(state);

        var response = await client.PostAsync(
            "/account/session/complete-signup",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["merchantId"] = workspace.Merchant.MerchantId.ToString("D"),
                ["plan"] = "starter",
                ["password"] = "very-secure-password",
                ["confirmPassword"] = "very-secure-password"
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(
            $"/portal/merchant/{workspace.Merchant.MerchantId:D}?tab=programmes&section=programmes&programmeSection=create",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task CompleteSignupEndpoint_DoesNotOverwriteExistingCredentials()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();
        var workspace = await CreateMerchantAsync(state);
        Assert.Equal(
            MerchantCredentialConfigurationStatus.Updated,
            state.ConfigureMerchantAccess(workspace.Merchant.MerchantId, "very-secure-password").Status);

        var response = await client.PostAsync(
            "/account/session/complete-signup",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["merchantId"] = workspace.Merchant.MerchantId.ToString("D"),
                ["plan"] = "starter",
                ["password"] = "another-secure-password",
                ["confirmPassword"] = "another-secure-password"
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"/portal/signup/billing/{workspace.Merchant.MerchantId:D}?error=credentials-already-configured&plan=starter", response.Headers.Location?.OriginalString);
    }

    private static Task<MerchantWorkspaceSnapshot> CreateMerchantAsync(DemoPlatformState state) =>
        state.CreateMerchantAsync(
            "Jo's Coffee",
            "Bristol",
            "BS1 4DJ",
            "owner@joscoffee.test",
            logoUpload: null,
            fallbackLogoUrl: FallbackLogoUrl,
            primaryColor: "#1f3731",
            accentColor: "#f4c15d",
            baseUri: BaseUri);
}
