// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

using Content.Client.UserInterface.Controls;
using Content.Shared.WhiteDream.BloodCult.Runes;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client.WhiteDream.BloodCult.Runes.UI;

[UsedImplicitly]
public sealed partial class RuneDrawerBUI : BoundUserInterface
{
    [Dependency] private IPrototypeManager _protoManager = default!;

    private readonly SimpleRadialMenu _menu;

    public RuneDrawerBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        // <Whiskey>
        // Was a hand-built RadialMenu + RadialContainer, which is the older control: it draws no
        // backdrop, so the runes floated bare over the game, and it opened at the mouse, which put
        // the whole menu down by the hotbar whenever the dagger was clicked there. SimpleRadialMenu
        // is what the spell selector already uses, so the two cult menus now look and behave alike.
        // </Whiskey>
        _menu = this.CreateWindow<SimpleRadialMenu>();
    }

    protected override void Open()
    {
        base.Open();

        _menu.OpenCentered();

        if (State is RuneDrawerMenuState runeDrawerState)
            FillMenu(runeDrawerState.AvailalbeRunes);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is RuneDrawerMenuState runeDrawerState)
            FillMenu(runeDrawerState.AvailalbeRunes);
    }

    private void FillMenu(List<ProtoId<RuneSelectorPrototype>>? runes = null)
    {
        if (runes is null)
            return;

        var models = new List<RadialMenuOptionBase>();

        foreach (var runeSelector in runes)
        {
            if (!_protoManager.TryIndex(runeSelector, out var runeSelectorProto) ||
                !_protoManager.TryIndex(runeSelectorProto.Prototype, out var runeProto))
                continue;

            models.Add(new RadialMenuActionOption<ProtoId<RuneSelectorPrototype>>(OnRunePressed, runeSelector)
            {
                // EntityPrototype.Name is already localised text, not a loc id.
                ToolTip = runeProto.Name,
                IconSpecifier = RadialMenuIconSpecifier.With(runeSelectorProto.Prototype)
            });
        }

        _menu.SetButtons(models);
    }

    private void OnRunePressed(ProtoId<RuneSelectorPrototype> runeSelector)
    {
        SendMessage(new RuneDrawerSelectedMessage(runeSelector));
        Close();
    }
}
