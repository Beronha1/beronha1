// Все модификации и наработки в ss14-wega под тегом Corvax-Wega и директориях _Wega лицензированы под GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using System.Linq;
using Content.Server.NPC.Components;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.NPC;

public sealed partial class NPCUseActionsOnTargetSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private NPCSystem _npc = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NPCUseActionsOnTargetComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<NPCUseActionsOnTargetComponent, HTNComponent>();
        while (query.MoveNext(out var uid, out var comp, out var htn))
        {
            if (!htn.Blackboard.TryGetValue<EntityUid>(comp.TargetKey, out var target, EntityManager) || !Exists(target))
                continue;

            if (_mobState.IsIncapacitated(uid))
            {
                _npc.SleepNPC(uid, htn);
                continue;
            }

            TryUseRandomAction((uid, comp), target);
        }
    }

    private void OnMapInit(Entity<NPCUseActionsOnTargetComponent> ent, ref MapInitEvent args)
    {
        foreach (var action in ent.Comp.ActionIds)
        {
            ent.Comp.ActionEnts[action] = _actions.AddAction(ent, action);
        }
    }

    public void SetActions(EntityUid uid,
        List<EntProtoId<TargetActionComponent>> actionIds,
        Dictionary<EntProtoId<TargetActionComponent>, float> chances,
        NPCUseActionsOnTargetComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        ClearAllActions(uid, comp);

        comp.ActionIds = actionIds?.ToList() ?? new();
        comp.ActionChances = chances?.ToDictionary(x => x.Key, x => x.Value) ?? new();

        InitializeActions(uid, comp);
    }

    public void ClearAllActions(EntityUid uid, NPCUseActionsOnTargetComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        foreach (var (_, actionEnt) in comp.ActionEnts)
        {
            if (actionEnt != null && Exists(actionEnt.Value))
            {
                _actions.RemoveAction(uid, actionEnt.Value);
            }
        }

        comp.ActionIds.Clear();
        comp.ActionEnts.Clear();
        comp.ActionChances.Clear();
        comp.RecentActions.Clear();
    }

    public void InitializeActions(EntityUid uid, NPCUseActionsOnTargetComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        foreach (var actionId in comp.ActionIds)
        {
            if (!comp.ActionEnts.ContainsKey(actionId))
            {
                var actionEnt = _actions.AddAction(uid, actionId);
                if (actionEnt != null)
                {
                    comp.ActionEnts[actionId] = actionEnt;
                }
            }
        }
    }

    public void SetActionChance(EntityUid uid, EntProtoId<TargetActionComponent> actionId,
        float chance, NPCUseActionsOnTargetComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.ActionChances[actionId] = Math.Clamp(chance, 0f, 1.0f);
    }

    public void RemoveAction(EntityUid uid, EntProtoId<TargetActionComponent> actionId,
        NPCUseActionsOnTargetComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        if (comp.ActionEnts.TryGetValue(actionId, out var actionEnt) && actionEnt != null)
        {
            _actions.RemoveAction(uid, actionEnt.Value);
        }

        comp.ActionIds.Remove(actionId);
        comp.ActionEnts.Remove(actionId);
        comp.ActionChances.Remove(actionId);
        comp.RecentActions.RemoveAll(id => id == actionId);
    }

    public void AddAction(EntityUid uid, EntProtoId<TargetActionComponent> actionId,
        float? chance = null, NPCUseActionsOnTargetComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        if (!comp.ActionIds.Contains(actionId))
        {
            comp.ActionIds.Add(actionId);
            var actionEnt = _actions.AddAction(uid, actionId);
            if (actionEnt != null)
            {
                comp.ActionEnts[actionId] = actionEnt;
            }
        }

        if (chance.HasValue)
        {
            comp.ActionChances[actionId] = Math.Clamp(chance.Value, 0f, 1.0f);
        }
    }

    public bool SetDelaySpeed(EntityUid uid, float delayModifier, NPCUseActionsOnTargetComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return false;

        comp.DelayModifier = Math.Max(delayModifier, 0.01f);
        return true;
    }

    public bool TryUseRandomAction(Entity<NPCUseActionsOnTargetComponent?> user, EntityUid target)
    {
        if (!Resolve(user, ref user.Comp, false))
            return false;

        if (_timing.CurTime < user.Comp.NextUseTime)
            return false;

        if (_timing.CurTime < user.Comp.ActionLockUntil)
            return false;

        var availableActions = new List<(EntProtoId<TargetActionComponent> id, EntityUid action, float chance)>();
        foreach (var (actionId, actionEnt) in user.Comp.ActionEnts)
        {
            if (actionEnt == null || !TryComp<ActionComponent>(actionEnt, out var actionComp))
                continue;

            var chance = user.Comp.ActionChances.TryGetValue(actionId, out var customChance)
                ? Math.Clamp(customChance, 0f, 1.0f) : Math.Clamp(user.Comp.DefaultChance, 0f, 1.0f);

            if (chance <= 0f)
                continue;

            if (!_actions.ValidAction((actionEnt.Value, actionComp)))
                continue;

            availableActions.Add((actionId, actionEnt.Value, chance));
        }

        if (availableActions.Count == 0)
            return false;

        // A history window can be configured larger than a phase's current repertoire. Keep at least one
        // action outside the window so phase swaps and single-action NPCs never deadlock the preferred pool.
        var effectiveMemory = Math.Min(user.Comp.RecentActionMemory, Math.Max(0, availableActions.Count - 1));
        while (user.Comp.RecentActions.Count > effectiveMemory)
            user.Comp.RecentActions.RemoveAt(0);

        // Prefer attacks outside the short history window. If they are all rejected by their boss-specific
        // state checks, retry the excluded attacks before giving up. Paradise bosses choose attacks from
        // explicit sequences; this provides the same repertoire coverage without making the order deterministic.
        var preferredActions = availableActions
            .Where(action => !user.Comp.RecentActions.Contains(action.id))
            .ToList();
        var fallbackActions = availableActions
            .Where(action => user.Comp.RecentActions.Contains(action.id))
            .ToList();

        if (TryPerformFromPool(user, target, preferredActions) ||
            TryPerformFromPool(user, target, fallbackActions))
        {
            return true;
        }

        user.Comp.NextUseTime = _timing.CurTime + user.Comp.FailedActionRetryDelay;
        return false;
    }

    /// <summary>
    /// Reserves the shared action channel for a multi-step attack sequence.
    /// </summary>
    public void LockActions(EntityUid uid, TimeSpan duration, NPCUseActionsOnTargetComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var requestedEnd = _timing.CurTime + duration;
        if (requestedEnd > component.ActionLockUntil)
            component.ActionLockUntil = requestedEnd;
    }

    public void UnlockActions(EntityUid uid, NPCUseActionsOnTargetComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.ActionLockUntil = TimeSpan.Zero;
    }

    private bool TryPerformFromPool(
        Entity<NPCUseActionsOnTargetComponent?> user,
        EntityUid target,
        List<(EntProtoId<TargetActionComponent> id, EntityUid action, float chance)> candidates)
    {
        while (candidates.Count > 0)
        {
            var selectedIndex = GetSelectedActionIndex(candidates);
            var selected = candidates[selectedIndex];
            candidates.RemoveAt(selectedIndex);

            if (!TryComp<ActionComponent>(selected.action, out var selectedComp))
                continue;

            _actions.SetEventTarget(selected.action, target);
            if (!_actions.PerformAction(user.Owner, (selected.action, selectedComp), predicted: false))
                continue;

            RecordSuccessfulAction(user.Comp!, selected.id);

            var delay = selectedComp.UseDelay ?? TimeSpan.FromSeconds(1);
            user.Comp!.NextUseTime = _timing.CurTime + delay * user.Comp.DelayModifier;
            return true;
        }

        return false;
    }

    private int GetSelectedActionIndex(
        List<(EntProtoId<TargetActionComponent> id, EntityUid action, float chance)> values)
    {
        var totalWeight = values.Sum(action => action.chance);
        var randomValue = _random.NextFloat(0f, totalWeight);
        var accumulated = 0f;

        // Shuffle first so equal weights do not inherit prototype/dictionary ordering as a hidden priority.
        _random.Shuffle(values);
        for (var index = 0; index < values.Count; index++)
        {
            accumulated += values[index].chance;
            if (randomValue <= accumulated)
                return index;
        }

        return values.Count - 1;
    }

    private static void RecordSuccessfulAction(
        NPCUseActionsOnTargetComponent component,
        EntProtoId<TargetActionComponent> actionId)
    {
        if (component.RecentActionMemory <= 0)
        {
            component.RecentActions.Clear();
            return;
        }

        component.RecentActions.RemoveAll(id => id == actionId);
        component.RecentActions.Add(actionId);

        while (component.RecentActions.Count > component.RecentActionMemory)
        {
            component.RecentActions.RemoveAt(0);
        }
    }
}
