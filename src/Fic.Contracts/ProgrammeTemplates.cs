namespace Fic.Contracts;

public sealed record ProgrammeTemplateOption(
    string TemplateKey,
    string TemplateLabel,
    string Headline,
    string Description,
    string RewardItemLabel,
    int RewardThreshold,
    string RewardCopy,
    string DeliveryLabel);
