using System.Text.Json;
using Fic.Contracts;

namespace Fic.Platform.Web.Services;

public interface IProgrammeCatalogueRepository
{
    ProgrammeCatalogueSnapshot Load();
}

public sealed record ProgrammeCatalogueSnapshot(
    IReadOnlyList<ShopTypeOption> ShopTypes,
    IReadOnlyList<CardTypeOption> CardTypes,
    IReadOnlyList<ProgrammeTemplateOption> ProgrammeTemplates);

public sealed class JsonProgrammeCatalogueRepository(
    ILogger<JsonProgrammeCatalogueRepository> logger) : IProgrammeCatalogueRepository
{
    private const string CatalogueRelativePath = "App_Data/catalogues/programme-catalogue.json";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public ProgrammeCatalogueSnapshot Load()
    {
        var cataloguePath = ResolveCataloguePath();

        using var stream = File.OpenRead(cataloguePath);
        var document = JsonSerializer.Deserialize<ProgrammeCatalogueDocument>(stream, SerializerOptions)
            ?? throw new InvalidOperationException($"Programme catalogue document is empty: {cataloguePath}");

        var shopTypes = document.ShopTypes
            .Select(type => new ShopTypeOption(
                NormalizeKey(type.ShopTypeKey),
                type.ShopTypeLabel.Trim(),
                type.Description.Trim(),
                type.IsActive))
            .ToArray();

        var cardTypes = document.CardTypes
            .Select(type => new CardTypeOption(
                NormalizeKey(type.CardTypeKey),
                type.CardTypeLabel.Trim(),
                type.Description.Trim(),
                type.IsActive))
            .ToArray();

        var templates = document.ProgrammeTemplates
            .Select(template =>
            {
                var normalizedCardTypeKey = NormalizeKey(template.CardTypeKey);
                var cardTypeLabel = cardTypes
                    .FirstOrDefault(type => string.Equals(type.CardTypeKey, normalizedCardTypeKey, StringComparison.OrdinalIgnoreCase))
                    ?.CardTypeLabel
                    ?? normalizedCardTypeKey;

                return new ProgrammeTemplateOption(
                    NormalizeKey(template.TemplateKey),
                    template.TemplateLabel.Trim(),
                    NormalizeKey(template.ProgrammeTypeKey),
                    template.ProgrammeTypeLabel.Trim(),
                    template.Headline.Trim(),
                    template.Description.Trim(),
                    template.RewardItemLabel.Trim(),
                    template.RewardThreshold,
                    template.RewardCopy.Trim(),
                    NormalizeKey(template.DeliveryTypeKey),
                    template.DeliveryTypeLabel.Trim(),
                    template.OutputLabel.Trim(),
                    NormalizeKey(template.ShopTypeKey),
                    normalizedCardTypeKey,
                    cardTypeLabel,
                    template.IsActive);
            })
            .ToArray();

        ValidateCatalogue(shopTypes, cardTypes, templates, cataloguePath);

        logger.LogInformation(
            "programme_catalogue_loaded path={Path} shop_types={ShopTypes} card_types={CardTypes} templates={Templates}",
            cataloguePath,
            shopTypes.Length,
            cardTypes.Length,
            templates.Length);

        return new ProgrammeCatalogueSnapshot(shopTypes, cardTypes, templates);
    }

    private static string ResolveCataloguePath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var directCandidate = Path.Combine(current.FullName, CatalogueRelativePath);
            if (File.Exists(directCandidate))
            {
                return directCandidate;
            }

            var sourceCandidate = Path.Combine(current.FullName, "src", "Fic.Platform.Web", CatalogueRelativePath);
            if (File.Exists(sourceCandidate))
            {
                return sourceCandidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException($"Could not locate programme catalogue at {CatalogueRelativePath}.");
    }

    private static void ValidateCatalogue(
        IReadOnlyList<ShopTypeOption> shopTypes,
        IReadOnlyList<CardTypeOption> cardTypes,
        IReadOnlyList<ProgrammeTemplateOption> templates,
        string cataloguePath)
    {
        if (shopTypes.Count == 0)
        {
            throw new InvalidOperationException($"Programme catalogue has no shop types: {cataloguePath}");
        }

        if (cardTypes.Count == 0)
        {
            throw new InvalidOperationException($"Programme catalogue has no card types: {cataloguePath}");
        }

        if (templates.Count == 0)
        {
            throw new InvalidOperationException($"Programme catalogue has no programme templates: {cataloguePath}");
        }

        EnsureNoDuplicates(
            shopTypes.Select(type => type.ShopTypeKey),
            "shop type key",
            cataloguePath);

        EnsureNoDuplicates(
            cardTypes.Select(type => type.CardTypeKey),
            "card type key",
            cataloguePath);

        EnsureNoDuplicates(
            templates.Select(template => template.TemplateKey),
            "template key",
            cataloguePath);

        var shopTypeKeys = shopTypes.Select(type => type.ShopTypeKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var cardTypeKeys = cardTypes.Select(type => type.CardTypeKey).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var template in templates)
        {
            if (!shopTypeKeys.Contains(template.ShopTypeKey))
            {
                throw new InvalidOperationException(
                    $"Template '{template.TemplateKey}' references unknown shop type '{template.ShopTypeKey}' in {cataloguePath}.");
            }

            if (!cardTypeKeys.Contains(template.CardTypeKey))
            {
                throw new InvalidOperationException(
                    $"Template '{template.TemplateKey}' references unknown card type '{template.CardTypeKey}' in {cataloguePath}.");
            }
        }
    }

    private static void EnsureNoDuplicates(
        IEnumerable<string> keys,
        string keyType,
        string cataloguePath)
    {
        var duplicates = keys
            .GroupBy(key => key, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (duplicates.Length > 0)
        {
            throw new InvalidOperationException(
                $"Programme catalogue contains duplicate {keyType} values ({string.Join(", ", duplicates)}) in {cataloguePath}.");
        }
    }

    private static string NormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Programme catalogue keys cannot be empty.");
        }

        return key.Trim().ToLowerInvariant();
    }

    private sealed class ProgrammeCatalogueDocument
    {
        public List<ShopTypeDocument> ShopTypes { get; set; } = [];

        public List<CardTypeDocument> CardTypes { get; set; } = [];

        public List<ProgrammeTemplateDocument> ProgrammeTemplates { get; set; } = [];
    }

    private sealed class ShopTypeDocument
    {
        public string ShopTypeKey { get; set; } = string.Empty;

        public string ShopTypeLabel { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    private sealed class CardTypeDocument
    {
        public string CardTypeKey { get; set; } = string.Empty;

        public string CardTypeLabel { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    private sealed class ProgrammeTemplateDocument
    {
        public string TemplateKey { get; set; } = string.Empty;

        public string TemplateLabel { get; set; } = string.Empty;

        public string ProgrammeTypeKey { get; set; } = string.Empty;

        public string ProgrammeTypeLabel { get; set; } = string.Empty;

        public string Headline { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string RewardItemLabel { get; set; } = string.Empty;

        public int RewardThreshold { get; set; }

        public string RewardCopy { get; set; } = string.Empty;

        public string DeliveryTypeKey { get; set; } = string.Empty;

        public string DeliveryTypeLabel { get; set; } = string.Empty;

        public string OutputLabel { get; set; } = string.Empty;

        public string ShopTypeKey { get; set; } = string.Empty;

        public string CardTypeKey { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
