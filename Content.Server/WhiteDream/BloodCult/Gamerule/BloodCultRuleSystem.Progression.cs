// WhiteDream - objective bookkeeping, the pentagram grace period and the victory wind-down.
using System.Linq;
using Content.Shared.Mobs.Components;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Components;
using Content.Shared.WhiteDream.BloodCult.Runes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Gamerule;

public sealed partial class BloodCultRuleSystem
{
    private static readonly EntProtoId SacrificeObjective = "KillTargetCultObjective";
    private static readonly EntProtoId SummonObjective = "SummonNarsieObjective";

    private static readonly TimeSpan ObjectiveCheckInterval = TimeSpan.FromSeconds(5);

    private static readonly ProtoId<RuneSelectorPrototype> RendingSelector = "CultRuneDimensionalRending";

    private static readonly SoundSpecifier AscensionSound =
        new SoundPathSpecifier("/Audio/WhiteDream/BloodCult/curse.ogg");

    [Dependency] private IPrototypeManager _proto = default!;

    private void TickProgression(BloodCultRuleComponent rule)
    {
        var now = _timing.CurTime;

        if (now >= rule.NextObjectiveCheck)
        {
            rule.NextObjectiveCheck = now + ObjectiveCheckInterval;
            EnsureOfferingTarget(rule);
            EnsureObjectives(rule);
            CheckRendingUnlocked(rule);
        }

        if (rule.PentagramTime is { } pentagramTime && now >= pentagramTime)
        {
            rule.PentagramTime = null;
            ApplyPentagrams(rule);
        }

        TickLeaderVote(rule);

        if (rule.VictoryEndTime is { } endTime && now >= endTime)
        {
            rule.VictoryEndTime = null;
            _roundEnd.EndRound();
        }
    }

    #region Objectives

    /// <summary>
    ///     The target used to be picked exactly once, when the first cultist was made. If nobody valid
    ///     had spawned yet it stayed null for the whole round and the cult never got a sacrifice.
    /// </summary>
    private void EnsureOfferingTarget(BloodCultRuleComponent rule)
    {
        // Nar'Sie names one offering per round. Once she has named them, that's it - dead, gibbed or
        // spaced, the objective stands.
        if (rule.OfferingTarget is { } target)
        {
            // The only re-roll: they joined us, so they can't be given to her.
            if (!TerminatingOrDeleted(target) && !HasComp<BloodCultistComponent>(target))
                return;

            if (TerminatingOrDeleted(target) || _mobState.IsDead(target))
                return;
        }

        var previous = rule.OfferingTarget;
        SetRandomCultTarget(rule);

        if (rule.OfferingTarget is not { } picked || picked == previous)
            return;

        NotifyCultists(Loc.GetString("cult-offering-target-chosen", ("name", Name(picked))));
    }

    /// <summary>
    ///     Hands out the sacrifice and summon objectives. Runs on a tick because the mind role isn't
    ///     always in place at the moment the cultist component is added.
    /// </summary>
    private void EnsureObjectives(BloodCultRuleComponent rule)
    {
        if (rule.OfferingTarget is null)
            return;

        foreach (var cultist in rule.Cultists)
        {
            if (cultist.Comp.ObjectivesGranted || TerminatingOrDeleted(cultist.Owner))
                continue;

            if (!_mind.TryGetMind(cultist.Owner, out var mindId, out var mind))
                continue;

            // The sacrifice target can't be one of us.
            if (rule.OfferingTarget != cultist.Owner)
                _mind.TryAddObjective(mindId, mind, SacrificeObjective);

            _mind.TryAddObjective(mindId, mind, SummonObjective);
            cultist.Comp.ObjectivesGranted = true;
        }
    }

    /// <summary>
    ///     How many cultists the rending rune needs, read straight off the rune selector so the
    ///     prototype stays the single source of truth.
    /// </summary>
    public int GetRendingCultistsRequired()
    {
        return _proto.TryIndex(RendingSelector, out var selector) ? selector.RequiredTotalCultists : 0;
    }

    public bool CanRendingBeDrawn(BloodCultRuleComponent rule)
    {
        return rule.Cultists.Count >= GetRendingCultistsRequired() && IsObjectiveFinished();
    }

    /// <summary>
    ///     Tells the cult, once, the moment both conditions are finally met.
    /// </summary>
    private void CheckRendingUnlocked(BloodCultRuleComponent rule)
    {
        if (rule.RendingUnlockedAnnounced || rule.VeilWeakened || !CanRendingBeDrawn(rule))
            return;

        rule.RendingUnlockedAnnounced = true;

        NotifyCultists(Loc.GetString("cult-rending-unlocked"));

        if (rule.EmergencyMarkersMode)
        {
            NotifyCultists(Loc.GetString("cult-status-rending-emergency", ("amount", rule.EmergencyMarkersCount)));
            return;
        }

        foreach (var site in GetAvailableRendingSites(rule))
            NotifyCultists(Loc.GetString("cult-status-rending-location", ("location", site.Name)));
    }

    #endregion

    #region Ascension

    /// <summary>
    ///     The cult hit the pentagram threshold. Warn them first, brand them later.
    /// </summary>
    private void BeginAscension(BloodCultRuleComponent rule)
    {
        if (rule.PentagramApplied || rule.PentagramTime is not null)
            return;

        rule.PentagramTime = _timing.CurTime + rule.PentagramWarningDelay;

        NotifyCultists(Loc.GetString("cult-ascension-warning",
            ("minutes", (int) Math.Round(rule.PentagramWarningDelay.TotalMinutes))));

        // The station notices before the crew knows why.
        FlickerStationLights(TimeSpan.FromSeconds(6));

        _chat.DispatchGlobalAnnouncement(Loc.GetString("cult-ascension-centcom-announcement"),
            Loc.GetString("cult-ascension-centcom-sender"),
            true,
            colorOverride: Color.Goldenrod);
    }

    private void ApplyPentagrams(BloodCultRuleComponent rule)
    {
        rule.PentagramApplied = true;

        foreach (var cultist in rule.Cultists)
        {
            if (!TerminatingOrDeleted(cultist.Owner))
                EnsureComp<PentagramComponent>(cultist);
        }

        NotifyCultists(Loc.GetString("cult-ascension-marked"));

        FlickerStationLights(TimeSpan.FromSeconds(10));
        PlayGlobalCultSound(AscensionSound, -4f);
    }

    #endregion
}
