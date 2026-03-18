using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using AchievementTranslator.Models;
using AchievementTranslator.Services;

namespace AchievementTranslator.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly TranslationService _service;
    private readonly AchievementTreeBuilder _treeBuilder;

    // Complete unfiltered node list (depth-first); rebuilt on load only.
    private List<AchievementNodeViewModel> _allNodes = new();
    // Currently displayed flat list (may be filtered).
    private List<AchievementNodeViewModel> _flatNodes = new();
    // Full root list (unfiltered), kept for restoring the tree view.
    private List<AchievementNodeViewModel> _allRoots = new();

    public ObservableCollection<AchievementNodeViewModel> RootNodes { get; } = new();
    public ObservableCollection<TranslationFieldViewModel> Fields { get; } = new();
    public ObservableCollection<string> Languages { get; } = new();

    private AchievementNodeViewModel? _selectedNode;
    public AchievementNodeViewModel? SelectedNode
    {
        get => _selectedNode;
        set
        {
            // Always reload fields even if same node re-clicked
            _selectedNode = value;
            OnPropertyChanged();
            LoadFields();
        }
    }

    private string _selectedLanguage = "en_us";
    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (Set(ref _selectedLanguage, value))
            {
                _service.SetLanguage(value);
                RefreshAll();
            }
        }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { if (Set(ref _searchText, value)) ApplyFilter(); }
    }

    private bool _showOnlyUntranslated;
    public bool ShowOnlyUntranslated
    {
        get => _showOnlyUntranslated;
        set { if (Set(ref _showOnlyUntranslated, value)) ApplyFilter(); }
    }

    private string _progressText = "—";
    public string ProgressText
    {
        get => _progressText;
        private set => Set(ref _progressText, value);
    }

    private double _progressPercent;
    public double ProgressPercent
    {
        get => _progressPercent;
        private set => Set(ref _progressPercent, value);
    }

    private bool _autoSave;
    public bool AutoSave
    {
        get => _autoSave;
        set => Set(ref _autoSave, value);
    }

    private string _statusMessage = "Prêt — cliquez sur 📂 ou placez les JSON à côté de l'exe.";
    public string StatusMessage
    {
        get => _statusMessage;
        set => Set(ref _statusMessage, value);
    }

    public RelayCommand LoadFilesCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand NextUntranslatedCommand { get; }
    public RelayCommand AddLanguageCommand { get; }
    public RelayCommand ShowStatsCommand { get; }

    public MainViewModel()
    {
        _service = new TranslationService();
        _treeBuilder = new AchievementTreeBuilder(_service);

        LoadFilesCommand = new RelayCommand(LoadFiles);
        SaveCommand = new RelayCommand(Save, () => _service.IsLoaded);
        NextUntranslatedCommand = new RelayCommand(GoToNextUntranslated, () => _service.IsLoaded);
        AddLanguageCommand = new RelayCommand(AddLanguage, () => _service.IsLoaded);
        ShowStatsCommand = new RelayCommand(ShowStats, () => _service.IsLoaded);

        TryAutoLoad();
    }

    private void TryAutoLoad()
    {
        var dir = AppDomain.CurrentDomain.BaseDirectory;
        var infoPath = Path.Combine(dir, "AchievementInfoMap.json");
        var strPath = Path.Combine(dir, "AchievementStringMap.json");
        if (File.Exists(infoPath) && File.Exists(strPath))
            PerformLoad(infoPath, strPath);
    }

    private void LoadFiles()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Sélectionner AchievementInfoMap.json",
            Filter = "JSON files (*.json)|*.json|All files|*.*"
        };
        if (dlg.ShowDialog() != true) return;
        var infoPath = dlg.FileName;

        dlg.Title = "Sélectionner AchievementStringMap.json";
        if (dlg.ShowDialog() != true) return;

        PerformLoad(infoPath, dlg.FileName);
    }

    private void PerformLoad(string infoPath, string strPath)
    {
        try
        {
            _service.Load(infoPath, strPath);
            InitializeTree();
            InitializeLanguages();
            RefreshProgress();
            StatusMessage = $"✅ Chargé — {_service.GetAchievements().Count} achievements.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur de chargement :\n{ex.Message}", "Erreur",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void InitializeTree()
    {
        RootNodes.Clear();
        _allRoots = _treeBuilder.BuildTree(_service.GetAchievements());
        foreach (var root in _allRoots) RootNodes.Add(root);
        _allNodes = AchievementTreeBuilder.Flatten(_allRoots);
        _flatNodes = _allNodes.ToList();
    }

    private void InitializeLanguages()
    {
        Languages.Clear();
        foreach (var lang in _service.GetLanguages()) Languages.Add(lang);
        var target = Languages.FirstOrDefault(l => l != "en_us") ?? "en_us";
        _selectedLanguage = target;
        _service.SetLanguage(target);
        OnPropertyChanged(nameof(SelectedLanguage));
    }

    private void AddLanguage()
    {
        var dlg = new AchievementTranslator.Views.InputDialog
        {
            Owner = Application.Current.MainWindow
        };
        if (dlg.ShowDialog() != true) return;
        var input = dlg.Result;

        if (string.IsNullOrWhiteSpace(input)) return;

        if (Languages.Contains(input))
        {
            MessageBox.Show($"La langue « {input} » existe déjà.", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _service.AddLanguage(input);
        Languages.Add(input);
        SelectedLanguage = input;
        StatusMessage = $"✅ Langue « {input} » ajoutée — {_service.GetAchievements().Count} entrées initialisées.";
    }

    public void ForceReloadFields() => LoadFields();

    private void LoadFields()
    {
        Fields.Clear();
        if (_selectedNode == null) return;

        switch (_selectedNode.NodeType)
        {
            case NodeType.Category:
                TryAddField("Catégorie", _selectedNode.StringId);
                break;

            case NodeType.SubCategory:
                TryAddField("Sous-catégorie", _selectedNode.StringId);
                break;

            case NodeType.Achievement:
                var a = _selectedNode.Achievement!;
                TryAddField("Nom (Name)", a.Name);
                TryAddField("En cours (InProgressStr)", a.InProgressStr);
                TryAddField("Complété (CompletedStr)", a.CompletedStr);
                TryAddField("Récompense (RewardStr)", a.RewardStr);
                TryAddField("Catégorie (CategoryStr)", a.CategoryStr);
                TryAddField("Sous-catégorie (SubCategoryStr)", a.SubCategoryStr);
                break;
        }
    }

    private void TryAddField(string label, ulong stringId)
    {
        if (stringId == 0) return;
        var field = new TranslationFieldViewModel(label, stringId, _service);
        field.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TranslationFieldViewModel.TranslatedText))
            {
                RefreshProgress();
                _selectedNode?.RefreshMissingState();
                // Refresh parent nodes too
                foreach (var root in _allRoots)
                    PropagateRefresh(root, _selectedNode!);
                if (AutoSave) Save();
            }
        };
        Fields.Add(field);
    }

    private static bool PropagateRefresh(AchievementNodeViewModel current, AchievementNodeViewModel target)
    {
        if (current == target) return true;
        foreach (var child in current.Children)
        {
            if (PropagateRefresh(child, target))
            {
                current.RefreshMissingState();
                return true;
            }
        }
        return false;
    }

    private void RefreshAll()
    {
        foreach (var field in Fields) field.Refresh();
        foreach (var node in _allNodes) node.RefreshMissingState();
        RefreshProgress();
    }

    private void RefreshProgress()
    {
        var (done, total) = _service.GetProgress();
        ProgressText = $"{done} / {total} traduits";
        ProgressPercent = total == 0 ? 0 : (double)done / total * 100.0;
    }

    private void GoToNextUntranslated()
    {
        if (_allNodes.Count == 0) return;

        // Start from next node after current selection
        int start = _selectedNode == null
            ? 0
            : (_allNodes.IndexOf(_selectedNode) + 1) % _allNodes.Count;

        for (int i = 0; i < _allNodes.Count; i++)
        {
            var candidate = _allNodes[(start + i) % _allNodes.Count];
            if (candidate.HasMissingTranslations && !candidate.Children.Any())
            {
                FocusNode(candidate);
                return;
            }
        }
        StatusMessage = "🎉 Tous les achievements sont entièrement traduits !";
    }

    public void FocusNode(AchievementNodeViewModel target)
    {
        if (_selectedNode != null) _selectedNode.IsSelected = false;
        foreach (var root in _allRoots) ExpandPathTo(root, target);
        target.IsExpanded = true;
        target.IsSelected = true;
        SelectedNode = target;
    }

    private static bool ExpandPathTo(AchievementNodeViewModel current, AchievementNodeViewModel target)
    {
        if (current == target) return true;
        foreach (var child in current.Children)
        {
            if (ExpandPathTo(child, target))
            {
                current.IsExpanded = true;
                return true;
            }
        }
        return false;
    }

    private void Save()
    {
        try
        {
            _service.Save();
            StatusMessage = $"💾 Sauvegardé à {DateTime.Now:HH:mm:ss}  →  {_service.StringMapPath}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur de sauvegarde :\n{ex.Message}", "Erreur",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ShowStats()
    {
        var stats = _service.GetProgressByCategory();
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Langue : {SelectedLanguage}");
        sb.AppendLine(new string('─', 50));
        foreach (var (cat, done, total) in stats)
        {
            double pct = total == 0 ? 100 : done * 100.0 / total;
            var bar = new string('█', (int)(pct / 5)) + new string('░', 20 - (int)(pct / 5));
            sb.AppendLine($"{cat,-30}  {bar}  {done,4}/{total} ({pct:0}%)");
        }
        MessageBox.Show(sb.ToString(), "Statistiques par catégorie",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ApplyFilter()
    {
        bool hasSearch = !string.IsNullOrWhiteSpace(SearchText);
        bool filterMiss = ShowOnlyUntranslated;

        if (!hasSearch && !filterMiss)
        {
            // Restore full hierarchical tree
            RootNodes.Clear();
            foreach (var root in _allRoots) RootNodes.Add(root);
            _flatNodes = _allNodes.ToList();
            return;
        }

        // Flat filtered list
        var filtered = _allNodes.Where(n =>
        {
            bool matchSearch = !hasSearch || n.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
            bool matchMiss = !filterMiss || n.HasMissingTranslations;
            return matchSearch && matchMiss;
        }).ToList();

        RootNodes.Clear();
        foreach (var node in filtered) RootNodes.Add(node);
        _flatNodes = filtered;
    }
}
