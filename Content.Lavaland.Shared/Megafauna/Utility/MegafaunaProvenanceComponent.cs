// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Lavaland.Shared.Megafauna.Utility;

/// <summary>
/// Human-readable chain-of-custody metadata for boss-derived matter and
/// equipment. Cargo can classify it through the accompanying marker
/// components without depending on individual prototype IDs.
/// </summary>
[RegisterComponent]
public sealed partial class MegafaunaProvenanceComponent : Component
{
    [DataField(required: true)]
    public LocId Source;

    [DataField]
    public MegafaunaProvenanceGrade Grade = MegafaunaProvenanceGrade.Raw;
}

public enum MegafaunaProvenanceGrade : byte
{
    Raw,
    Intact,
    Processed,
}

[RegisterComponent]
public sealed partial class MegafaunaRawSampleComponent : Component;

[RegisterComponent]
public sealed partial class MegafaunaIntactSampleComponent : Component;

[RegisterComponent]
public sealed partial class MegafaunaProcessedRewardComponent : Component;
