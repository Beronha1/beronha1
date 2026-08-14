// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityTable.EntitySelectors;
using Content.Shared.FixedPoint;
using Content.Shared.Whitelist;

namespace Content.Lavaland.Server.Mobs;

/// <summary>
/// Drops some loot when boss having this component dies.
/// </summary>
[RegisterComponent]
public sealed partial class SpawnLootOnDeathComponent : Component
{
    [DataField]
    public EntityTableSelector? Table;

    [DataField]
    public EntityTableSelector? SpecialTable;

    /// <summary>
    /// Whitelist for weapons whose damage contributes towards the special loot threshold.
    /// </summary>
    [DataField("weaponWhitelist")]
    public EntityWhitelist? SpecialWeaponWhitelist;

    /// <summary>
    /// Fraction of the boss's death threshold that must be dealt with qualifying weapons.
    /// Contributions from multiple miners are combined.
    /// </summary>
    [DataField]
    public float SpecialDamageFraction = 0.6f;

    [DataField]
    public bool DeleteOnDeath;

    /// <summary>
    /// Whether the loot tables are resolved as soon as the entity dies. Encounters
    /// with a post-death phase can defer this and call SpawnOnDeathSystem.TryDropLoot.
    /// </summary>
    [DataField]
    public bool DropOnDeath = true;

    /// <summary>
    /// If true and the mob was killed with special weapon,
    /// and both loots are not null, drops both loots at once.
    /// </summary>
    [DataField]
    public bool DropBoth;

    /// <summary>
    /// Compatibility/debug value resolved from <see cref="QualifiedDamage"/> when loot is dropped.
    /// </summary>
    [ViewVariables]
    public bool DoSpecialLoot = true;

    /// <summary>
    /// Actual post-resistance damage dealt by qualifying Crushers and portable PKAs.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 QualifiedDamage;

    /// <summary>
    /// Origins whose next synchronous damage event was initiated by a qualifying weapon.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> PendingQualifyingOrigins = new();

    /// <summary>
    /// Prevents delayed or duplicate death events from resolving the tables twice.
    /// </summary>
    [ViewVariables]
    public bool HasDropped;
}
