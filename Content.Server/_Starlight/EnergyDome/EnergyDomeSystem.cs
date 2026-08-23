// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Content.Server.Power.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Damage.Systems;
using Content.Shared.Interaction;
using Content.Shared.Power.Components;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Content.Shared.Toggleable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Server.EnergyDome;

public sealed partial class EnergyDomeSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private BatterySystem _battery = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private UseDelaySystem _useDelay = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EnergyDomeGeneratorComponent, ActivateInWorldEvent>(OnActivatedInWorld);
        SubscribeLocalEvent<EnergyDomeGeneratorComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<EnergyDomeGeneratorComponent, ToggleActionEvent>(OnToggleAction);
        SubscribeLocalEvent<EnergyDomeGeneratorComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<EnergyDomeComponent, DamageChangedEvent>(OnDomeDamaged);
    }

    private void OnActivatedInWorld(Entity<EnergyDomeGeneratorComponent> generator, ref ActivateInWorldEvent args)
    {
        AttemptToggle(generator, !generator.Comp.Enabled);
    }

    private void OnGetActions(Entity<EnergyDomeGeneratorComponent> generator, ref GetItemActionsEvent args)
    {
        args.AddAction(ref generator.Comp.ToggleActionEntity, generator.Comp.ToggleAction);
    }

    private void OnToggleAction(Entity<EnergyDomeGeneratorComponent> generator, ref ToggleActionEvent args)
    {
        if (args.Handled)
            return;

        AttemptToggle(generator, !generator.Comp.Enabled);
        args.Handled = true;
    }

    private void OnDomeDamaged(Entity<EnergyDomeComponent> dome, ref DamageChangedEvent args)
    {
        if (dome.Comp.Generator is not { } generatorUid ||
            args.DamageDelta is not { } damage ||
            !TryComp<EnergyDomeGeneratorComponent>(generatorUid, out var generator) ||
            !TryComp<BatteryComponent>(generatorUid, out var battery))
            return;

        _audio.PlayPvs(generator.ParrySound, dome);
        _battery.UseCharge(generatorUid, damage.GetTotal().Float() * generator.DamageEnergyDraw);

        if (_battery.GetCharge((generatorUid, battery)) <= 0f)
            TurnOff((generatorUid, generator), true);
    }

    private void OnComponentRemove(Entity<EnergyDomeGeneratorComponent> generator, ref ComponentRemove args)
    {
        TurnOff(generator, false);
    }

    public bool AttemptToggle(Entity<EnergyDomeGeneratorComponent> generator, bool status)
    {
        if (!status)
        {
            TurnOff(generator, false);
            return true;
        }

        if (_useDelay.IsDelayed(generator.Owner))
        {
            _popup.PopupEntity(Loc.GetString("energy-dome-recharging"), generator);
            return false;
        }

        if (!TryComp<BatteryComponent>(generator, out var battery) ||
            _battery.GetCharge((generator, battery)) <= 0f)
        {
            _popup.PopupEntity(Loc.GetString("energy-dome-no-power"), generator);
            return false;
        }

        TurnOn(generator);
        return true;
    }

    private void TurnOn(Entity<EnergyDomeGeneratorComponent> generator)
    {
        if (generator.Comp.Enabled)
            return;

        var protectedEntity = GetProtectedEntity(generator.Owner);
        var dome = Spawn(generator.Comp.DomePrototype, Transform(protectedEntity).Coordinates);
        _transform.SetParent(dome, protectedEntity);

        if (TryComp<EnergyDomeComponent>(dome, out var domeComp))
            domeComp.Generator = generator.Owner;

        generator.Comp.DomeParentEntity = protectedEntity;
        generator.Comp.SpawnedDome = dome;
        generator.Comp.Enabled = true;
        _audio.PlayPvs(generator.Comp.TurnOnSound, generator);
    }

    private void TurnOff(Entity<EnergyDomeGeneratorComponent> generator, bool startReloading)
    {
        if (!generator.Comp.Enabled)
            return;

        generator.Comp.Enabled = false;
        QueueDel(generator.Comp.SpawnedDome);
        generator.Comp.SpawnedDome = null;
        _audio.PlayPvs(generator.Comp.TurnOffSound, generator);

        if (!startReloading)
            return;

        _audio.PlayPvs(generator.Comp.EnergyOutSound, generator);
        if (TryComp<UseDelayComponent>(generator, out var useDelay))
            _useDelay.TryResetDelay((generator, useDelay));
    }

    private EntityUid GetProtectedEntity(EntityUid entity)
    {
        return _container.TryGetOuterContainer(entity, Transform(entity), out var container)
            ? container.Owner
            : entity;
    }
}
