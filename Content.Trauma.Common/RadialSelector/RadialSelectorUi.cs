// SPDX-License-Identifier: AGPL-3.0-or-later


namespace Content.Trauma.Common.RadialSelector;

[NetSerializable, Serializable]
public enum RadialSelectorUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class RadialSelectorState(List<RadialSelectorEntry> entries, bool openCentered = false)
    : BoundUserInterfaceState
{
    public List<RadialSelectorEntry> Entries = entries;

    // WhiteDream - Blood Cult
    public bool OpenCentered { get; private set; } = openCentered;
}

[Serializable, NetSerializable]
public sealed class RadialSelectorSelectedMessage(string selectedItem) : BoundUserInterfaceMessage
{
    public readonly string SelectedItem = selectedItem;
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class RadialSelectorEntry
{
    [DataField]
    public string? Prototype { get; set; }

    // <WhiteDream> - Blood Cult
    [DataField]
    public string? Name { get; set; }

    [DataField]
    public bool CloseUiOnSelect = true;
    // </WhiteDream>

    [DataField]
    public SpriteSpecifier? Icon { get; set; }

    [DataField]
    public RadialSelectorCategory? Category { get; set; }
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class RadialSelectorCategory
{
    [DataField(required: true)]
    public string Name = string.Empty;

    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    [DataField(required: true)]
    public List<RadialSelectorEntry> Entries = default!;
}
