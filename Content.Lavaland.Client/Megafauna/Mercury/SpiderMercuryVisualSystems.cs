// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Lavaland.Shared.Megafauna.Mercury;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Lavaland.Client.Megafauna.Mercury;

public sealed partial class SpiderMercuryPhaseVisualSystem : EntitySystem
{
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private SpriteSystem _sprites = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PhaseConversionComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<PhaseConversionComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null ||
            !_appearance.TryGetData<bool>(ent.Owner, PhaseConversionVisuals.IsRanged, out var ranged, args.Component))
        {
            return;
        }

        var sprite = ranged ? ent.Comp.RangedSprite : ent.Comp.MeleeSprite;
        if (sprite is SpriteSpecifier.Rsi rsi)
            _sprites.LayerSetRsiState((ent.Owner, args.Sprite), 0, rsi.RsiState);
    }
}

public sealed class SpiderMercuryEffectVisualSystem : EntitySystem
{
    private const float MinimumSpriteScale = 0.01f;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ExpandAndCollapseComponent, MapInitEvent>(OnExpandMapInit);
    }

    private void OnExpandMapInit(Entity<ExpandAndCollapseComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.Accumulator = 0f;
        ent.Comp.Collapsing = false;
        ent.Comp.CurrentScale = MathF.Max(ent.Comp.StartingScale, MinimumSpriteScale);
        if (TryComp<SpriteComponent>(ent, out var sprite))
            sprite.Scale = new Vector2(ent.Comp.CurrentScale);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var expandQuery = EntityQueryEnumerator<ExpandAndCollapseComponent, SpriteComponent>();
        while (expandQuery.MoveNext(out _, out var expand, out var sprite))
        {
            expand.Accumulator += frameTime;
            if (!expand.Collapsing)
            {
                var progress = MathF.Min(expand.Accumulator / MathF.Max(expand.ExpandTime, 0.01f), 1f);
                expand.CurrentScale = MathF.Max(
                    MathHelper.Lerp(expand.StartingScale, expand.MaxScale, progress),
                    MinimumSpriteScale);
                if (progress >= 1f)
                {
                    expand.Collapsing = true;
                    expand.Accumulator = 0f;
                }
            }
            else
            {
                var progress = MathF.Min(expand.Accumulator / MathF.Max(expand.CollapseTime, 0.01f), 1f);
                expand.CurrentScale = MathF.Max(
                    MathHelper.Lerp(expand.MaxScale, expand.StartingScale, progress),
                    MinimumSpriteScale);
            }
            sprite.Scale = new Vector2(expand.CurrentScale);
        }

        var rotateQuery = EntityQueryEnumerator<SpriteRotaterComponent, SpriteComponent>();
        while (rotateQuery.MoveNext(out _, out var rotate, out var sprite))
        {
            rotate.CurrentSpeed = rotate.IncreaseOvertime
                ? MathF.Min(rotate.CurrentSpeed + rotate.IncreaseBy * frameTime, rotate.MaximumSpeed)
                : rotate.RotationSpeed;
            sprite.Rotation += Angle.FromDegrees(rotate.CurrentSpeed * frameTime);
        }
    }
}
