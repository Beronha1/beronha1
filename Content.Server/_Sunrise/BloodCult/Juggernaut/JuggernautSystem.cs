using Content.Server.Hands.Systems;

namespace Content.Server._Sunrise.BloodCult.Juggernaut;

public sealed partial class JuggernautSystem : EntitySystem
{
    [Dependency] private HandsSystem _handsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        // 🌇Sunset🌇 - this fork's body-system rework dropped BodyInitEvent; MapInitEvent (entity finished
        // spawning) covers the same "give the Juggernaut its hammer on spawn" intent.
        SubscribeLocalEvent<JuggernautComponent, MapInitEvent>(OnBodyInit);
        SubscribeLocalEvent<JuggernautComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnBodyInit(EntityUid uid, JuggernautComponent component, MapInitEvent args)
    {
        var hammer = Spawn(component.HummerSpawnId, Transform(uid).Coordinates);
        component.Hammer = hammer;
        _handsSystem.TryForcePickupAnyHand(uid, hammer);
    }

    private void OnShutdown(EntityUid uid, JuggernautComponent component, ComponentShutdown args)
    {
        if (Exists(component.Hammer))
            QueueDel(component.Hammer);

        component.Hammer = EntityUid.Invalid;
    }
}
