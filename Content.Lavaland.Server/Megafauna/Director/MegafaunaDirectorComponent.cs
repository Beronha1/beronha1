// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.FixedPoint;
using Robust.Shared.Map;

namespace Content.Lavaland.Server.Megafauna.Director;

/// <summary>
/// Opt-in dynamic difficulty for a Lavaland boss. The director scales an encounter
/// from its immutable prototype values using party size and bosses already defeated
/// on the same map.
/// </summary>
[RegisterComponent, Access(typeof(MegafaunaDirectorSystem))]
public sealed partial class MegafaunaDirectorComponent : Component
{
    [DataField]
    public float HealthPerAdditionalPlayer = 0.20f;

    [DataField]
    public float HealthPerDefeatedBoss = 0.04f;

    [DataField]
    public float ActionSpeedPerAdditionalPlayer = 0.05f;

    [DataField]
    public float ActionSpeedPerDefeatedBoss = 0.02f;

    [DataField, Access(Other = AccessPermissions.ReadWrite)]
    public TimeSpan ElapsedDifficultyInterval = TimeSpan.FromMinutes(15);

    [DataField, Access(Other = AccessPermissions.ReadWrite)]
    public float HealthPerElapsedInterval = 0.02f;

    [DataField, Access(Other = AccessPermissions.ReadWrite)]
    public float ActionSpeedPerElapsedInterval = 0.01f;

    [DataField, Access(Other = AccessPermissions.ReadWrite)]
    public int MaximumElapsedIntervals = 4;

    [DataField]
    public float MaximumHealthMultiplier = 1.75f;

    [DataField]
    public float MinimumActionDelayMultiplier = 0.55f;

    /// <summary>
    /// Intermediate forms can use director scaling without advancing map progression.
    /// </summary>
    [DataField]
    public bool CountKill = true;

    [ViewVariables]
    public FixedPoint2 BaseHealthThreshold;

    [ViewVariables]
    public float BaseActionDelay = 1f;

    [ViewVariables]
    public float? BaseMegafaunaActionDelay;

    [ViewVariables]
    public float AppliedHealthMultiplier = 1f;

    [ViewVariables]
    public int PeakPartySize = 1;

    [ViewVariables]
    public int ProgressionKills;

    [ViewVariables]
    public int ElapsedDifficultySteps;

    [ViewVariables]
    public bool CountedKill;

    [ViewVariables]
    public MapId EncounterMap = MapId.Nullspace;
}
