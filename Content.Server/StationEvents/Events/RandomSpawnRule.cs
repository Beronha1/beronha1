using Content.Server.Pinpointer;
using Content.Server.Radio.EntitySystems;
using Content.Server.StationEvents.Components;
using Content.Shared._Starlight.Lock;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Utility;

namespace Content.Server.StationEvents.Events;

public sealed class RandomSpawnRule : StationEventSystem<RandomSpawnRuleComponent>
{
    [Dependency] private NavMapSystem _navMap = default!;
    [Dependency] private RadioSystem _radio = default!;

    protected override void Started(EntityUid uid, RandomSpawnRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        if (TryFindRandomTile(out _, out _, out _, out var coords))
        {
            Sawmill.Info($"Spawning {comp.Prototype} at {coords}");
            var spawned = Spawn(comp.Prototype, coords);

            if (comp.RadioMessage is not { } radioMessage)
                return;

            var location = FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString(spawned));
            var message = Loc.GetString(radioMessage.Message, ("location", location));

            if (TryComp<DigitalLockComponent>(spawned, out var digitalLock) && digitalLock.Code == string.Empty)
            {
                var code = string.Empty;
                for (var i = 0; i < digitalLock.MaxCodeLength; i++)
                    code += RobustRandom.Next(0, 10).ToString();

                digitalLock.Code = code;
                message += Loc.GetString("dead-drop-code-announcement", ("code", code));
            }

            _radio.SendRadioMessage(spawned, message, radioMessage.Channel, spawned);
        }
    }
}
