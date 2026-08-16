using System.Linq;
using Content.Shared.Bible.Components;
using Content.Trauma.Common.Language.Systems;
using Content.Shared.Gibbing;
using Content.Server.Cuffs;
using Content.Server.Mind;
using Content.Goobstation.Common.Religion;
using Content.Shared.Stunnable;
using Content.Server.WhiteDream.BloodCult.Gamerule;
using Content.Server.WhiteDream.BloodCult.Runes.Revive;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.Damage.Systems;

namespace Content.Server.WhiteDream.BloodCult.Runes.Offering;

public sealed partial class CultRuneOfferingSystem : EntitySystem
{
    [Dependency] private BloodCultRuleSystem _bloodCultRule = default!;
    [Dependency] private CommonLanguageSystem _language = default!;
    [Dependency] private GibbingSystem _gibbing = default!;
    [Dependency] private CuffableSystem _cuffable = default!;
    [Dependency] private CultRuneBaseSystem _cultRune = default!;
    [Dependency] private CultRuneReviveSystem _cultRuneRevive = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;
    [Dependency] private SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CultRuneOfferingComponent, TryInvokeCultRuneEvent>(OnOfferingRuneInvoked);
    }

    private void OnOfferingRuneInvoked(Entity<CultRuneOfferingComponent> rune, ref TryInvokeCultRuneEvent args)
    {
        var possibleTargets = _cultRune.GetTargetsNearRune(
            rune,
            rune.Comp.OfferingRange,
            entity => HasComp<BloodCultistComponent>(entity));

        if (possibleTargets.Count == 0)
        {
            args.Cancel();
            return;
        }

        var target = possibleTargets.First();
        if (!TryOffer(rune, target, args.User, args.Invokers.Count))
            args.Cancel();
    }

    private bool TryOffer(Entity<CultRuneOfferingComponent> rune, EntityUid target, EntityUid user, int invokersTotal)
    {
        // if the target is dead we should always sacrifice it.
        if (_mobState.IsDead(target))
        {
            Sacrifice(rune, target);
            return true;
        }

        if (!_mind.TryGetMind(target, out _, out _) || _bloodCultRule.IsTarget(target) ||
            HasComp<BibleUserComponent>(target) || HasComp<MindShieldComponent>(target))
            return TrySacrifice(rune, target, invokersTotal);

        return TryConvert(rune, target, user, invokersTotal);
    }

    private bool TrySacrifice(Entity<CultRuneOfferingComponent> rune, EntityUid target, int invokersAmount)
    {
        if (invokersAmount < rune.Comp.AliveSacrificeInvokersAmount)
            return false;

        Sacrifice(rune, target);
        return true;
    }

    private bool TryConvert(Entity<CultRuneOfferingComponent> rune, EntityUid target, EntityUid user, int invokersTotal)
    {
        if (invokersTotal < rune.Comp.ConvertInvokersAmount)
            return false;

        _cultRuneRevive.AddCharges(rune, rune.Comp.ReviveChargesPerOffering);
        Convert(rune, target, user);
        return true;
    }

    private void Sacrifice(Entity<CultRuneOfferingComponent> rune, EntityUid target)
    {
        _cultRuneRevive.AddCharges(rune, rune.Comp.ReviveChargesPerOffering);
        var transform = Transform(target);

        if (!_mind.TryGetMind(target, out var mindId, out _))
            Spawn(rune.Comp.SoulShardGhostProto, transform.Coordinates);
        else
        {
            var shard = Spawn(rune.Comp.SoulShardProto, transform.Coordinates);
            _mind.TransferTo(mindId, shard);
            _mind.UnVisit(mindId);
            _language.UpdateEntityLanguages(shard);
        }

        _gibbing.Gib(target);
    }

    private void Convert(Entity<CultRuneOfferingComponent> rune, EntityUid target, EntityUid user)
    {
        _bloodCultRule.Convert(target);
        _stun.TryAddStunDuration(target, TimeSpan.FromSeconds(2f));
        if (TryComp(target, out CuffableComponent? cuffs) && cuffs.Container.ContainedEntities.Count >= 1)
        {
            var lastAddedCuffs = cuffs.Container.ContainedEntities[^1];
            _cuffable.Uncuff(target, user, lastAddedCuffs);
        }

        _statusEffects.TryRemoveStatusEffect(target, "Muted");
        _damageable.TryChangeDamage(target, rune.Comp.ConvertHealing);
    }
}
