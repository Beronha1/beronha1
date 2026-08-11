using System.Numerics;
using Content.Shared.Chasm;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;

namespace Content.Client.Chasm;

/// <summary>
/// Handles the falling animation for entities that fall into an entity with <see cref="ChasmComponent"/>.
/// </summary>
public sealed partial class ChasmFallingVisualsSystem : EntitySystem
{
    private const float MinimumSpriteScale = 0.01f;

    [Dependency] private AnimationPlayerSystem _anim = default!;
    [Dependency] private SpriteSystem _sprite = default!;

    [Dependency] private EntityQuery<AnimationPlayerComponent> _animationPlayerQuery;
    [Dependency] private EntityQuery<SpriteComponent> _spriteQuery;

    private const string ChasmFallAnimationKey = "chasm_fall";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChasmFallingComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ChasmFallingComponent, ComponentRemove>(OnComponentRemove);
    }

    private void OnComponentInit(Entity<ChasmFallingComponent> entity, ref ComponentInit args)
    {
        if (!_spriteQuery.TryComp(entity, out var sprite) ||
            TerminatingOrDeleted(entity))
        {
            return;
        }

        entity.Comp.OriginalScale = sprite.Scale;

        if (!_animationPlayerQuery.TryComp(entity, out var player) ||
            _anim.HasRunningAnimation(player, ChasmFallAnimationKey))
        {
            return;
        }

        _anim.Play((entity, player), GetFallingAnimation(entity.Comp), ChasmFallAnimationKey);
    }

    private void OnComponentRemove(Entity<ChasmFallingComponent> entity, ref ComponentRemove args)
    {
        if (!_spriteQuery.TryComp(entity, out var sprite))
        {
            return;
        }

        _sprite.SetScale((entity, sprite), GetSafeScale(entity.Comp.OriginalScale, Vector2.One));

        if (!_animationPlayerQuery.TryComp(entity, out var player) ||
            !_anim.HasRunningAnimation(player, ChasmFallAnimationKey))
        {
            return;
        }

        _anim.Stop((entity, player), ChasmFallAnimationKey);
    }

    private static Animation GetFallingAnimation(ChasmFallingComponent component)
    {
        return new Animation
        {
            Length = component.AnimationTime,
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Scale),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(component.OriginalScale, 0.0f),
                        new AnimationTrackProperty.KeyFrame(
                            GetSafeScale(component.AnimationScale, new Vector2(MinimumSpriteScale)),
                            component.AnimationTime.Seconds),
                    },
                    // A scale animation must stay between its endpoints. Cubic
                    // interpolation can overshoot below the renderer's minimum.
                    InterpolationMode = AnimationInterpolationMode.Linear,
                },
            },
        };
    }

    private static Vector2 GetSafeScale(Vector2 scale, Vector2 fallback)
    {
        if (!float.IsFinite(scale.X) ||
            !float.IsFinite(scale.Y) ||
            MathF.Abs(scale.X) < MinimumSpriteScale ||
            MathF.Abs(scale.Y) < MinimumSpriteScale)
        {
            return fallback;
        }

        return scale;
    }
}
