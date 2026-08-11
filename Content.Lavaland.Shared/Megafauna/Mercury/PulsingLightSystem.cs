// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Lavaland.Shared.Megafauna.Mercury;

/// <summary>
/// Pulses an entity's light and plays its optional boot sound.
/// Ported from Goobstation PR #6542.
/// </summary>
public sealed partial class PulsingLightSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedPointLightSystem _lights = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PulsingLightComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextUpdate)
                continue;

            comp.NextUpdate = _timing.CurTime + comp.Interval;
            OnBootUp((uid, comp));
        }
    }

    private void OnBootUp(Entity<PulsingLightComponent> ent)
    {
        var (uid, comp) = ent;

        if (!comp.SoundPlayed && comp.ShouldPlaySound)
        {
            _audio.PlayPredicted(comp.BootUpSound, uid, uid);
            comp.SoundPlayed = true;
        }

        var light = _lights.EnsureLight(uid);
        _lights.SetColor(uid, comp.LightColor, light);

        if (comp.ReduceGlow)
        {
            var nextGlow = Math.Clamp(comp.CurrentGlow - comp.IncreaseBy, 0f, comp.GlowIntensity);
            _lights.SetEnergy(uid, nextGlow, light);
            _lights.SetRadius(uid, nextGlow, light);
            comp.CurrentGlow = nextGlow;

            if (comp.CurrentGlow <= 0)
            {
                comp.CurrentGlow = 0;
                comp.ReduceGlow = false;
                comp.SoundPlayed = false;
            }
        }
        else
        {
            var nextGlow = Math.Clamp(comp.CurrentGlow + comp.IncreaseBy, 0f, comp.GlowIntensity);
            _lights.SetEnergy(uid, nextGlow, light);
            _lights.SetRadius(uid, nextGlow, light);
            comp.CurrentGlow = nextGlow;

            if (comp.CurrentGlow >= comp.GlowIntensity)
            {
                comp.CurrentGlow = comp.GlowIntensity;
                comp.ReduceGlow = true;
            }
        }

        Dirty(uid, comp);
    }
}
