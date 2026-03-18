using AchievementTranslator.Models;
using AchievementTranslator.ViewModels;

namespace AchievementTranslator.Services;

public class AchievementTreeBuilder
{
    private readonly TranslationService _service;

    public AchievementTreeBuilder(TranslationService service)
    {
        _service = service;
    }

    public List<AchievementNodeViewModel> BuildTree(List<Achievement> achievements)
    {
        // Un achievement est "racine" si son ParentId vaut 0 ou si son parent
        // n'existe pas dans la liste.
        var allIds = achievements.Select(a => a.Id).ToHashSet();

        var roots = achievements.Where(a => a.ParentId == 0 || !allIds.Contains(a.ParentId)).ToList();
        var children = achievements.Where(a => a.ParentId != 0 && allIds.Contains(a.ParentId)).ToList();

        // Index parent > enfants
        var childrenByParent = children
            .GroupBy(a => a.ParentId)
            .ToDictionary(g => g.Key, g => g.OrderBy(a => a.DisplayOrder).ToList());

        // Étape 2 : grouper les racines par Catégorie > Sous-catégorie
        // Clé : (CategoryStr, SubCategoryStr)
        var groups = roots
            .GroupBy(a => (Cat: a.CategoryStr, Sub: a.SubCategoryStr))
            .ToList();

        // Un nœud catégorie par CategoryStr unique.
        var categoryNodes = new Dictionary<ulong, AchievementNodeViewModel>();

        foreach (var group in groups)
        {
            var catId = group.Key.Cat;
            var subId = group.Key.Sub;

            // Nœud catégorie (créé une seule fois par catId)
            if (!categoryNodes.TryGetValue(catId, out var catNode))
            {
                catNode = new AchievementNodeViewModel(NodeType.Category, catId, _service);
                categoryNodes[catId] = catNode;
            }

            // Nœud sous-catégorie (un seul par (catId, subId))
            var subNode = catNode.Children
                .FirstOrDefault(c => c.NodeType == NodeType.SubCategory && c.StringId == subId);

            if (subNode == null)
            {
                subNode = new AchievementNodeViewModel(NodeType.SubCategory, subId, _service);
                catNode.Children.Add(subNode);
            }

            // Étape 4 : achievements racines dans cette sous-catégorie
            foreach (var achievement in group.OrderBy(a => a.DisplayOrder))
            {
                var achNode = new AchievementNodeViewModel(achievement, _service);

                // Enfants directs de cet achievement
                if (childrenByParent.TryGetValue(achievement.Id, out var directChildren))
                {
                    foreach (var child in directChildren)
                        achNode.Children.Add(new AchievementNodeViewModel(child, _service));
                }

                subNode.Children.Add(achNode);
            }
        }

        var result = categoryNodes.Values.ToList();

        // Catégories : alphabétique sur le texte anglais
        result.Sort((a, b) =>
            string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));

        // Sous-catégories : alphabétique
        foreach (var cat in result)
        {
            cat.Children.Sort((a, b) =>
                string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));

            // Achievements dans chaque sous-catégorie : déjà triés par DisplayOrder
            // Les enfants d'achievements : déjà triés par DisplayOrder
        }

        return result;
    }

    /// <summary>Aplatit l'arbre en ordre depth-first pour la navigation.</summary>
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
