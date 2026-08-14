using Content.Server.Atmos.Rotting;
using Content.Server.Body.Systems;
using Content.Server.DoAfter;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Forensics;
using Content.Server.Popups;
using Content.Shared.Body.Components;
using Content.Shared.Body;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Content.Shared.Gibbing;
using Content.Shared._Impstation.Kodepiia;
using Content.Shared._Impstation.Kodepiia.Components;
using System.Diagnostics.CodeAnalysis;
using Robust.Server.GameObjects;

namespace Content.Server._Impstation.Kodepiia;

public sealed partial class ConsumeSystem : SharedKodepiiaConsumeSystem
{
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private BodySystem _body = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private GibbingSystem _gibbing = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private ForensicsSystem _forensics = default!;
    [Dependency] private IngestionSystem _ingestion = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private PuddleSystem _puddle = default!;
    [Dependency] private RottingSystem _rotting = default!;
    [Dependency] private SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private StomachSystem _stomach = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KodepiiaConsumeActionComponent, KodepiiaConsumeEvent>(Consume);
        SubscribeLocalEvent<KodepiiaConsumeActionComponent, KodepiiaConsumeDoAfterEvent>(ConsumeDoAfter);
    }

    public bool CanConsume(Entity<KodepiiaConsumeActionComponent> performer, EntityUid target, [NotNullWhen(false)] out string? failMessage)
    {
        failMessage = null;

        EntityUid targetIdentity = Identity.Entity(target, EntityManager);

        if (!_ingestion.HasMouthAvailable(performer, performer))
            failMessage = Loc.GetString("kodepiia-consume-fail-blocked");
        else if (!_whitelist.CheckBoth(target, performer.Comp.Blacklist, performer.Comp.Whitelist))
            failMessage = Loc.GetString("kodepiia-consume-fail-inedible", ("target", targetIdentity));
        else if (!_mobState.IsIncapacitated(target))
            failMessage = Loc.GetString("kodepiia-consume-fail-not-incapacitated", ("target", targetIdentity));

        return failMessage is null;
    }

    public void Consume(Entity<KodepiiaConsumeActionComponent> ent, ref KodepiiaConsumeEvent args)
    {
        if (!CanConsume(ent, args.Target, out string? failMessage))
        {
            _popup.PopupEntity(failMessage, ent, ent);
            return;
        }

        PlayConsumeSound(ent);

        if (!TryComp(args.Performer, out PhysicsComponent? performerPhysics)
            || !TryComp(args.Target, out PhysicsComponent? targetPhysics))
            return;

        string popupSelf = Loc.GetString("kodepiia-consume-start-self",
            ("user", Identity.Entity(ent, EntityManager)),
            ("target", Identity.Entity(args.Target, EntityManager)));
        string popupOthers = Loc.GetString("kodepiia-consume-start-others",
            ("user", Identity.Entity(ent, EntityManager)),
            ("target", Identity.Entity(args.Target, EntityManager)));

        _popup.PopupEntity(popupSelf, ent, ent);
        _popup.PopupEntity(popupOthers, ent, Filter.Pvs(ent).RemovePlayersByAttachedEntity(ent), true, PopupType.MediumCaution);

        float consumeTime = targetPhysics.Mass / performerPhysics.Mass * ent.Comp.BaseConsumeSpeed;

        DoAfterArgs doAfterArgs = new DoAfterArgs(EntityManager, ent, consumeTime, new KodepiiaConsumeDoAfterEvent(), ent, args.Target)
        {
            DistanceThreshold = 1.5f,
            BreakOnDamage = true,
            BreakOnHandChange = false,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            AttemptFrequency = AttemptFrequency.StartAndEnd
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }

    public void ConsumeDoAfter(Entity<KodepiiaConsumeActionComponent> ent, ref KodepiiaConsumeDoAfterEvent args)
    {
        if (args.Target == null || args.Cancelled || !TryComp<PhysicsComponent>(args.Target, out var targetPhysics))
            return;

        if (!_body.TryGetOrgansWithComponent<StomachComponent>(ent.Owner, out var stomachs))
            return;

        var highestAvailable = FixedPoint2.Zero;
        Entity<StomachComponent>? stomachToUse = null;
        foreach (var stomach in stomachs)
        {
            var owner = stomach.Owner;
            if (!_solutionContainer.ResolveSolution(owner, "stomach", ref stomach.Comp.Solution, out var stomachSol))
                continue;

            if (stomachSol.AvailableVolume <= highestAvailable)
                continue;

            stomachToUse = stomach;
            highestAvailable = stomachSol.AvailableVolume;
        }

        // All stomachs are full or we have no stomachs
        if (stomachToUse == null)
        {
            _popup.PopupClient(Loc.GetString("ingestion-you-cannot-ingest-any-more", ("verb", "eat")), ent, ent);
            return;
        }

        // Drink Bloodstream
        _solutionContainer.TryGetSolution(args.Target.Value, ent.Comp.SolutionToDrinkFrom, out var targetSolutionComp, out var targetBloodstream);
        if (targetBloodstream != null && targetSolutionComp != null)
        {
            var foodReagentQuantity = targetPhysics.Mass * ent.Comp.MeatMultiplier;

            var consumedSolution = _solutionContainer.SplitSolution(targetSolutionComp.Value, targetBloodstream.Volume * ent.Comp.PortionDrunk);

            if (_rotting.IsRotten(args.Target.Value))
            {
                consumedSolution.AddReagent(ent.Comp.Toxin, foodReagentQuantity * ent.Comp.ToxinRatio);
                foodReagentQuantity *= 1 - ent.Comp.ToxinRatio; // this math is bad i just know it
            }

            consumedSolution.AddReagent(ent.Comp.FoodReagentPrototype, foodReagentQuantity);

            if (consumedSolution.Volume > highestAvailable)
            {
                var split = consumedSolution.SplitSolution(consumedSolution.Volume - highestAvailable);
                _puddle.TrySpillAt(ent.Owner, split, out _);
            }
            _stomach.TryTransferSolution(stomachToUse.Value.AsNullable(), consumedSolution);
        }

        // Transfer DNA
        _forensics.TransferDna(args.Target.Value, ent, false);

        // Deal Damage
        _damage.TryChangeDamage(args.Target.Value, ent.Comp.Damage, true, false);

        // Play Sound
        PlayConsumeSound(ent);

        var popupSelf = Loc.GetString("kodepiia-consume-end-self",
            ("user", Identity.Entity(ent, EntityManager)),
            ("target", Identity.Entity(args.Target.Value, EntityManager)));
        _popup.PopupEntity(popupSelf, ent, ent);

        var popupOthers = Loc.GetString("kodepiia-consume-end-others",
            ("user", Identity.Entity(ent, EntityManager)),
            ("target", Identity.Entity(args.Target.Value, EntityManager)));
        _popup.PopupEntity(popupOthers, ent, Filter.Pvs(ent).RemovePlayersByAttachedEntity(ent), true, PopupType.MediumCaution);

        //Consumed Componentry Stuff lol
        EnsureComp<KodepiiaConsumedComponent>(args.Target.Value, out var consumed);

        consumed.Count++;
        Dirty(args.Target.Value, consumed);

        if (consumed.Count >= ent.Comp.GibThreshold && HasComp<BodyComponent>(args.Target.Value))
            _gibbing.Gib(args.Target.Value);
    }

    public void PlayConsumeSound(Entity<KodepiiaConsumeActionComponent> ent)
    {
        var soundPool = new SoundCollectionSpecifier("gib");
        _audio.PlayPvs(soundPool, ent, AudioParams.Default.WithVolume(-3f));
    }
}
