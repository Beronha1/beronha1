using Content.Server.Hands.Systems;

namespace Content.Server._Sunrise.BloodCult.Juggernaut;

public sealed class JuggernautSystem : EntitySystem
{
    [Dependency] private readonly HandsSystem _handsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        // 🌇Sunset🌇 - this fork's body-system rework dropped BodyInitEvent; MapInitEvent (entity finished
        // spawning) covers the same "give the Juggernaut its hammer on spawn" intent.
        SubscribeLocalEvent<JuggernautComponent, MapInitEvent>(OnBodyInit);
    }

    private void OnBodyInit(EntityUid uid, JuggernautComponent component, MapInitEvent args)
    {
        var hammer = Spawn(component.HummerSpawnId, Transform(uid).Coordinates);
        _handsSystem.TryForcePickupAnyHand(uid, hammer);
    }
}
