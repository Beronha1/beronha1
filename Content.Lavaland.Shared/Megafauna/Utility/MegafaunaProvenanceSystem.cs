// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Examine;

namespace Content.Lavaland.Shared.Megafauna.Utility;

public sealed class MegafaunaProvenanceSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MegafaunaProvenanceComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<MegafaunaProvenanceComponent> ent, ref ExaminedEvent args)
    {
        var grade = ent.Comp.Grade switch
        {
            MegafaunaProvenanceGrade.Intact => "megafauna-provenance-grade-intact",
            MegafaunaProvenanceGrade.Processed => "megafauna-provenance-grade-processed",
            _ => "megafauna-provenance-grade-raw",
        };

        args.PushMarkup(Loc.GetString(
            "megafauna-provenance-examine",
            ("source", Loc.GetString(ent.Comp.Source)),
            ("grade", Loc.GetString(grade))));
    }
}
