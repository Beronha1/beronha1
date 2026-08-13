// All modifications and additions under the Corvax-Wega tag and _Wega directories are licensed under GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using System.Numerics;
using Content.Lavaland.Server.NPC;
using Content.Lavaland.Shared.Megafauna.Events;
using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Megafauna.Classic;

/// <summary>
/// Reimplements the current Paradise Colossus repertoire as cancellable SS14 attack sequences.
/// The shared NPC action selector decides which family to use; this system owns telegraphs,
/// health gates and the individual projectile patterns.
/// </summary>
public sealed partial class ColossusSystem : EntitySystem
{
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedGunSystem _gun = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MobThresholdSystem _threshold = default!;
    [Dependency] private NPCUseActionsOnTargetSystem _npcActions = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ColossusBossComponent, ColossusFractionActionEvent>(OnShotgunAction);
        SubscribeLocalEvent<ColossusBossComponent, ColossusCrossActionEvent>(OnAlternatingAction);
        SubscribeLocalEvent<ColossusBossComponent, ColossusSpiralActionEvent>(OnSpiralAction);
        SubscribeLocalEvent<ColossusBossComponent, ColossusWrathActionEvent>(OnWrathAction);
        SubscribeLocalEvent<ColossusBossComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnShotgunAction(Entity<ColossusBossComponent> ent, ref ColossusFractionActionEvent args)
    {
        if (!TryBeginSelectedAttack(ent, args.Target, out var finalStarted))
        {
            args.Handled = finalStarted;
            return;
        }

        args.Handled = true;
        const float windup = 1.5f;
        var sequence = BeginSequence(ent, TimeSpan.FromSeconds(2));
        var target = args.Target;
        Telegraph(ent, "RETRIBUTION");
        Schedule(ent, sequence, windup, () => ShootShotgun(ent.Owner, target));
    }

    private void OnAlternatingAction(Entity<ColossusBossComponent> ent, ref ColossusCrossActionEvent args)
    {
        if (!TryBeginSelectedAttack(ent, args.Target, out var finalStarted))
        {
            args.Handled = finalStarted;
            return;
        }

        args.Handled = true;
        var sequence = BeginSequence(ent, TimeSpan.FromSeconds(5));
        Telegraph(ent, "LAMENT");

        Schedule(ent, sequence, 1.5f, () => ShootDirections(ent.Owner, diagonal: true));
        Schedule(ent, sequence, 2.5f, () => ShootDirections(ent.Owner, diagonal: false));
        Schedule(ent, sequence, 3.5f, () => ShootDirections(ent.Owner, diagonal: true));
        Schedule(ent, sequence, 4.5f, () => ShootDirections(ent.Owner, diagonal: false));
    }

    private void OnWrathAction(Entity<ColossusBossComponent> ent, ref ColossusWrathActionEvent args)
    {
        if (!TryBeginSelectedAttack(ent, args.Target, out var finalStarted))
        {
            args.Handled = finalStarted;
            return;
        }

        args.Handled = true;
        var sequence = BeginSequence(ent, TimeSpan.FromSeconds(2));
        var projectileCount = args.ProjectileCount;
        var radius = args.Radius;
        Telegraph(ent, "WRATH");
        Schedule(ent, sequence, 1.5f, () => ShootRandomField(ent.Owner, projectileCount, radius));
    }

    private void OnSpiralAction(Entity<ColossusBossComponent> ent, ref ColossusSpiralActionEvent args)
    {
        if (!TryBeginSelectedAttack(ent, args.Target, out var finalStarted))
        {
            args.Handled = finalStarted;
            return;
        }

        args.Handled = true;
        var doubleSpiral = IsBelowHealthFraction(ent, args.DieHealthModifier);
        var windup = doubleSpiral ? 2.5f : 1.5f;
        var projectileCount = doubleSpiral ? args.DieProjectileCount : args.JudgementProjectileCount;
        var projectileDelay = doubleSpiral ? args.DieProjectileDelay : args.JudgementProjectileDelay;

        // Paradise emits 80 bolts. Older local prototypes supplied much smaller values, so keep their
        // cadence but enforce the reference pattern density.
        projectileCount = Math.Max(projectileCount, 80);
        projectileDelay = Math.Max(projectileDelay, 0.08f);

        var duration = windup + projectileCount * projectileDelay + 0.25f;
        var sequence = BeginSequence(ent, TimeSpan.FromSeconds(duration));
        Telegraph(ent, doubleSpiral ? "DIE" : "JUDGEMENT");
        ScheduleSpiral(ent, sequence, windup, projectileCount, projectileDelay, doubleSpiral);
    }

    private bool TryBeginSelectedAttack(
        Entity<ColossusBossComponent> ent,
        EntityUid target,
        out bool finalStarted)
    {
        finalStarted = false;
        if (_mobState.IsDead(ent.Owner))
            return false;

        if (ent.Comp.FinalAttackAvailable && IsBelowHealthFraction(ent, ent.Comp.FinalAttackHealthFraction))
        {
            ent.Comp.FinalAttackAvailable = false;
            StartFinalAttack(ent, target);
            finalStarted = true;
            return false;
        }

        return true;
    }

    private void StartFinalAttack(Entity<ColossusBossComponent> ent, EntityUid target)
    {
        const float duration = 18f;
        var sequence = BeginSequence(ent, TimeSpan.FromSeconds(duration));
        Telegraph(ent, "PERISH MORTAL");

        // The Paradise finale layers aimed blasts over increasingly dense random shots, then closes with
        // full-field and directional barrages. Timings are compressed slightly for SS14's movement scale.
        for (var wave = 0; wave < 10; wave++)
        {
            var capturedWave = wave;
            Schedule(ent, sequence, 2.5f + wave * 0.8f, () =>
            {
                ShootShotgun(ent.Owner, target);
                ShootRandomField(ent.Owner, 6 + capturedWave, 12f);
            });
        }

        for (var wave = 0; wave < 3; wave++)
        {
            Schedule(ent, sequence, 11f + wave * 1.25f,
                () => ShootRandomField(ent.Owner, 28, 12f));
        }

        Schedule(ent, sequence, 15f, () => ShootDirections(ent.Owner, diagonal: true, allDirections: true));
        Schedule(ent, sequence, 16f, () => ShootDirections(ent.Owner, diagonal: true, allDirections: true));
        Schedule(ent, sequence, 17f, () => ShootDirections(ent.Owner, diagonal: true, allDirections: true));
    }

    private int BeginSequence(Entity<ColossusBossComponent> ent, TimeSpan duration)
    {
        ent.Comp.SequenceId++;
        _npcActions.LockActions(ent.Owner, duration);
        return ent.Comp.SequenceId;
    }

    private void Schedule(Entity<ColossusBossComponent> ent, int sequence, float delay, Action callback)
    {
        Timer.Spawn(TimeSpan.FromSeconds(delay), () =>
        {
            if (!TryComp<ColossusBossComponent>(ent.Owner, out var component) ||
                component.SequenceId != sequence ||
                _mobState.IsDead(ent.Owner))
            {
                return;
            }

            callback();
        });
    }

    private void ScheduleSpiral(
        Entity<ColossusBossComponent> ent,
        int sequence,
        float windup,
        int projectileCount,
        float delay,
        bool doubleSpiral)
    {
        var startAngle = _random.NextFloat(0f, MathF.Tau);
        var clockwise = _random.Prob(0.5f) ? 1f : -1f;

        for (var index = 0; index < projectileCount; index++)
        {
            var shotIndex = index;
            Schedule(ent, sequence, windup + index * delay, () =>
            {
                var angle = startAngle + clockwise * shotIndex * MathF.Tau / 16f;
                ShootAtAngle(ent.Owner, angle);
                if (doubleSpiral)
                    ShootAtAngle(ent.Owner, angle + MathF.PI);
            });
        }
    }

    private void Telegraph(Entity<ColossusBossComponent> ent, string message)
    {
        _chat.TrySendInGameICMessage(ent.Owner, message, InGameICChatType.Speak,
            false, true, ignoreActionBlocker: true);
        _audio.PlayPvs(ent.Comp.TelegraphSound, ent.Owner);
    }

    private void ShootShotgun(EntityUid colossus, EntityUid target)
    {
        if (!Exists(target))
            return;

        var origin = _transform.GetWorldPosition(colossus);
        var delta = _transform.GetWorldPosition(target) - origin;
        if (delta == Vector2.Zero)
            return;

        var baseAngle = MathF.Atan2(delta.Y, delta.X);
        ReadOnlySpan<float> offsets = [-12.5f, -7.5f, -2.5f, 2.5f, 7.5f, 12.5f];
        foreach (var offset in offsets)
        {
            ShootAtAngle(colossus, baseAngle + offset * MathF.PI / 180f);
        }
    }

    private void ShootDirections(EntityUid colossus, bool diagonal, bool allDirections = false)
    {
        var start = diagonal ? MathF.PI / 4f : 0f;
        var count = allDirections ? 8 : 4;
        var step = allDirections ? MathF.PI / 4f : MathF.PI / 2f;

        for (var index = 0; index < count; index++)
        {
            ShootAtAngle(colossus, start + index * step);
        }
    }

    private void ShootRandomField(EntityUid colossus, int count, float radius)
    {
        for (var index = 0; index < count; index++)
        {
            // Random endpoints replicate Paradise's sparse selection of turfs within a 12 tile field.
            var angle = _random.NextFloat(0f, MathF.Tau);
            var distance = MathF.Sqrt(_random.NextFloat()) * radius;
            ShootAtOffset(colossus, new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance);
        }
    }

    private void ShootAtAngle(EntityUid colossus, float angle)
    {
        ShootAtOffset(colossus, new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 12f);
    }

    private void ShootAtOffset(EntityUid colossus, Vector2 offset)
    {
        var mapUid = _transform.GetMap(colossus);
        if (mapUid == null || offset == Vector2.Zero || !TryComp<GunComponent>(colossus, out var gun))
            return;

        var target = new EntityCoordinates(mapUid.Value, _transform.GetWorldPosition(colossus) + offset);
        gun.NextFire = TimeSpan.Zero;
        _gun.AttemptShoot(colossus, (colossus, gun), target);
    }

    private bool IsBelowHealthFraction(EntityUid entity, float fraction)
    {
        if (!_threshold.TryGetThresholdForState(entity, MobState.Dead, out var threshold) ||
            !TryComp<DamageableComponent>(entity, out var damageable))
        {
            return false;
        }

        return _damage.GetTotalDamage((entity, damageable)) >= threshold * (1f - fraction);
    }

    private void OnMobStateChanged(Entity<ColossusBossComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        ent.Comp.SequenceId++;
        _npcActions.UnlockActions(ent.Owner);
    }
}
