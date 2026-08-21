using Robust.Shared.Serialization;

namespace Content.Shared._Sunrise.SharpeningSystem;

// Estava em _Sunrise/BloodCult/Items, e saiu junto com o culto. Quem usa
// afiacao e a ferramenta de reparo do Trauma, entao o enum passou a morar
// perto do sistema que o usa em vez de dentro de um antagonista.
[Serializable, NetSerializable]
public enum SharpenerVisuals : byte
{
    Visual,
    Sharp,
    Used
}
