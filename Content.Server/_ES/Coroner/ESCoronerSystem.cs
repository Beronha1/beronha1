using System.Linq;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Shared.Body;
using Content.Shared._ES.Coroner;
using Content.Shared._ES.Masks;
using Content.Shared._ES.Masks.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.ColorNaming;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._ES.Coroner;

public sealed partial class ESCoronerSystem : ESSharedCoronerSystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private GameTicker _gameTicker = default!;
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private HumanoidProfileSystem _humanoidProfile = default!;

    protected override FormattedMessage GetReport(EntityUid target)
    {
        var msg = new FormattedMessage();
        if (_humanoidProfile.CreateProfile(target) is not { } profile)
            return msg;

        var name = Name(target);
        var age = profile.Age;
        var sex = profile.Sex switch
        {
            Sex.Male => Loc.GetString("es-clue-sex-male"),
            Sex.Female => Loc.GetString("es-clue-sex-female"),
            _ => Loc.GetString("es-clue-sex-nb"),
        };
        var eye = ColorNaming.Describe(profile.Appearance.EyeColor, Loc);
        var hairColor = profile.Appearance.Markings.Values
            .SelectMany(markings => markings.GetValueOrDefault(HumanoidVisualLayers.Hair) ?? [])
            .SelectMany(marking => marking.MarkingColors)
            .Cast<Color?>()
            .FirstOrDefault();
        var hair = hairColor is { } color
            ? ColorNaming.Describe(color, Loc)
            : Loc.GetString("es-clue-hair-none");

        var timeOfDeath = _timing.CurTime;
        if (_mind.TryGetMind(target, out _, out var mind) && mind.TimeOfDeath.HasValue)
            timeOfDeath = mind.TimeOfDeath.Value;
        var time = (timeOfDeath - _gameTicker.RoundStartTimeSpan).ToString("hh\\:mm\\:ss");

        var mask = TryComp<ESBodyLastMaskComponent>(target, out var bodyLastMask)
            ? _prototype.Index(bodyLastMask.LastMask)
            : _random.Pick(_prototype.EnumeratePrototypes<ESMaskPrototype>().Where(p => !p.Abstract).ToList());

        msg.AddMarkupPermissive(Loc.GetString("es-coroner-report-paper",
            ("name", name),
            ("age", age),
            ("sex", sex),
            ("eye", eye),
            ("hair", hair),
            ("time", time),
            ("mask1", Loc.GetString(mask.Name))));
        return msg;
    }
}
