namespace Content.Shared._Sunrise.BloodCult.CultBiocode;

/// <summary>
/// Raised when attempting to throw an entity to check if the user can throw it based on cult-biocode restrictions.
/// </summary>
[ByRefEvent]
public struct AttemptThrowCultBiocodeEvent(EntityUid itemUid, EntityUid? user)
{
    public readonly EntityUid ItemUid = itemUid;
    public readonly EntityUid? User = user;
    public bool Cancelled;
}
