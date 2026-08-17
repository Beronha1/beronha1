// Ported from funky-station (PR #1922, Terkala/Rainbeon) and adapted to the WhiteDream cult.
using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared.WhiteDream.BloodCult.Commune;

/// <summary>
///     Raised when a cultist uses the commune action. Opens the commune window.
/// </summary>
public sealed partial class BloodCultCommuneEvent : InstantActionEvent;

[Serializable, NetSerializable]
public enum BloodCultCommuneUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class BloodCultCommuneBuiState(string message) : BoundUserInterfaceState
{
    public readonly string Message = message;
}

[Serializable, NetSerializable]
public sealed class BloodCultCommuneSendMessage(string message) : BoundUserInterfaceMessage
{
    public readonly string Message = message;
}

/// <summary>
///     Raised when a cultist studies the veil. Reports the cult's progress back to them.
/// </summary>
public sealed partial class BloodCultStudyVeilEvent : InstantActionEvent;
