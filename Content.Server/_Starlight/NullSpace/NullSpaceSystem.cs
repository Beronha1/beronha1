// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Content.Shared.Eye;
using Content.Shared._Starlight.Immunity;
using Robust.Server.GameObjects;
using Content.Server.Atmos.Components;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using System.Linq;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Hands;
using Content.Shared.Shuttles.Components;
using Content.Shared.Stunnable;
using Content.Shared.Gravity;
using Content.Shared._Starlight.NullSpace.Systems;
using Content.Shared._Starlight.NullSpace.Components;
using Content.Shared._Starlight.Bluespace;
using Content.Shared.Atmos;

namespace Content.Server._Starlight.NullSpace;

public sealed partial class NullSpaceSystem : SharedNullSpaceSystem
{
    /// <summary>
    ///     Acrescenta o componente e anota que foi o NullSpace quem pôs. Se a
    ///     entidade já tinha, não anota, e a saída não vai tirar.
    /// </summary>
    private void Conceder<T>(EntityUid uid, NullSpaceComponent component) where T : IComponent, new()
    {
        if (HasComp<T>(uid))
            return;

        AddComp<T>(uid);
        component.Instalados.Add(typeof(T));
    }

    /// <summary> Tira o componente só se tiver sido o NullSpace que o concedeu. </summary>
    private void Devolver<T>(EntityUid uid, NullSpaceComponent component) where T : IComponent
    {
        if (!component.Instalados.Remove(typeof(T)))
            return;

        RemComp<T>(uid);
    }

    [Dependency] private SharedStealthSystem _stealth = default!;
    [Dependency] private EyeSystem _eye = default!;
    [Dependency] private NpcFactionSystem _factions = default!;
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private SharedVirtualItemSystem _virtualItem = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private NullSpacePhaseSystem _phaseSystem = default!;
    [Dependency] private SharedGravitySystem _gravity = default!;
    [Dependency] private VisibilitySystem _visibility = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NullSpaceComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<NullSpaceComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<NullSpaceComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<NullSpaceComponent, AtmosExposedGetAirEvent>(OnExpose);
        SubscribeLocalEvent<NullSpaceComponent, VirtualItemDeletedEvent>(OnVirtualItemDeleted);
        SubscribeLocalEvent<NullSpaceComponent, NullSpaceShuntEvent>(NullSpaceShunt);
        SubscribeLocalEvent<NullSpaceComponent, GetVisMaskEvent>(OnGetVisMask);
    }

    private void OnGetVisMask(Entity<NullSpaceComponent> uid, ref GetVisMaskEvent args) =>
        args.VisibilityMask |= (int)VisibilityFlags.NullSpace;

    public void OnStartup(EntityUid uid, NullSpaceComponent component, MapInitEvent args)
    {
        var visibility = EnsureComp<VisibilityComponent>(uid);
        _visibility.RemoveLayer((uid, visibility), (int)VisibilityFlags.Normal, false);
        _visibility.AddLayer((uid, visibility), (int)VisibilityFlags.NullSpace, false);
        _visibility.RefreshVisibility(uid, visibility);

        _eye.RefreshVisibilityMask(uid);

        Conceder<StealthComponent>(uid, component);
        _stealth.SetVisibility(uid, 0.8f);

        SuppressFactions(uid, component, true);

        RemComp<KnockedDownComponent>(uid);

        // Whiskey: anota o que foi realmente acrescentado, para a saída tirar só isso.
        Conceder<PressureImmunityComponent>(uid, component);
        Conceder<FTLSmashImmuneComponent>(uid, component);
        Conceder<TemperatureImmunityComponent>(uid, component);

        if (TryComp<GravityAffectedComponent>(uid, out var grav))
            _gravity.RefreshWeightless((uid, grav), false);

        if (TryComp<HandsComponent>(uid, out var handsComponent))
        {
            foreach (var hand in _hands.EnumerateHands((uid, handsComponent)))
            {
                if (_hands.GetHeldItem((uid, handsComponent), hand) is var item)
                {
                    if (HasComp<UnremoveableComponent>(item))
                        continue;

                    if (TryComp<VirtualItemComponent>(item, out var vcomp))
                        if (HasComp<NullSpacePulledComponent>(vcomp.BlockingEntity) && TryComp<PullableComponent>(vcomp.BlockingEntity, out var pulling) && pulling.BeingPulled)
                        {
                            RemComp<NullSpacePulledComponent>(vcomp.BlockingEntity);
                            // safety check just to make sure you dont pull something out of nullspace by phasing in
                            if (!HasComp<NullSpaceComponent>(vcomp.BlockingEntity)) _phaseSystem.Phase(vcomp.BlockingEntity);
                            continue;
                        }

                    _hands.DoDrop((uid, handsComponent), hand, true);
                }

                if (_virtualItem.TrySpawnVirtualItemInHand(uid, uid, out var virtItem))
                    EnsureComp<UnremoveableComponent>(virtItem.Value);
            }
        }

        if (TryComp<PullableComponent>(uid, out var pullable) && pullable.BeingPulled)
        {
            // if thing pulling is in nullspace, you're coming along with them.
            if (!HasComp<NullSpaceComponent>(pullable.Puller!.Value))
                _pulling.TryStopPull(uid, pullable);
        }
    }

    public void OnShutdown(EntityUid uid, NullSpaceComponent component, ComponentShutdown args)
    {
        if (TryComp<VisibilityComponent>(uid, out var visibility))
        {
            _visibility.RemoveLayer((uid, visibility), (int)VisibilityFlags.NullSpace, false);
            _visibility.AddLayer((uid, visibility), (int)VisibilityFlags.Normal, false);
            _visibility.RefreshVisibility(uid, visibility);
        }

        SuppressFactions(uid, component, false);

        // Whiskey: antes daqui saía um RemComp cego de cada um, o que apagava também
        // imunidade que a pessoa já tinha por traje, espécie ou outro sistema. Agora
        // devolve só o que o NullSpace concedeu.
        //
        // E a imunidade térmica: tanto aqui quanto no Starlight a saída fazia
        // EnsureComp, ou seja quem passasse pela escuridão ficava imune a
        // temperatura PARA SEMPRE. Lá era condicionado a culto cósmico de nível 3, que
        // este fork não tem. Isso é defeito, não regra, e virou devolução.
        Devolver<PressureImmunityComponent>(uid, component);
        Devolver<FTLSmashImmuneComponent>(uid, component);
        Devolver<TemperatureImmunityComponent>(uid, component);
        Devolver<StealthComponent>(uid, component);

        _virtualItem.DeleteInHandsMatching(uid, uid);
    }

    public void OnRemove(EntityUid uid, NullSpaceComponent component, ComponentRemove args)
    {
        _eye.RefreshVisibilityMask(uid);

        if (TryComp<GravityAffectedComponent>(uid, out var grav))
            _gravity.RefreshWeightless((uid, grav));
    }

    private void OnVirtualItemDeleted(EntityUid uid, NullSpaceComponent component, VirtualItemDeletedEvent args)
    {
        if (TryComp<HandsComponent>(uid, out var handsComponent))
        {
            foreach (var hand in _hands.EnumerateHands((uid, handsComponent)))
            {
                if (_hands.GetHeldItem((uid, handsComponent), hand) is var item)
                {
                    if (HasComp<UnremoveableComponent>(item))
                        continue;

                    if (TryComp<VirtualItemComponent>(item, out var vcomp))
                    {
                        // safety check just to make sure you dont pull something into nullspace by phasing out.
                        if (HasComp<NullSpaceComponent>(vcomp.BlockingEntity)) _phaseSystem.Phase(vcomp.BlockingEntity);
                        continue;
                    }

                    _hands.DoDrop((uid, handsComponent), hand, true);
                }

                if (_virtualItem.TrySpawnVirtualItemInHand(uid, uid, out var virtItem))
                    EnsureComp<UnremoveableComponent>(virtItem.Value);
            }
        }
    }

    private void NullSpaceShunt(EntityUid uid, NullSpaceComponent component, NullSpaceShuntEvent args)
    {
        SpawnAtPosition(_shadekinShadow, Transform(uid).Coordinates);
        RemComp(uid, component);
    }

    public void SuppressFactions(EntityUid uid, NullSpaceComponent component, bool set)
    {
        if (set)
        {
            if (!TryComp<NpcFactionMemberComponent>(uid, out var factions))
                return;

            component.SuppressedFactions = factions.Factions.ToList();

            foreach (var faction in factions.Factions)
                _factions.RemoveFaction(uid, faction);
        }
        else
        {
            foreach (var faction in component.SuppressedFactions)
                _factions.AddFaction(uid, faction);

            component.SuppressedFactions.Clear();
        }
    }

    private void OnExpose(EntityUid uid, NullSpaceComponent component, ref AtmosExposedGetAirEvent args)
    {
        if (args.Handled)
            return;

        args.Gas = null;
        args.Handled = true;
    }
}
