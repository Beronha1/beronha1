using Content.Shared.Charges.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Charges.Components;

// 🌇Sunset🌇 - ported from sunrise-station for the BloodCult port; marks an action
// as self-removing from the action bar once its LimitedCharges hit zero.
[RegisterComponent, NetworkedComponent, Access(typeof(SharedChargesSystem))]
public sealed partial class DeleteWithoutChargesComponent : Component
{
}
