// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Construction;
using Content.Client.UserInterface.Controls;
using Content.Shared.Construction.Prototypes;
using Content.Trauma.Common.RadialSelector;

namespace Content.Trauma.Client.RadialSelector;

public sealed partial class RadialSelectorMenuBUI : BoundUserInterface
{
    [Dependency] private IPrototypeManager _proto = default!;
    private ConstructionSystem _construction;

    public SimpleRadialMenu Menu;

    private Action<string> OnPressed;

    private bool _openCentered; // WhiteDream - Blood Cult

    public RadialSelectorMenuBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _construction = EntMan.System<ConstructionSystem>();

        Menu = this.CreateWindow<SimpleRadialMenu>();

        OnPressed = proto =>
        {
            SendPredictedMessage(new RadialSelectorSelectedMessage(proto));
        };
    }

    protected override void Open()
    {
        base.Open();

        // WhiteDream - Blood Cult
        if (_openCentered)
            Menu.OpenCentered();
        else
            Menu.OpenOverMouseScreenPosition();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not RadialSelectorState cast)
            return;

        _openCentered = cast.OpenCentered; // WhiteDream - Blood Cult
        CreateMenu(cast.Entries);
    }

    private void CreateMenu(List<RadialSelectorEntry> entries)
    {
        Menu.SetButtons(CreateModels(entries));
    }

    private List<RadialMenuOptionBase> CreateModels(List<RadialSelectorEntry> entries)
    {
        var models = new List<RadialMenuOptionBase>();
        foreach (var entry in entries)
        {
            if (entry.Category is {} category)
            {
                var children = CreateModels(category.Entries);
                models.Add(new RadialMenuNestedLayerOption(children)
                {
                    ToolTip = category.Name,
                    IconSpecifier = RadialMenuIconSpecifier.With(category.Icon)
                });
            }
            else if (entry.Prototype is {} proto)
            {
                models.Add(new RadialMenuActionOption<string>(OnPressed, proto)
                {
                    ToolTip = entry.Name ?? GetName(proto), // WhiteDream - Blood Cult
                    // <WhiteDream> - Blood Cult
                    IconSpecifier = RadialMenuIconSpecifier.With(entry.Icon)
                                    ?? RadialMenuIconSpecifier.With(EntMan.GetEntity(entry.IconEntity))
                                    ?? GetIcon(proto)
                    // </WhiteDream>
                });
            }
        }

        return models;
    }

    /// <summary>
    /// Get the name for an entity or construction prototype.
    /// </summary>
    private string GetName(string proto)
    {
        if (_proto.TryIndex(proto, out var prototype))
            return prototype.Name;

        if (_proto.Resolve<ConstructionPrototype>(proto, out var construction))
            return construction.Name ?? proto;

        return proto;
    }

    /// <summary>
    /// Get the icon for an entity or construction prototype.
    /// </summary>
    private RadialMenuIconSpecifier? GetIcon(string proto)
    {
        if (!_construction.TryGetRecipePrototype(proto, out var result))
            result = proto;

        // <WhiteDream> - Blood Cult
        // An id that is not a prototype at all (an EntityUid printed as text, say) used to reach
        // RadialMenuEntityPrototypeIconSpecifier and throw EntityCreationException from inside the
        // BUI constructor, which killed the entire menu instead of costing one icon. Guarding here
        // also puts `result` to use: it was being computed and then thrown away.
        if (_proto.TryIndex(result, out var resultProto) && resultProto is not null)
            return RadialMenuIconSpecifier.With(result);

        if (_proto.TryIndex(proto, out var rawProto) && rawProto is not null)
            return RadialMenuIconSpecifier.With(proto);

        return null;
        // </WhiteDream>
    }
}
