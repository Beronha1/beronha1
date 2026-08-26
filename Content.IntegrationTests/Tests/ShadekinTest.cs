// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Movement.Events;
using Content.Shared.Preferences.Loadouts.Effects;
using Content.Shared._Starlight.Shadekin.Components;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class ShadekinTest : GameTest
{
    private static readonly ProtoId<SpeciesPrototype> ShadekinSpecies = "Shadekin";
    private static readonly ProtoId<LoadoutEffectGroupPrototype> OxygenBreather = "OxygenBreather";

    [Test]
    public async Task ReceivesEmergencyOxygenLoadout()
    {
        var group = SProtoMan.Index(OxygenBreather);
        var speciesEffect = group.Effects.OfType<SpeciesLoadoutEffect>().Single();

        Assert.That(speciesEffect.Species, Does.Contain(ShadekinSpecies));
    }

    [Test]
    public async Task FootstepsAreSilentOnlyInDarkness()
    {
        await Server.WaitAssertion(() =>
        {
            var uid = SSpawn("MobShadekin");
            var shadekin = SComp<ShadekinComponent>(uid);

            shadekin.CurrentState = ShadekinState.Dark;
            var darkStep = new BeforeFootstepSoundEvent();
            SEntMan.EventBus.RaiseLocalEvent(uid, darkStep);
            Assert.That(darkStep.Cancelled, Is.True);

            shadekin.CurrentState = ShadekinState.Low;
            var litStep = new BeforeFootstepSoundEvent();
            SEntMan.EventBus.RaiseLocalEvent(uid, litStep);
            Assert.That(litStep.Cancelled, Is.False);
        });
    }
}
