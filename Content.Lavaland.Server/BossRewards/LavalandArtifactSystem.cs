// All modifications and original work in ss14-wega under the Corvax-Wega tag
// and _Wega directories are licensed under GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Content.Lavaland.Shared.Artifacts;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.Polymorph.Systems;
using Content.Server.Stunnable;
using Content.Shared.Administration.Managers;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Follower.Components;
using Content.Shared.Ghost.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Implants;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Tiles;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Artifacts;

public sealed partial class LavalandArtifactSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ISharedAdminManager _admin = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private PolymorphSystem _polymorph = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private StunSystem _stun = default!;
    [Dependency] private TagSystem _tag = default!;
    [Dependency] private ITileDefinitionManager _tiles = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private UseDelaySystem _useDelay = default!;
    [Dependency] private FirestarterSystem _firestarter = default!;

    private static readonly ProtoId<TagPrototype> LavaWalkingTag = "LavaWalking";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LavaStaffComponent, BeforeRangedInteractEvent>(OnLavaStaffInteract);
        SubscribeLocalEvent<LavaStaffComponent, LavaStaffTerraformDoAfterEvent>(OnLavaStaffComplete);
        SubscribeLocalEvent<LavaStaffComponent, EntityTerminatingEvent>(OnLavaStaffTerminating);
        SubscribeLocalEvent<DragonBloodComponent, UseInHandEvent>(OnDragonBloodUse);
        SubscribeLocalEvent<DragonBloodComponent, DragonBloodDoAfterEvent>(OnDragonBloodComplete);
        SubscribeLocalEvent<BecomeToDrakeActionEvent>(OnBecomeToDrake);
        SubscribeLocalEvent<DrakeReturnBackActionEvent>(OnReturnFromDrake);
        SubscribeLocalEvent<SacredFlameActionEvent>(OnSacredFlame);
        SubscribeLocalEvent<SoulStorageComponent, MeleeHitEvent>(OnSpectralBladeHit);
        SubscribeLocalEvent<SoulStorageComponent, UseInHandEvent>(OnSpectralBladeUse);
        SubscribeLocalEvent<SoulStorageComponent, ExaminedEvent>(OnSpectralBladeExamine);
        SubscribeLocalEvent<HumanoidProfileComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<DivineVocalCordsImplantComponent, ImplantImplantedEvent>(OnDivineVoiceImplanted);
        SubscribeLocalEvent<DivineVocalCordsImplantComponent, ImplantRemovedEvent>(OnDivineVoiceRemoved);
        SubscribeLocalEvent<DivineVoiceCarrierComponent, EntitySpokeEvent>(OnDivineVoice);
        SubscribeLocalEvent<DivineVocalCordsImplantComponent, ColossusRoarActionEvent>(OnDivineRoar);
        SubscribeLocalEvent<StabilizedLegionCoreImplantComponent, ImplantRelayEvent<MobStateChangedEvent>>(OnLegionCoreStateChanged);
        SubscribeLocalEvent<StabilizedLegionCoreImplantComponent, MapInitEvent>(OnLegionCoreMapInit);
    }

    private void OnLavaStaffInteract(Entity<LavaStaffComponent> ent, ref BeforeRangedInteractEvent args)
    {
        if (args.Handled ||
            ent.Comp.ActiveTarget != null ||
            !TryComp(ent, out UseDelayComponent? useDelay) ||
            _useDelay.IsDelayed((ent, useDelay)))
            return;

        var userCoordinates = _transform.GetMapCoordinates(args.User);
        var targetCoordinates = _transform.ToMapCoordinates(args.ClickLocation);
        if (userCoordinates.MapId != targetCoordinates.MapId ||
            !userCoordinates.InRange(targetCoordinates, ent.Comp.MaxRange) ||
            !TryGetTerraformTile(args.ClickLocation, ent.Comp, out var tileCoordinates))
        {
            return;
        }

        var marker = Spawn(ent.Comp.TargetPrototype, tileCoordinates);
        var doAfter = new DoAfterArgs(
            EntityManager,
            args.User,
            ent.Comp.TerraformTime,
            new LavaStaffTerraformDoAfterEvent(),
            ent,
            target: marker,
            used: ent)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
        {
            QueueDel(marker);
            return;
        }

        ent.Comp.ActiveTarget = marker;
        args.Handled = true;
    }

    private void OnLegionCoreMapInit(Entity<StabilizedLegionCoreImplantComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.ActivationsRemaining = Math.Max(1, ent.Comp.MaxActivations);
    }

    private void OnLavaStaffComplete(Entity<LavaStaffComponent> ent, ref LavaStaffTerraformDoAfterEvent args)
    {
        var marker = args.Target;
        if (ent.Comp.ActiveTarget != marker)
        {
            if (marker != null && Exists(marker.Value))
                QueueDel(marker.Value);
            return;
        }

        ent.Comp.ActiveTarget = null;
        if (args.Cancelled || args.Handled || marker == null || !Exists(marker.Value))
        {
            if (marker != null && Exists(marker.Value))
                QueueDel(marker.Value);
            return;
        }

        var coordinates = Transform(marker.Value).Coordinates;
        QueueDel(marker.Value);
        if (!TryGetTerraformTile(coordinates, ent.Comp, out var tileCoordinates) ||
            !TryComp(ent, out UseDelayComponent? useDelay))
        {
            return;
        }

        if (!_map.TryFindGridAt(_transform.ToMapCoordinates(coordinates), out var gridUid, out var grid))
            return;

        var tile = _map.GetTileRef(gridUid, grid, coordinates);
        var anchored = _map.GetAnchoredEntitiesEnumerator(gridUid, grid, tile.GridIndices);
        EntityUid? lava = null;
        while (anchored.MoveNext(out var anchoredEntity))
        {
            if (MetaData(anchoredEntity.Value).EntityPrototype?.ID is { } prototypeId &&
                prototypeId == ent.Comp.LavaEntity)
            {
                lava = anchoredEntity.Value;
                break;
            }
        }

        if (lava != null)
        {
            QueueDel(lava.Value);
        }
        else
        {
            Spawn(ent.Comp.LavaEntity, tileCoordinates);
        }

        _useDelay.TryResetDelay((ent, useDelay));
        _audio.PlayPvs(ent.Comp.UseSound, ent);
        args.Handled = true;
    }

    private bool TryGetTerraformTile(
        EntityCoordinates coordinates,
        LavaStaffComponent component,
        out EntityCoordinates tileCoordinates)
    {
        tileCoordinates = default;
        if (!_map.TryFindGridAt(_transform.ToMapCoordinates(coordinates), out var gridUid, out var grid))
            return false;

        var tile = _map.GetTileRef(gridUid, grid, coordinates);
        var tileDef = (ContentTileDefinition) _tiles[tile.Tile.TypeId];
        var anchored = _map.GetAnchoredEntitiesEnumerator(gridUid, grid, tile.GridIndices);
        var hasLava = false;
        while (anchored.MoveNext(out var anchoredEntity))
        {
            if (MetaData(anchoredEntity.Value).EntityPrototype?.ID is not { } prototypeId ||
                prototypeId != component.LavaEntity)
                continue;

            hasLava = true;
            break;
        }

        if (!hasLava && tileDef.ID != component.BasaltTile)
            return false;

        tileCoordinates = _map.GridTileToLocal(gridUid, grid, tile.GridIndices);
        return true;
    }

    private void OnLavaStaffTerminating(Entity<LavaStaffComponent> ent, ref EntityTerminatingEvent args)
    {
        if (ent.Comp.ActiveTarget is { } target && Exists(target))
            QueueDel(target);
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

        // /tg/ dragon blood has four permanent outcomes. These are reimplemented using
        // native SS14 systems rather than copying the upstream BYOND implementation.
        var effect = _random.Next(1, 5);
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
            case 4:
                _actions.AddAction(args.User, ent.Comp.FireBreathAction);
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
        var souls = ent.Comp.StolenSouls.Count + CountOrbitingGhosts(ent);
        if (souls == 0)
            return;

        var amount = Math.Min(
            souls * ent.Comp.BonusDamagePerSoul,
            ent.Comp.MaxBonusDamage);
        args.BonusDamage += new DamageSpecifier
        {
            DamageDict = { ["Slash"] = amount },
        };
    }

    private void OnSpectralBladeUse(Entity<SoulStorageComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        var orbiting = CountOrbitingGhosts(ent);
        _audio.PlayPvs(ent.Comp.CallSound, ent);
        _popup.PopupEntity(
            Loc.GetString("spectral-blade-call", ("ghosts", orbiting)),
            ent,
            PopupType.Medium);
        args.Handled = true;
    }

    private void OnSacredFlame(SacredFlameActionEvent args)
    {
        if (args.Handled || Deleted(args.Performer))
            return;

        var coordinates = Transform(args.Performer).Coordinates;
        _firestarter.IgniteNearby(args.Performer, coordinates, args.Severity, args.Radius);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/rumble.ogg"), args.Performer);
        args.Handled = true;
    }

    private void OnSpectralBladeExamine(Entity<SoulStorageComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(
            "spectral-blade-examine",
            ("stolen", ent.Comp.StolenSouls.Count),
            ("orbiting", CountOrbitingGhosts(ent)),
            ("maximum", ent.Comp.MaxOrbitingGhosts)));
    }

    private int CountOrbitingGhosts(Entity<SoulStorageComponent> blade)
    {
        if (!TryComp<FollowedComponent>(blade, out var followed))
            return 0;

        var count = 0;
        foreach (var follower in followed.Following)
        {
            if (!HasComp<GhostComponent>(follower) ||
                !TryComp<ActorComponent>(follower, out var actor) ||
                _admin.IsAdmin(actor.PlayerSession))
            {
                continue;
            }

            count++;
            if (count >= blade.Comp.MaxOrbitingGhosts)
                break;
        }

        return count;
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
        if (!message.Contains("stop") &&
            !message.Contains("halt") &&
            !message.Contains("pare") &&
            !message.Contains("parar") &&
            !message.Contains("стой"))
            return;

        TryUseDivineVoice(ent, implant);
    }

    private void OnDivineRoar(Entity<DivineVocalCordsImplantComponent> ent, ref ColossusRoarActionEvent args)
    {
        if (args.Handled || !TryUseDivineVoice(args.Performer, ent.Comp))
            return;

        _actions.SetCooldown(args.Action.Owner, ent.Comp.Cooldown);
        args.Handled = true;
    }

    private bool TryUseDivineVoice(EntityUid user, DivineVocalCordsImplantComponent implant)
    {
        if (implant.NextUse > _timing.CurTime)
            return false;

        foreach (var target in _lookup.GetEntitiesInRange(Transform(user).Coordinates, implant.Radius))
        {
            if (target == user || !HasComp<MobStateComponent>(target))
                continue;

            // Knockdown drops held items, providing both the requested disarm and stun.
            _stun.TryKnockdown(target, TimeSpan.FromSeconds(2), true);
        }

        implant.NextUse = _timing.CurTime + implant.Cooldown;
        return true;
    }

    private void OnLegionCoreStateChanged(
        Entity<StabilizedLegionCoreImplantComponent> ent,
        ref ImplantRelayEvent<MobStateChangedEvent> args)
    {
        if (ent.Comp.ActivationsRemaining <= 0 ||
            args.Args.NewMobState is not (MobState.Critical or MobState.Dead) ||
            !HasComp<DamageableComponent>(args.ImplantedEntity))
        {
            return;
        }

        ent.Comp.ActivationsRemaining--;
        _damage.ClearAllDamage(args.ImplantedEntity);
        _mobState.ChangeMobState(args.ImplantedEntity, MobState.Alive, origin: ent);
        _popup.PopupEntity(
            Loc.GetString("stabilized-legion-core-activate"),
            args.ImplantedEntity,
            args.ImplantedEntity,
            PopupType.LargeCaution);
        if (ent.Comp.ActivationsRemaining <= 0)
            QueueDel(ent);
    }
}
