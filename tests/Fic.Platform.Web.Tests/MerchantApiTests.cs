using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fic.Contracts;
using Fic.Platform.Web.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Fic.Platform.Web.Tests;

public sealed class MerchantApiTests
{
    private const string BaseUri = "https://demo.fic.test/";
    private const string FallbackLogoUrl = "data:image/svg+xml;base64,ZmFrZQ==";

    [Fact]
    public async Task CreateMerchantEndpoint_ReturnsWorkspacePayload()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/merchants",
            new
            {
                displayName = "Jo's Coffee",
                contactEmail = "owner@joscoffee.test",
                shopTypeKey = "coffee"
            });

        response.EnsureSuccessStatusCode();

        var workspace = await response.Content.ReadFromJsonAsync<MerchantWorkspaceSnapshot>();
        Assert.NotNull(workspace);
        Assert.Equal("Jo's Coffee", workspace.Merchant.DisplayName);
        Assert.Equal("owner@joscoffee.test", workspace.Merchant.ContactEmail);
    }

    [Fact]
    public async Task CreateMerchantEndpoint_AllowsMissingShopTypeKey()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/merchants",
            new
            {
                displayName = "No Type Coffee",
                contactEmail = "owner@notype.test"
            });

        response.EnsureSuccessStatusCode();

        var workspace = await response.Content.ReadFromJsonAsync<MerchantWorkspaceSnapshot>();
        Assert.NotNull(workspace);
        Assert.Equal("No Type Coffee", workspace.Merchant.DisplayName);
        Assert.False(string.IsNullOrWhiteSpace(workspace.Merchant.ShopTypeKey));
    }

    [Fact]
    public async Task BrandLogoUploadEndpoint_UpdatesMerchantLogo()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();
        var workspace = await CreateMerchantAsync(state);

        var completeSignup = await client.PostAsJsonAsync(
            "/api/v1/session/complete-signup",
            new
            {
                merchantId = workspace.Merchant.MerchantId,
                plan = "starter",
                password = "very-secure-password",
                confirmPassword = "very-secure-password"
            });
        Assert.Equal(HttpStatusCode.OK, completeSignup.StatusCode);

        var pngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO7+5f8AAAAASUVORK5CYII=");
        using var form = new MultipartFormDataContent();
        using var content = new ByteArrayContent(pngBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(content, "logo", "logo.png");

        var response = await client.PostAsync($"/api/v1/merchants/{workspace.Merchant.MerchantId}/brand/logo", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<MerchantWorkspaceSnapshot>();
        Assert.NotNull(updated);
        Assert.Contains("/merchant-brand-assets/", updated.BrandProfile.LogoUrl, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MerchantWorkspaceApi_RequiresAuthentication()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();
        var workspace = await CreateMerchantAsync(state);

        var response = await client.GetAsync($"/api/v1/merchants/{workspace.Merchant.MerchantId}/workspace");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CompleteSignupApi_SignsInAndAllowsWorkspaceFetch()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();
        var workspace = await CreateMerchantAsync(state);

        var completeSignup = await client.PostAsJsonAsync(
            "/api/v1/session/complete-signup",
            new
            {
                merchantId = workspace.Merchant.MerchantId,
                plan = "starter",
                password = "very-secure-password",
                confirmPassword = "very-secure-password"
            });

        Assert.Equal(HttpStatusCode.OK, completeSignup.StatusCode);

        var workspaceResponse = await client.GetAsync($"/api/v1/merchants/{workspace.Merchant.MerchantId}/workspace");

        Assert.Equal(HttpStatusCode.OK, workspaceResponse.StatusCode);
        var payload = await workspaceResponse.Content.ReadFromJsonAsync<MerchantWorkspaceSnapshot>();
        Assert.NotNull(payload);
        Assert.Equal(workspace.Merchant.MerchantId, payload.Merchant.MerchantId);
    }

    [Fact]
    public async Task ProgrammeMutationApis_SupportCreateConfigureOperateAndRedeemFlow()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();
        var workspace = await CreateMerchantAsync(state);

        var completeSignup = await client.PostAsJsonAsync(
            "/api/v1/session/complete-signup",
            new
            {
                merchantId = workspace.Merchant.MerchantId,
                plan = "starter",
                password = "very-secure-password",
                confirmPassword = "very-secure-password"
            });
        Assert.Equal(HttpStatusCode.OK, completeSignup.StatusCode);

        var createProgramme = await client.PostAsJsonAsync(
            $"/api/v1/merchants/{workspace.Merchant.MerchantId}/programmes",
            new
            {
                templateKey = "coffee-visits"
            });
        Assert.Equal(HttpStatusCode.OK, createProgramme.StatusCode);

        var workspacePayload = await createProgramme.Content.ReadFromJsonAsync<MerchantWorkspaceSnapshot>();
        Assert.NotNull(workspacePayload);
        Assert.NotNull(workspacePayload.SelectedProgramme);
        var selectedProgramme = workspacePayload.SelectedProgramme!;

        var updateProgramme = await client.PutAsJsonAsync(
            $"/api/v1/merchants/{workspace.Merchant.MerchantId}/programmes/{selectedProgramme.ProgrammeId}",
            new
            {
                rewardItemLabel = "Coffee",
                rewardThreshold = 2,
                rewardCopy = "Buy 2 and get one free",
                startsOn = DateOnly.FromDateTime(DateTime.Today),
                endsOn = DateOnly.FromDateTime(DateTime.Today.AddMonths(6))
            });
        Assert.Equal(HttpStatusCode.OK, updateProgramme.StatusCode);

        var joinResponse = await client.PostAsync($"/api/v1/join/{selectedProgramme.JoinCode}", content: null);
        Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);
        var joinedCard = await joinResponse.Content.ReadFromJsonAsync<WalletCardSnapshot>();
        Assert.NotNull(joinedCard);

        var firstAward = await client.PostAsJsonAsync(
            $"/api/v1/merchants/{workspace.Merchant.MerchantId}/programmes/{selectedProgramme.ProgrammeId}/award-visit",
            new { scannedCode = joinedCard.CardCode });
        Assert.Equal(HttpStatusCode.OK, firstAward.StatusCode);

        var secondAward = await client.PostAsJsonAsync(
            $"/api/v1/merchants/{workspace.Merchant.MerchantId}/programmes/{selectedProgramme.ProgrammeId}/award-visit",
            new { scannedCode = joinedCard.CardCode });
        Assert.Equal(HttpStatusCode.OK, secondAward.StatusCode);

        var redeem = await client.PostAsync(
            $"/api/v1/merchants/{workspace.Merchant.MerchantId}/programmes/{selectedProgramme.ProgrammeId}/cards/{joinedCard.CardId}/redeem",
            content: null);
        Assert.Equal(HttpStatusCode.OK, redeem.StatusCode);
    }

    [Fact]
    public async Task ProgrammeMutationApis_RejectWrongMerchantOwner()
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

        var completeSignup = await client.PostAsJsonAsync(
            "/api/v1/session/complete-signup",
            new
            {
                merchantId = firstWorkspace.Merchant.MerchantId,
                plan = "starter",
                password = "very-secure-password",
                confirmPassword = "very-secure-password"
            });
        Assert.Equal(HttpStatusCode.OK, completeSignup.StatusCode);

        var forbiddenResponse = await client.PostAsJsonAsync(
            $"/api/v1/merchants/{secondWorkspace.Merchant.MerchantId}/programmes",
            new
            {
                templateKey = "coffee-visits"
            });

        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
    }

    [Fact]
    public async Task SignupPortalNavigationEndpoint_ReturnsContractForRequestedStep()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();
        var workspace = await CreateMerchantAsync(state);

        var response = await client.GetAsync(
            $"/api/v1/portal/navigation/signup?step=plan&merchantId={workspace.Merchant.MerchantId:D}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contract = await response.Content.ReadFromJsonAsync<PortalNavigationContract>();
        Assert.NotNull(contract);
        Assert.Equal("signup", contract.Surface);
        Assert.Equal("plan", contract.ActiveKey);
        Assert.NotNull(contract.Roadmap);
        Assert.Equal(6, contract.Roadmap!.TotalCount);
        Assert.Contains(contract.Roadmap.Steps, step => step.Key == "owner");
        Assert.Contains(contract.Items, item => item.Key == "signup");
        Assert.Contains(contract.Items, item => item.Key == "plan");
        Assert.Contains(contract.Items, item => item.Key == "billing");
        Assert.NotEmpty(contract.UtilityLinks);
        Assert.Contains(contract.UtilityLinks, link => link.Key == "blogs" && link.Href == "/blogs" && !link.IsExternal);
        Assert.Contains(contract.UtilityLinks, link => link.Key == "logout" && link.Href == "/account/logout" && link.IsExternal);
        Assert.Null(contract.NextAction);
    }

    [Fact]
    public async Task WorkspacePortalNavigationEndpoint_RequiresAuthentication()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();
        var workspace = await CreateMerchantAsync(state);

        var response = await client.GetAsync($"/api/v1/merchants/{workspace.Merchant.MerchantId}/portal/navigation");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WorkspacePortalNavigationEndpoint_ReturnsContractForAuthenticatedMerchant()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();
        var workspace = await CreateMerchantAsync(state);

        var completeSignup = await client.PostAsJsonAsync(
            "/api/v1/session/complete-signup",
            new
            {
                merchantId = workspace.Merchant.MerchantId,
                plan = "starter",
                password = "very-secure-password",
                confirmPassword = "very-secure-password"
            });
        Assert.Equal(HttpStatusCode.OK, completeSignup.StatusCode);

        var navResponse = await client.GetAsync(
            $"/api/v1/merchants/{workspace.Merchant.MerchantId}/portal/navigation?step=operate");

        Assert.Equal(HttpStatusCode.OK, navResponse.StatusCode);
        var contract = await navResponse.Content.ReadFromJsonAsync<PortalNavigationContract>();
        Assert.NotNull(contract);
        Assert.Equal("workspace", contract.Surface);
        Assert.Equal("operate", contract.ActiveKey);
        Assert.NotNull(contract.Roadmap);
        Assert.Equal(6, contract.Roadmap!.TotalCount);
        Assert.Contains(contract.Roadmap.Steps, step => step.Key == "owner");
        Assert.Contains(contract.Items, item => item.Key == "operate");
        Assert.Contains(contract.Items, item => item.Key == "configure");
        Assert.Contains(contract.Items, item => item.Key == "customers");
        Assert.NotEmpty(contract.UtilityLinks);
        Assert.Contains(contract.UtilityLinks, link => link.Key == "blogs" && link.Href == "/blogs" && !link.IsExternal);
        Assert.Contains(contract.UtilityLinks, link => link.Key == "logout" && link.Href == "/account/logout" && link.IsExternal);
        Assert.NotNull(contract.NextAction);
        Assert.Equal("programme", contract.NextAction!.Key);
        Assert.Equal("Create programme", contract.NextAction.CtaLabel);
        Assert.Contains(contract.NextAction.Tasks, task => task.Key == "shop" && task.IsComplete);
        Assert.Contains(contract.NextAction.Tasks, task => task.Key == "programme" && !task.IsComplete);
    }

    [Fact]
    public async Task CardLifecycleApis_SupportSingleAndBulkActions()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();
        var workspace = await CreateMerchantAsync(state);

        var completeSignup = await client.PostAsJsonAsync(
            "/api/v1/session/complete-signup",
            new
            {
                merchantId = workspace.Merchant.MerchantId,
                plan = "starter",
                password = "very-secure-password",
                confirmPassword = "very-secure-password"
            });
        Assert.Equal(HttpStatusCode.OK, completeSignup.StatusCode);

        var createProgramme = await client.PostAsJsonAsync(
            $"/api/v1/merchants/{workspace.Merchant.MerchantId}/programmes",
            new { templateKey = "coffee-visits" });
        Assert.Equal(HttpStatusCode.OK, createProgramme.StatusCode);
        var createdWorkspace = await createProgramme.Content.ReadFromJsonAsync<MerchantWorkspaceSnapshot>();
        Assert.NotNull(createdWorkspace?.SelectedProgramme);
        var programmeId = createdWorkspace!.SelectedProgramme!.ProgrammeId;
        var joinCode = createdWorkspace.SelectedProgramme.JoinCode;

        var joinOne = await client.PostAsync($"/api/v1/join/{joinCode}", content: null);
        var joinTwo = await client.PostAsync($"/api/v1/join/{joinCode}", content: null);
        Assert.Equal(HttpStatusCode.OK, joinOne.StatusCode);
        Assert.Equal(HttpStatusCode.OK, joinTwo.StatusCode);
        var cardOne = await joinOne.Content.ReadFromJsonAsync<WalletCardSnapshot>();
        var cardTwo = await joinTwo.Content.ReadFromJsonAsync<WalletCardSnapshot>();
        Assert.NotNull(cardOne);
        Assert.NotNull(cardTwo);

        var suspend = await client.PostAsJsonAsync(
            $"/api/v1/merchants/{workspace.Merchant.MerchantId}/programmes/{programmeId}/cards/{cardOne!.CardId}/lifecycle",
            new { action = "suspend" });
        Assert.Equal(HttpStatusCode.OK, suspend.StatusCode);
        var suspendPayload = await suspend.Content.ReadFromJsonAsync<MerchantWorkspaceSnapshot>();
        Assert.NotNull(suspendPayload);
        Assert.Equal(
            "Suspended",
            suspendPayload!.SelectedProgrammeCards.Single(card => card.CardId == cardOne.CardId).CustomerCardStatusLabel);

        var bulkArchive = await client.PostAsJsonAsync(
            $"/api/v1/merchants/{workspace.Merchant.MerchantId}/programmes/{programmeId}/cards/lifecycle",
            new { action = "archive", cardIds = new[] { cardOne.CardId, cardTwo!.CardId } });
        Assert.Equal(HttpStatusCode.OK, bulkArchive.StatusCode);
        var archivePayload = await bulkArchive.Content.ReadFromJsonAsync<MerchantWorkspaceSnapshot>();
        Assert.NotNull(archivePayload);
        Assert.Equal(
            2,
            archivePayload!.SelectedProgrammeCards.Count(card => card.CustomerCardStatusLabel == "Archived"));
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
