using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Fic.WalletPasses;

internal static class AppleWalletCertificateLoader
{
    public static bool TryLoadSigningCertificate(AppleWalletPassOptions options, out X509Certificate2? certificate, out string? issue)
    {
        try
        {
            certificate = LoadPkcs12Certificate(
                options.P12CertificatePath,
                options.P12CertificatePassword);

            if (!certificate.HasPrivateKey)
            {
                issue = "Signing certificate does not include a private key.";
                certificate.Dispose();
                certificate = null;
                return false;
            }

            issue = null;
            return true;
        }
        catch (CryptographicException)
        {
            certificate = null;
            issue = "Signing certificate could not be opened. Check the .p12 path and password.";
            return false;
        }
        catch (IOException)
        {
            certificate = null;
            issue = "Signing certificate could not be opened. Check the .p12 path and password.";
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            certificate = null;
            issue = "Signing certificate could not be opened. Check the .p12 path and password.";
            return false;
        }
    }

    public static bool TryLoadWwdrCertificate(AppleWalletPassOptions options, out X509Certificate2? certificate, out string? issue)
    {
        try
        {
            certificate = X509CertificateLoader.LoadCertificateFromFile(options.WwdrCertificatePath);
            issue = null;
            return true;
        }
        catch (CryptographicException)
        {
            certificate = null;
            issue = "Apple WWDR certificate could not be opened.";
            return false;
        }
        catch (IOException)
        {
            certificate = null;
            issue = "Apple WWDR certificate could not be opened.";
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            certificate = null;
            issue = "Apple WWDR certificate could not be opened.";
            return false;
        }
    }

    public static X509Certificate2 LoadSigningCertificate(AppleWalletPassOptions options)
    {
        if (!TryLoadSigningCertificate(options, out var certificate, out var issue))
        {
            throw new InvalidOperationException(issue);
        }

        return certificate!;
    }

    public static X509Certificate2 LoadWwdrCertificate(AppleWalletPassOptions options)
    {
        if (!TryLoadWwdrCertificate(options, out var certificate, out var issue))
        {
            throw new InvalidOperationException(issue);
        }

        return certificate!;
    }

    private static X509Certificate2 LoadPkcs12Certificate(string path, string password)
    {
        const X509KeyStorageFlags baseFlags = X509KeyStorageFlags.Exportable;

        try
        {
            return X509CertificateLoader.LoadPkcs12FromFile(
                path,
                password,
                baseFlags | X509KeyStorageFlags.EphemeralKeySet);
        }
        catch (PlatformNotSupportedException)
        {
            return X509CertificateLoader.LoadPkcs12FromFile(
                path,
                password,
                baseFlags);
        }
    }
}
