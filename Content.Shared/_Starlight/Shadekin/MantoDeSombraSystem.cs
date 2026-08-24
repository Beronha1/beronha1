// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT

using Content.Shared._Starlight.Shadekin.Components;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;

namespace Content.Shared._Starlight.Shadekin;

/// <summary> Liga e desliga o manto conforme o estado de luz do Shadekin. </summary>
public sealed partial class MantoDeSombraSystem : EntitySystem
{
    [Dependency] private SharedStealthSystem _stealth = default!;

    /// <summary>
    ///     Chamado pelo ShadekinSystem a cada atualização de luz. Só mexe quando o
    ///     estado muda, para não sujar a entidade toda hora.
    /// </summary>
    public void Atualizar(EntityUid uid, MantoDeSombraComponent manto, ShadekinState estado)
    {
        var deveValer = estado == ShadekinState.Dark;
        if (deveValer == manto.Ativo)
            return;

        manto.Ativo = deveValer;

        if (deveValer)
        {
            var furtividade = EnsureComp<StealthComponent>(uid);
            _stealth.SetVisibility(uid, manto.VisibilidadeNoEscuro, furtividade);
            EnsureComp<StealthOnMoveComponent>(uid);
        }
        else
        {
            RemComp<StealthOnMoveComponent>(uid);
            RemComp<StealthComponent>(uid);
        }

        Dirty(uid, manto);
    }
}
