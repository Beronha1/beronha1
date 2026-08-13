// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Flammability;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Lavaland.Shared.Megafauna.Utility;

/// <summary>
/// Reference-counted heat protection shared by armour and temporary Crusher trophy effects.
/// </summary>
public sealed partial class MegafaunaHeatProtectionSystem : EntitySystem
{
    [Dependency] private TagSystem _tags = default!;

    private static readonly ProtoId<TagPrototype> LavaWalking = "LavaWalking";

    public int AddOrRefreshSource(EntityUid target, EntityUid source)
    {
        var protection = EnsureComp<MegafaunaHeatProtectionSourcesComponent>(target);
        if (protection.Sources.Count == 0)
        {
            protection.PreserveFireImmunity = HasComp<FireImmunityComponent>(target);
            protection.PreserveLavaWalking = _tags.HasTag(target, LavaWalking);
        }

        var generation = ++protection.NextGeneration;
        protection.Sources[source] = generation;
        EnsureComp<FireImmunityComponent>(target);
        _tags.TryAddTag(target, LavaWalking);
        return generation;
    }

    public void RemoveSource(EntityUid target, EntityUid source, int generation)
    {
        if (!TryComp<MegafaunaHeatProtectionSourcesComponent>(target, out var protection) ||
            !protection.Sources.TryGetValue(source, out var currentGeneration) ||
            currentGeneration != generation)
        {
            return;
        }

        protection.Sources.Remove(source);
        if (protection.Sources.Count > 0)
            return;

        if (!protection.PreserveFireImmunity)
            RemCompDeferred<FireImmunityComponent>(target);
        if (!protection.PreserveLavaWalking)
            _tags.RemoveTag(target, LavaWalking);

        RemCompDeferred<MegafaunaHeatProtectionSourcesComponent>(target);
    }
}
