using Content.Shared.NPC.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunrise.BloodCult.CultBiocode;

// 🌇Sunset🌇 - ported from sunrise-station's Content.Shared._Sunrise.Biocode.BiocodeComponent,
// renamed to CultBiocode: this fork already has an unrelated Biocode system
// (Content.Shared._Sunset.Biocode, single-body-owner suit self-destruct) registered under the
// name "Biocode", so sunrise's faction-based gear lock needs a distinct component name.
[RegisterComponent, NetworkedComponent]
public sealed partial class CultBiocodeComponent : Component
{
    [DataField]
    public string AlertText = "item-biocode-refused";

    [DataField(required: true)]
    public HashSet<ProtoId<NpcFactionPrototype>> Factions = [];
}
