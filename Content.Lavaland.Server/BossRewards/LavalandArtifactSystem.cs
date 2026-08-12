// All modifications and original work in ss14-wega under the Corvax-Wega tag
// and _Wega directories are licensed under GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Content.Lavaland.Shared.Artifacts;
using Content.Server.Chat.Systems;
using Content.Server.Polymorph.Systems;
using Content.Server.Stunnable;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Implants;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Tiles;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Artifacts;

public sealed partial class LavalandArtifactSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private PolymorphSystem _polymorph = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private StunSystem _stun = default!;
    [Dependency] private TagSystem _tag = default!;
    [Dependency] private ITileDefinitionManager _tiles = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private UseDelaySystem _useDelay = default!;

    private static readonly ProtoId<TagPrototype> LavaWalkingTag = "LavaWalking";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LavaStaffComponent, AfterInteractEvent>(OnLavaStaffInteract);
        SubscribeLocalEvent<DragonBloodComponent, UseInHandEvent>(OnDragonBloodUse);
        SubscribeLocalEvent<DragonBloodComponent, DragonBloodDoAfterEvent>(OnDragonBloodComplete);
        SubscribeLocalEvent<BecomeToDrakeActionEvent>(OnBecomeToDrake);
        SubscribeLocalEvent<DrakeReturnBackActionEvent>(OnReturnFromDrake);
        SubscribeLocalEvent<SoulStorageComponent, MeleeHitEvent>(OnSpectralBladeHit);
        SubscribeLocalEvent<HumanoidProfileComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<DivineVocalCordsImplantComponent, ImplantImplantedEvent>(OnDivineVoiceImplanted);
        SubscribeLocalEvent<DivineVocalCordsImplantComponent, ImplantRemovedEvent>(OnDivineVoiceRemoved);
        SubscribeLocalEvent<DivineVoiceCarrierComponent, EntitySpokeEvent>(OnDivineVoice);
    }

    private void OnLavaStaffInteract(Entity<LavaStaffComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled ||
            !args.CanReach ||
            !TryComp(ent, out UseDelayComponent? useDelay) ||
            _useDelay.IsDelayed((ent, useDelay)))
            return;

        if (!_map.TryFindGridAt(_transform.ToMapCoordinates(args.ClickLocation), out var gridUid, out var grid))
            return;

        var tile = _map.GetTileRef(gridUid, grid, args.ClickLocation);
        var tileDef = (ContentTileDefinition) _tiles[tile.Tile.TypeId];

        if (args.Target is { } target && HasComp<TileEntityEffectComponent>(target))
        {
            if (MetaData(target).EntityPrototype?.ID is not { } targetPrototype ||
                targetPrototype != ent.Comp.LavaEntity)
                return;

            QueueDel(target);
        }
        else
        {
            if (args.Target != null || tileDef.ID != ent.Comp.BasaltTile)
                return;

            Spawn(ent.Comp.LavaEntity, _map.GridTileToLocal(gridUid, grid, tile.GridIndices));
        }

        args.Handled = _useDelay.TryResetDelay((ent, useDelay));
        _audio.PlayPvs(ent.Comp.UseSound, ent);
    }

    private void OnDragonBloodUse(Entity<DragonBloodComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        var doAfter = new DoAfterArgs(
            EntityManager,
            args.User,
            ent.Comp.UseTime,
            new DragonBloodDoAfterEvent(),
            ent,
            used: ent)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
        };

        args.Handled = _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDragonBloodComplete(Entity<DragonBloodComponent> ent, ref DragonBloodDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var effect = _random.Next(1, 4);
        switch (effect)
        {
            case 1:
                _polymorph.PolymorphEntity(args.User, ent.Comp.Skeleton);
                break;
            case 2:
                _tag.AddTag(args.User, LavaWalkingTag);
                break;
            case 3:
                _actions.AddAction(args.User, ent.Comp.LowerDrakeAction);
                break;
        }

        _audio.PlayPvs(ent.Comp.UseSound, args.User);
        _popup.PopupEntity(Loc.GetString($"dragon-blood-effect-{effect}"), args.User, args.User);
        QueueDel(ent);
        args.Handled = true;
    }

    private void OnBecomeToDrake(BecomeToDrakeActionEvent args)
    {
        var polymorph = _polymorph.PolymorphEntity(args.Performer, args.LowerDrake);
        if (polymorph == null)
            return;

        _actions.AddAction(polymorph.Value, args.ReturnBack);
        args.Handled = true;
    }

    private void OnReturnFromDrake(DrakeReturnBackActionEvent args)
    {
        _actions.RemoveAction(args.Performer, args.Action.Owner);
        _polymorph.Revert(args.Performer);
        args.Handled = true;
    }

    private void OnSpectralBladeHit(Entity<SoulStorageComponent> ent, ref MeleeHitEvent args)
    {
        if (ent.Comp.StolenSouls.Count == 0)
            return;

        var amount = Math.Min(
            ent.Comp.StolenSouls.Count * ent.Comp.BonusDamagePerSoul,
            ent.Comp.MaxBonusDamage);
        args.BonusDamage += new DamageSpecifier
        {
            DamageDict = { ["Slash"] = amount },
        };
    }

    private void OnMobStateChanged(Entity<HumanoidProfileComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || args.Origin == null)
            return;

        var weapon = _hands.GetActiveItemOrSelf(args.Origin.Value);
        if (TryComp<SoulStorageComponent>(weapon, out var storage))
            storage.StolenSouls.Add(ent.Owner);
    }

    private void OnDivineVoiceImplanted(
        Entity<DivineVocalCordsImplantComponent> ent,
        ref ImplantImplantedEvent args)
    {
        EnsureComp<DivineVoiceCarrierComponent>(args.Implanted).Implant = ent;
    }

    private void OnDivineVoiceRemoved(
        Entity<DivineVocalCordsImplantComponent> ent,
        ref ImplantRemovedEvent args)
    {
        RemCompDeferred<DivineVoiceCarrierComponent>(args.Implanted);
    }

    private void OnDivineVoice(Entity<DivineVoiceCarrierComponent> ent, ref EntitySpokeEvent args)
    {
        if (!TryComp<DivineVocalCordsImplantComponent>(ent.Comp.Implant, out var implant) ||
            implant.NextUse > _timing.CurTime ||
            args.IsWhisper)
        {
            return;
        }

        var message = args.Message.ToLowerInvariant();
        if (!message.Contains("stop") && !message.Contains("halt") && !message.Contains("стой"))
            return;

        foreach (var target in _lookup.GetEntitiesInRange(Transform(ent).Coordinates, implant.Radius))
        {
            if (target == ent.Owner || !HasComp<MobStateComponent>(target))
                continue;

            _stun.TryKnockdown(target, TimeSpan.FromSeconds(2), true);
        }

        implant.NextUse = _timing.CurTime + implant.Cooldown;
    }
}
