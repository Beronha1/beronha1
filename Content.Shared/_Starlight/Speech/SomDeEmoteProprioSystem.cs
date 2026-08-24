// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT

using Content.Goobstation.Common.Speech;
using Content.Shared.Humanoid;

namespace Content.Shared._Starlight.Speech;

/// <summary> Responde qual conjunto de som de emote a criatura usa. </summary>
public sealed partial class SomDeEmoteProprioSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void AoPedirSomDeEmote(Entity<SomDeEmoteProprioComponent> ent, ref GetEmoteSoundsEvent args)
    {
        if (args.Handled)
            return;

        var sexo = CompOrNull<HumanoidProfileComponent>(ent)?.Sex ?? Sex.Unsexed;

        args.EmoteSoundProtoId = sexo switch
        {
            Sex.Female => ent.Comp.Feminino,
            Sex.Unsexed => ent.Comp.SemSexo ?? ent.Comp.Masculino,
            _ => ent.Comp.Masculino,
        };
        args.Handled = true;
    }
}
