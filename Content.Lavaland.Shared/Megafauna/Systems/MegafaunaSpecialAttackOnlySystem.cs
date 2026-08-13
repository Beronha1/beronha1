// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Shared.Megafauna.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Lavaland.Shared.Megafauna.Systems;

/// <summary>
/// Suppresses only ordinary melee execution. The MeleeWeapon component deliberately remains present because
/// SimpleHostileCompound uses it to select and pursue the target consumed by both megafauna combat directors.
/// </summary>
public sealed class MegafaunaSpecialAttackOnlySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MegafaunaSpecialAttackOnlyComponent, AttemptMeleeEvent>(OnAttemptMelee);
    }

    private void OnAttemptMelee(
        Entity<MegafaunaSpecialAttackOnlyComponent> ent,
        ref AttemptMeleeEvent args)
    {
        args.Cancelled = !ent.Comp.AllowActionMelee;
    }
}
