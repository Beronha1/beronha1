using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.Whetstone;

[RegisterComponent]
public sealed partial class WhetstoneComponent : Component
{
    [DataField]
    public int Uses = 1;

    [DataField]
    public DamageSpecifier DamageIncrease = new()
    {
        // Trauma - DamageDict is keyed by ProtoId now
        DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
        {
            ["Slash"] = 4
        }
    };

    [DataField]
    public float MaximumIncrease = 25;

    [DataField]
    public EntityWhitelist Whitelist = new();

    [DataField]
    public EntityWhitelist Blacklist = new();

    [DataField]
    public SoundSpecifier SharpenAudio = new SoundPathSpecifier("/Audio/Weapons/bladeslice.ogg");
}
