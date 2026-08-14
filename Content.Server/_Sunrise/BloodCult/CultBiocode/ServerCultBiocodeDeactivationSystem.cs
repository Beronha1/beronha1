using Content.Server.Pinpointer;
using Content.Server.Popups;
using Content.Shared._Sunrise.BloodCult.CultBiocode;
using Content.Shared.Pinpointer;

namespace Content.Server._Sunrise.BloodCult.CultBiocode;

/// <summary>
/// Server-side implementation of the cult biocode deactivation system.
/// </summary>
public sealed class ServerCultBiocodeDeactivationSystem : CultBiocodeDeactivationSystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PinpointerSystem _pinpointerSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    protected override void ShowAlert(EntityUid user, string alertText)
    {
        _popup.PopupEntity(Loc.GetString(alertText), user, user);
    }

    protected override void DeactivateItem(EntityUid uid)
    {
        if (TryComp<PinpointerComponent>(uid, out var pinpointer))
        {
            _pinpointerSystem.SetActive(uid, false, pinpointer);

            if (TryComp<AppearanceComponent>(uid, out var appearance))
            {
                _appearance.SetData(uid, PinpointerVisuals.IsActive, false, appearance);
                _appearance.SetData(uid, PinpointerVisuals.TargetDistance, Distance.Unknown, appearance);
            }
        }
    }
}
