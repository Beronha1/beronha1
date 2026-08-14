// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Chemistry.Reagent;

/// <summary>
/// Raised on a body after a reagent is successfully processed by metabolism.
/// Compatibility hook used by systems that need to observe a consumed dose.
/// </summary>
[ByRefEvent]
public readonly record struct GetReagentEffectsEvent(ReagentId Reagent);
