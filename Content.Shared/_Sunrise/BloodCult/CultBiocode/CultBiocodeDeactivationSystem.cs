using Content.Shared.Hands;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.Events;

namespace Content.Shared._Sunrise.BloodCult.CultBiocode;

/// <summary>
/// System that handles automatic deactivation of biocoded cult items when they're not in an authorized user's possession.
/// </summary>
public abstract partial class CultBiocodeDeactivationSystem : EntitySystem
{
    [Dependency] private CultBiocodeSystem _biocodeSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CultBiocodeDeactivationComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<CultBiocodeDeactivationComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<CultBiocodeDeactivationComponent, DroppedEvent>(OnItemDropped);
        SubscribeLocalEvent<CultBiocodeDeactivationComponent, GotEquippedEvent>(OnItemPickedUp);
    }

    private void OnItemDropped(EntityUid uid, CultBiocodeDeactivationComponent component, DroppedEvent args)
    {
        if (!component.DeactivateOnRemoval)
            return;

        if (!TryComp<CultBiocodeComponent>(uid, out _))
            return;

        DeactivateItem(uid);
    }

    private void OnItemPickedUp(EntityUid uid, CultBiocodeDeactivationComponent component, GotEquippedEvent args)
    {
        if (!component.DeactivateOnUnauthorized)
            return;

        if (!TryComp<CultBiocodeComponent>(uid, out var biocodeComponent))
            return;

        if (_biocodeSystem.CanUse(args.EquipTarget, biocodeComponent.Factions))
            return;

        DeactivateItem(uid);
    }

    private void OnActivate(EntityUid uid, CultBiocodeDeactivationComponent component, ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        if (!TryComp<CultBiocodeComponent>(uid, out var biocodeComponent))
            return;

        if (_biocodeSystem.CanUse(args.User, biocodeComponent.Factions))
            return;

        var alertText = component.AlertText ?? biocodeComponent.AlertText;
        ShowAlert(args.User, alertText);
        args.Handled = true;
    }

    private void OnUseInHand(EntityUid uid, CultBiocodeDeactivationComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<CultBiocodeComponent>(uid, out var biocodeComponent))
            return;

        if (_biocodeSystem.CanUse(args.User, biocodeComponent.Factions))
            return;

        var alertText = component.AlertText ?? biocodeComponent.AlertText;
        ShowAlert(args.User, alertText);
        args.Handled = true;
    }

    /// <summary>
    /// Shows an alert to the user. Override this method to implement specific alert display logic.
    /// </summary>
    protected abstract void ShowAlert(EntityUid user, string alertText);

    /// <summary>
    /// Deactivates the item. Override this method in the server system to implement specific deactivation logic.
    /// </summary>
    protected abstract void DeactivateItem(EntityUid uid);
}
