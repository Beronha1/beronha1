// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Lavaland.Shared.Megafauna.Components;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Lavaland.Shared.Megafauna.Mercury;

/// <summary>
/// Components adapted from Goobstation PR #6542. They remain local to the
/// Lavaland module, but retain the source encounter's timings and geometry.
/// </summary>
[RegisterComponent]
public sealed partial class AddOrRemoveComponentComponent : Component
{
    public ComponentRegistry? TargetComponent;
    public bool RemoveAfterTimer;
    public TimeSpan TimeToRemoval;
    public TimeSpan RemovalTime;
}

[RegisterComponent]
public sealed partial class VicinitySpawnerComponent : Component
{
    [DataField]
    public TimeSpan SpawnInterval = TimeSpan.FromSeconds(0.5);

    public TimeSpan NextSpawn;

    [DataField]
    public int NumberToSpawn = 1;

    [DataField(required: true)]
    public List<EntProtoId> Prototype = new();

    [DataField]
    public EntProtoId EmptyPrototype = "VoidPortalEmpty";

    [DataField]
    public int OffsetForSpawn;
}

[RegisterComponent]
public sealed partial class EtherDrainComponent : Component
{
    [DataField]
    public int StaminaDrain = 10;

    [DataField]
    public float Range = 30f;

    [DataField]
    public EntProtoId Prototype = "ORTBeamWarning";
}

[RegisterComponent]
public sealed partial class CosmicRayCirculatorComponent : Component
{
    [DataField(required: true)]
    public EntProtoId WarningPrototype;

    [DataField] public float Radius = 3f;
    [DataField] public int Count = 9;
    [DataField] public TimeSpan Delay = TimeSpan.FromSeconds(1);
    [DataField] public TimeSpan WaveDelay = TimeSpan.FromSeconds(0.15f);
    [DataField] public int WaveCount = 10;
    [DataField] public float RadiusIncrease = 1f;

    public bool Active;
    public TimeSpan NextWaveTime;
    public int CurrentWave;
}

[RegisterComponent]
public sealed partial class EnvironmentalResonanceComponent : Component
{
    [DataField(required: true)] public EntProtoId RightPrototype;
    [DataField(required: true)] public EntProtoId LeftPrototype;
    [DataField(required: true)] public EntProtoId UpPrototype;
    [DataField(required: true)] public EntProtoId DownPrototype;
    [DataField] public float HorizontalOffset;
    [DataField] public float VerticalOffset;
    [DataField] public float TileSkip = 2f;
    [DataField] public int RowNumber;
}

[RegisterComponent]
public sealed partial class DirectionalMovementComponent : Component
{
    [DataField] public Vector2 Direction;
    [DataField] public float Speed = 10f;
    [DataField] public float Acceleration;
    public float CurrentSpeed;
}

[RegisterComponent]
public sealed partial class ORTSolarStormComponent : Component
{
    [DataField] public float ChargeTime = 10f;
    [DataField] public float ParticleSpawnRate = 0.2f;
    [DataField] public float ParticleIncreaseBy = 0.01f;
    [DataField] public float ParticleSpawnRadius = 10f;
    [DataField] public EntProtoId ParticlePrototype = "ORTSolarParticle";
    [DataField] public EntProtoId WarningPrototype = "ORTSolarStormWarning";
    [DataField] public EntProtoId StormPrototype = "ORTSolarStorm";
    [DataField] public float StormRadius = 3f;
    [DataField] public float StormDuration = 8f;
    [DataField] public DamageSpecifier StormDamage = new();
    [DataField] public SoundSpecifier ChargeSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/Mercury/ChargeSolarStorm.ogg");
    [DataField] public SoundSpecifier FireSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/Mercury/heavyimpact.ogg");
    [DataField] public float WaitForIt = 1f;

    public float CurrentParticleSpawnRate;
    public TimeSpan NextParticleSpawn;
    public TimeSpan NextDamageTick;
    public TimeSpan StormStartTime;
    public TimeSpan StormEndTime;
    public TimeSpan ChargeEndTime;
    public EntityUid? WarningEntity;
    public EntityUid? StormEntity;
    public bool IsCharging;
    public bool StormSoon;
    public bool IsActive;
}

[RegisterComponent]
public sealed partial class ParadigmInflationComponent : Component
{
    [DataField] public float AnalyzeTime = 5f;
    [DataField] public float DivideDamage = 2f;
    [DataField] public EntProtoId WarningPrototype = "ParadigmInflationTarget";
    [DataField] public SoundSpecifier AnalyzeSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/Mercury/communicating.ogg");
    [DataField] public SoundSpecifier ParadigmSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/Mercury/glitch.ogg");

    public EntityUid? Target;
    public EntityUid? WarningEntity;
    public TimeSpan AnalyzeEndTime;
    public bool IsAnalyzing;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class PhaseConversionComponent : Component
{
    [DataField] public SoundSpecifier SwitchSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/Mercury/conversion.ogg");
    [DataField] public SpriteSpecifier MeleeSprite = new SpriteSpecifier.Rsi(new ResPath("_Lavaland/Mobs/Bosses/96x96.rsi"), "adapted_melee");
    [DataField] public SpriteSpecifier RangedSprite = new SpriteSpecifier.Rsi(new ResPath("_Lavaland/Mobs/Bosses/96x96.rsi"), "adapted_ranged");
    [DataField] public ProtoId<MegafaunaSelectorPrototype> MeleeSelector = "ORTMelee";
    [DataField] public ProtoId<MegafaunaSelectorPrototype> RangedSelector = "ORTRanged";
    [DataField] public EntProtoId EffectPrototype = "ORTPhaseConversionEffect";
    [DataField] public float SwitchDelay = 1f;

    public EntityUid? EffectEntity;
    public bool SwitchSoon;
    public bool IsRanged = true;
    public TimeSpan SwitchTime;
}

[Serializable, NetSerializable]
public enum PhaseConversionVisuals : byte
{
    IsRanged,
}

[RegisterComponent]
public sealed partial class ReflectiveThreadsComponent : Component
{
    [DataField] public EntProtoId EffectPrototype = "ORTReflectiveThreadsEffect";
    [DataField] public SoundSpecifier ReflectSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/Mercury/crystal_swoop.ogg");
    [DataField] public float ReflectDuration = 5f;

    public EntityUid? EffectEntity;
    public bool Reflecting;
    public TimeSpan ReflectEndTime;
}

[RegisterComponent]
public sealed partial class OrbitingRingComponent : Component
{
    [DataField] public float RingDistance = 2f;
    [DataField] public float GrowSpeed = 1f;
    [DataField] public int Count = 7;
    [DataField(required: true)] public EntProtoId Prototype;
    [DataField] public SoundSpecifier? Sound;
    public List<EntityUid> Entities = new();
}

[RegisterComponent]
public sealed partial class OrbitingComponent : Component
{
    [DataField] public float MaxRadius = 2f;
    [DataField] public float GrowSpeed = 1f;
    public float Radius;
    public float Angle;
}

[RegisterComponent]
public sealed partial class ORTConvergenceComponent : Component
{
    [DataField] public EntProtoId WarningPrototype = "ORTWarningBox";
    [DataField] public EntProtoId SafeZonePrototype = "ORTSafeZoneIndicator";
    [DataField] public float SafeZoneRadius = 2f;
    [DataField] public float StartRadius = 12f;
    [DataField] public int Count = 48;
    [DataField] public int MinCount = 8;
    [DataField] public TimeSpan InitialDelay = TimeSpan.FromSeconds(5);
    [DataField] public TimeSpan WaveDelay = TimeSpan.FromSeconds(0.25f);
    [DataField] public int WaveCount = 10;
    [DataField] public float MinDistance = 4f;
    [DataField] public float MaxDistance = 10f;

    public bool Active;
    public TimeSpan NextWaveTime;
    public int CurrentWave;
    public EntityUid? SafeZoneEntity;
}

[RegisterComponent]
public sealed partial class SafeZoneComponent : Component
{
    [DataField] public List<EntProtoId> Blacklist = new();
    [DataField] public float SafeRadius = 3f;
    [DataField] public TimeSpan LookupInterval = TimeSpan.FromSeconds(1);
    public TimeSpan NextLookupTime;
}

[RegisterComponent]
public sealed partial class DangerZoneComponent : Component
{
    [DataField] public float PopUpRange = 10f;
    [DataField] public List<LocId> Popup = new();
    [DataField] public TimeSpan Interval = TimeSpan.FromSeconds(5);
    public TimeSpan NextPopup;
}

[RegisterComponent]
public sealed partial class ExpandAndCollapseComponent : Component
{
    [DataField] public float ExpandTime = 9f;
    [DataField] public float CollapseTime = 1f;
    [DataField] public float StartingScale = 0.1f;
    [DataField] public float MaxScale = 1f;
    public float CurrentScale;
    public float Accumulator;
    public bool Collapsing;
}

[RegisterComponent]
public sealed partial class SpriteRotaterComponent : Component
{
    [DataField] public float RotationSpeed = 15f;
    [DataField] public float MaximumSpeed = 100f;
    [DataField] public float IncreaseBy = 10f;
    [DataField] public bool IncreaseOvertime;
    public float CurrentSpeed;
}

[RegisterComponent]
public sealed partial class ORTTransportMatterComponent : Component
{
    [DataField] public EntProtoId AnchorPrototype = "ORTAnchor";
    [DataField] public float TeleportDistance = 5f;
    [DataField] public float TeleportDelay = 4f;
    [DataField] public bool ShouldPlaySound = true;
    [DataField] public SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Effects/Shadowkin/futuristic-teleport.ogg");
    [DataField] public float MoveSpeed = 25f;
    [DataField] public float DashOvershootDistance = 2f;
    [DataField] public float TeleportDelayMultiplier = 0.5f;
    [DataField] public EntProtoId DashWarningPrototype = "ORTXibalbaGhost";
    [DataField] public EntProtoId PlayerTargetPrototype = "TransportMatterTarget";
    [DataField] public EntProtoId DashDamagePrototype = "ORTBeamWarning";
    [DataField] public float DashDamageInterval = 0.05f;
    [DataField] public EntProtoId DashLandPrototype = "ORTMegaSlashEffect";
    [DataField] public float FadeOutTime = 0.75f;

    public EntityUid? AnchorEntity;
    public EntityUid? DashWarningEntity;
    public EntityUid? PlayerTargetEntity;
    public Vector2? MoveTarget;
    public TimeSpan NextTransport;
    public TimeSpan DashEndTime;
    public TimeSpan NextDashDamage;
    public bool Dashing;
}

[RegisterComponent]
public sealed partial class ORTLightningImmuneComponent : Component;
