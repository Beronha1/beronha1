// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT

using Content.Shared._Starlight.Shadekin;
using Content.Shared.Actions;
using Content.Shared.IdentityManagement;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Server._Starlight.Shadekin;

/// <summary> Apaga a lâmpada acesa mais próxima de quem usou a habilidade. </summary>
public sealed partial class ApagarLuzSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _acoes = default!;
    [Dependency] private SharedPoweredLightSystem _luz = default!;
    [Dependency] private EntityLookupSystem _busca = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ApagarLuzComponent, MapInitEvent>(AoNascer);
        SubscribeLocalEvent<ApagarLuzComponent, ApagarLuzActionEvent>(AoUsar);
    }

    private void AoNascer(Entity<ApagarLuzComponent> ent, ref MapInitEvent args)
    {
        _acoes.AddAction(ent, ref ent.Comp.AcaoEntidade, ent.Comp.Acao);
    }

    private void AoUsar(Entity<ApagarLuzComponent> ent, ref ApagarLuzActionEvent args)
    {
        if (args.Handled)
            return;

        var origem = Transform(ent).Coordinates;

        // pega a lâmpada acesa mais perto, para o jogador não gastar a espera
        // apagando algo que já estava apagado
        EntityUid? alvo = null;
        var menorDistancia = float.MaxValue;

        foreach (var candidata in _busca.GetEntitiesInRange<PoweredLightComponent>(origem, ent.Comp.Alcance))
        {
            if (!candidata.Comp.On)
                continue;

            var posicao = Transform(candidata.Owner).Coordinates;
            if (!_transform.InRange(origem, posicao, ent.Comp.Alcance))
                continue;

            var distancia = (_transform.ToMapCoordinates(posicao).Position
                             - _transform.ToMapCoordinates(origem).Position).Length();
            if (distancia >= menorDistancia)
                continue;

            menorDistancia = distancia;
            alvo = candidata.Owner;
        }

        if (alvo is null)
        {
            _popup.PopupEntity(Loc.GetString("shadekin-apagar-luz-nenhuma"), ent, ent);
            return;
        }

        if (!TryComp<PoweredLightComponent>(alvo, out var luz) || !_luz.TryDestroyBulb(alvo.Value, luz))
        {
            _popup.PopupEntity(Loc.GetString("shadekin-apagar-luz-nenhuma"), ent, ent);
            return;
        }

        args.Handled = true;
        _popup.PopupEntity(Loc.GetString("shadekin-apagar-luz-sucesso"), ent, ent);
        _popup.PopupEntity(Loc.GetString("shadekin-apagar-luz-visto", ("quem", Identity.Entity(ent, EntityManager))),
            ent, Filter.PvsExcept(ent), true, PopupType.SmallCaution);
    }
}
