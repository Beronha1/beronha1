// Все модификации и наработки в ss14-wega под тегом Corvax-Wega и директориях _Wega лицензированы под GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Robust.Shared.Audio;

namespace Content.Lavaland.Server.Megafauna.Classic;

[RegisterComponent, Access(typeof(ColossusSystem))]
public sealed partial class ColossusBossComponent : Component
{
    [DataField]
    public bool FinalAttackAvailable = true;

    [DataField]
    public float FinalAttackHealthFraction = 0.11f;

    [DataField]
    public SoundSpecifier TelegraphSound = new SoundPathSpecifier("/Audio/_Lavaland/Effects/invoke_general.ogg");

    /// <summary>
    /// Incremented whenever a new sequence starts or death cancels the current one.
    /// Delayed callbacks only run while their captured value still matches this token.
    /// </summary>
    public int SequenceId;
}
