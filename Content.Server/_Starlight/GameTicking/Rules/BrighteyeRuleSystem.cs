// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using System.Text;
using Content.Server._Starlight.GameTicking.Rules.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.Objectives;

namespace Content.Server._Starlight.GameTicking.Rules;

/// <summary>
///     Escreve o cabeçalho do Brighteye no resumo de fim de rodada.
///
///     No Starlight ele também mostrava a contagem de pisos escuros na estação,
///     que vinha do RailroadDarkTaskSystem. Esse sistema é do Railroading, que
///     este fork não tem, então a linha saiu e o resto continua igual.
/// </summary>
public sealed partial class BrighteyeRuleSystem : GameRuleSystem<BrighteyeRuleComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BrighteyeRuleComponent, ObjectivesTextPrependEvent>(OnTextPrepend);
    }

    private void OnTextPrepend(EntityUid uid, BrighteyeRuleComponent comp, ref ObjectivesTextPrependEvent args)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Loc.GetString("brighteye-thedark"));
        sb.AppendLine(Loc.GetString("brighteye-darkstation"));
        args.Text = sb.ToString();
    }
}
