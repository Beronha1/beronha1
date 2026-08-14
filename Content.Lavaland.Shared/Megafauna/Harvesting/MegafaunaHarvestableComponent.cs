// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Content.Shared.Damage;
using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Lavaland.Shared.Megafauna.Harvesting;

/// <summary>
/// Keeps a dead megafauna in the world until each configured harvest stage is completed.
/// Stages are ordered and can require different tool qualities.
/// </summary>
[RegisterComponent]
public sealed partial class MegafaunaHarvestableComponent : Component
{
    [DataField(required: true)]
    public List<MegafaunaHarvestStage> Stages = [];

    [DataField]
    public bool DeleteOnCompletion = true;

    /// <summary>
    /// Optional inert carcass spawned after the final harvest stage. This lets
    /// the combat entity be removed while preserving an extractable organic
    /// reservoir without keeping AI, actions, or boss systems alive.
    /// </summary>
    [DataField]
    public EntProtoId? CompletionCarcass;

    [ViewVariables(VVAccess.ReadOnly)]
    public int CurrentStage;
}

/// <summary>
/// Lets a worn item act as a hands-free harvesting implement. This is intentionally
/// separate from ToolComponent so the item cannot masquerade as a hand-held tool.
/// </summary>
[RegisterComponent]
public sealed partial class WearableMegafaunaHarvesterComponent : Component
{
    [DataField]
    public string InventorySlot = "head";

    [DataField(required: true)]
    public List<string> ToolQualities = [];

    [DataField]
    public float SpeedModifier = 1f;

    /// <summary>
    /// Healing applied only when this item is used to finish the final carcass stage.
    /// </summary>
    [DataField]
    public DamageSpecifier CompletionHeal = new();
}

[DataDefinition]
public sealed partial class MegafaunaHarvestStage
{
    [DataField(required: true)]
    public LocId Name;

    [DataField(required: true)]
    public List<string> ToolQualities = [];

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(8);

    [DataField(required: true)]
    public EntityTableSelector Loot = default!;
}

[Serializable, NetSerializable]
public sealed partial class MegafaunaHarvestDoAfterEvent : DoAfterEvent
{
    [DataField]
    public int Stage;

    private MegafaunaHarvestDoAfterEvent()
    {
    }

    public MegafaunaHarvestDoAfterEvent(int stage)
    {
        Stage = stage;
    }

    public override DoAfterEvent Clone() => new MegafaunaHarvestDoAfterEvent(Stage);

    public override bool IsDuplicate(DoAfterEvent other)
        => other is MegafaunaHarvestDoAfterEvent harvest && harvest.Stage == Stage;
}
