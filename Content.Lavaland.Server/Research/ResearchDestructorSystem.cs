// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Shared.Research;
using Content.Server.Power.EntitySystems;
using Content.Server.Research.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Research.Components;
using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Research;

/// <summary>
/// Consumes boss artifacts and writes their points/direct technology unlocks to the
/// research server connected through ResearchClientComponent.
/// </summary>
public sealed partial class ResearchDestructorSystem : EntitySystem
{
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;
    [Dependency] private ResearchSystem _research = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResearchDestructorComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<ResearchDestructorComponent, ResearchDestructorDoAfterEvent>(OnAnalyzeComplete);
    }

    private void OnInteractUsing(Entity<ResearchDestructorComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || ent.Comp.Busy || !TryComp<ResearchArtifactComponent>(args.Used, out var artifact))
            return;

        if (!this.IsPowered(ent, EntityManager))
        {
            _popup.PopupEntity(Loc.GetString("research-destructor-no-power"), ent, args.User);
            return;
        }

        if (!_research.TryGetClientServer(ent, out _, out _))
        {
            _popup.PopupEntity(Loc.GetString("research-destructor-no-server"), ent, args.User);
            return;
        }

        if (ent.Comp.PendingArtifact != args.Used || _timing.CurTime > ent.Comp.PendingUntil)
        {
            ent.Comp.PendingArtifact = args.Used;
            ent.Comp.PendingUntil = _timing.CurTime + ent.Comp.ConfirmationWindow;
            args.Handled = true;

            _popup.PopupEntity(
                Loc.GetString(
                    "research-destructor-confirm",
                    ("points", artifact.Points),
                    ("technologies", DescribeTechnologies(artifact.Technologies))),
                ent,
                args.User,
                PopupType.MediumCaution);
            return;
        }

        ent.Comp.PendingArtifact = null;
        ent.Comp.PendingUntil = TimeSpan.Zero;

        var doAfter = new DoAfterArgs(
            EntityManager,
            args.User,
            ent.Comp.AnalyzeTime,
            new ResearchDestructorDoAfterEvent(),
            ent,
            target: ent,
            used: args.Used)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
            return;

        ent.Comp.Busy = true;
        args.Handled = true;
        _popup.PopupEntity(Loc.GetString("research-destructor-start"), ent, args.User);
    }

    private void OnAnalyzeComplete(Entity<ResearchDestructorComponent> ent, ref ResearchDestructorDoAfterEvent args)
    {
        ent.Comp.Busy = false;

        if (args.Cancelled || args.Handled || args.Used is not { } artifact ||
            !TryComp<ResearchArtifactComponent>(artifact, out var researchArtifact) ||
            !this.IsPowered(ent, EntityManager) ||
            !_research.TryGetClientServer(ent, out var server, out _) ||
            !TryComp<TechnologyDatabaseComponent>(server, out var database))
        {
            return;
        }

        if (researchArtifact.Points != 0)
            _research.ModifyServerPoints(server.Value, researchArtifact.Points);

        var unlocked = new List<string>();
        foreach (var technologyId in researchArtifact.Technologies)
        {
            var alreadyUnlocked = false;
            foreach (var unlockedTechnology in database.UnlockedTechnologies)
            {
                if (unlockedTechnology != technologyId)
                    continue;

                alreadyUnlocked = true;
                break;
            }

            if (alreadyUnlocked || !_prototypes.TryIndex<TechnologyPrototype>(technologyId, out var technology))
            {
                continue;
            }

            _research.AddTechnology(server.Value, technology, database);
            unlocked.Add(Loc.GetString(technology.Name));
        }

        _research.SyncClientWithServer(ent);
        QueueDel(artifact);
        args.Handled = true;

        _popup.PopupEntity(
            Loc.GetString(
                "research-destructor-complete",
                ("points", researchArtifact.Points),
                ("technologies", unlocked.Count == 0
                    ? Loc.GetString("research-destructor-no-new-technologies")
                    : string.Join(", ", unlocked))),
            ent,
            args.User,
            PopupType.Medium);
    }

    private string DescribeTechnologies(IEnumerable<ProtoId<TechnologyPrototype>> technologyIds)
    {
        var names = new List<string>();
        foreach (var technologyId in technologyIds)
        {
            if (_prototypes.TryIndex<TechnologyPrototype>(technologyId, out var technology))
                names.Add(Loc.GetString(technology.Name));
        }

        return names.Count == 0
            ? Loc.GetString("research-destructor-no-new-technologies")
            : string.Join(", ", names);
    }
}
