// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Lavaland.Server.Megafauna.Mercury;

[RegisterComponent, Access(typeof(SpiderMercurySystem))]
public sealed partial class SpiderMercuryStageComponent : Component
{
    [DataField]
    public EntProtoId? NextStage;

    [DataField]
    public EntProtoId? TransitionEffect;
}
