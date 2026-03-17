using AchievementTranslator.Models;
using AchievementTranslator.Services;

namespace AchievementTranslator.ViewModels;

public class AchievementNodeViewModel : BaseViewModel
{
    private readonly TranslationService _service;
    private bool _isExpanded;
    private bool _isSelected;

    public Achievement Achievement { get; }
    public List<AchievementNodeViewModel> Children { get; } = new();

    public string DisplayName
    {
        get
        {
            var name = _service.GetText(Achievement.Name, "en_us");
            return !string.IsNullOrEmpty(name) ? name : $"[ID: {Achievement.Id}]";
        }
    }

    /// <summary>
    /// True if the current language is missing any translation for this node's string IDs.
    /// Also checks children recursively so parent nodes light up when a child is missing.
    /// </summary>
    public bool HasMissingTranslations =>
        Achievement.AllStringIds().Any(id => string.IsNullOrWhiteSpace(_service.GetText(id)))
        || Children.Any(c => c.HasMissingTranslations);

    public bool IsDisabled => !Achievement.Enabled;

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

    public AchievementNodeViewModel(Achievement achievement, TranslationService service)
    {
        Achievement = achievement;
        _service = service;
    }

    public void RefreshMissingState()
    {
        OnPropertyChanged(nameof(HasMissingTranslations));
        OnPropertyChanged(nameof(DisplayName));
    }
}
