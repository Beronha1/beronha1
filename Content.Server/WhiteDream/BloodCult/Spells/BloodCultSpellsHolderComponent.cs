using Robust.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Psionics;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Spells;

[RegisterComponent]
public sealed partial class BloodCultSpellsHolderComponent : Component
{
    [DataField]
    public int DefaultMaxSpells = 1;

    [DataField]
    public TimeSpan SpellCreationTime = TimeSpan.FromSeconds(2);

    /// <summary>
    ///     WhiteDream - carving a spell into yourself hurts, same as drawing a rune.
    /// </summary>
    [DataField]
    public DamageSpecifier SpellCreationDamage = new() { DamageDict = new() { ["Slash"] = 15 } };

    /// <summary>
    ///     WhiteDream - same audio as drawing a rune: the stab, then the blood.
    /// </summary>
    [DataField]
    public SoundSpecifier SpellCreationStartSound = new SoundPathSpecifier("/Audio/WhiteDream/BloodCult/butcher.ogg");

    [DataField]
    public SoundSpecifier SpellCreationEndSound = new SoundPathSpecifier("/Audio/WhiteDream/BloodCult/blood.ogg");

    [DataField]
    public ProtoId<PsionicPowerPoolPrototype> PowersPoolPrototype = "BloodCultPowers";

    [DataField]
    public List<EntProtoId> ManagementActions =
    [
        "ActionBloodCultSelectSpells",
        "ActionBloodCultRemoveSpells"
    ];

    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> SelectedSpells = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> ManagementActionEnts = new();

    public int MaxSpells;

    public DoAfterId? DoAfterId;

    /// <summary>
    ///     Since radial selector menu doesn't have metadata, we use this to toggle between remove and
    ///     add spells modes.
    /// </summary>
    public bool AddSpellsMode = true;
}
