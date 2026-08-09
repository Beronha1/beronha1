namespace Content.Shared.Kitchen;

/// <summary>
/// Raised on an entity when it is inside a microwave and it starts cooking.
/// </summary>
public sealed class BeingMicrowavedEvent(
    EntityUid microwave,
    EntityUid? user,
    uint time = 0,                 // Frontier
    bool heating = true,           // Frontier
    bool irradiating = true)       // Frontier
    : HandledEntityEventArgs
{
    public EntityUid Microwave = microwave;
    public EntityUid? User = user;

    // Frontier
    public uint Time = time;

    // Frontier
    public bool BeingHeated = heating;
    public bool BeingIrradiated = irradiating;
    // End Frontier
}