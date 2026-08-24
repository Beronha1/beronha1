// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using System.Collections.Generic;
using Content.Shared.Humanoid;
using Content.Shared.Alert;
using System.Linq;
using Content.Shared._Starlight.Bluespace;
using Content.Shared.Examine;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Damage;
using Robust.Shared.Timing;
using Robust.Shared.Prototypes;
using Content.Shared.Actions;
using Content.Shared.Station;
using Content.Shared.Popups;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Inventory;
using Content.Shared.Tag;
using Robust.Shared.Random;
using Content.Shared.Damage.Systems;
using Content.Shared.Ensnaring;
using Robust.Shared.Audio.Systems;
using Content.Shared.StatusEffectNew;
using Content.Shared.Mobs.Components;
using Robust.Shared.Map.Components;
using Content.Shared._Starlight.Medical.Body.Events;
using Robust.Shared.Containers;
using Content.Shared._Starlight.Shadekin.Components;
using Content.Shared._Starlight.Overlay.Components;
using Content.Shared._Starlight.NullSpace.Components;
using Content.Trauma.Common.Language.Systems;
using Content.Shared._Starlight.NullSpace.Systems;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Network;
using Content.Shared.Overlays;
using Content.Shared._Starlight.Light;

namespace Content.Shared._Starlight.Shadekin;

public sealed partial class ShadekinSystem : EntitySystem
{
    [Dependency] private MantoDeSombraSystem _manto = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private MovementSpeedModifierSystem _speed = default!;
    [Dependency] private SharedActionsSystem _actionsSystem = default!;
    [Dependency] private SharedStationSystem _station = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private HumanoidProfileSystem _humanoidProfile = default!;
    [Dependency] private BodySystem _bodySystem = default!;
    [Dependency] private InventorySystem _inventorySystem = default!;
    [Dependency] private TagSystem _tag = default!;
    [Dependency] private SharedMapSystem _mapSystem = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private NullSpacePhaseSystem _nullspace = default!;
    [Dependency] private SharedStunSystem _stunSystem = default!;
    [Dependency] private SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private SharedEnsnareableSystem _ensnareable = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private StatusEffectsSystem _status = default!;
    [Dependency] private SharedGameTicker _gameTicker = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private ExamineSystemShared _examine = default!;
    [Dependency] private CommonLanguageSystem _language = default!;
    [Dependency] private SharedPointLightSystem _pointLight = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private IPrototypeManager _prototype = default!;

    [Dependency] private EntityQuery<DarkLightComponent> _darkLightQuery = default!;
    [Dependency] private EntityQuery<ShadegenAffectedComponent> _shadegenAffected = default!;

    private static readonly ProtoId<TagPrototype> _theDarkTag = "TheDark";
    private static readonly ProtoId<TagPrototype> _coreTag = "ShadekinCore";
    private static readonly ProtoId<TagPrototype> _damagedCoreTag = "DamagedShadekinCore";
    private static readonly ProtoId<DamageTypePrototype> _heatType = "Heat";
    private static readonly ProtoId<DamageTypePrototype> _cellularType = "Cellular";
    private static readonly EntProtoId<GameRuleComponent> _theDarkMap = "TheDarkMap";
    private TimeSpan _nextUpdate = TimeSpan.Zero;
    private readonly TimeSpan _updateCooldown = TimeSpan.FromSeconds(1f);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OrganShadekinCoreComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<OrganShadekinCoreComponent, OrganAddedToBodyEvent>(CoreOrganInit);

        SubscribeLocalEvent<ShadekinComponent, ComponentShutdown>((ent, ref _) =>
        {
            if (_timing.ApplyingState)
                return;

            RemComp<BrighteyeComponent>(ent);
        });
        SubscribeLocalEvent<ShadekinComponent, EyeColorInitEvent>(OnEyeColorChange);
        SubscribeLocalEvent<ShadekinComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeedModifiers);
        SubscribeLocalEvent<ShadekinComponent, NullSpaceShuntEvent>(NullSpaceShunt);
        SubscribeLocalEvent<ShadekinComponent, BeforeDamageChangedEvent>((_, ref args) => args.Damage.DamageDict["Asphyxiation"] = 0);

        InitializeBrighteye();
        InitializeAbilities();
    }

    private void CoreOrganInit(EntityUid uid, OrganShadekinCoreComponent component, OrganAddedToBodyEvent args)
        => component.OrganOwner ??= args.Body;

    private void OnExamined(EntityUid uid, OrganShadekinCoreComponent component, ref ExaminedEvent args)
    {
        if (!component.Damaged)
            args.PushMarkup(Loc.GetString("shadekin-core-undamaged"));

        if (component.OrganOwner == args.Examiner)
            args.PushMarkup(Loc.GetString("shadekin-core-owner"));
    }

    private void OnEyeColorChange(EntityUid uid, ShadekinComponent component, EyeColorInitEvent args)
    {
        // No Starlight isto desligava o brilho do olho na criação do personagem.
        // A Whiskey não tem EyeGlowing, e o EyeColorInitEvent aqui é coto, então
        // este tratador nunca roda. Fica declarado para o porte continuar
        // parecido com a origem, o que facilita atualizar depois.
    }

    private void NullSpaceShunt(EntityUid uid, ShadekinComponent component, NullSpaceShuntEvent args)
    {
        if (TryComp<BodyComponent>(uid, out var body) && _bodySystem.TryGetOrgansWithComponent<OrganShadekinCoreComponent>((uid, body), out _)) // Wizden
        {
            // TODO STARLIGHT predict this properly, right now all callers are on server
            if (_net.IsServer)
                _popup.PopupEntity(Loc.GetString("shadekin-shunt"), uid, uid, PopupType.LargeCaution);

            _stunSystem.TryKnockdown(uid, TimeSpan.FromSeconds(1), autoStand: false);
            ApplyCoreDamage(uid, 5);
        }
    }

    public void UpdateAlert(EntityUid uid, ShadekinComponent component, short state)
        => _alerts.ShowAlert(uid, component.ShadekinAlert, state);

    private Angle GetAngle(EntityUid lightUid, SharedPointLightComponent lightComp, EntityUid targetUid)
    {
        var (lightPos, lightRot) = _transform.GetWorldPositionRotation(lightUid);
        lightPos += lightRot.RotateVec(lightComp.Offset);
        var targetPos = _transform.GetWorldPosition(targetUid);
        var mapDiff = targetPos - lightPos;

        if (MathHelper.CloseTo(mapDiff.LengthSquared(), 0f))
            return Angle.Zero;

        var maskRotation = SharedPointLightSystem.GetMaskWorldRotation(lightComp, lightRot);
        return mapDiff.ToWorldAngle() - maskRotation;
    }

    /// <summary>
    /// Return an illumination float value with is how many "energy" of light is hitting our ent.
    /// WARNING: This function might be expensive, Avoid calling it too much and CACHE THE RESULT!
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public float GetLightExposure(EntityUid uid)
    {
        // TODO STARLIGHT replace this with RobustToolbox's LightLevelSystem
        var illumination = 0f;

        var xform = Transform(uid);
        var shadeQuery = _lookup.GetEntitiesInRange<ShadegenComponent>(xform.Coordinates, 10); // Why 10 when theres different ranges? because light check does not go above 20.

        foreach (var shadegen in shadeQuery)
            if (_transform.InRange(xform.Coordinates, Transform(shadegen.Owner).Coordinates, shadegen.Comp.Range))
                return illumination;

        var lightQuery = _lookup.GetEntitiesInRange<SLPointLightComponent>(xform.Coordinates, 10, LookupFlags.All | LookupFlags.Approximate);

        foreach (var light in lightQuery)
        {
            if (_darkLightQuery.HasComp(light.Owner) || _shadegenAffected.HasComp(light.Owner))
                continue;

            SharedPointLightComponent? lightComp = null;
            if (!_pointLight.ResolveLight(light, ref lightComp))
                continue;

            if (!lightComp.Enabled
                || lightComp.Radius < 1
                || lightComp.Energy <= 0)
                continue;

            // Check if our entity is in a container with OccludesLight, if yes, is it the same as the light?
            if (_container.TryGetContainingContainer(uid, out var uidcontainer) && uidcontainer.OccludesLight && !_container.IsInSameOrNoContainer(uid, light.Owner))
                continue;

            // Same as above but this time we check the light entity instead of our entity.
            if (_container.TryGetContainingContainer(light.Owner, out var lightcontainer) && lightcontainer.OccludesLight && !_container.IsInSameOrNoContainer(uid, light.Owner))
                continue;

            if (!_examine.InRangeUnOccluded(light, uid, lightComp.Radius))
                continue;

            xform.Coordinates.TryDistance(EntityManager, Transform(light).Coordinates, out var dist);

            var denom = dist / lightComp.Radius;
            var attenuation = 1 - (denom * denom);
            var calculatedLight = 0f;

            if (_prototype.TryIndex(lightComp.LightMask, out var mask))
            {
                var angleToTarget = GetAngle(light, lightComp, uid);
                foreach (var cone in mask.LightCones)
                {
                    var angleOffset = Math.Abs(Angle.ShortestDistance(angleToTarget, cone.Direction));

                    if (angleOffset > cone.OuterWidth)
                        continue;

                    var coneLight = lightComp.Energy * attenuation * attenuation;
                    if (angleOffset > cone.InnerWidth)
                    {
                        var angleAttenuation = (float) ((cone.OuterWidth - angleOffset) /
                            (cone.OuterWidth - cone.InnerWidth));
                        coneLight *= angleAttenuation;
                    }

                    calculatedLight = Math.Max(calculatedLight, coneLight);
                }
            }
            else
                calculatedLight = lightComp.Energy * attenuation * attenuation;

            illumination += calculatedLight; //Math.Max(illumination, calculatedLight);
        }

        return illumination;
    }

    /// <summary>
    ///     Ajusta a cura passiva conforme a luz.
    ///
    ///     Whiskey: o original mexia no campo Interval, que ninguém lê, porque o
    ///     PassiveDamageSystem crava um segundo. Então "cura mais rápida no escuro"
    ///     não fazia nada. Agora quem muda é a quantidade, que tem efeito de verdade
    ///     e não exige tocar em sistema base do jogo.
    ///
    ///     E o ramo de luz forte estava VAZIO, ou seja a cura nunca era desligada e o
    ///     Shadekin se curava debaixo de holofote. Agora a lista de estados fica vazia.
    /// </summary>
    private void SetPassiveBuff(EntityUid uid, ShadekinComponent component, ShadekinState shadekinState)
    {
        if (!TryComp<PassiveDamageComponent>(uid, out var passive))
            return;

        // guarda o valor que veio do prototype na primeira passada
        component.CuraBase ??= new DamageSpecifier(passive.Damage);

        switch (shadekinState)
        {
            case ShadekinState.Annoying:
            case ShadekinState.High:
            case ShadekinState.Extreme:
                passive.AllowedStates = new List<MobState>();
                break;

            case ShadekinState.Low:
                passive.AllowedStates = new List<MobState> { MobState.Alive };
                passive.Damage = new DamageSpecifier(component.CuraBase);
                break;

            case ShadekinState.Dark:
                passive.AllowedStates = new List<MobState> { MobState.Alive, MobState.Critical, MobState.Dead };
                passive.Damage = component.CuraBase * 2f;
                break;
        }

        Dirty(uid, passive);
    }

    private void ApplyLightDamage(EntityUid uid, float dmg)
    {
        var damage = new DamageSpecifier();
        damage.DamageDict.Add(_heatType, dmg);
        _damageable.TryChangeDamage(uid, damage, true, false);
    }

    private void ApplyCoreDamage(EntityUid uid, float dmg)
    {
        var damage = new DamageSpecifier();
        damage.DamageDict.Add(_cellularType, dmg);
        _damageable.TryChangeDamage(uid, damage, false, false);
    }

    private void OnRefreshMovementSpeedModifiers(EntityUid uid, ShadekinComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (component.CurrentState is ShadekinState.High or ShadekinState.Extreme)
        {
            if (!TryComp<MovementSpeedModifierComponent>(uid, out var movement))
                return;

            var sprintDif = movement.BaseWalkSpeed / movement.BaseSprintSpeed;
            args.ModifySpeed(1f, sprintDif);
        }
    }

    private void ToggleNightVision(EntityUid uid, ShadekinState shadekinState)
    {
        var nightVision = EnsureComp<NightVisionComponent>(uid);
        var shouldBeActive = shadekinState == ShadekinState.Dark;

        // avoid dirtying if we don't need to
        if(nightVision.Enabled == shouldBeActive)
            return;

        // update whether or not nightVision should be active based on light level
        nightVision.Enabled = shouldBeActive;

        // ensure nightVision updates to reflect the new state
        Dirty(uid, nightVision);
    }

    private void CheckThresholds(EntityUid uid, ShadekinComponent component, float lightExposure)
    {
        foreach (var (threshold, shadekinState) in component.Thresholds.Reverse())
        {
            var selectedstate = shadekinState;
            if (lightExposure < threshold)
            {
                if (selectedstate == ShadekinState.Low) // If Low is below the threshold, then we auto-jump to Dark.
                    selectedstate = ShadekinState.Dark;
                else
                    continue;
            }

            component.CurrentState = selectedstate;
            UpdateAlert(uid, component, (short)selectedstate);
            Dirty(uid, component);
            break;
        }
    }

    /// <summary>
    /// Makes a simple check to see if the ent is in the dark.
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public bool AreWeInTheDark(EntityUid uid)
    {
        var mapUid = Transform(uid).MapUid;
        if (mapUid is not null && _tag.HasTag(mapUid.Value, _theDarkTag))
            return true;

        return false;
    }

    /// <summary>
    /// Spawn "The Dark"
    /// </summary>
    public void SpawnTheDark()
    {
        var query = EntityQueryEnumerator<MapComponent>();
        while (query.MoveNext(out var mapuid, out var mapcomp))
        {
            if (mapcomp.MapPaused)
                continue;

            if (_tag.HasTag(mapuid, _theDarkTag))
                return;
        }
        // Whiskey: iniciar regra de jogo é do GameTicker do servidor, e o
        // SharedGameTicker daqui não expõe isso. Levantamos evento e quem
        // resolve é o GarantirEscuridaoSystem, no lado servidor.
        var ev = new GarantirEscuridaoEvent(_theDarkMap);
        RaiseLocalEvent(ref ev);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        var query = EntityQueryEnumerator<ShadekinComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (_timing.CurTime < component.NextUpdate)
                continue;

            component.NextUpdate = _timing.CurTime + component.UpdateCooldown;

            var lightExposure = 0f;

            if (HasComp<NullSpaceComponent>(uid) || AreWeInTheDark(uid)) // Were in NullSpace, NullSpace is dark... and "The Dark" is dark too!
            {
                // I had a brain moment, apprently if one is false its does not check for the other?
            }
            else
                lightExposure = GetLightExposure(uid);

            CheckThresholds(uid, component, lightExposure);

            ToggleNightVision(uid, component.CurrentState);
            SetPassiveBuff(uid, component, component.CurrentState);

            // Whiskey: o manto de sombra acompanha o mesmo estado de luz.
            if (TryComp<MantoDeSombraComponent>(uid, out var manto))
                _manto.Atualizar(uid, manto, component.CurrentState, lightExposure);
            _speed.RefreshMovementSpeedModifiers(uid);

            // Whiskey: o dano direto por luz foi retirado por decisão da administração.
            // O peso da luz agora é parar de curar e perder a corrida.

            if (TryComp<BrighteyeComponent>(uid, out var brighteye))
                UpdateEnergy(uid, component, brighteye);
        }

        // The Dark Effects - This only applies for Ents that are IN THE DARK.
        if (_timing.CurTime > _nextUpdate)
        {
            _nextUpdate = _timing.CurTime + _updateCooldown;

            var thedarkmobquery = EntityQueryEnumerator<MobStateComponent>();
            while (thedarkmobquery.MoveNext(out var uid, out var _))
            {
                var remove = false;

                if (_status.HasStatusEffect(uid, "StatusEffectTheDarkMap"))
                {
                    if (HasComp<ShadekinComponent>(uid) || HasComp<TheDarkImmuneComponent>(uid))
                        remove = true;

                    if (!remove)
                        foreach (var entity in _lookup.GetEntitiesIntersecting(Transform(uid).Coordinates))
                            if (TryComp<TheDarkImmuneComponent>(entity, out var blocker) && blocker.Ranged)
                                remove = true;
                }

                if (AreWeInTheDark(uid) && !remove)
                    _status.TrySetStatusEffectDuration(uid, "StatusEffectTheDarkMap");
                else
                    _status.TryRemoveStatusEffect(uid, "StatusEffectTheDarkMap");
            }
        }
    }
}
