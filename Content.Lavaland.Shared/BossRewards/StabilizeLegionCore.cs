// All modifications and original work in ss14-wega under the Corvax-Wega tag
// and _Wega directories are licensed under GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Lavaland.Shared.Artifacts;

public sealed partial class StabilizeLegionCoreSystem : EntityEffectSystem<LegionCoreComponent, StabilizeLegionCore>
{
    protected override void Effect(Entity<LegionCoreComponent> entity, ref EntityEffectEvent<StabilizeLegionCore> args)
    {
        if (!entity.Comp.Active)
            return;

        entity.Comp.Stabilized = true;
    }
}

public sealed partial class StabilizeLegionCore : EntityEffectBase<StabilizeLegionCore>
{
    public override string? EntityEffectGuidebookText(
        IPrototypeManager prototype,
        IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-stabilize-legion-core");
    }
}
