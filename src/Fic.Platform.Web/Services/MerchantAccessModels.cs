using Fic.Contracts;

namespace Fic.Platform.Web.Services;

public enum MerchantAuthenticationStatus
{
    Authenticated,
    NotFound,
    InvalidCredentials,
    CredentialsNotConfigured
}

public sealed record MerchantAuthenticationResult(
    MerchantAuthenticationStatus Status,
    MerchantAccountSnapshot? Merchant = null);

public enum MerchantCredentialConfigurationStatus
{
    Updated,
    NotFound,
    InvalidPassword
}

public sealed record MerchantCredentialConfigurationResult(
    MerchantCredentialConfigurationStatus Status,
    MerchantAccountSnapshot? Merchant = null,
    string? ErrorMessage = null);
