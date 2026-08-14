namespace Content.Server.EnergyDome;

[RegisterComponent, Access(typeof(EnergyDomeSystem))]
public sealed partial class EnergyDomeComponent : Component
{
    [DataField]
    public EntityUid? Generator;
}
