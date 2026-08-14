using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._ES.Telesci.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
[Access(typeof(ESSharedTelesciSystem))]
public sealed partial class ESPortalGeneratorConsoleComponent : Component
{
    /// <summary>
    /// Time between updates
    /// </summary>
    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Time when next update occurs
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdateTime;
}

[Serializable, NetSerializable]
public enum ESPortalGeneratorConsoleUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class ESPortalGeneratorConsoleBuiState : BoundUserInterfaceState
{
    public float Charge;
    public bool Charging;
    public int CurrentResearchStage;
    public int MaxResearchStage;
    public int ThreatsLeft;
}

[Serializable, NetSerializable]
public sealed class ESActivePortalGeneratorBuiMessage : BoundUserInterfaceMessage;

/// <summary>
/// Raised locally after the shared portal-generator UI accepts an activation request.
/// Server game modes can react without registering duplicate handlers for the same BUI message.
/// </summary>
[ByRefEvent]
public record struct ESPortalGeneratorActivatedEvent;
