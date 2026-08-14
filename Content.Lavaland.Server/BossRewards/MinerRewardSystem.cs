// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Shared.Artifacts;
using Content.Lavaland.Shared.Megafauna.Harvesting;
using Content.Server.Administration.Logs;
using Content.Server.Polymorph.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.EntityTable;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Artifacts;

/// <summary>
/// Server-authoritative reward mechanics shared by the Blood-Drunk and Demonic Frost miners.
/// </summary>
public sealed partial class MinerRewardSystem : EntitySystem
{
    [Dependency] private IAdminLogManager _adminLog = default!;
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private EntityTableSystem _entityTable = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private PolymorphSystem _polymorph = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedItemSystem _item = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CleavingSawComponent, MapInitEvent>(OnSawMapInit);
        SubscribeLocalEvent<CleavingSawComponent, UseInHandEvent>(OnSawToggle);
        SubscribeLocalEvent<CleavingSawComponent, MeleeHitEvent>(OnSawHit);

        SubscribeLocalEvent<TrophyRecyclableComponent, InteractUsingEvent>(OnTrophyInteractUsing);
        SubscribeLocalEvent<TrophyRecyclableComponent, RecycleTrophyDoAfterEvent>(OnTrophyRecycleComplete);

        SubscribeLocalEvent<DemonicJackhammerComponent, MeleeHitEvent>(OnJackhammerHit);

        SubscribeLocalEvent<ResurrectionCrystalComponent, UseInHandEvent>(OnCrystalUse);
        SubscribeLocalEvent<ResurrectionCrystalWardComponent, MobStateChangedEvent>(OnWardStateChanged);

        SubscribeLocalEvent<CursedIceBootsComponent, GotEquippedEvent>(OnBootsEquipped);
        SubscribeLocalEvent<CursedIceBootsComponent, GotUnequippedEvent>(OnBootsUnequipped);
        SubscribeLocalEvent<CursedIceBootsComponent, ToggleCursedIceBootsActionEvent>(OnBootsToggle);
        SubscribeLocalEvent<CursedIceTrailCarrierComponent, MoveEvent>(OnIceTrailMove);

        SubscribeLocalEvent<GodslayerArmorComponent, GotEquippedEvent>(OnGodslayerEquipped);
        SubscribeLocalEvent<GodslayerArmorComponent, GotUnequippedEvent>(OnGodslayerUnequipped);
        SubscribeLocalEvent<GodslayerArmorComponent, ExaminedEvent>(OnGodslayerExamined);
        SubscribeLocalEvent<GodslayerCarrierComponent, MobStateChangedEvent>(OnGodslayerStateChanged);
    }

    private void OnSawMapInit(Entity<CleavingSawComponent> ent, ref MapInitEvent args)
    {
        ApplySawMode(ent);
    }

    private void OnSawToggle(Entity<CleavingSawComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.Open = !ent.Comp.Open;
        Dirty(ent);
        ApplySawMode(ent);
        _appearance.SetData(ent, CleavingSawVisuals.Open, ent.Comp.Open);
        _item.SetHeldPrefix(ent, ent.Comp.Open ? "open" : null);
        _popup.PopupClient(
            Loc.GetString(ent.Comp.Open ? "cleaving-saw-opened" : "cleaving-saw-closed"),
            ent,
            args.User);
        args.Handled = true;
    }

    private void ApplySawMode(Entity<CleavingSawComponent> ent)
    {
        if (!TryComp<MeleeWeaponComponent>(ent, out var melee))
            return;

        melee.AttackRate = ent.Comp.Open ? ent.Comp.OpenAttackRate : ent.Comp.ClosedAttackRate;
        melee.Angle = ent.Comp.Open ? ent.Comp.OpenAngle : ent.Comp.ClosedAngle;
        melee.Damage = new DamageSpecifier(ent.Comp.Open ? ent.Comp.OpenDamage : ent.Comp.ClosedDamage);
        Dirty(ent, melee);
    }

    private void OnSawHit(Entity<CleavingSawComponent> ent, ref MeleeHitEvent args)
    {
        var hitLiving = false;
        foreach (var target in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(target) || _mobState.IsDead(target))
                continue;

            hitLiving = true;
            if (!ent.Comp.Open && !ent.Comp.ClosedBleed.Empty)
                _damage.TryChangeDamage(target, ent.Comp.ClosedBleed, origin: args.User);

            if (ent.Comp.Open && HasComp<MegafaunaHarvestableComponent>(target))
                args.BonusDamage += args.BaseDamage * (ent.Comp.MegafaunaDamageMultiplier - 1f);
        }

        if (hitLiving && ent.Comp.Open)
            _popup.PopupClient(Loc.GetString("cleaving-saw-cleave"), ent, args.User);
    }

    private void OnTrophyInteractUsing(Entity<TrophyRecyclableComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || ent.Comp.ActiveKnife != null || !HasComp<WildhunterKnifeComponent>(args.Used))
            return;

        var doAfter = new DoAfterArgs(
            EntityManager,
            args.User,
            ent.Comp.Duration,
            new RecycleTrophyDoAfterEvent(),
            ent,
            target: ent,
            used: args.Used)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
            return;

        ent.Comp.ActiveKnife = args.Used;
        args.Handled = true;
        _popup.PopupClient(Loc.GetString("wildhunter-recycle-start"), ent, args.User);
    }

    private void OnTrophyRecycleComplete(Entity<TrophyRecyclableComponent> ent, ref RecycleTrophyDoAfterEvent args)
    {
        var knife = ent.Comp.ActiveKnife;
        ent.Comp.ActiveKnife = null;
        if (args.Cancelled || args.Handled || knife == null || args.Used != knife || !HasComp<WildhunterKnifeComponent>(knife.Value))
            return;

        args.Handled = true;
        foreach (var prototype in _entityTable.GetSpawns(ent.Comp.Loot))
            Spawn(prototype, Transform(ent).Coordinates);

        _adminLog.Add(
            LogType.Action,
            $"{ToPrettyString(args.User):player} recycled {ToPrettyString(ent):item} with {ToPrettyString(knife.Value):tool}");
        _popup.PopupEntity(Loc.GetString("wildhunter-recycle-complete"), ent, args.User);
        QueueDel(ent);
    }

    private void OnJackhammerHit(Entity<DemonicJackhammerComponent> ent, ref MeleeHitEvent args)
    {
        var hit = false;
        foreach (var target in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(target) || _mobState.IsDead(target))
                continue;

            hit = true;
            var direction = (_transform.GetWorldPosition(target) - _transform.GetWorldPosition(args.User)).Normalized();
            _throwing.TryThrow(target, direction, ent.Comp.ThrowStrength);
        }

        if (hit && !ent.Comp.MeleeHeal.Empty)
            _damage.TryChangeDamage(args.User, ent.Comp.MeleeHeal, true, false, origin: ent);
    }

    private void OnCrystalUse(Entity<ResurrectionCrystalComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled || HasComp<ResurrectionCrystalWardComponent>(args.User) || !HasComp<DamageableComponent>(args.User))
            return;

        var ward = EnsureComp<ResurrectionCrystalWardComponent>(args.User);
        ward.ResurrectionPolymorph = ent.Comp.ResurrectionPolymorph;
        _adminLog.Add(
            LogType.Action,
            LogImpact.High,
            $"{ToPrettyString(args.User):player} absorbed the resurrection ward from {ToPrettyString(ent):item}");
        _popup.PopupEntity(Loc.GetString("resurrection-crystal-absorbed"), args.User, args.User, PopupType.LargeCaution);
        args.Handled = true;
        QueueDel(ent);
    }

    private void OnWardStateChanged(Entity<ResurrectionCrystalWardComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || !HasComp<DamageableComponent>(ent))
            return;

        var polymorph = ent.Comp.ResurrectionPolymorph;
        RemCompDeferred<ResurrectionCrystalWardComponent>(ent);
        _damage.ClearAllDamage(ent.Owner);
        _mobState.ChangeMobState(ent, MobState.Alive, origin: ent);
        _popup.PopupEntity(Loc.GetString("resurrection-crystal-triggered"), ent, ent, PopupType.LargeCaution);
        _adminLog.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(ent):player} was revived by a demonic resurrection crystal");
        _polymorph.PolymorphEntity(ent, polymorph);
    }

    private void OnBootsEquipped(Entity<CursedIceBootsComponent> ent, ref GotEquippedEvent args)
    {
        if (!args.SlotFlags.HasFlag(SlotFlags.FEET))
            return;

        var carrier = EnsureComp<CursedIceTrailCarrierComponent>(args.EquipTarget);
        carrier.Source = ent;
        carrier.NextTrail = _timing.CurTime;
    }

    private void OnBootsUnequipped(Entity<CursedIceBootsComponent> ent, ref GotUnequippedEvent args)
    {
        if (TryComp<CursedIceTrailCarrierComponent>(args.EquipTarget, out var carrier) && carrier.Source == ent.Owner)
            RemCompDeferred<CursedIceTrailCarrierComponent>(args.EquipTarget);
    }

    private void OnBootsToggle(Entity<CursedIceBootsComponent> ent, ref ToggleCursedIceBootsActionEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.Enabled = !ent.Comp.Enabled;
        Dirty(ent);
        _popup.PopupClient(
            Loc.GetString(ent.Comp.Enabled ? "cursed-ice-boots-enabled" : "cursed-ice-boots-disabled"),
            ent,
            args.Performer);
        args.Handled = true;
    }

    private void OnIceTrailMove(Entity<CursedIceTrailCarrierComponent> ent, ref MoveEvent args)
    {
        if (_timing.CurTime < ent.Comp.NextTrail ||
            args.OldPosition == args.NewPosition ||
            !TryComp<CursedIceBootsComponent>(ent.Comp.Source, out var boots) ||
            !boots.Enabled)
        {
            return;
        }

        ent.Comp.NextTrail = _timing.CurTime + boots.TrailInterval;
        Spawn(boots.TrailPrototype, args.OldPosition);
    }

    private void OnGodslayerEquipped(Entity<GodslayerArmorComponent> ent, ref GotEquippedEvent args)
    {
        if (!args.SlotFlags.HasFlag(SlotFlags.OUTERCLOTHING))
            return;

        ent.Comp.Wearer = args.EquipTarget;
        EnsureComp<GodslayerCarrierComponent>(args.EquipTarget).Armor = ent;
    }

    private void OnGodslayerUnequipped(Entity<GodslayerArmorComponent> ent, ref GotUnequippedEvent args)
    {
        if (ent.Comp.Wearer != args.EquipTarget)
            return;

        if (TryComp<GodslayerCarrierComponent>(args.EquipTarget, out var carrier) && carrier.Armor == ent.Owner)
            RemCompDeferred<GodslayerCarrierComponent>(args.EquipTarget);
        ent.Comp.Wearer = null;
    }

    private void OnGodslayerExamined(Entity<GodslayerArmorComponent> ent, ref ExaminedEvent args)
    {
        var remaining = Math.Max(0, (ent.Comp.NextRevival - _timing.CurTime).TotalSeconds);
        args.PushMarkup(Loc.GetString(
            remaining <= 0 ? "godslayer-revival-ready" : "godslayer-revival-cooldown",
            ("seconds", (int) Math.Ceiling(remaining))));
    }

    private void OnGodslayerStateChanged(Entity<GodslayerCarrierComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState is not (MobState.Critical or MobState.Dead) ||
            !TryComp<GodslayerArmorComponent>(ent.Comp.Armor, out var armor) ||
            armor.Wearer != ent.Owner ||
            armor.NextRevival > _timing.CurTime ||
            !HasComp<DamageableComponent>(ent))
        {
            return;
        }

        armor.NextRevival = _timing.CurTime + armor.Cooldown;
        _damage.ClearAllDamage(ent.Owner);
        _mobState.ChangeMobState(ent, MobState.Alive, origin: ent.Comp.Armor);
        _popup.PopupEntity(Loc.GetString("godslayer-revival-triggered"), ent, ent, PopupType.LargeCaution);
        _adminLog.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(ent):player} was revived by {ToPrettyString(ent.Comp.Armor):item}");
    }
}
