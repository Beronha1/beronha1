using Content.Shared.Mobs;

namespace Content.Server._Sunrise.BecomeDustOnDeathSystem;

public sealed class BecomeDustOnDeathSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<BecomeDustOnDeathComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(EntityUid uid, BecomeDustOnDeathComponent component, MobStateChangedEvent args)
    {
        // 🌇Sunset🌇 - upstream sunrise-station fired this on every mob-state change (including
        // Critical), turning the entity to dust before it actually died. Guard on Dead specifically.
        if (args.NewMobState != MobState.Dead)
            return;

        var xform = Transform(uid);
        Spawn(component.SpawnOnDeathPrototype, xform.Coordinates);

        QueueDel(uid);
    }
}
