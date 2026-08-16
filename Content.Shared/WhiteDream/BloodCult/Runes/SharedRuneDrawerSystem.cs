using System.Linq;
using Content.Trauma.Common.RadialSelector;
using Content.Shared.UserInterface;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Constructs;
using Content.Shared.WhiteDream.BloodCult.Runes;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Shared.WhiteDream.BloodCult.Runes;

public sealed partial class SharedRuneDrawerSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _protoManager = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RuneDrawerComponent, BeforeActivatableUIOpenEvent>(OnBeforeUiOpen);
    }

    private void OnBeforeUiOpen(Entity<RuneDrawerComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateUi(ent);
    }

    public void UpdateUi(EntityUid uid)
    {
        var availableRunes = new List<ProtoId<RuneSelectorPrototype>>();
        var totalCultists = CountCultists();

        foreach (var runeSelector in _protoManager.EnumeratePrototypes<RuneSelectorPrototype>().OrderBy(r => r.ID))
        {
            if (runeSelector.RequiredTotalCultists > totalCultists)
                continue;

            availableRunes.Add(runeSelector.ID);
        }

        _ui.SetUiState(uid, RuneDrawerBuiKey.Key, new RuneDrawerMenuState(availableRunes));
    }

    private int CountCultists()
    {
        var count = 0;

        var cultistQuery = EntityQueryEnumerator<BloodCultistComponent>();
        while (cultistQuery.MoveNext(out _, out _))
            count++;

        var constructQuery = EntityQueryEnumerator<ConstructComponent>();
        while (constructQuery.MoveNext(out _, out _))
            count++;

        return count;
    }
}
