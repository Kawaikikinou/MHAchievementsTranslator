using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AchievementTranslator.ViewModels;

namespace AchievementTranslator.Views;

public partial class MainWindow : Window
{
    private MainViewModel _vm = null!;

    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainViewModel();
        DataContext = _vm;

        // Primary: fires when selection actually changes
        AchievementTree.SelectedItemChanged += (_, e) =>
        {
            if (e.NewValue is AchievementNodeViewModel node)
                _vm.SelectedNode = node;
        };

        // Secondary: re-fires even if same node is clicked again (re-click guard)
        AchievementTree.MouseLeftButtonUp += (_, e) =>
        {
            if (AchievementTree.SelectedItem is AchievementNodeViewModel node
                && node == _vm.SelectedNode)
            {
                // Same node clicked again — force reload of fields
                _vm.ForceReloadFields();
            }
        };
    }
}
