// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Starfall.Particles;
using Content.Shared.EntityEffects;

namespace Content.Trauma.Shared.Particles;

/// <summary>
/// Compatibility entity effect for Trauma prototypes that spawn Starfall particles.
/// </summary>
public sealed partial class SpawnParticles : EntityEffectBase<SpawnParticles>
{
    [DataField(required: true)]
    public ProtoId<ParticleEffectPrototype> ParticleProto;

    [DataField]
    public bool Attached;

    [DataField]
    public int Number = 1;

    [DataField]
    public Color? Color;
}

public abstract class SharedSpawnParticlesEffectSystem : EntityEffectSystem<TransformComponent, SpawnParticles>
{
    protected override void Effect(Entity<TransformComponent> ent, ref EntityEffectEvent<SpawnParticles> args)
    {
        var effect = args.Effect.ParticleProto;
        var quantity = args.Effect.Number * (int) Math.Floor(args.Scale);
        var color = args.Effect.Color;
        var attach = args.Effect.Attached;

        SpawnParticles(effect, ent.Owner, color, attach, quantity, args.User);
    }

    protected virtual void SpawnParticles(ProtoId<ParticleEffectPrototype> particleProto,
        EntityUid target,
        Color? color,
        bool attached,
        int number,
        EntityUid? user)
    {
    }
}

[Serializable, NetSerializable]
public sealed partial class SpawnParticlesEvent(NetEntity target,
    ProtoId<ParticleEffectPrototype> proto,
    bool attached,
    int number,
    Color? color) : EntityEventArgs
{
    public NetEntity Target = target;
    public ProtoId<ParticleEffectPrototype> ParticleProto = proto;
    public bool Attached = attached;
    public int Number = number;
    public Color? Color = color;
}
