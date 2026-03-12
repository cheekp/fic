namespace Fic.WalletPasses;

public sealed record WalletPassPackage(
    string FileName,
    string ContentType,
    byte[] Bytes);
