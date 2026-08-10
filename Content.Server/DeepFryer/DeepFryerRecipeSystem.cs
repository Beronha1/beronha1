using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.DeepFryer;

namespace Content.Server.DeepFryer;

public sealed partial class DeepFryerRecipeSystem : EntitySystem
{
    public bool TryGetRecipe(
        List<EntityUid> entities,
        [NotNullWhen(true)] out DeepFryerRecipePrototype? recipe)
    {
        recipe = null;

        var found = new Dictionary<string, int>();
        foreach (var uid in entities)
        {
            var proto = MetaData(uid).EntityPrototype?.ID;
            if (proto == null)
                continue;

            found.TryAdd(proto, 0);
            found[proto]++;
        }

        var candidates = ProtoMan.EnumeratePrototypes<DeepFryerRecipePrototype>()
            .OrderByDescending(r => r.Ingredients.Count);

        foreach (var r in candidates)
        {
            var ok = true;
            foreach (var ingredient in r.Ingredients)
            {
                if (!found.TryGetValue(ingredient.Key, out var amount) || amount < ingredient.Value)
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
            {
                recipe = r;
                return true;
            }
        }

        return false;
    }
}