// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Shared.MobPhases;
using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Megafauna;

/// <summary>
/// Drives reusable phase-specific boss dialogue without making MobPhases
/// depend on chat or server-only systems.
/// </summary>
public sealed partial class MegafaunaPhaseDialogueSystem : EntitySystem
{
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MegafaunaPhaseDialogueComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<MegafaunaPhaseDialogueComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.LastPhase = CompOrNull<MobPhasesComponent>(ent)?.CurrentPhase ?? 0;
        ScheduleNext(ent.Comp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MegafaunaPhaseDialogueComponent, MobPhasesComponent, MobStateComponent>();
        while (query.MoveNext(out var uid, out var dialogue, out var phases, out var mobState))
        {
            if (mobState.CurrentState == MobState.Dead)
                continue;

            if (dialogue.LastPhase != phases.CurrentPhase)
            {
                dialogue.LastPhase = phases.CurrentPhase;
                if (dialogue.Phases.TryGetValue(phases.CurrentPhase, out var entry) &&
                    entry.TransitionLine is { } transition)
                {
                    Speak(uid, transition);
                }

                ScheduleNext(dialogue);
                continue;
            }

            if (_timing.CurTime < dialogue.NextLineAt)
                continue;

            if (dialogue.Phases.TryGetValue(phases.CurrentPhase, out var current) && current.Lines.Count > 0)
                Speak(uid, _random.Pick(current.Lines));

            ScheduleNext(dialogue);
        }
    }

    private void ScheduleNext(MegafaunaPhaseDialogueComponent component)
    {
        var minimum = MathF.Max(0.1f, component.MinimumInterval);
        var maximum = MathF.Max(minimum, component.MaximumInterval);
        component.NextLineAt = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(minimum, maximum));
    }

    private void Speak(EntityUid uid, LocId line)
    {
        _chat.TrySendInGameICMessage(
            uid,
            Loc.GetString(line),
            InGameICChatType.Speak,
            hideChat: false,
            ignoreActionBlocker: true);
    }
}
