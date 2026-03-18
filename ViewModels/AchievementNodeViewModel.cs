using AchievementTranslator.Models;
using AchievementTranslator.Services;

namespace AchievementTranslator.ViewModels;

public class AchievementNodeViewModel : BaseViewModel
{
    private readonly TranslationService _service;
    private bool _isExpanded;
    private bool _isSelected;

    public NodeType NodeType { get; }

    public Achievement? Achievement { get; }

    /// <summary>
    /// Pour Category/SubCategory : le stringId du texte de catégorie.
    /// Pour Achievement : non utilisé ici (on passe par Achievement.Name etc.).
    /// Vaut 0 pour le nœud virtuel "(No category)".
    /// </summary>
    public ulong StringId { get; }

    public List<AchievementNodeViewModel> Children { get; } = new();

    public string DisplayName
    {
        get
        {
            return NodeType switch
            {
                NodeType.Category or NodeType.SubCategory =>
                    StringId == 0
                        ? "(No category)"
                        : _service.GetText(StringId, "en_us") ?? $"[ID: {StringId}]",

                NodeType.Achievement =>
                    _service.GetText(Achievement!.Name, "en_us") ?? $"[ID: {Achievement!.Id}]",

                _ => "?"
            };
        }
    }

    public bool IsDisabled => NodeType == NodeType.Achievement && !Achievement!.Enabled;

    public bool HasMissingTranslations
    {
        get
        {
            // Propres IDs manquants
            bool selfMissing = NodeType switch
            {
                NodeType.Category or NodeType.SubCategory =>
                    StringId != 0 && string.IsNullOrWhiteSpace(_service.GetText(StringId)),

                NodeType.Achievement =>
                    Achievement!.AllStringIds().Any(id =>!string.IsNullOrWhiteSpace(_service.GetText(id, "en_us")) // a un texte anglais source
                    && string.IsNullOrWhiteSpace(_service.GetText(id))),

                _ => false
            };

            return selfMissing || Children.Any(c => c.HasMissingTranslations);
        }
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => Set(ref _isExpanded, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => Set(ref _isSelected, value);
    }

    /// <summary>Constructeur pour nœuds Category / SubCategory.</summary>
    public AchievementNodeViewModel(NodeType nodeType, ulong stringId, TranslationService service)
    {
        NodeType = nodeType;
        StringId = stringId;
        _service = service;
    }

    /// <summary>Constructeur pour nœuds Achievement.</summary>
    public AchievementNodeViewModel(Achievement achievement, TranslationService service)
    {
        NodeType = NodeType.Achievement;
        Achievement = achievement;
        StringId = 0;
        _service = service;
    }

    public void RefreshMissingState()
    {
        OnPropertyChanged(nameof(HasMissingTranslations));
        OnPropertyChanged(nameof(DisplayName));
    }
}
