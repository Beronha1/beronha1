// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

using Content.Server.Popups;
using Content.Shared.Interaction;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Runes;

namespace Content.Server.WhiteDream.BloodCult.CultBarrier;

public sealed partial class BloodCultBarrierSystem : EntitySystem
{
    [Dependency] private PopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BloodCultBarrierComponent, InteractUsingEvent>(OnInteract);
    }

    private void OnInteract(Entity<BloodCultBarrierComponent> ent, ref InteractUsingEvent args)
    {
        if (!HasComp<RuneDrawerComponent>(args.Used) || !HasComp<BloodCultistComponent>(args.User))
            return;

        // WhiteDream - this was passing the raw loc key straight to the popup.
        _popup.PopupEntity(Loc.GetString("cult-barrier-destroyed"), args.User, args.User);
        Del(args.Target);
    }
}
