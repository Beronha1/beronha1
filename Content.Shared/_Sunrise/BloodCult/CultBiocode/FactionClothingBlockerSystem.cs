using Content.Shared.Inventory.Events;
using Robust.Shared.Network;

namespace Content.Shared._Sunrise.BloodCult.CultBiocode;

public sealed partial class FactionClothingBlockerSystem : EntitySystem
{
    [Dependency] private CultBiocodeSystem _biocodeSystem = default!;
    [Dependency] private INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FactionClothingBlockerComponent, BeingEquippedAttemptEvent>(OnEquippedAttempt);
    }

    private void OnEquippedAttempt(EntityUid uid, FactionClothingBlockerComponent component, BeingEquippedAttemptEvent args)
    {
        // Does not work correctly on the client due to poor serialization of NpcFactionMemberComponent
        if (_net.IsClient)
            return;

        if (TryComp<CultBiocodeComponent>(args.Equipment, out var biocodedComponent))
        {
            if (!_biocodeSystem.CanUse(args.EquipTarget, biocodedComponent.Factions))
                args.Cancel();
        }
    }
}
