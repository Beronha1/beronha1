// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Administration;
using Content.Shared.CCVar.CVarAccess;
using Robust.Shared.Configuration;

namespace Content.Lavaland.Shared.CCVar;

[CVarDefs]
public sealed partial class LavalandCVars
{
    /// <summary>
    ///     Should the Lavaland roundstart generation be enabled.
    /// </summary>
    [CVarControl(AdminFlags.Server | AdminFlags.Mapping)]
    public static readonly CVarDef<bool> LavalandEnabled =
        CVarDef.Create("lavaland.enabled", true, CVar.SERVERONLY);

    /// <summary>
    ///     Enables Whiskey's dynamic party/progression scaling for entities that
    ///     opt in with MegafaunaDirectorComponent.
    /// </summary>
    public static readonly CVarDef<bool> MegafaunaDirectorEnabled =
        CVarDef.Create("lavaland.megafauna_director_enabled", true, CVar.SERVERONLY);
}
