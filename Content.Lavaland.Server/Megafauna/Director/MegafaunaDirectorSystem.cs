// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Server.NPC;
using Content.Lavaland.Shared.Aggression;
using Content.Lavaland.Shared.CCVar;
using Content.Lavaland.Shared.Megafauna.Components;
using Content.Lavaland.Shared.Megafauna.NumberSelectors;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Shared.GameTicking;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Map;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Megafauna.Director;

/// <summary>
/// Whiskey's encounter director. Scaling is monotonic during an encounter so a
/// departing player can never lower a boss's death threshold below damage already dealt.
/// </summary>
public sealed partial class MegafaunaDirectorSystem : EntitySystem
{
    [Dependency] private MobThresholdSystem _thresholds = default!;
    [Dependency] private NPCUseActionsOnTargetSystem _npcActions = default!;
    [Dependency] private INetConfigurationManager _configuration = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private AggressorsSystem _aggressors = default!;

    private readonly Dictionary<MapId, int> _killsByMap = new();
    public bool Enabled { get; private set; } = true;
    private TimeSpan _roundStartedAt;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaDirectorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MegafaunaDirectorComponent, AggressorAddedEvent>(OnAggressorAdded);
        SubscribeLocalEvent<MegafaunaDirectorComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<MapRemovedEvent>(OnMapRemoved);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);

        Subs.CVar(_configuration, LavalandCVars.MegafaunaDirectorEnabled, OnEnabledChanged, true);
        _roundStartedAt = _timing.CurTime;
    }

    private void OnMapInit(Entity<MegafaunaDirectorComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.EncounterMap = Transform(ent).MapID;
        if (!_thresholds.TryGetThresholdForState(ent, MobState.Dead, out var threshold))
        {
            Log.Error($"Megafauna director attached to {ToPrettyString(ent)} without a dead mob threshold.");
            return;
        }

        ent.Comp.BaseHealthThreshold = threshold.Value;
        if (TryComp<NPCUseActionsOnTargetComponent>(ent, out var actions))
            ent.Comp.BaseActionDelay = actions.DelayModifier;
        if (TryComp<MegafaunaAiComponent>(ent, out var ai)
            && ai.ActionDelaySelector is MegafaunaConstantNumberSelector constantDelay)
            ent.Comp.BaseMegafaunaActionDelay = constantDelay.Value;

        if (Enabled)
            ApplyDifficulty(ent);
    }

    private void OnAggressorAdded(Entity<MegafaunaDirectorComponent> ent, ref AggressorAddedEvent args)
    {
        if (!Enabled)
            return;

        if (TryComp<AggressiveComponent>(ent, out var aggressive))
            ent.Comp.PeakPartySize = Math.Max(ent.Comp.PeakPartySize,
                _aggressors.CountActivePlayers((ent.Owner, aggressive)));

        ApplyDifficulty(ent);
    }

    private void OnMobStateChanged(Entity<MegafaunaDirectorComponent> ent, ref MobStateChangedEvent args)
    {
        if (!Enabled || args.NewMobState != MobState.Dead || ent.Comp.CountedKill)
            return;

        ent.Comp.CountedKill = true;
        if (!ent.Comp.CountKill)
            return;

        var map = ent.Comp.EncounterMap;
        if (map == MapId.Nullspace)
            return;

        _killsByMap[map] = GetProgressionKills(map) + 1;

        var query = EntityQueryEnumerator<MegafaunaDirectorComponent>();
        while (query.MoveNext(out var uid, out var director))
        {
            if (uid == ent.Owner || director.CountedKill || director.EncounterMap != map)
                continue;

            ApplyDifficulty((uid, director));
        }
    }

    /// <summary>
    /// Recalculates one live encounter from its prototype baseline.
    /// Exposed for focused tests and future admin/debug tooling.
    /// </summary>
    public void ApplyDifficulty(Entity<MegafaunaDirectorComponent> ent)
    {
        if (!Enabled || ent.Comp.BaseHealthThreshold <= 0 || ent.Comp.CountedKill)
            return;

        if (TryComp<AggressiveComponent>(ent, out var aggressive))
            ent.Comp.PeakPartySize = Math.Max(ent.Comp.PeakPartySize,
                Math.Max(1, _aggressors.CountActivePlayers((ent.Owner, aggressive))));

        var additionalPlayers = Math.Max(0, ent.Comp.PeakPartySize - 1);
        var progression = GetProgressionKills(ent.Comp.EncounterMap);
        var elapsedSteps = GetElapsedDifficultySteps(ent.Comp);
        ent.Comp.ProgressionKills = progression;
        ent.Comp.ElapsedDifficultySteps = elapsedSteps;

        var healthMultiplier = 1f
            + additionalPlayers * ent.Comp.HealthPerAdditionalPlayer
            + progression * ent.Comp.HealthPerDefeatedBoss
            + elapsedSteps * ent.Comp.HealthPerElapsedInterval;
        healthMultiplier = Math.Clamp(healthMultiplier, ent.Comp.AppliedHealthMultiplier, ent.Comp.MaximumHealthMultiplier);
        ent.Comp.AppliedHealthMultiplier = healthMultiplier;

        _thresholds.SetMobStateThreshold(ent, ent.Comp.BaseHealthThreshold * healthMultiplier, MobState.Dead);

        var delayMultiplier = 1f
            - additionalPlayers * ent.Comp.ActionSpeedPerAdditionalPlayer
            - progression * ent.Comp.ActionSpeedPerDefeatedBoss
            - elapsedSteps * ent.Comp.ActionSpeedPerElapsedInterval;
        delayMultiplier = Math.Max(delayMultiplier, ent.Comp.MinimumActionDelayMultiplier);
        if (TryComp<NPCUseActionsOnTargetComponent>(ent, out var actions))
            _npcActions.SetDelaySpeed(ent, ent.Comp.BaseActionDelay * delayMultiplier, actions);
        if (TryComp<MegafaunaAiComponent>(ent, out var ai)
            && ai.ActionDelaySelector is MegafaunaConstantNumberSelector constantDelay
            && ent.Comp.BaseMegafaunaActionDelay is { } baseAiDelay)
            constantDelay.Value = Math.Max(0.01f, baseAiDelay * delayMultiplier);
    }

    public int GetProgressionKills(MapId map)
        => map == MapId.Nullspace ? 0 : _killsByMap.GetValueOrDefault(map);

    private int GetElapsedDifficultySteps(MegafaunaDirectorComponent component)
    {
        if (component.ElapsedDifficultyInterval <= TimeSpan.Zero || component.MaximumElapsedIntervals <= 0)
            return 0;

        var elapsed = _timing.CurTime > _roundStartedAt
            ? _timing.CurTime - _roundStartedAt
            : TimeSpan.Zero;
        return Math.Min(component.MaximumElapsedIntervals,
            (int) (elapsed.Ticks / component.ElapsedDifficultyInterval.Ticks));
    }

    private void OnRoundStarting(RoundStartingEvent args)
    {
        _roundStartedAt = _timing.CurTime;
        _killsByMap.Clear();
    }

    private void OnEnabledChanged(bool enabled)
    {
        Enabled = enabled;

        var query = EntityQueryEnumerator<MegafaunaDirectorComponent>();
        while (query.MoveNext(out var uid, out var director))
        {
            if (enabled)
            {
                ApplyDifficulty((uid, director));
                continue;
            }

            RestorePrototypeDifficulty((uid, director));
        }
    }

    private void RestorePrototypeDifficulty(Entity<MegafaunaDirectorComponent> ent)
    {
        if (ent.Comp.BaseHealthThreshold <= 0 || ent.Comp.CountedKill)
            return;

        _thresholds.SetMobStateThreshold(ent, ent.Comp.BaseHealthThreshold, MobState.Dead);
        ent.Comp.AppliedHealthMultiplier = 1f;
        ent.Comp.ProgressionKills = 0;
        ent.Comp.ElapsedDifficultySteps = 0;

        if (TryComp<NPCUseActionsOnTargetComponent>(ent, out var actions))
            _npcActions.SetDelaySpeed(ent, ent.Comp.BaseActionDelay, actions);
        if (TryComp<MegafaunaAiComponent>(ent, out var ai)
            && ai.ActionDelaySelector is MegafaunaConstantNumberSelector constantDelay
            && ent.Comp.BaseMegafaunaActionDelay is { } baseAiDelay)
            constantDelay.Value = baseAiDelay;
    }

    private void OnMapRemoved(MapRemovedEvent args)
        => _killsByMap.Remove(args.MapId);

    private void OnRoundRestart(RoundRestartCleanupEvent args)
        => _killsByMap.Clear();
}
