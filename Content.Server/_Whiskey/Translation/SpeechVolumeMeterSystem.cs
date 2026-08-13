using System.Collections.Generic;
using System.Linq;
using System.Text;
using Content.Shared.Chat;
using Robust.Shared.Timing;

namespace Content.Server._Whiskey.Translation;

/// <summary>
/// <para>
/// Mede o volume de fala do servidor por idioma. Não altera nada no jogo: só
/// escuta o <see cref="EntitySpokeEvent"/>, que já é disparado em toda fala, e
/// escreve um resumo no log de tempos em tempos.
/// </para>
/// <para>
/// Existe para responder uma pergunta antes de qualquer tradutor ser
/// construído: quantas mensagens por minuto, em quais idiomas, e com quantos
/// caracteres. Sem esse número, qualquer estimativa de custo de tradução é
/// chute. Depois de medido, este sistema pode ser removido ou deixado
/// desligado.
/// </para>
/// </summary>
public sealed class SpeechVolumeMeterSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <summary>
    /// Intervalo entre resumos no log. Dez minutos dá amostra suficiente sem
    /// encher o log de um servidor que roda a noite toda.
    /// </summary>
    private static readonly TimeSpan ReportInterval = TimeSpan.FromMinutes(10);

    private sealed class LanguageTally
    {
        public int Messages;
        public int Characters;
        public int Whispers;
        public int RadioMessages;
    }

    private readonly Dictionary<string, LanguageTally> _tallies = new();

    private TimeSpan _windowStart;
    private TimeSpan _nextReport;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("whiskey.speech_meter");

        SubscribeLocalEvent<EntitySpokeEvent>(OnEntitySpoke);

        _windowStart = _timing.CurTime;
        _nextReport = _windowStart + ReportInterval;
    }

    private void OnEntitySpoke(EntitySpokeEvent args)
    {
        // O idioma nunca é nulo aqui: o ChatSystem sempre resolve um antes de
        // disparar o evento, caindo no Universal quando não há outro.
        var id = args.Language.ID;

        if (!_tallies.TryGetValue(id, out var tally))
        {
            tally = new LanguageTally();
            _tallies[id] = tally;
        }

        tally.Messages++;
        tally.Characters += args.Message.Length;

        if (args.IsWhisper)
            tally.Whispers++;

        if (args.Channel != null)
            tally.RadioMessages++;
    }

    public override void Update(float frameTime)
    {
        if (_timing.CurTime < _nextReport)
            return;

        Report();

        _tallies.Clear();
        _windowStart = _timing.CurTime;
        _nextReport = _windowStart + ReportInterval;
    }

    /// <summary>
    /// Escreve o resumo da janela atual. Público para que um comando de admin
    /// possa pedir o relatório sem esperar o intervalo.
    /// </summary>
    public void Report()
    {
        var minutes = (_timing.CurTime - _windowStart).TotalMinutes;
        if (minutes <= 0)
            return;

        if (_tallies.Count == 0)
        {
            _sawmill.Info($"Nenhuma fala nos ultimos {minutes:F1} minutos.");
            return;
        }

        var totalMessages = _tallies.Values.Sum(t => t.Messages);
        var totalChars = _tallies.Values.Sum(t => t.Characters);

        var linha = new StringBuilder();
        linha.Append($"Janela de {minutes:F1} min: {totalMessages} mensagens ");
        linha.Append($"({totalMessages / minutes:F1}/min), {totalChars} caracteres ");
        linha.Append($"({totalChars / minutes:F0}/min). Por idioma: ");

        foreach (var (id, tally) in _tallies.OrderByDescending(p => p.Value.Messages))
        {
            linha.Append($"{id}={tally.Messages} msg/{tally.Characters} car");
            if (tally.Whispers > 0)
                linha.Append($"/{tally.Whispers} sussurro");
            if (tally.RadioMessages > 0)
                linha.Append($"/{tally.RadioMessages} radio");
            linha.Append("; ");
        }

        _sawmill.Info(linha.ToString());
    }
}
