using Fic.Contracts;

namespace Fic.WalletPasses;

public sealed record WalletCardView(
    Guid CardId,
    string VendorDisplayName,
    string RewardItemLabel,
    string RewardCopy,
    string PrimaryColor,
    string AccentColor,
    string ProgressDisplayText,
    RewardState RewardState,
    string WalletPassId);
