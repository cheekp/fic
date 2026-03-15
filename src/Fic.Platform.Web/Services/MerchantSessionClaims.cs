using System.Security.Claims;

namespace Fic.Platform.Web.Services;

public static class MerchantSessionClaims
{
    public const string MerchantId = "fic:merchant_id";
    public const string MerchantEmail = ClaimTypes.Email;
    public const string MerchantName = ClaimTypes.Name;

    public static Guid? TryGetMerchantId(ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(MerchantId);
        return Guid.TryParse(value, out var merchantId) ? merchantId : null;
    }
}
