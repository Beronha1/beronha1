// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.EnergyDome;

[RegisterComponent, Access(typeof(EnergyDomeSystem))]
public sealed partial class EnergyDomeGeneratorComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public bool Enabled;

    [DataField]
    public float DamageEnergyDraw = 10f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId DomePrototype = "EnergyDomeSmallRed";

    [DataField]
    public EntityUid? SpawnedDome;

    [DataField]
    public EntityUid? DomeParentEntity;

    [DataField]
    public EntProtoId ToggleAction = "ActionToggleDome";

    [DataField]
    public EntityUid? ToggleActionEntity;

    [DataField]
    public SoundSpecifier TurnOnSound = new SoundPathSpecifier("/Audio/Machines/anomaly_sync_connect.ogg");

    [DataField]
    public SoundSpecifier EnergyOutSound = new SoundPathSpecifier("/Audio/Machines/energyshield_down.ogg");

    [DataField]
    public SoundSpecifier TurnOffSound = new SoundPathSpecifier("/Audio/Machines/button.ogg");

    [DataField]
    public SoundSpecifier ParrySound = new SoundPathSpecifier("/Audio/Machines/energyshield_parry.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.05f),
    };
}
