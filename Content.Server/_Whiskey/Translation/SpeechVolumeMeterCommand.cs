using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._Whiskey.Translation;

/// <summary>
/// Pede o resumo do medidor de fala sem esperar a janela de dez minutos fechar.
/// Útil para conferir que o medidor está contando, e para tirar uma leitura
/// no meio de uma rodada cheia sem depender do relógio.
/// </summary>
[AdminCommand(AdminFlags.Admin)]
public sealed partial class SpeechVolumeMeterCommand : IConsoleCommand
{
    [Dependency] private IEntityManager _entityManager = default!;

    public string Command => "whiskey_falametrica";
    public string Description => "Escreve no log o resumo de volume de fala por idioma da janela atual.";
    public string Help => "Uso: whiskey_falametrica";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var sistema = _entityManager.System<SpeechVolumeMeterSystem>();
        sistema.Report();
        shell.WriteLine("Resumo escrito no log do servidor, sawmill whiskey.speech_meter.");
    }
}
