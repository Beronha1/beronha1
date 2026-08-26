using Robust.Shared.Audio;

namespace Content.Shared.Movement.Events;

/// <summary>
/// Raised on a mover immediately before a footstep sound would be played.
/// Cancel this event to consume the step without emitting its sound.
/// </summary>
public sealed class BeforeFootstepSoundEvent : CancellableEntityEventArgs;

/// <summary>
/// Raised directed on an entity when trying to get a relevant footstep sound
/// </summary>
[ByRefEvent]
public record struct GetFootstepSoundEvent(EntityUid User)
{
    public readonly EntityUid User = User;

    /// <summary>
    /// Set the sound to specify a footstep sound and mark as handled.
    /// </summary>
    public SoundSpecifier? Sound;
}
