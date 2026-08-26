using Content.Client.Interactable.Components;
using Content.Client.Stealth;
using Content.IntegrationTests.Fixtures;
using Content.Shared.Stealth.Components;

namespace Content.IntegrationTests.Tests.Stealth;

[TestFixture]
public sealed class StealthOutlineTest : GameTest
{
    [Test]
    public async Task InteractionOutlineIsHiddenAndRestoredWithStealth()
    {
        EntityUid entity = default;

        await Client.WaitPost(() =>
        {
            entity = CEntMan.Spawn("BigBox");
            Assert.That(CEntMan.HasComponent<InteractionOutlineComponent>(entity), Is.True);

            CEntMan.EnsureComponent<StealthComponent>(entity);
        });

        // The outline is removed deferred so that the stealth shader cannot be bypassed by hovering the entity.
        await Client.WaitRunTicks(1);
        await Client.WaitAssertion(() =>
        {
            Assert.That(CEntMan.HasComponent<InteractionOutlineComponent>(entity), Is.False);
        });

        await Client.WaitPost(() =>
        {
            CEntMan.System<StealthSystem>().SetEnabled(entity, false);
        });

        await Client.WaitAssertion(() =>
        {
            Assert.That(CEntMan.HasComponent<InteractionOutlineComponent>(entity), Is.True);
        });
    }
}
