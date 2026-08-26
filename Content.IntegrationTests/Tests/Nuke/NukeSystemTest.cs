using Content.IntegrationTests.Fixtures;
using Content.IntegrationTests.Fixtures.Attributes;
using Content.Server.Nuke;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;

namespace Content.IntegrationTests.Tests.Nuke;

[TestOf(typeof(NukeSystem))]
public sealed class NukeSystemTest : GameTest
{
    private const string TestNuke = "TestNuclearBomb";

    [TestPrototypes]
    private const string Prototypes = $"""
        - type: entity
          parent: NuclearBomb
          id: {TestNuke}
          components:
          - type: Nuke
            explosionType: Default
            totalIntensity: 10
            intensitySlope: 5
            maxIntensity: 5
        """;

    [SidedDependency(Side.Server)] private readonly NukeSystem _nuke = default!;
    [SidedDependency(Side.Server)] private readonly DamageableSystem _damageable = default!;

    [Test]
    public async Task ActivatingNukeQueuesPhysicalExplosion()
    {
        var map = await Pair.CreateTestMap();
        EntityUid target = default;
        FixedPoint2 initialDamage = default;

        await Server.WaitAssertion(() =>
        {
            target = SEntMan.SpawnEntity("MobMouse", map.GridCoords);
            initialDamage = _damageable.GetTotalDamage(target);

            var bomb = SEntMan.SpawnEntity(TestNuke, map.GridCoords);
            var component = SEntMan.GetComponent<NukeComponent>(bomb);

            _nuke.ActivateBomb(bomb, component);
        });

        await Pair.RunTicksSync(5);

        await Server.WaitAssertion(() =>
        {
            Assert.That(_damageable.GetTotalDamage(target), Is.GreaterThan(initialDamage),
                "The nuclear bomb completed activation without producing a physical explosion.");
        });
    }
}
