// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using System.Text;
using Content.Server.Antag;
using Content.Server._Starlight.GameTicking.Rules.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.Objectives;
using Content.Server.Spawners.Components;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

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
    private const string SpawnPointPrototype = "SpawnPointBrighteye";
    private static readonly ProtoId<TagPrototype> TheDarkTag = "TheDark";

    [Dependency] private TagSystem _tag = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BrighteyeRuleComponent, ObjectivesTextPrependEvent>(OnTextPrepend);
        SubscribeLocalEvent<BrighteyeRuleComponent, AntagSelectLocationEvent>(OnSelectLocation);
    }

    /// <summary>
    ///     O NestedRule da Whiskey carrega o mapa filho sem encaminhar os grids
    ///     para o RuleGrids da regra pai. Resolve somente o ponto do Bright-eye,
    ///     procurando seu marcador no mapa já identificado como TheDark.
    /// </summary>
    private void OnSelectLocation(Entity<BrighteyeRuleComponent> ent, ref AntagSelectLocationEvent args)
    {
        if (args.Handled)
            return;

        var query = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            if (MetaData(uid).EntityPrototype?.ID != SpawnPointPrototype ||
                xform.MapUid is not { } map ||
                !_tag.HasTag(map, TheDarkTag))
            {
                continue;
            }

            args.Coordinates.Add(_transform.GetMapCoordinates(xform));
        }
    }

    private void OnTextPrepend(EntityUid uid, BrighteyeRuleComponent comp, ref ObjectivesTextPrependEvent args)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Loc.GetString("brighteye-thedark"));
        sb.AppendLine(Loc.GetString("brighteye-darkstation"));
        args.Text = sb.ToString();
    }
}
