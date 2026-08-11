using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Chemistry;
using Robust.Shared.Audio;

namespace Content.Server.Chemistry.Components
{
    /// <summary>
    /// An industrial grade chemical manipulator with pill and bottle production included.
    /// <seealso cref="ChemMasterSystem"/>
    /// </summary>
    [RegisterComponent]
    [Access(typeof(ChemMasterSystem))]
    public sealed partial class ChemMasterComponent : Component
    {
        [DataField("pillType"), ViewVariables(VVAccess.ReadWrite)]
        public uint PillType = 0;

        [DataField("mode"), ViewVariables(VVAccess.ReadWrite)]
        public ChemMasterMode Mode = ChemMasterMode.Transfer;

        // <Whiskey> O dispensador ao lado já ordena a lista sozinho, então o
        // ChemMaster começar sem ordenação obrigava a clicar no botão toda vez.
        // A ordenação usa LocalizedName, então acompanha o idioma do servidor.
        [DataField]
        public ChemMasterSortingType SortingType = ChemMasterSortingType.Alphabetical;
        // </Whiskey>

        [DataField("pillDosageLimit", required: true), ViewVariables(VVAccess.ReadWrite)]
        public uint PillDosageLimit;

        [DataField("clickSound"), ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier ClickSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

        /// <summary>
        /// Which source the chem master should draw from when making pills/bottles.
        /// </summary>
        [DataField]
        public ChemMasterDrawSource DrawSource = ChemMasterDrawSource.Internal;
    }
}
