// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Shared.Megafauna.Utility;
using Content.Shared.Atmos.Rotting;
using Content.Shared.EntityEffects;
using Content.Shared.Examine;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Megafauna.Utility;

/// <summary>
/// Owns boss-organ deterioration and freezer preservation. AntiRottingContainer is deliberately reused so
/// kitchen/medical freezers work without a second, visually identical kind of refrigerated storage.
/// </summary>
public sealed partial class PerishableBossOrganSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private IGameTiming _timing = default!;

    private readonly List<Entity<PerishableBossOrganComponent>> _expired = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PerishableBossOrganComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PerishableBossOrganComponent, EntGotInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<PerishableBossOrganComponent, EntGotRemovedFromContainerMessage>(OnRemoved);
        SubscribeLocalEvent<PerishableBossOrganComponent, ExaminedEvent>(OnExamine);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _expired.Clear();

        var query = EntityQueryEnumerator<PerishableBossOrganComponent>();
        while (query.MoveNext(out var uid, out var organ))
        {
            if (organ.State != PerishableBossOrganState.Fresh ||
                organ.PreservedBy != null ||
                organ.DecayAt == null ||
                organ.DecayAt > _timing.CurTime)
            {
                continue;
            }

            _expired.Add((uid, organ));
        }

        foreach (var organ in _expired)
            Deteriorate(organ);
    }

    private void OnMapInit(Entity<PerishableBossOrganComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.State != PerishableBossOrganState.Fresh)
            return;

        ent.Comp.RemainingFreshness = ent.Comp.FreshDuration;
        ent.Comp.DecayAt = _timing.CurTime + ent.Comp.FreshDuration;
    }

    private void OnInserted(Entity<PerishableBossOrganComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (ent.Comp.State != PerishableBossOrganState.Fresh ||
            ent.Comp.PreservedBy != null ||
            !HasComp<AntiRottingContainerComponent>(args.Container.Owner))
        {
            return;
        }

        ent.Comp.RemainingFreshness = Remaining(ent.Comp);
        ent.Comp.DecayAt = null;
        ent.Comp.PreservedBy = args.Container.Owner;
    }

    private void OnRemoved(Entity<PerishableBossOrganComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (ent.Comp.State != PerishableBossOrganState.Fresh || ent.Comp.PreservedBy != args.Container.Owner)
            return;

        ent.Comp.PreservedBy = null;
        ent.Comp.DecayAt = _timing.CurTime + ent.Comp.RemainingFreshness;
    }

    private void OnExamine(Entity<PerishableBossOrganComponent> ent, ref ExaminedEvent args)
    {
        var message = ent.Comp.State switch
        {
            PerishableBossOrganState.Stabilized => Loc.GetString("megafauna-organ-examine-stabilized"),
            PerishableBossOrganState.Deteriorated => Loc.GetString("megafauna-organ-examine-deteriorated"),
            _ when ent.Comp.PreservedBy != null => Loc.GetString(
                "megafauna-organ-examine-preserved",
                ("seconds", Math.Max(0, (int) Math.Ceiling(ent.Comp.RemainingFreshness.TotalSeconds)))),
            _ => Loc.GetString(
                "megafauna-organ-examine-fresh",
                ("seconds", Math.Max(0, (int) Math.Ceiling(Remaining(ent.Comp).TotalSeconds)))),
        };

        args.PushMarkup(message);
    }

    private TimeSpan Remaining(PerishableBossOrganComponent organ)
    {
        if (organ.DecayAt is not { } decayAt)
            return organ.RemainingFreshness;

        return TimeSpan.FromTicks(Math.Max(0, (decayAt - _timing.CurTime).Ticks));
    }

    private void Deteriorate(Entity<PerishableBossOrganComponent> ent)
    {
        ent.Comp.State = PerishableBossOrganState.Deteriorated;
        ent.Comp.RemainingFreshness = TimeSpan.Zero;
        ent.Comp.DecayAt = null;
        ent.Comp.PreservedBy = null;

        if (ent.Comp.DestroyContentsOf is not { } containerId ||
            !_containers.TryGetContainer(ent, containerId, out var container))
        {
            return;
        }

        foreach (var contained in container.ContainedEntities)
            QueueDel(contained);
    }
}

/// <summary>
/// Applies chemical preservation without restoring organs which have already deteriorated.
/// </summary>
public sealed partial class StabilizeMegafaunaOrganSystem
    : EntityEffectSystem<PerishableBossOrganComponent, StabilizeMegafaunaOrgan>
{
    protected override void Effect(
        Entity<PerishableBossOrganComponent> entity,
        ref EntityEffectEvent<StabilizeMegafaunaOrgan> args)
    {
        if (entity.Comp.State != PerishableBossOrganState.Fresh)
            return;

        entity.Comp.State = PerishableBossOrganState.Stabilized;
        entity.Comp.DecayAt = null;
        entity.Comp.PreservedBy = null;
    }
}
