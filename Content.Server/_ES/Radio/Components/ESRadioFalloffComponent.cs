namespace Content.Server._ES.Radio.Components;

[RegisterComponent]
[Access(typeof(ESRadioSystem))]
public sealed partial class ESRadioFalloffComponent : Component
{
    [DataField]
    public float MaxClearSendDistance = 15f;

    [DataField]
    public float MaxSendDistance = 50f;
}
