namespace Fic.Contracts;

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
    string OutputLabel);
