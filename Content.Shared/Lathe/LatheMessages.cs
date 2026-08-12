using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Lathe;

[Serializable, NetSerializable]
public sealed class LatheUpdateState : BoundUserInterfaceState
{
    public List<ProtoId<LatheRecipePrototype>> Recipes;

    public LatheRecipeBatch[] Queue;

    public ProtoId<LatheRecipePrototype>? CurrentlyProducing;

    // <Mono>
    public bool Looping;

    public bool Skipping;
    // </Mono>

    public LatheUpdateState(List<ProtoId<LatheRecipePrototype>> recipes, LatheRecipeBatch[] queue, ProtoId<LatheRecipePrototype>? currentlyProducing = null, bool looping = false, bool skipping = false) // Mono - looping, skipping
    {
        Recipes = recipes;
        Queue = queue;
        CurrentlyProducing = currentlyProducing;
        // <Mono>
        Looping = looping;
        Skipping = skipping;
        // </Mono>
    }
}

/// <summary>
///     Sent to the server to sync material storage and the recipe queue.
/// </summary>
[Serializable, NetSerializable]
public sealed class LatheSyncRequestMessage : BoundUserInterfaceMessage
{

}

/// <summary>
///     Sent to the server when a client queues a new recipe.
/// </summary>
[Serializable, NetSerializable]
public sealed class LatheQueueRecipeMessage : BoundUserInterfaceMessage
{
    public readonly string ID;
    public readonly int Quantity;
    public LatheQueueRecipeMessage(string id, int quantity)
    {
        ID = id;
        Quantity = quantity;
    }
}

/// <summary>
///     Sent to the server to remove a batch from the queue.
/// </summary>
[Serializable, NetSerializable]
public sealed class LatheDeleteRequestMessage(int index) : BoundUserInterfaceMessage
{
    public int Index = index;
}

/// <summary>
///     Sent to the server to move the position of a batch in the queue.
/// </summary>
[Serializable, NetSerializable]
public sealed class LatheMoveRequestMessage(int index, int change) : BoundUserInterfaceMessage
{
    public int Index = index;
    public int Change = change;
}

/// <summary>
///     Sent to the server to stop producing the current item.
/// </summary>
[Serializable, NetSerializable]
public sealed class LatheAbortFabricationMessage() : BoundUserInterfaceMessage
{
}

// <Mono>
/// <summary>
///     Sent to the server when the player toggles looping the queue.
/// </summary>
[Serializable, NetSerializable]
public sealed class LatheSetLoopingMessage : BoundUserInterfaceMessage
{
    public readonly bool ShouldLoop;

    public LatheSetLoopingMessage(bool shouldLoop)
    {
        ShouldLoop = shouldLoop;
    }
}

/// <summary>
///     Sent to the server when the player toggles skipping recipes that lack materials.
/// </summary>
[Serializable, NetSerializable]
public sealed class LatheSetSkipMessage : BoundUserInterfaceMessage
{
    public readonly bool ShouldSkip;

    public LatheSetSkipMessage(bool shouldSkip)
    {
        ShouldSkip = shouldSkip;
    }
}
// </Mono>

[NetSerializable, Serializable]
public enum LatheUiKey
{
    Key,
}
