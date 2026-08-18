// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

using Content.Shared.Interaction;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.WhiteDream.BloodCult.Repulsor;

public sealed partial class RepulseSystem : EntitySystem
{
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedStunSystem _stunSystem = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RepulseOnTouchComponent, StartCollideEvent>(HandleCollision);
        SubscribeLocalEvent<RepulseComponent, InteractHandEvent>(OnHandInteract);
    }

    private void HandleCollision(Entity<RepulseOnTouchComponent> touchRepulsor, ref StartCollideEvent args)
    {
        if (!TryComp(touchRepulsor, out RepulseComponent? repulse))
            return;

        Repulse((touchRepulsor.Owner, repulse), args.OtherEntity);
    }

    private void OnHandInteract(Entity<RepulseComponent> repulsor, ref InteractHandEvent args)
    {
        Repulse(repulsor, args.User);
    }

    public void Repulse(Entity<RepulseComponent> repulsor, EntityUid user)
    {
        var ev = new BeforeRepulseEvent(user);
        RaiseLocalEvent(repulsor, ev);
        if (ev.Cancelled)
            return;

        var direction = _transform.GetMapCoordinates(user).Position - _transform.GetMapCoordinates(repulsor).Position;
        var impulse = direction * repulsor.Comp.ForceMultiplier;

        _physics.ApplyLinearImpulse(user, impulse);
        _stunSystem.TryAddStunDuration(user, repulsor.Comp.StunDuration);
        _stunSystem.TryKnockdown(user, repulsor.Comp.KnockdownDuration, force: true);
    }
}
