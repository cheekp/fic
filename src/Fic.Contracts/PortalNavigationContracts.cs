namespace Fic.Contracts;

public sealed record PortalThemeContract(
    string Primary,
    string Accent,
    string Surface,
    string Ink,
    string Radius,
    string Shadow);

public sealed record PortalNavItemContract(
    string Key,
    string Label,
    string Href,
    string? Badge,
    bool IsComplete,
    bool IsDisabled);

public sealed record PortalRoadmapStepContract(
    string Key,
    string Label,
    string? CompactLabel,
    string Href,
    bool IsComplete,
    bool IsCurrent,
    bool IsNavigable);

public sealed record PortalRoadmapContract(
    string CurrentKey,
    int CompleteCount,
    int TotalCount,
    string? Hint,
    IReadOnlyList<PortalRoadmapStepContract> Steps);

public sealed record PortalNextActionTaskContract(
    string Key,
    string Label,
    bool IsComplete,
    bool IsBlocked,
    string? BlockedReason);

public sealed record PortalNextActionContract(
    string Key,
    string Title,
    string Summary,
    string CtaLabel,
    string? CtaHref,
    string? BlockedReason,
    IReadOnlyList<PortalNextActionTaskContract> Tasks);

public sealed record PortalNavigationContract(
    string Surface,
    string ActiveKey,
    PortalThemeContract Theme,
    IReadOnlyList<PortalNavItemContract> Items,
    PortalRoadmapContract? Roadmap,
    PortalNextActionContract? NextAction);
