// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Disease;
using Content.Goobstation.Shared.Disease.Components;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests._Goobstation.Disease;

[TestFixture]
[TestOf(typeof(DiseaseSystem))]
public sealed class DiseaseSpreadTest : GameTest
{
    [TestPrototypes]
    private const string Prototypes = @"
- type: entity
  id: DiseaseSpreadTestCarrier
  components:
  - type: DiseaseCarrier

- type: entity
  id: DiseaseSpreadTestHighlyMutable
  parent: DiseaseBase
  components:
  - type: Disease
    genotype: 4242
    mutationRate: 100
    mutationMutationCoefficient: 0
    immunityGainMutationCoefficient: 0
    infectionRateMutationCoefficient: 0
    complexityMutationCoefficient: 0
    severityMutationCoefficient: 0
    effectMutationCoefficient: 0
    genotypeMutationCoefficient: 1
    effects:
      DiseaseBehaviorCough: 1
";

    [SidedDependency(Side.Server)]
    private DiseaseSystem _disease = default!;

    [Test]
    [RunOnSide(Side.Server)]
    public void RepeatedSpreadDoesNotStackMutatedGenotypes()
    {
        var source = SEntMan.SpawnEntity("DiseaseSpreadTestHighlyMutable", MapCoordinates.Nullspace);
        var target = SEntMan.SpawnEntity("DiseaseSpreadTestCarrier", MapCoordinates.Nullspace);
        var sourceComp = SComp<DiseaseComponent>(source);

        Assert.That(_disease.DoInfectionAttempt(target, source, 1f, 1f, "Aerial"), Is.True);

        for (var i = 0; i < 100; i++)
            Assert.That(_disease.DoInfectionAttempt(target, source, 1f, 1f, "Aerial"), Is.False);

        var infections = new List<Entity<DiseaseComponent>>();
        var query = SEntMan.EntityQueryEnumerator<DiseaseComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var disease, out var transform))
        {
            if (transform.ParentUid == target)
                infections.Add((uid, disease));
        }

        Assert.That(infections, Has.Count.EqualTo(1));
        Assert.That(infections[0].Comp.Genotype, Is.EqualTo(sourceComp.Genotype));
    }
}
