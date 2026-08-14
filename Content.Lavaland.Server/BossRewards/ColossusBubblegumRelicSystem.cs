// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Lavaland.Shared.Artifacts;
using Content.Lavaland.Shared.Megafauna.Harvesting;
using Content.Server.Administration.Logs;
using Content.Server.Traits.Assorted;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Gibbing;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Traits.Assorted;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Artifacts;

/// <summary>
/// Server-authoritative mechanics for the Colossus and Bubblegum relics imported from /tg/.
/// </summary>
public sealed partial class ColossusBubblegumRelicSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private IAdminLogManager _adminLog = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private GibbingSystem _gibbing = default!;
    [Dependency] private SharedGunSystem _gun = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MobThresholdSystem _threshold = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private ParacusiaSystem _paracusia = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    private TimeSpan _nextUpdate;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MayhemBottleComponent, UseInHandEvent>(OnMayhemUse);
        SubscribeLocalEvent<MayhemFrenzyComponent, RefreshMovementSpeedModifiersEvent>(OnFrenzySpeed);
        SubscribeLocalEvent<MayhemFrenzyComponent, AttemptMeleeEvent>(OnFrenzyAttack);
        SubscribeLocalEvent<DamageableComponent, DamageModifyEvent>(OnDamageModify);

        SubscribeLocalEvent<CainAbelComponent, MeleeHitEvent>(OnCainAbelHit);
        SubscribeLocalEvent<CainAbelWispActionEvent>(OnCainAbelWisps);
        SubscribeLocalEvent<SoulScytheComponent, MapInitEvent>(OnSoulScytheInit);
        SubscribeLocalEvent<SoulScytheComponent, MeleeHitEvent>(OnSoulScytheHit);
        SubscribeLocalEvent<SoulScytheComponent, ExaminedEvent>(OnSoulScytheExamine);
        SubscribeLocalEvent<SoulScytheWaveActionEvent>(OnSoulScytheWave);

        SubscribeLocalEvent<HeckSuitComponent, GotEquippedEvent>(OnHeckSuitEquipped);
        SubscribeLocalEvent<HeckSuitComponent, GotUnequippedEvent>(OnHeckSuitUnequipped);
        SubscribeLocalEvent<MobStateComponent, InteractHandEvent>(OnCorpseInteract);
        SubscribeLocalEvent<MobStateComponent, HeckConsumeCorpseDoAfterEvent>(OnCorpseConsumed);

        SubscribeLocalEvent<BloodContractActionEvent>(OnBloodContract);
        SubscribeLocalEvent<BloodContractMarkComponent, MobStateChangedEvent>(OnBloodContractTargetStateChanged);
        SubscribeLocalEvent<BloodContractMarkComponent, ExaminedEvent>(OnBloodContractMarkExamined);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime < _nextUpdate)
            return;

        _nextUpdate = _timing.CurTime + TimeSpan.FromSeconds(1);
        UpdateFrenzies();
        UpdateRelics();
        UpdateBloodContractMarks();
    }

    private void OnMayhemUse(Entity<MayhemBottleComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        if (_timing.CurTime >= ent.Comp.ArmedUntil)
        {
            ent.Comp.ArmedUntil = _timing.CurTime + ent.Comp.ConfirmWindow;
            _popup.PopupClient(Loc.GetString("mayhem-bottle-confirm"), ent, args.User, PopupType.LargeCaution);
            return;
        }

        var affected = 0;
        foreach (var target in _lookup.GetEntitiesInRange<HumanoidProfileComponent>(Transform(args.User).Coordinates, ent.Comp.Radius))
        {
            if (_mobState.IsDead(target))
                continue;

            var frenzy = EnsureComp<MayhemFrenzyComponent>(target);
            frenzy.EndTime = _timing.CurTime + ent.Comp.FrenzyDuration;
            frenzy.LastViolence = _timing.CurTime;
            frenzy.NextAgony = _timing.CurTime + frenzy.ViolenceGrace;
            if (frenzy.AgonyDamage.Empty)
            {
                frenzy.AgonyDamage = new DamageSpecifier
                {
                    DamageDict = { ["Bloodloss"] = 3 },
                };
            }
            _movement.RefreshMovementSpeedModifiers(target.Owner);
            affected++;
        }

        _audio.PlayPvs("/Audio/Effects/glass_break1.ogg", args.User);
        _adminLog.Add(
            LogType.Action,
            LogImpact.High,
            $"{ToPrettyString(args.User):player} shattered {ToPrettyString(ent):item}, applying mayhem to {affected} humanoids");
        _popup.PopupEntity(Loc.GetString("mayhem-bottle-shattered", ("affected", affected)), args.User, args.User, PopupType.LargeCaution);
        QueueDel(ent);
    }

    private void OnFrenzySpeed(Entity<MayhemFrenzyComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(ent.Comp.MovementMultiplier, ent.Comp.MovementMultiplier);
    }

    private void OnFrenzyAttack(Entity<MayhemFrenzyComponent> ent, ref AttemptMeleeEvent args)
    {
        ent.Comp.LastViolence = _timing.CurTime;
    }

    private void OnDamageModify(Entity<DamageableComponent> ent, ref DamageModifyEvent args)
    {
        if (TryComp<BloodContractMarkComponent>(ent, out var mark) && args.Damage.GetTotal() > 0)
            args.Damage *= mark.IncomingDamageMultiplier;

        if (args.Origin is not { } origin || origin == ent.Owner ||
            !TryComp<MayhemFrenzyComponent>(origin, out var frenzy) ||
            args.Damage.GetTotal() <= 0)
        {
            return;
        }

        args.Damage *= frenzy.DamageMultiplier;
        frenzy.LastViolence = _timing.CurTime;
    }

    private void UpdateFrenzies()
    {
        var query = EntityQueryEnumerator<MayhemFrenzyComponent>();
        while (query.MoveNext(out var uid, out var frenzy))
        {
            if (_timing.CurTime >= frenzy.EndTime)
            {
                RemCompDeferred<MayhemFrenzyComponent>(uid);
                _movement.RefreshMovementSpeedModifiers(uid);
                continue;
            }

            if (_timing.CurTime < frenzy.LastViolence + frenzy.ViolenceGrace ||
                _timing.CurTime < frenzy.NextAgony || frenzy.AgonyDamage.Empty)
            {
                continue;
            }

            _damage.TryChangeDamage(uid, frenzy.AgonyDamage, origin: uid);
            frenzy.NextAgony = _timing.CurTime + TimeSpan.FromSeconds(2);
            _popup.PopupEntity(Loc.GetString("mayhem-frenzy-demands-violence"), uid, uid);
        }
    }

    private void OnCainAbelHit(Entity<CainAbelComponent> ent, ref MeleeHitEvent args)
    {
        if (!HasLivingHit(args.HitEntities))
            return;

        if (ent.Comp.Combo > 0)
        {
            var multiplier = MathF.Pow(ent.Comp.DamageMultiplierPerCombo, ent.Comp.Combo);
            args.BonusDamage += args.BaseDamage * (multiplier - 1f);
        }

        ent.Comp.Combo = Math.Min(ent.Comp.MaxCombo, ent.Comp.Combo + 1);
        ent.Comp.ComboExpires = _timing.CurTime + ent.Comp.ComboTimeout;
    }

    private void OnCainAbelWisps(CainAbelWispActionEvent args)
    {
        if (args.Handled || !TryGetRelic<CainAbelComponent>(args.Performer, out var relic))
            return;

        var (uid, component) = relic;
        if (component.Combo <= 0)
        {
            _popup.PopupClient(Loc.GetString("cain-abel-no-wisps"), uid, args.Performer);
            return;
        }

        var origin = _transform.GetMapCoordinates(uid);
        var target = _transform.ToMapCoordinates(args.Target);
        if (origin.MapId != target.MapId)
            return;

        var direction = target.Position - origin.Position;
        for (var i = 0; i < component.Combo; i++)
        {
            var projectile = Spawn(component.WispProjectile, Transform(uid).Coordinates);
            var spread = new Angle(_random.NextFloat(-0.12f, 0.12f)).RotateVec(direction);
            _gun.ShootProjectile(projectile, spread, Vector2.Zero, uid, args.Performer, component.ProjectileSpeed);
        }

        component.Combo = 0;
        _actions.SetCooldown(args.Action.Owner, TimeSpan.FromSeconds(2));
        args.Handled = true;
    }

    private void OnSoulScytheInit(Entity<SoulScytheComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.Blood = ent.Comp.StartingBlood;
        ent.Comp.LastUpdate = _timing.CurTime;
    }

    private void OnSoulScytheHit(Entity<SoulScytheComponent> ent, ref MeleeHitEvent args)
    {
        if (!HasLivingHit(args.HitEntities))
            return;

        ent.Comp.Blood = Math.Min(ent.Comp.MaxBlood, ent.Comp.Blood + ent.Comp.BloodPerHit);
        if (ent.Comp.Blood < ent.Comp.EmpoweredHitCost || ent.Comp.EmpoweredHitDamage.Empty)
            return;

        ent.Comp.Blood -= ent.Comp.EmpoweredHitCost;
        args.BonusDamage += ent.Comp.EmpoweredHitDamage;
    }

    private void OnSoulScytheExamine(Entity<SoulScytheComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(
            "soulscythe-blood-examine",
            ("blood", MathF.Round(ent.Comp.Blood)),
            ("maximum", MathF.Round(ent.Comp.MaxBlood))));
    }

    private void OnSoulScytheWave(SoulScytheWaveActionEvent args)
    {
        if (args.Handled || !TryGetRelic<SoulScytheComponent>(args.Performer, out var relic))
            return;

        var (uid, component) = relic;
        if (component.Blood < component.WaveCost)
        {
            _popup.PopupClient(Loc.GetString("soulscythe-not-enough-blood"), uid, args.Performer);
            return;
        }

        var origin = _transform.GetMapCoordinates(uid);
        var target = _transform.ToMapCoordinates(args.Target);
        if (origin.MapId != target.MapId)
            return;

        component.Blood -= component.WaveCost;
        var projectile = Spawn(component.WaveProjectile, Transform(uid).Coordinates);
        _gun.ShootProjectile(projectile, target.Position - origin.Position, Vector2.Zero, uid, args.Performer, component.ProjectileSpeed);
        _actions.SetCooldown(args.Action.Owner, TimeSpan.FromSeconds(3));
        args.Handled = true;
    }

    private void UpdateRelics()
    {
        var cainQuery = EntityQueryEnumerator<CainAbelComponent>();
        while (cainQuery.MoveNext(out _, out var cain))
        {
            if (cain.Combo > 0 && _timing.CurTime >= cain.ComboExpires)
                cain.Combo = 0;
        }

        var soulQuery = EntityQueryEnumerator<SoulScytheComponent>();
        while (soulQuery.MoveNext(out _, out var soul))
        {
            var elapsed = (_timing.CurTime - soul.LastUpdate).TotalSeconds;
            soul.LastUpdate = _timing.CurTime;
            soul.Blood = Math.Min(soul.MaxBlood, soul.Blood + soul.BloodRegenPerSecond * (float) elapsed);
        }
    }

    private void OnHeckSuitEquipped(Entity<HeckSuitComponent> ent, ref GotEquippedEvent args)
    {
        if (!args.SlotFlags.HasFlag(SlotFlags.OUTERCLOTHING))
            return;

        var carrier = EnsureComp<HeckCurseCarrierComponent>(args.EquipTarget);
        carrier.Source = ent;
        carrier.AddedParacusia = !TryComp<ParacusiaComponent>(args.EquipTarget, out var paracusia);

        if (carrier.AddedParacusia)
        {
            paracusia = EnsureComp<ParacusiaComponent>(args.EquipTarget);
            _paracusia.SetSounds(args.EquipTarget, ent.Comp.CurseSounds, paracusia);
            _paracusia.SetTime(args.EquipTarget,
                ent.Comp.MinTimeBetweenIncidents,
                ent.Comp.MaxTimeBetweenIncidents,
                paracusia);
            _paracusia.SetDistance(args.EquipTarget, ent.Comp.MaxSoundDistance, paracusia);
        }

        _popup.PopupEntity(Loc.GetString("heck-suit-curse-equipped"), args.EquipTarget, args.EquipTarget, PopupType.MediumCaution);
    }

    private void OnHeckSuitUnequipped(Entity<HeckSuitComponent> ent, ref GotUnequippedEvent args)
    {
        if (!TryComp<HeckCurseCarrierComponent>(args.EquipTarget, out var carrier) || carrier.Source != ent.Owner)
            return;

        if (carrier.AddedParacusia)
            RemComp<ParacusiaComponent>(args.EquipTarget);

        RemCompDeferred<HeckCurseCarrierComponent>(args.EquipTarget);
        _popup.PopupEntity(Loc.GetString("heck-suit-curse-removed"), args.EquipTarget, args.EquipTarget);
    }

    private void OnCorpseInteract(Entity<MobStateComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled || !_mobState.IsDead(ent) || args.User == ent.Owner ||
            HasComp<MegafaunaHarvestableComponent>(ent) || HasComp<HeckCorpseReservationComponent>(ent) ||
            !TryComp<InjurableComponent>(ent, out var injurable) ||
            injurable.DamageContainer is not { } container || container.Id != "Biological" ||
            !_inventory.TryGetSlotEntity(args.User, "head", out var helmetUid) || helmetUid is not { } helmet ||
            !TryComp<HeckHelmetComponent>(helmet, out var heckHelmet) ||
            !HasComp<DamageableComponent>(args.User))
        {
            return;
        }

        var reservation = EnsureComp<HeckCorpseReservationComponent>(ent);
        reservation.User = args.User;
        reservation.Helmet = helmet;

        var doAfter = new DoAfterArgs(
            EntityManager,
            args.User,
            heckHelmet.ConsumeDuration,
            new HeckConsumeCorpseDoAfterEvent(),
            ent,
            target: ent,
            used: helmet)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = false,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
        {
            RemComp<HeckCorpseReservationComponent>(ent);
            return;
        }

        args.Handled = true;
        _popup.PopupEntity(Loc.GetString("heck-helmet-consuming", ("corpse", ent.Owner)), ent, args.User, PopupType.MediumCaution);
    }

    private void OnCorpseConsumed(Entity<MobStateComponent> ent, ref HeckConsumeCorpseDoAfterEvent args)
    {
        if (!TryComp<HeckCorpseReservationComponent>(ent, out var reservation))
            return;

        RemComp<HeckCorpseReservationComponent>(ent);
        if (args.Cancelled || args.Handled || args.Used != reservation.Helmet || args.User != reservation.User ||
            !_mobState.IsDead(ent) || HasComp<MegafaunaHarvestableComponent>(ent) ||
            !_inventory.TryGetSlotEntity(args.User, "head", out var equipped) || equipped != reservation.Helmet ||
            !TryComp<HeckHelmetComponent>(reservation.Helmet, out var helmet) ||
            !TryComp<DamageableComponent>(args.User, out var userDamage) ||
            !_threshold.TryGetThresholdForState(ent, Content.Shared.Mobs.MobState.Dead, out var corpseMaximum))
        {
            return;
        }

        var current = _damage.GetPositiveDamage((args.User, userDamage));
        var currentTotal = current.GetTotal();
        var healAmount = corpseMaximum.Value * helmet.HealFraction;
        if (currentTotal > 0 && healAmount > 0)
        {
            var scale = (float) Math.Min(1d, healAmount.Double() / currentTotal.Double());
            _damage.TryChangeDamage(args.User, current * -scale, true, false, origin: reservation.Helmet);
        }

        _adminLog.Add(
            LogType.Action,
            LogImpact.Medium,
            $"{ToPrettyString(args.User):player} consumed {ToPrettyString(ent):corpse} with {ToPrettyString(reservation.Helmet):helmet}");
        _popup.PopupEntity(Loc.GetString("heck-helmet-consumed", ("corpse", ent.Owner)), args.User, args.User, PopupType.Medium);
        _gibbing.Gib(ent, user: args.User);
        args.Handled = true;
    }

    private void OnBloodContract(BloodContractActionEvent args)
    {
        if (args.Handled || !TryGetRelic<BloodContractComponent>(args.Performer, out var contract))
            return;

        if (args.Target == args.Performer || !HasComp<HumanoidProfileComponent>(args.Target) ||
            _mobState.IsDead(args.Target) || HasComp<BloodContractMarkComponent>(args.Target))
        {
            _popup.PopupClient(Loc.GetString("blood-contract-invalid-target"), contract, args.Performer, PopupType.MediumCaution);
            return;
        }

        var mark = EnsureComp<BloodContractMarkComponent>(args.Target);
        mark.Source = args.Performer;
        mark.IncomingDamageMultiplier = contract.Comp.IncomingDamageMultiplier;
        mark.PulseDamage = new DamageSpecifier(contract.Comp.PulseDamage);
        mark.PulseInterval = contract.Comp.PulseInterval;
        mark.RewardPrototype = contract.Comp.RewardPrototype;
        mark.ExpiresAt = _timing.CurTime + contract.Comp.MarkDuration;
        mark.NextPulse = _timing.CurTime + contract.Comp.PulseInterval;

        _popup.PopupEntity(Loc.GetString("blood-contract-target-marked"), args.Target, args.Target, PopupType.LargeCaution);
        _popup.PopupClient(Loc.GetString("blood-contract-user-marked", ("target", args.Target)), contract, args.Performer, PopupType.Medium);
        _adminLog.Add(
            LogType.Action,
            LogImpact.High,
            $"{ToPrettyString(args.Performer):player} used {ToPrettyString(contract):contract} to mark {ToPrettyString(args.Target):target} for death");

        args.Handled = true;
        QueueDel(contract);
    }

    private void OnBloodContractTargetStateChanged(Entity<BloodContractMarkComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != Content.Shared.Mobs.MobState.Dead)
            return;

        Spawn(ent.Comp.RewardPrototype, Transform(ent).Coordinates);
        _adminLog.Add(
            LogType.Action,
            LogImpact.Medium,
            $"{ToPrettyString(ent):target} died under a blood contract from {ToPrettyString(ent.Comp.Source):source}");
        RemCompDeferred<BloodContractMarkComponent>(ent);
    }

    private void OnBloodContractMarkExamined(Entity<BloodContractMarkComponent> ent, ref ExaminedEvent args)
    {
        var remaining = Math.Max(0, (ent.Comp.ExpiresAt - _timing.CurTime).TotalSeconds);
        args.PushMarkup(Loc.GetString("blood-contract-mark-examine", ("seconds", (int) Math.Ceiling(remaining))));
    }

    private void UpdateBloodContractMarks()
    {
        var query = EntityQueryEnumerator<BloodContractMarkComponent>();
        while (query.MoveNext(out var uid, out var mark))
        {
            if (_timing.CurTime >= mark.ExpiresAt)
            {
                _popup.PopupEntity(Loc.GetString("blood-contract-mark-expired"), uid, uid);
                RemCompDeferred<BloodContractMarkComponent>(uid);
                continue;
            }

            if (_timing.CurTime < mark.NextPulse || mark.PulseDamage.Empty || _mobState.IsDead(uid))
                continue;

            mark.NextPulse = _timing.CurTime + mark.PulseInterval;
            _damage.TryChangeDamage(uid, mark.PulseDamage, origin: mark.Source);
        }
    }

    private bool HasLivingHit(IReadOnlyList<EntityUid> targets)
    {
        foreach (var target in targets)
        {
            if (HasComp<MobStateComponent>(target) && !_mobState.IsDead(target))
                return true;
        }

        return false;
    }

    private bool TryGetRelic<T>(EntityUid performer, out Entity<T> relic) where T : Component
    {
        if (TryComp<T>(performer, out var direct))
        {
            relic = (performer, direct);
            return true;
        }

        var held = _hands.GetActiveItemOrSelf(performer);
        if (held != performer && TryComp<T>(held, out var component))
        {
            relic = (held, component);
            return true;
        }

        relic = default;
        return false;
    }
}
