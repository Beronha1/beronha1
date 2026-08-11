// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Shared.Audio;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Lavaland.Shared.Aggression;

public sealed partial class AggressorsSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedBossMusicSystem _bossMusic = default!;
    [Dependency] private SharedTransformSystem _xform = default!;
    [Dependency] private INetManager _net = default!;

    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AggressiveComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<AggressiveComponent, EntityTerminatingEvent>(OnDeleted);
        SubscribeLocalEvent<AggressiveComponent, MobStateChangedEvent>(OnStateChange);

        SubscribeLocalEvent<AggressorComponent, MobStateChangedEvent>(OnAggressorStateChange);
        SubscribeLocalEvent<AggressorComponent, EntityTerminatingEvent>(OnAggressorDeleted);
        SubscribeLocalEvent<AggressorComponent, AggressiveAddedEvent>(OnAggressorAdded);
        SubscribeLocalEvent<AggressorComponent, AggressiveRemovedEvent>(OnAggressorRemoved);

        _xformQuery = GetEntityQuery<TransformComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;

        // All who are aggressive check their aggressors, and remove them if they are too far away.
        var query = EntityQueryEnumerator<AggressiveComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var aggressive, out var xform))
        {
            if (aggressive.ForgiveRange == null
                || aggressive.NextUpdate > curTime)
                continue;

            aggressive.NextUpdate = curTime + aggressive.UpdateDelay;

            foreach (var aggressor in aggressive.Aggressors.ToArray())
            {
                // ActorComponent is removed when an admin/player changes body.
                // Keeping that old body here inflated player-scaled boss health
                // and kept combat music alive for a participant that had left.
                if (!HasComp<ActorComponent>(aggressor))
                {
                    RemoveAggressor((uid, aggressive), aggressor);
                    continue;
                }

                if (!_xformQuery.TryComp(aggressor, out var aggroXform))
                    continue;

                var aggroPos = _xform.GetWorldPosition(aggroXform);
                var aggressivePos = _xform.GetWorldPosition(xform);
                var distance = (aggressivePos - aggroPos).Length();

                if (distance > aggressive.ForgiveRange
                    || xform.MapID != aggroXform.MapID)
                    RemoveAggressor((uid, aggressive), aggressor);
            }
        }
    }

    #region Event Handling

    private void OnDamageChanged(Entity<AggressiveComponent> ent, ref DamageChangedEvent args)
    {
        var aggro = args.Origin;

        if (aggro == null
            || !HasComp<ActorComponent>(aggro))
            return;

        AddAggressor(ent, aggro.Value);
    }

    private void OnDeleted(Entity<AggressiveComponent> ent, ref EntityTerminatingEvent args)
        => RemoveAllAggressors(ent);

    private void OnStateChange(Entity<AggressiveComponent> ent, ref MobStateChangedEvent args)
        => RemoveAllAggressors(ent);

    private void OnAggressorAdded(Entity<AggressorComponent> ent, ref AggressiveAddedEvent args)
    {
        if (ent.Comp.Aggressives.TryFirstOrNull(out var boss))
            _bossMusic.StartBossMusic(boss.Value);
    }

    private void OnAggressorRemoved(Entity<AggressorComponent> ent, ref AggressiveRemovedEvent args)
        => _bossMusic.EndAllMusic(); // Stop the music if we are no longer get attacked by anyone.

    private void OnAggressorStateChange(Entity<AggressorComponent> ent, ref MobStateChangedEvent args)
    {
        if (_mobState.IsDead(ent.Owner))
            CleanAggressions((ent.Owner, ent.Comp));
    }

    private void OnAggressorDeleted(Entity<AggressorComponent> ent, ref EntityTerminatingEvent args)
        => CleanAggressions((ent.Owner, ent.Comp));

    #endregion

    #region Aggressive API

    /// <summary>
    /// Counts only players that are still attached to their aggressor entity.
    /// Admin ghost/body changes remove <see cref="ActorComponent"/> from the old
    /// body, so those stale entities must not keep increasing boss health.
    /// </summary>
    public int CountActivePlayers(Entity<AggressiveComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return 0;

        var sessions = new HashSet<ICommonSession>();
        foreach (var aggressor in ent.Comp.Aggressors)
        {
            if (TryComp<ActorComponent>(aggressor, out var actor))
                sessions.Add(actor.PlayerSession);
        }

        return sessions.Count;
    }

    public void AddAggressor(Entity<AggressiveComponent> ent, EntityUid aggressor)
    {
        var (uid, comp) = ent;
        if (!ent.Comp.Aggressors.Add(aggressor))
            return;

        var aggComp = EnsureComp<AggressorComponent>(aggressor);
        aggComp.Aggressives.Add(uid);

        var ev = new AggressorAddedEvent(aggressor);
        RaiseLocalEvent(uid, ref ev);
        var ev2 = new AggressiveAddedEvent(uid);
        RaiseLocalEvent(aggressor, ref ev2);

        Dirty(uid, comp);
        Dirty(aggressor, aggComp);
    }

    public void RemoveAggressor(Entity<AggressiveComponent> ent, Entity<AggressorComponent?> aggressor)
    {
        if (!ent.Comp.Aggressors.Remove(aggressor))
            return;

        RemoveAggressorFrom(ent, aggressor);
    }

    public void RemoveAllAggressors(Entity<AggressiveComponent> ent)
    {
        foreach (var aggressor in ent.Comp.Aggressors)
        {
            RemoveAggressorFrom(ent, aggressor);
        }

        ent.Comp.Aggressors.Clear();
    }

    private void RemoveAggressorFrom(Entity<AggressiveComponent> ent, Entity<AggressorComponent?> aggressor)
    {
        if (!Resolve(aggressor, ref aggressor.Comp, false))
        {
            // AggressiveComponent and AggressorComponent are separate networked
            // states. During client-side game-state deletion their removals are
            // not atomic, so the reverse component can legitimately be gone by
            // the time the boss's termination cleanup runs.
            if (_net.IsClient)
            {
                _bossMusic.EndAllMusic();
                return;
            }

            // On the authoritative server this still indicates a broken pair
            // and must remain visible instead of being silently tolerated.
            Log.Error($"Aggressor {ToPrettyString(aggressor.Owner)} is missing its reverse component while cleaning {ToPrettyString(ent.Owner)}.");
            return;
        }

        aggressor.Comp.Aggressives.Remove(ent);
        if (aggressor.Comp.Aggressives.Count > 0)
            return;

        var ev = new AggressorRemovedEvent(aggressor);
        RaiseLocalEvent(ent, ref ev);
        var ev2 = new AggressiveRemovedEvent(ent);
        RaiseLocalEvent(aggressor, ref ev2);
        RemComp(aggressor, aggressor.Comp);
    }

    #endregion

    #region Aggressor API

    public void CleanAggressions(Entity<AggressorComponent?> aggressor)
    {
        if (!Resolve(aggressor, ref aggressor.Comp))
            return;

        foreach (var aggressive in aggressor.Comp.Aggressives.ToArray())
        {
            if (TryComp<AggressiveComponent>(aggressive, out var aggressors))
                RemoveAggressor((aggressive, aggressors), aggressor);
        }

        RemComp(aggressor, aggressor.Comp);
    }

    #endregion
}
