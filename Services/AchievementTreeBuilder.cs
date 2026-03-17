using AchievementTranslator.Models;
using AchievementTranslator.ViewModels;

namespace AchievementTranslator.Services;

public class AchievementTreeBuilder
{
    private readonly TranslationService _translationService;

    public AchievementTreeBuilder(TranslationService translationService)
    {
        _translationService = translationService;
    }

    public List<AchievementNodeViewModel> BuildTree(List<Achievement> achievements)
    {
        var nodeMap = achievements
            .ToDictionary(a => a.Id, a => new AchievementNodeViewModel(a, _translationService));

        var roots = new List<AchievementNodeViewModel>();

        foreach (var node in nodeMap.Values)
        {
            var parentId = node.Achievement.ParentId;
            if (parentId != 0 && nodeMap.TryGetValue(parentId, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node);
        }

        SortTree(roots);
        return roots;
    }

    private static void SortTree(List<AchievementNodeViewModel> nodes)
    {
        nodes.Sort((a, b) => a.Achievement.DisplayOrder.CompareTo(b.Achievement.DisplayOrder));
        foreach (var node in nodes)
            SortTree(node.Children);
    }

    /// <summary>
    /// Flattens the tree in depth-first order for navigation.
    /// </summary>
    public static List<AchievementNodeViewModel> Flatten(IEnumerable<AchievementNodeViewModel> nodes)
    {
        var result = new List<AchievementNodeViewModel>();
        foreach (var node in nodes)
        {
            result.Add(node);
            result.AddRange(Flatten(node.Children));
        }
        return result;
    }
}
