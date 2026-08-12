using Content.Shared.Construction.Prototypes;
using Content.Shared.Lathe.Prototypes;
using Content.Shared.Research.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Lathe
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class LatheComponent : Component
    {
        /// <summary>
        /// All of the recipe packs that the lathe has by default
        /// </summary>
        [DataField]
        public List<ProtoId<LatheRecipePackPrototype>> StaticPacks = new();

        /// <summary>
        /// All of the recipe packs that the lathe is capable of researching
        /// </summary>
        [DataField]
        public List<ProtoId<LatheRecipePackPrototype>> DynamicPacks = new();
        // Note that this shouldn't be modified dynamically.
        // I.e., this + the static recipies should represent all recipies that the lathe can ever make
        // Otherwise the material arbitrage test and/or LatheSystem.GetAllBaseRecipes needs to be updated

        /// <summary>
        /// The lathe's construction queue.
        /// </summary>
        /// <remarks>
        /// This is a LinkedList to allow for constant time insertion/deletion (vs a List), and more efficient
        /// moves (vs a Queue).
        /// </remarks>
        [DataField]
        public LinkedList<LatheRecipeBatch> Queue = new();

        /// <summary>
        /// The sound that plays when the lathe is producing an item, if any
        /// </summary>
        [DataField]
        public SoundSpecifier? ProducingSound;

        [DataField]
        public string? ReagentOutputSlotId;

        /// <summary>
        /// The default amount that's displayed in the UI for selecting the print amount.
        /// </summary>
        [DataField, AutoNetworkedField]
        public int DefaultProductionAmount = 1;

        #region Visualizer info
        [DataField]
        public string? IdleState;

        [DataField]
        public string? RunningState;

        [DataField]
        public string? UnlitIdleState;

        [DataField]
        public string? UnlitRunningState;
        #endregion

        /// <summary>
        /// The recipe the lathe is currently producing
        /// </summary>
        [ViewVariables]
        public ProtoId<LatheRecipePrototype>? CurrentRecipe;

        #region MachineUpgrading
        /// <summary>
        /// A modifier that changes how long it takes to print a recipe
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float TimeMultiplier = 1;

        /// <summary>
        /// A modifier that changes how much of a material is needed to print a recipe
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public float MaterialUseMultiplier = 1;
        #endregion

        #region Mono
        /// <summary>
        /// Whether to add recipes back to the end of the queue after fabricating them.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public bool Loop;

        /// <summary>
        /// Whether to skip recipes if lacking resources, as opposed to blocking the
        /// queue while waiting for them.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public bool SkipBad;
        #endregion
    }

    public sealed class LatheGetRecipesEvent : EntityEventArgs
    {
        public readonly EntityUid Lathe;
        public readonly LatheComponent Comp;

        public bool GetUnavailable;

        public HashSet<ProtoId<LatheRecipePrototype>> Recipes = new();

        public LatheGetRecipesEvent(Entity<LatheComponent> lathe, bool forced)
        {
            (Lathe, Comp) = lathe;
            GetUnavailable = forced;
        }
    }

    [Serializable]
    public sealed partial class LatheRecipeBatch
    {
        public ProtoId<LatheRecipePrototype> Recipe;
        public int ItemsPrinted;
        public int ItemsRequested;

        // <Mono>
        /// <summary>
        /// Whether the materials for this batch have already been taken from the lathe.
        /// Batches queued by hand pay up front; batches put back by <see cref="LatheComponent.Loop"/>
        /// only pay once they reach the head of the queue, so the loop stops on its own
        /// when the lathe runs dry.
        /// </summary>
        public bool Paid;
        // </Mono>

        public LatheRecipeBatch(ProtoId<LatheRecipePrototype> recipe, int itemsPrinted, int itemsRequested, bool paid = true) // Mono - paid
        {
            Recipe = recipe;
            ItemsPrinted = itemsPrinted;
            ItemsRequested = itemsRequested;
            Paid = paid; // Mono
        }
    }

    /// <summary>
    /// Event raised on a lathe when it starts producing a recipe.
    /// </summary>
    [ByRefEvent]
    public readonly record struct LatheStartPrintingEvent(LatheRecipePrototype Recipe);
}
