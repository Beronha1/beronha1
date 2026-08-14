using Content.Shared._ES.Objectives;
using Content.Shared._ES.Objectives.Components;

namespace Content.Client._ES.Objectives;

/// <inheritdoc/>
public sealed class ESObjectiveSystem : ESSharedObjectiveSystem
{
    public event Action<Entity<ESObjectiveComponent>>? OnObjectiveProgressChanged;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ESObjectiveComponent, AfterAutoHandleStateEvent>(OnObjectiveAfterAutoHandleState);
    }

    private void OnObjectiveAfterAutoHandleState(Entity<ESObjectiveComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        OnObjectiveProgressChanged?.Invoke(ent);
    }
}
