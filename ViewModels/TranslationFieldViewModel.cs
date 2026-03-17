using AchievementTranslator.Services;

namespace AchievementTranslator.ViewModels;

/// <summary>
/// ViewModel for one translatable field (Name, InProgressStr, etc.).
/// Handles shared-string awareness: shows how many other achievements use the same ID.
/// </summary>
public class TranslationFieldViewModel : BaseViewModel
{
    private readonly TranslationService _service;
    private readonly ulong _stringId;
    private string _translatedText = string.Empty;

    public string FieldLabel { get; }
    public string EnglishText { get; }
    public ulong StringId => _stringId;
    public bool HasStringId => _stringId != 0;

    /// <summary>Number of achievements sharing this string ID (shown as info badge).</summary>
    public int SharedCount { get; }
    public bool IsShared => SharedCount > 1;
    public string SharedLabel => IsShared ? $"⚠ ID shared by {SharedCount} achievements" : string.Empty;

    public string TranslatedText
    {
        get => _translatedText;
        set
        {
            if (Set(ref _translatedText, value))
            {
                _service.SetText(_stringId, value);
                OnPropertyChanged(nameof(IsMissing));
            }
        }
    }

    public bool IsMissing => HasStringId && string.IsNullOrWhiteSpace(_translatedText);

    public TranslationFieldViewModel(string label, ulong stringId, TranslationService service)
    {
        FieldLabel = label;
        _stringId = stringId;
        _service = service;

        EnglishText = service.GetText(stringId, "en_us") ?? string.Empty;
        _translatedText = service.GetText(stringId) ?? string.Empty;
        SharedCount = service.GetAchievementsUsingStringId(stringId).Count;
    }

    public void Refresh()
    {
        var fresh = _service.GetText(_stringId) ?? string.Empty;
        // Only fire if actually changed (avoids re-triggering saves on language switch)
        if (_translatedText != fresh)
        {
            _translatedText = fresh;
            OnPropertyChanged(nameof(TranslatedText));
            OnPropertyChanged(nameof(IsMissing));
        }
    }
}
