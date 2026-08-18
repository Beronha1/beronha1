// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

using Robust.Shared.Prototypes;
using Content.Shared.Antag;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Actions;
using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Shared.Gibbing;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Hands.Systems;
using Content.Trauma.Common.Language.Systems;
using Content.Server.Mind;
using Content.Server.NPC.Systems;
using Content.Server.Pinpointer;
using Content.Server.Roles;
using Content.Server.RoundEnd;
using Content.Server.WhiteDream.BloodCult.Items.BloodSpear;
using Content.Server.WhiteDream.BloodCult.Objectives;
using Content.Server.WhiteDream.BloodCult.RendingRunePlacement;
using Content.Server.WhiteDream.BloodCult.Spells;
using Content.Shared.Cloning.Events;
using Content.Shared.Cuffs.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.Pinpointer;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.WhiteDream.BloodCult.Components;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Items;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.WhiteDream.BloodCult.Gamerule;

public sealed partial class BloodCultRuleSystem : GameRuleSystem<BloodCultRuleComponent>
{
    [Dependency] private IRobustRandom _random = default!;

    [Dependency] private ActionsSystem _actions = default!;
    [Dependency] private AntagSelectionSystem _antagSelection = default!;
    [Dependency] private BloodSpearSystem _bloodSpear = default!;
    [Dependency] private GibbingSystem _gibbing = default!;
    [Dependency] private HumanoidProfileSystem _humanoid = default!; // Trauma
    [Dependency] private ContainerSystem _container = default!;
    [Dependency] private HandsSystem _hands = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private CommonLanguageSystem _language = default!;
    [Dependency] private NavMapSystem _navMap = default!;
    [Dependency] private NpcFactionSystem _faction = default!;
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private RoleSystem _role = default!;
    [Dependency] private RoundEndSystem _roundEnd = default!;
    [Dependency] private TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultRuleComponent, AfterAntagEntitySelectedEvent>(AfterEntitySelected);

        SubscribeLocalEvent<BloodCultNarsieSummoned>(OnNarsieSummon);

        SubscribeLocalEvent<BloodCultistComponent, ComponentInit>(OnCultistComponentInit);
        SubscribeLocalEvent<BloodCultistComponent, ComponentRemove>(OnCultistComponentRemoved);
        SubscribeLocalEvent<BloodCultistComponent, MobStateChangedEvent>(OnCultistsStateChanged);
        SubscribeLocalEvent<BloodCultistComponent, CloningEvent>(OnClone);

        SubscribeLocalEvent<BloodCultistRoleComponent, GetBriefingEvent>(OnGetBriefing);

        InitializeStatus();
    }

    protected override void Started(
        EntityUid uid,
        BloodCultRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args
    )
    {
        base.Started(uid, component, gameRule, args);

        GetRandomRunePlacements(component);

        // WhiteDream - give the cult a few minutes to find each other before they pick a leader.
        ScheduleLeaderVote(component, component.LeaderVoteDelay);
    }

    protected override void AppendRoundEndText(
        EntityUid uid,
        BloodCultRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args
    )
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);
        var winText = Loc.GetString($"blood-cult-condition-{component.WinCondition.ToString().ToLower()}");
        args.AddLine(winText);

        args.AddLine(Loc.GetString("blood-cult-roundend-stats-cultists", ("count", component.Cultists.Count)));
        args.AddLine(Loc.GetString("blood-cult-roundend-stats-constructs", ("count", component.Constructs.Count)));
        args.AddLine(Loc.GetString("blood-cult-roundend-stats-stage",
            ("stage", Loc.GetString(GetStageLocId(component.Stage)))));

        args.AddLine(Loc.GetString("blood-cultists-list-start"));

        var sessionData = _antagSelection.GetAntagIdentifiers(uid);
        foreach (var (_, data, name) in sessionData)
        {
            var lising = Loc.GetString("blood-cultists-list-name", ("name", name), ("user", data.UserName));
            args.AddLine(lising);
        }
    }

    #region EventHandlers

    private void AfterEntitySelected(Entity<BloodCultRuleComponent> ent, ref AfterAntagEntitySelectedEvent args) =>
        MakeCultist(args.EntityUid, ent);

    private void OnNarsieSummon(BloodCultNarsieSummoned ev)
    {
        var rulesQuery = QueryActiveRules();
        while (rulesQuery.MoveNext(out _, out var cult, out _))
        {
            cult.WinCondition = CultWinCondition.Win;

            // <WhiteDream>
            // Query the world instead of cult.Cultists: gibbing mutates that list as we go, which used
            // to throw halfway through and leave everyone but the first cultist as a ghost.
            var cultists = new List<EntityUid>();
            var cultistQuery = EntityQueryEnumerator<BloodCultistComponent>();
            while (cultistQuery.MoveNext(out var cultistUid, out _))
                cultists.Add(cultistUid);

            foreach (var cultist in cultists)
            {
                if (TerminatingOrDeleted(cultist) || !_mind.TryGetMind(cultist, out var mindId, out _))
                    continue;

                var harvester = Spawn(cult.HarvesterPrototype, Transform(cultist).Coordinates);
                _mind.TransferTo(mindId, harvester);
                _language.UpdateEntityLanguages(harvester);
                _gibbing.Gib(cultist);
            }

            // Let them actually be harvesters for a bit before the round is called.
            cult.VictoryEndTime = _timing.CurTime + cult.VictoryEndDelay;
            // </WhiteDream>
            return;
        }
    }

    private void OnCultistComponentInit(Entity<BloodCultistComponent> cultist, ref ComponentInit args)
    {
        _language.AddLanguage(cultist.Owner, cultist.Comp.CultLanguageId);

        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var cult, out _))
        {
            cult.Cultists.Add(cultist);
            UpdateCultStage(cult);

            // WhiteDream - anyone converted after the cult already reached a stage still gets its marks.
            ApplyCurrentStageAppearance(cult, cultist);
        }
    }

    /// <summary>
    ///     Brings a single cultist up to date with the stage the cult is already at.
    /// </summary>
    private void ApplyCurrentStageAppearance(BloodCultRuleComponent cultRule, Entity<BloodCultistComponent> cultist)
    {
        if (cultRule.Stage >= CultStage.RedEyes)
        {
            cultist.Comp.OriginalEyeColor ??= _humanoid.GetEyeColor(_humanoid.GetOrgansData(cultist));
            _humanoid.SetEyeColor(cultist, cultRule.EyeColor);
        }

        if (cultRule.PentagramApplied)
            EnsureComp<PentagramComponent>(cultist);
    }

    private void OnCultistComponentRemoved(Entity<BloodCultistComponent> cultist, ref ComponentRemove args)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var cult, out _))
            cult.Cultists.Remove(cultist);

        CheckWinCondition();

        if (TerminatingOrDeleted(cultist.Owner))
            return;

        RemoveAllCultItems(cultist);
        RemoveCultistAppearance(cultist);
        RemoveObjectiveAndRole(cultist.Owner);
        _language.RemoveLanguage(cultist.Owner, cultist.Comp.CultLanguageId);

        if (!TryComp(cultist, out BloodCultSpellsHolderComponent? powersHolder))
            return;

        foreach (var power in powersHolder.SelectedSpells)
            _actions.RemoveAction(cultist.Owner, power);
    }

    private void OnCultistsStateChanged(Entity<BloodCultistComponent> cultist, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        CheckWinCondition();
        CheckLeaderAlive(cultist); // WhiteDream - Nar'Sie calls a new vote if her voice fell.
    }

    private void OnClone(Entity<BloodCultistComponent> cultist, ref CloningEvent args) =>
        RemoveObjectiveAndRole(cultist);

    private void OnGetBriefing(Entity<BloodCultistRoleComponent> cultist, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("blood-cult-role-briefing-short"));
        var rulesQuery = QueryActiveRules();
        while (rulesQuery.MoveNext(out _, out var rule, out _))
        {
            if (!rule.EmergencyMarkersMode)
                continue;

            args.Append(
                Loc.GetString("blood-cult-role-briefing-emergency-rending", ("amount", rule.EmergencyMarkersCount)));
            return;
        }

        // WhiteDream - beacon-picked sites go in the briefing too.
        var siteQuery = QueryActiveRules();
        while (siteQuery.MoveNext(out _, out var siteRule, out _))
        {
            foreach (var site in GetAvailableRendingSites(siteRule))
                args.Append(Loc.GetString("blood-cult-role-briefing-rending-site", ("location", site.Name)));
        }

        var query = EntityQueryEnumerator<RendingRunePlacementMarkerComponent>();
        while (query.MoveNext(out var uid, out var marker))
        {
            if (!marker.IsActive)
                continue;

            var navMapLocation = FormattedMessage.RemoveMarkupPermissive(_navMap.GetNearestBeaconString(uid));
            var coordinates = Transform(uid).Coordinates;
            var msg = Loc.GetString(
                "blood-cult-role-briefing-rending-locations",
                ("location", navMapLocation),
                ("coordinates", coordinates.Position));
            args.Append(msg); // WhiteDream - msg is already localised
        }
    }

    #endregion

    // Trauma - rule + antag specifier used when converting an existing player mid-round
    private static readonly EntProtoId DefaultRule = "BloodCult";

    public void Convert(EntityUid target)
    {
        if (!TryComp(target, out ActorComponent? actor))
            return;

        // <WhiteDream>
        // Two bugs lived here. It used to bail unless the rule entity carried an
        // AntagSelectionComponent (it doesn't), and then it asked for BloodCultistMidround, which is
        // not one of the rule's own antag definitions - "Antag Prototype ... does not exist".
        // This is exactly what the admin verb does, and that has always worked.
        _antagSelection.ForceMakeAntag<BloodCultRuleComponent>(actor.PlayerSession, DefaultRule);
        // </WhiteDream>
    }

    public bool IsObjectiveFinished() =>
        !TryGetTarget(out var target) || !HasComp<MobStateComponent>(target) || _mobState.IsDead(target.Value);

    public bool TryGetTarget([NotNullWhen(true)] out EntityUid? target)
    {
        target = GetTarget();
        return target is not null;
    }

    public EntityUid? GetTarget()
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var bloodCultRule, out _))
            if (bloodCultRule.OfferingTarget.HasValue)
                return bloodCultRule.OfferingTarget.Value;

        return null;
    }

    public bool IsTarget(EntityUid entityUid)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var rule, out _))
            return entityUid == rule.OfferingTarget;

        return false;
    }

    public int GetTotalCultists()
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var rule, out _))
            return rule.Cultists.Count + rule.Constructs.Count;

        return 0;
    }

    public void RemoveObjectiveAndRole(EntityUid uid)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;

        var objectives = mind.Objectives.FindAll(HasComp<KillTargetCultComponent>);
        foreach (var obj in objectives)
            _mind.TryRemoveObjective(mindId, mind, mind.Objectives.IndexOf(obj));

        if (_role.MindHasRole<BloodCultistRoleComponent>(mindId))
            _role.MindRemoveRole<BloodCultistRoleComponent>(mindId);
    }

    public bool CanDrawRendingRune(EntityUid user)
    {
        var ruleQuery = QueryActiveRules();
        while (ruleQuery.MoveNext(out _, out var rule, out _))
        {
            if (rule is { EmergencyMarkersMode: true, EmergencyMarkersCount: > 0 })
                return true;

            // WhiteDream - beacon-picked sites. Checking only, the site is spent on activation.
            if (IsNearRendingSite(rule, user, out _))
                return true;
        }

        var query = EntityQueryEnumerator<RendingRunePlacementMarkerComponent>();
        while (query.MoveNext(out var uid, out var marker))
        {
            if (!marker.IsActive)
                continue;

            var userLocation = Transform(user).Coordinates;
            var placementCoordinates = Transform(uid).Coordinates;
            if (_transform.InRange(placementCoordinates, userLocation, marker.DrawingRange))
                return true;
        }

        return false;
    }

    public void SetRandomCultTarget(BloodCultRuleComponent rule)
    {
        var querry =
            EntityQueryEnumerator<MindContainerComponent, HumanoidProfileComponent, ActorComponent>();

        var potentialTargets = new List<EntityUid>();

        while (querry.MoveNext(out var uid, out _, out _, out _))
        {
            if (HasComp<BloodCultistComponent>(uid))
                continue;

            potentialTargets.Add(uid);
        }

        rule.OfferingTarget = potentialTargets.Count > 0 ? _random.Pick(potentialTargets) : null;
    }

    public bool TryConsumeNearestMarker(EntityUid user)
    {
        var ruleQuery = QueryActiveRules();
        while (ruleQuery.MoveNext(out _, out var rule, out _))
        {
            if (rule is { EmergencyMarkersMode: true, EmergencyMarkersCount: > 0 })
            {
                rule.EmergencyMarkersCount--;
                return true;
            }

            // WhiteDream - spend the beacon site the cultist is standing at.
            if (IsNearRendingSite(rule, user, out var site) && site is not null)
            {
                site.Used = true;
                return true;
            }
        }

        var userLocation = Transform(user).Coordinates;
        var query = EntityQueryEnumerator<RendingRunePlacementMarkerComponent>();
        while (query.MoveNext(out var markerUid, out var marker))
        {
            if (!marker.IsActive)
                continue;

            var placementCoordinates = Transform(markerUid).Coordinates;
            if (!_transform.InRange(placementCoordinates, userLocation, marker.DrawingRange))
                continue;

            marker.IsActive = false;
            break;
        }

        return false;
    }

    private void CheckWinCondition()
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var cult, out _))
        {
            var aliveCultists = cult.Cultists.Count(cultist => !_mobState.IsDead(cultist));
            if (aliveCultists != 0)
                return;

            cult.WinCondition = CultWinCondition.Failure;
        }
    }

    private void MakeCultist(EntityUid cultist, Entity<BloodCultRuleComponent> rule)
    {
        if (!_mind.TryGetMind(cultist, out var mindId, out var mind))
            return;

        EnsureComp<BloodCultSpellsHolderComponent>(cultist);

        _faction.RemoveFaction(cultist, rule.Comp.NanoTrasenFaction);
        _faction.AddFaction(cultist, rule.Comp.BloodCultFaction);

        if (rule.Comp.OfferingTarget is null)
            SetRandomCultTarget(rule.Comp);

        if (rule.Comp.OfferingTarget is { } target && target != cultist)
            _mind.TryAddObjective(mindId, mind, "KillTargetCultObjective");

        // WhiteDream - the round start popup was removed: the antag briefing (chat + character menu)
        // already carries the same text, and the Study the Veil action gives live status on demand.
    }

    private static string GetStageLocId(CultStage stage) => stage switch
    {
        CultStage.RedEyes => "blood-cult-stage-red-eyes",
        CultStage.Pentagram => "blood-cult-stage-pentagram",
        _ => "blood-cult-stage-start",
    };

    private void GetRandomRunePlacements(BloodCultRuleComponent component)
    {
        var allMarkers = EntityQuery<RendingRunePlacementMarkerComponent>().ToList();
        if (allMarkers.Count == 0)
        {
            // WhiteDream - no mapper placed markers, so pick a few station beacons instead. The rune
            // stays restricted to a handful of named places rather than "anywhere".
            PickRendingSitesFromBeacons(component);
            return;
        }

        var maxRunes = component.RendingRunePlacementsAmount;
        if (allMarkers.Count < component.RendingRunePlacementsAmount)
            maxRunes = allMarkers.Count;

        for (var i = maxRunes; i > 0; i--)
        {
            var marker = _random.PickAndTake(allMarkers);
            marker.IsActive = true;
        }
    }

    /// <summary>
    ///     Chooses the places where the veil is thin from the station's own beacons.
    /// </summary>
    private void PickRendingSitesFromBeacons(BloodCultRuleComponent component)
    {
        var beacons = new List<EntityUid>();
        var query = EntityQueryEnumerator<NavMapBeaconComponent>();
        while (query.MoveNext(out var uid, out var beacon))
        {
            // WhiteDream - only beacons that belong to a station. Otherwise the veil ends up thin
            // on the escape shuttle, on debris, or on some off-station ruin.
            if (beacon.Enabled && _station.GetOwningStation(uid) is not null)
                beacons.Add(uid);
        }

        if (beacons.Count == 0)
        {
            // Truly nothing to anchor to. Fall back to the old free-for-all so the round isn't stuck.
            component.EmergencyMarkersMode = true;
            component.EmergencyMarkersCount = component.RendingRunePlacementsAmount;
            return;
        }

        _random.Shuffle(beacons);
        var amount = Math.Min(component.RendingRunePlacementsAmount, beacons.Count);

        for (var i = 0; i < amount; i++)
        {
            var beacon = beacons[i];
            component.RendingSites.Add(new RendingSite
            {
                Beacon = beacon,
                Name = FormattedMessage.RemoveMarkupPermissive(_navMap.GetNearestBeaconString(beacon)),
            });
        }
    }

    /// <summary>
    ///     Every site the cult can still tear open.
    /// </summary>
    public IEnumerable<RendingSite> GetAvailableRendingSites(BloodCultRuleComponent component)
    {
        return component.RendingSites.Where(site => !site.Used && !TerminatingOrDeleted(site.Beacon));
    }

    private bool IsNearRendingSite(BloodCultRuleComponent component, EntityUid user, out RendingSite? found)
    {
        found = null;
        var userLocation = Transform(user).Coordinates;

        foreach (var site in GetAvailableRendingSites(component))
        {
            if (!_transform.InRange(Transform(site.Beacon).Coordinates, userLocation, component.RendingSiteRange))
                continue;

            found = site;
            return true;
        }

        return false;
    }

    private void RemoveAllCultItems(Entity<BloodCultistComponent> cultist)
    {
        if (!_inventory.TryGetContainerSlotEnumerator(cultist.Owner, out var enumerator))
            return;

        _bloodSpear.DetachSpearFromMaster(cultist);
        while (enumerator.MoveNext(out var container))
            if (container.ContainedEntity != null && HasComp<CultItemComponent>(container.ContainedEntity.Value))
                _container.Remove(container.ContainedEntity.Value, container, true, true);

        foreach (var item in _hands.EnumerateHeld((cultist.Owner, null)))
            if (TryComp(item, out CultItemComponent? cultItem) && !cultItem.AllowUseToEveryone &&
                !_hands.TryDrop(cultist.Owner, item, null, false, false))
                QueueDel(item);
    }

    private void RemoveCultistAppearance(Entity<BloodCultistComponent> cultist)
    {
        // Trauma - eye colour is stored on the eye organ now
        if (cultist.Comp.OriginalEyeColor is { } originalEyeColor)
            _humanoid.SetEyeColor(cultist, originalEyeColor);

        RemComp<PentagramComponent>(cultist);
    }

    private void UpdateCultStage(BloodCultRuleComponent cultRule)
    {
        var cultistsCount = cultRule.Cultists.Count;
        var prevStage = cultRule.Stage;

        if (cultistsCount >= cultRule.PentagramThreshold)
        {
            cultRule.Stage = CultStage.Pentagram;
            SelectRandomLeader(cultRule);
        }
        else if (cultistsCount >= cultRule.ReadEyeThreshold)
            cultRule.Stage = CultStage.RedEyes;
        else
            cultRule.Stage = CultStage.Start;

        if (cultRule.Stage != prevStage)
            UpdateCultistsAppearance(cultRule, prevStage);
    }

    private void UpdateCultistsAppearance(BloodCultRuleComponent cultRule, CultStage prevStage)
    {
        switch (cultRule.Stage)
        {
            case CultStage.Start when prevStage == CultStage.RedEyes:
                foreach (var cultist in cultRule.Cultists)
                    RemoveCultistAppearance(cultist);

                break;
            case CultStage.RedEyes when prevStage == CultStage.Start:
                foreach (var cultist in cultRule.Cultists)
                {
                    // Trauma - eye colour is stored on the eye organ now
                    cultist.Comp.OriginalEyeColor ??= _humanoid.GetEyeColor(_humanoid.GetOrgansData(cultist));
                    _humanoid.SetEyeColor(cultist, cultRule.EyeColor);
                }

                break;
            case CultStage.Pentagram:
                // WhiteDream - warn the cult first, brand them two minutes later.
                BeginAscension(cultRule);
                break;
        }
    }

    /// <summary>
    ///     A crutch while we have no NORMAL voting system. The DarkRP one fucking sucks.
    /// </summary>
    private void SelectRandomLeader(BloodCultRuleComponent cultRule)
    {
        if (cultRule.LeaderSelected)
            return;

        var candidats = cultRule.Cultists;
        candidats.RemoveAll(
            entity =>
                TryComp(entity, out PullableComponent? pullable) && pullable.BeingPulled ||
                TryComp(entity, out CuffableComponent? cuffable) && cuffable.CuffedHandCount > 0);

        if (candidats.Count == 0)
            return;

        var leader = _random.Pick(candidats);
        AddComp<BloodCultLeaderComponent>(leader);
        cultRule.LeaderSelected = true;
        cultRule.CultLeader = leader;
    }
}
