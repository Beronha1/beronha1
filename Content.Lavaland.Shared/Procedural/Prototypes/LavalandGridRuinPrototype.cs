// SPDX-License-Identifier: AGPL-3.0-or-later


namespace Content.Lavaland.Shared.Procedural.Prototypes;

/// <summary>
/// Contains information about Lavaland ruin configuration.
/// </summary>
[Prototype]
public sealed partial class LavalandGridRuinPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField] public LocId Name = "lavaland-ruin-unknown";

    [DataField(required: true)]
    public ResPath Path;

    [DataField]
    public int SpawnAttempts = 8;

    [DataField]
    public bool PatchToPlanet = true;

    /// <summary>
    /// Extra space reserved around this ruin when checking overlap.
    /// Useful for boss arenas whose combat footprint exceeds their tiles.
    /// </summary>
    [DataField]
    public float Clearance;

    [DataField(required: true)]
    public int Priority = int.MinValue;

    /// <summary>
    /// List of components to grant to entities that enter the ruin.
    /// </summary>
    [DataField]
    public ComponentRegistry ComponentsToGrant = new();
}
