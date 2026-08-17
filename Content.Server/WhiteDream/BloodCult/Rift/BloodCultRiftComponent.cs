// Ported from funky-station (BloodCultRiftComponent) and adapted.
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Rift;

/// <summary>
///     The bleeding wound in reality that opens once the veil is weakened.
///     Cultists chant on the runes around it to drag Nar'Sie through.
/// </summary>
[RegisterComponent]
public sealed partial class BloodCultRiftComponent : Component
{
    public const string SolutionName = "sanguine_pool";
    public static readonly ProtoId<ReagentPrototype> Reagent = "SanguinePerniculate";

    #region Bleeding

    [DataField]
    public float PulseInterval = 30f;

    [DataField]
    public float BloodPerPulse = 50f;

    [ViewVariables(VVAccess.ReadOnly)]
    public float TimeUntilNextPulse;

    #endregion

    #region Ritual

    /// <summary>
    ///     The offering runes placed around the rift when it spawned.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> SummoningRunes = new();

    [DataField]
    public float RuneRange = 1.5f;

    /// <summary>
    ///     How many cultists have to be standing on the runes. Drops by one after every sacrifice,
    ///     since the sacrifice was one of them.
    /// </summary>
    [DataField]
    public int RequiredCultists = 3;

    /// <summary>
    ///     How many cultists Nar'Sie eats before she comes through herself.
    /// </summary>
    [DataField]
    public int RequiredSacrifices = 3;

    /// <summary>
    ///     Seconds between chants before the first offering. Slow, sparse, ominous.
    ///     Each cycle below is tuned so the whole ritual runs about as long as the music.
    /// </summary>
    [DataField]
    public List<float> ChantDelaysFirst = new() { 16f, 15f, 14f, 12f, 11f, 10f };

    /// <summary>
    ///     After the first offering the cult finds its rhythm.
    /// </summary>
    [DataField]
    public List<float> ChantDelaysSecond = new() { 9f, 8f, 7.5f, 7f, 6.5f, 6f, 5f, 4.5f, 4f };

    /// <summary>
    ///     After the second offering it becomes a frenzy.
    /// </summary>
    [DataField]
    public List<float> ChantDelaysThird = new()
    {
        4f, 3.6f, 3.3f, 3f, 2.8f, 2.6f, 2.4f, 2.2f,
        2f, 1.8f, 1.6f, 1.4f, 1.2f, 1f, 0.9f, 0.8f,
    };

    /// <summary>
    ///     The chant cycle for however many offerings have already been made.
    /// </summary>
    public List<float> CurrentCycle => SacrificesDone switch
    {
        0 => ChantDelaysFirst,
        1 => ChantDelaysSecond,
        _ => ChantDelaysThird,
    };

    /// <summary>
    ///     How many words each cycle puts in a follower's mouth. It builds.
    /// </summary>
    public int FollowerChantWords => SacrificesDone switch
    {
        0 => 1,
        1 => 2,
        _ => 3,
    };

    /// <summary>
    ///     The one marked for the veil always speaks longer than everyone else.
    /// </summary>
    public int LeaderChantWords => FollowerChantWords + 3;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool RitualInProgress;

    [ViewVariables(VVAccess.ReadOnly)]
    public int ChantsInCycle;

    [ViewVariables(VVAccess.ReadOnly)]
    public int SacrificesDone;

    [ViewVariables(VVAccess.ReadOnly)]
    public float TimeUntilNextChant;

    /// <summary>
    ///     Whoever is speaking the long chant is the next to die.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? PendingSacrifice;

    #endregion

    #region Music

    /// <summary>
    ///     Roughly 174 seconds long. The chant cycles above are tuned to finish with it.
    /// </summary>
    [DataField]
    public SoundSpecifier RitualMusic = new SoundPathSpecifier("/Audio/WhiteDream/BloodCult/tear_of_veil.ogg");

    [ViewVariables(VVAccess.ReadOnly)]
    public bool MusicPlaying;

    #endregion

    [DataField]
    public EntProtoId NarsiePrototype = "MobNarsieSpawn";

    /// <summary>
    ///     What the offered cultists come back as. Nar'Sie's heralds, not soulstones.
    /// </summary>
    [DataField]
    public EntProtoId HarvesterProto = "ConstructHarvester";

    [DataField]
    public EntProtoId SoulShardProto = "SoulShard";

    [DataField]
    public EntProtoId SoulShardGhostProto = "SoulShardGhost";
}

/// <summary>
///     Marks the runes around a rift so invoking them starts the final summoning.
/// </summary>
[RegisterComponent]
public sealed partial class FinalSummoningRuneComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Rift;
}
