// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Lavaland.Shared.Megafauna.Events;

public sealed partial class DemonicFrostOrbActionEvent : EntityTargetActionEvent;

public sealed partial class DemonicFrostMachineGunActionEvent : EntityTargetActionEvent;

public sealed partial class DemonicFrostShotgunActionEvent : EntityTargetActionEvent;

[Serializable, NetSerializable]
public enum DemonicFrostMinerVisuals : byte
{
    Enraged,
}
