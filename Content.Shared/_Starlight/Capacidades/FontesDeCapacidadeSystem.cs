// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT

namespace Content.Shared._Starlight.Capacidades;

/// <summary>
///     Concede e devolve capacidade contando as fontes, para duas fontes não se
///     atrapalharem e para nada apagar capacidade que já existia.
/// </summary>
public sealed partial class FontesDeCapacidadeSystem : EntitySystem
{
    /// <summary>
    ///     Dá a capacidade ao alvo e anota a fonte.
    ///
    ///     Se o alvo já tinha a capacidade e ninguém tinha concedido, a fonte não
    ///     assume a posse: quem já tinha continua dono, e devolver não vai tirar.
    /// </summary>
    public void Conceder<T>(EntityUid alvo, EntityUid fonte) where T : IComponent, new()
    {
        var chave = typeof(T).Name;
        var registro = EnsureComp<FontesDeCapacidadeComponent>(alvo);

        if (!registro.Fontes.TryGetValue(chave, out var fontes))
        {
            // já existia sem ninguém ter concedido: não assume posse
            if (HasComp<T>(alvo))
                return;

            registro.Fontes[chave] = fontes = new HashSet<EntityUid>();
        }

        if (!fontes.Add(fonte))
            return;

        EnsureComp<T>(alvo);
    }

    /// <summary> Tira a anotação desta fonte, e só remove a capacidade se foi a última. </summary>
    public void Devolver<T>(EntityUid alvo, EntityUid fonte) where T : IComponent
    {
        if (!TryComp<FontesDeCapacidadeComponent>(alvo, out var registro))
            return;

        var chave = typeof(T).Name;
        if (!registro.Fontes.TryGetValue(chave, out var fontes) || !fontes.Remove(fonte))
            return;

        if (fontes.Count > 0)
            return;

        registro.Fontes.Remove(chave);
        RemComp<T>(alvo);

        if (registro.Fontes.Count == 0)
            RemComp<FontesDeCapacidadeComponent>(alvo);
    }
}
