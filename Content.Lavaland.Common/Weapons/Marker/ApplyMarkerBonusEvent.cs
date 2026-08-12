// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;

namespace Content.Lavaland.Common.Weapons.Marker;

/// <summary>
/// Raised on the weapon after attacking a damage-marker mob.
/// </summary>
[ByRefEvent]
public record struct ApplyMarkerBonusEvent(EntityUid Target, EntityUid User);

/// <summary>
/// Raised on a crusher before its marker bonus damage and healing are calculated.
/// Installed trophies can modify the supplied multipliers.
/// </summary>
[ByRefEvent]
public record struct MarkerAttackAttemptEvent(
    EntityUid Weapon,
    EntityUid User,
    EntityUid Target,
    float DamageModifier = 1f,
    float HealModifier = 1f);

/// <summary>
/// Raised on a crusher after one of its damage markers has been consumed.
/// </summary>
[ByRefEvent]
public record struct AfterMarkerAttackedEvent(
    EntityUid Weapon,
    EntityUid User,
    EntityUid Target,
    DamageSpecifier Damage);
