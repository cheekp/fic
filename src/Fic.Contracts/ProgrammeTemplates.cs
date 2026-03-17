namespace Fic.Contracts;

public sealed record ShopTypeOption(
    string ShopTypeKey,
    string ShopTypeLabel,
    string Description,
    bool IsActive);

public sealed record CardTypeOption(
    string CardTypeKey,
    string CardTypeLabel,
    string Description,
    bool IsActive);

public sealed record ProgrammeTemplateOption(
    string TemplateKey,
    string TemplateLabel,
    string ProgrammeTypeKey,
    string ProgrammeTypeLabel,
    string Headline,
    string Description,
    string RewardItemLabel,
    int RewardThreshold,
    string RewardCopy,
    string DeliveryTypeKey,
    string DeliveryTypeLabel,
    string OutputLabel,
    string ShopTypeKey,
    string CardTypeKey,
    string CardTypeLabel,
    bool IsActive);
