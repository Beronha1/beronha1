// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Lavaland.Shared.Megafauna.Components;

/// <summary>
/// Keeps an NPC's melee weapon available to HTN target acquisition and pursuit, but prevents the entity from
/// executing basic melee swings. Boss damage must come from its explicit, telegraphed action repertoire.
/// </summary>
[RegisterComponent]
public sealed partial class MegafaunaSpecialAttackOnlyComponent : Component
{
    /// <summary>
    /// Narrow server-owned bypass used when a boss action explicitly includes a melee strike. This keeps HTN's
    /// ordinary attack branch disabled while still allowing movesets such as the Blood-Drunk Miner's saw combo.
    /// </summary>
    [ViewVariables]
    public bool AllowActionMelee;
}
