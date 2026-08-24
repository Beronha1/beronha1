// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Content.Shared.Rejuvenate;
using Content.Shared.Popups;
using Content.Shared._Starlight.Medical.Surgery.Events;
using Content.Shared.Body.Components;
using Content.Shared.Mobs;
using Content.Shared.Inventory;
using Content.Shared.Zombies;
using Content.Shared._Starlight.Bluespace;
using Content.Shared.Mindshield.Components;
using Content.Shared._Starlight.Shadekin.Components;
using Content.Shared._Starlight.Station;
using Content.Shared.Cargo.Components;
using Content.Shared.Spawners.Components;
using Content.Shared.Body;
using Content.Shared.Humanoid;

namespace Content.Shared._Starlight.Shadekin;

public sealed partial class ShadekinSystem
{
    public void InitializeBrighteye()
    {
        SubscribeLocalEvent<BrighteyeComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<BrighteyeComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<BrighteyeComponent, RejuvenateEvent>(OnRejuvenate);
        SubscribeLocalEvent<BrighteyeComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<BrighteyeComponent, NullSpaceShuntEvent>(NullSpaceShunt);
        SubscribeLocalEvent<BrighteyeComponent, EntityZombifiedEvent>((uid, _, _) => RemComp<BrighteyeComponent>(uid));
        SubscribeLocalEvent<MindShieldComponent, ComponentStartup>(MindShieldImplanted);
        SubscribeLocalEvent<BrighteyeComponent, ForcedPrototypeDoSpecialEvent>(ForcedPrototypeDoSpecial);

        SubscribeLocalEvent<OrganShadekinCoreComponent, SurgeryOrganImplantationCompleted>(OnCoreOrganImplanted);
        SubscribeLocalEvent<OrganShadekinCoreComponent, SurgeryOrganExtracted>(OnCoreOrganExtracted);
    }

    private void OnInit(EntityUid uid, BrighteyeComponent component, ComponentStartup args)
    {
        if (!HasComp<ShadekinComponent>(uid))
        {
            RemComp<BrighteyeComponent>(uid);
            return;
        }

        RemCompDeferred<MindShieldComponent>(uid);

        // Whiskey: o Starlight dava o idioma Empathy ao Brighteye. Esse idioma não
        // existe aqui, e o sistema de conhecimento monta LanguageEmpathy a partir do
        // nome, não acha, e estoura DebugAssert. Só dispara em build Debug, que é o
        // que o CI roda, por isso passou no Release local. O Shadekin já fala Marish,
        // que é o idioma próprio dele e esse foi portado.

        _alerts.ShowAlert(uid, component.BrighteyeAlert);
        _alerts.ShowAlert(uid, component.PortalAlert);

        _actionsSystem.AddAction(uid, ref component.PortalAction, component.BrighteyePortalAction, uid);
        _actionsSystem.AddAction(uid, ref component.PhaseAction, component.BrighteyePhaseAction, uid);
        _actionsSystem.AddAction(uid, ref component.ShadeSkipAction, component.BrighteyeShadeSkipAction, uid);
        _actionsSystem.AddAction(uid, ref component.CreateShadeAction, component.BrighteyeCreateShadeAction, uid);
        _actionsSystem.AddAction(uid, ref component.DarkTrapAction, component.BrighteyeDarkTrapAction, uid);

        if (TryComp<BodyComponent>(uid, out var body))
            foreach (var core in _bodySystem.GetOrgans<OrganShadekinCoreComponent>((uid, body)))
            {
                core.Comp.Damaged = false;

                _tag.AddTag(core, _coreTag);
                _tag.RemoveTag(core, _damagedCoreTag);

                if (core.Comp.OrganOwner != uid)
                {
                    component.LesserKin = true;
                    component.MaxEnergy = 100;
                    component.PhaseCost = 100;

                    _alerts.ClearAlert(uid, component.PortalAlert);
                    _actionsSystem.RemoveAction(uid, component.PortalAction);
                    _actionsSystem.RemoveAction(uid, component.ShadeSkipAction);
                    _actionsSystem.RemoveAction(uid, component.DarkTrapAction);
                }
            }

        SetBrighteyes(uid);
    }

    private void ForcedPrototypeDoSpecial(EntityUid uid, BrighteyeComponent component, ForcedPrototypeDoSpecialEvent args)
    {
        SetBrighteyes(uid);

        if (TryComp<BodyComponent>(uid, out var body))
            foreach (var core in _bodySystem.GetOrgans<OrganShadekinCoreComponent>((uid, body)))
            {
                core.Comp.Damaged = false;
                _tag.AddTag(core, _coreTag);
                _tag.RemoveTag(core, _damagedCoreTag);
            }

        component.PortalNeedStation = false;

        RemCompDeferred<MindShieldComponent>(uid);
    }

    // ! Gosh this is bad... But there no event to get implanted shit? or mindshield? il do this for now change if need later!
    private void MindShieldImplanted(EntityUid uid, MindShieldComponent comp, ComponentStartup args)
    {
        if (HasComp<BrighteyeComponent>(uid))
            RemCompDeferred<MindShieldComponent>(uid);
    }

    private void OnCoreOrganImplanted(Entity<OrganShadekinCoreComponent> ent, ref SurgeryOrganImplantationCompleted args)
    {
        if (!ent.Comp.Damaged)
            EnsureComp<BrighteyeComponent>(args.Body);
    }

    private void OnCoreOrganExtracted(Entity<OrganShadekinCoreComponent> ent, ref SurgeryOrganExtracted args)
    {
        if (HasComp<BrighteyeComponent>(args.Body) && !ent.Comp.Damaged)
            RemComp<BrighteyeComponent>(args.Body);
    }

    private void OnShutdown(EntityUid uid, BrighteyeComponent component, ComponentShutdown args)
    {
        _alerts.ClearAlert(uid, component.BrighteyeAlert);
        _alerts.ClearAlert(uid, component.PortalAlert);
        _alerts.ClearAlert(uid, component.RejuvenationAlert);

        _actionsSystem.RemoveAction(uid, component.PortalAction);
        _actionsSystem.RemoveAction(uid, component.PhaseAction);
        _actionsSystem.RemoveAction(uid, component.ShadeSkipAction);
        _actionsSystem.RemoveAction(uid, component.CreateShadeAction);
        _actionsSystem.RemoveAction(uid, component.DarkTrapAction);

        if (component.Portal is not null)
        {
            PredictedSpawnAtPosition(component.ShadekinShadow, Transform(component.Portal.Value).Coordinates);
            PredictedQueueDel(component.Portal.Value);
        }

        if (TryComp<BodyComponent>(uid, out var body))
            foreach (var core in _bodySystem.GetOrgans<OrganShadekinCoreComponent>((uid, body)))
            {
                core.Comp.Damaged = true;

                _tag.AddTag(core, _damagedCoreTag);
                _tag.RemoveTag(core, _coreTag);
            }

        SetBlackeyes(uid);
    }

    private void OnRejuvenate(EntityUid uid, BrighteyeComponent component, RejuvenateEvent args)
    {
        component.Energy = component.MaxEnergy;
        Dirty(uid, component);
    }

    private void NullSpaceShunt(EntityUid uid, BrighteyeComponent component, NullSpaceShuntEvent args)
    {
        component.Energy = 0;
        Dirty(uid, component);
    }

    private void OnMobStateChanged(EntityUid uid, BrighteyeComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Alive)
            return;

        // We hit Crit/Death we lose energy... EVERYTIME!
        component.Energy = 0;
        Dirty(uid, component);

        // Make shit modular! (Aka for future devs, this can be used to block Rejuvenation)
        var ev = new OnBrighteyeRejuvenateAttemptEvent(uid);
        RaiseLocalEvent(uid, ev);

        if (ev.Cancelled)
            return;

        // ZombifyOnDeath? Yeah no Regen for you buddy!
        if (HasComp<ZombifyOnDeathComponent>(uid))
            return;

        // Do we have a portal? if no... WE DIE!
        if (component.Portal is null && !AreWeInTheDark(uid))
            return;

        // Whiskey: SpawnPointComponent só existe do lado servidor aqui, e este
        // arquivo é compartilhado. Quem procura destino na escuridão é o
        // DestinoNaEscuridaoSystem, do servidor. Sem destino, o Shadekin morre
        // como morreria no original.
        var pedido = new PedirDestinoNaEscuridaoEvent(_theDarkTag);
        RaiseLocalEvent(ref pedido);

        if (pedido.Destino is not { } destino)
            return;

        // First, Drop Everything we have.
        if (TryComp<InventoryComponent>(uid, out var inventoryComponent) && _inventorySystem.TryGetSlots(uid, out var slots))
            foreach (var slot in slots)
                _inventorySystem.TryUnequip(uid, slot.Name, true, true, false, inventoryComponent);

        // Spawn the Shadow.
        PredictedSpawnAtPosition(component.ShadekinShadow, Transform(uid).Coordinates);

        // Teleport to "The Dark"
        _transform.SetCoordinates(uid, destino);

        var effect = PredictedSpawnAtPosition(component.ShadekinPhaseInEffect2, Transform(uid).Coordinates);
        Transform(effect).LocalRotation = Transform(uid).LocalRotation;

        RaiseLocalEvent(uid, new RejuvenateEvent());

        component.Energy = 0;
        component.Rejuvenating = true;
        _alerts.ShowAlert(uid, component.RejuvenationAlert);
        Dirty(uid, component);
    }

    /// <summary>
    /// Change the humanoid eye to be bright and glow!
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="humanoid"></param>
    public void SetBrighteyes(EntityUid uid)
    {
        // Whiskey: cor de olho aqui não é campo do componente humanoide, e sim
        // dado do órgão, aplicado pelo HumanoidProfileSystem. E o fork não tem
        // EyeGlowing, então o olho muda de cor mas não brilha.
        if (GetEyeColor(uid) is not { } atual)
            return;

        _humanoidProfile.SetEyeColor(uid, EyeColor.MakeBrighteyeValid(atual));
    }

    /// <summary>
    /// Change the humanoid eye to be validated by HumanoidEyeColor.Shadekin (Blackeyes)
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="humanoid"></param>
    public void SetBlackeyes(EntityUid uid)
    {
        if (GetEyeColor(uid) is not { } atual)
            return;

        _humanoidProfile.SetEyeColor(uid, EyeColor.MakeShadekinValid(atual));
    }

    /// <summary>
    ///     Lê a cor do olho pelo órgão, porque na Whiskey ela vive ali e não no
    ///     componente humanoide.
    /// </summary>
    private Color? GetEyeColor(EntityUid uid)
    {
        if (_bodySystem.GetOrgan(uid, HumanoidProfileSystem.EyesCategory) is not { } olhos
            || !TryComp<VisualOrganComponent>(olhos, out var visual))
            return null;

        return visual.Profile.EyeColor;
    }

    /// <summary>
    /// When triggered, will check if we have enough energy and if yes drain the energy and return the value.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <param name="cost">cost of energy (if null then no cost needed)</param>
    /// <returns></returns>
    public bool OnAttemptEnergyUse(EntityUid uid, BrighteyeComponent component, int? cost = null)
    {
        var ev = new OnAttemptEnergyUseEvent(uid);
        RaiseLocalEvent(uid, ev);

        if (ev.Cancelled)
            return false;

        if (cost is null)
            return true;

        if (component.Energy >= cost)
        {
            component.Energy -= (int)cost;
            Dirty(uid, component);
        }
        else
        {
            _popup.PopupClient(Loc.GetString("shadekin-noenergy"), uid, uid, PopupType.LargeCaution);
            return false;
        }

        return true;
    }

    private void UpdateEnergy(EntityUid uid, ShadekinComponent component, BrighteyeComponent brighteye)
    {
        if (brighteye.Rejuvenating && brighteye.Energy >= brighteye.MaxEnergy)
        {
            brighteye.Rejuvenating = false;
            _popup.PopupClient(Loc.GetString("shadekin-rejuvenate-compleated"), uid, uid, PopupType.LargeCaution);
            _alerts.ClearAlert(uid, brighteye.RejuvenationAlert);
        }

        if (component.CurrentState == ShadekinState.Low) // On Low State, we gain and lose nothing!
            return;

        var newEnergy = 0;

        if (brighteye.Energy > 0 && component.CurrentState != ShadekinState.Dark) // First we will handle energy drain on light.
        {
            if (component.CurrentState == ShadekinState.Extreme)
                newEnergy = -5;
            else if (component.CurrentState == ShadekinState.High)
                newEnergy = -2;
            else if (component.CurrentState == ShadekinState.Annoying)
                newEnergy = -1;
        }
        else if (brighteye.Energy < brighteye.MaxEnergy && component.CurrentState == ShadekinState.Dark) // We now handle energy gain.
        {
            // TODO: Add buffs here depanding on different situations?
            newEnergy = 1;
        }

        brighteye.Energy = Math.Clamp(brighteye.Energy + newEnergy, 0, brighteye.MaxEnergy);
        Dirty(uid, brighteye);
    }
}
