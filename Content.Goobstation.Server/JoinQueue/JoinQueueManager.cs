// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Threading.Tasks;
using Content.Server.Administration.Managers;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.JoinQueue;
using Content.Goobstation.Shared.JoinQueue;
using Content.Server.Connection;
using Content.Shared.CCVar;
using Content.Trauma.Common.LinkAccount;
using Prometheus;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Log;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.JoinQueue;

/// <summary>
///     Manages new player connections when the server is full and queues them up, granting access when a slot becomes free
/// </summary>
public sealed partial class JoinQueueManager : IJoinQueueManager
{
    private static readonly Gauge QueueCount = Metrics.CreateGauge(
        "join_queue_total_count",
        "Amount of players in queue.");

    private static readonly Counter QueueBypassCount = Metrics.CreateCounter(
        "join_queue_bypass_count",
        "Amount of players who bypassed queue by privileges.");

    private static readonly Histogram QueueTimings = Metrics.CreateHistogram(
        "join_queue_timings",
        "Timings of players in queue",
        new HistogramConfiguration()
        {
            LabelNames = new[] { "type" },
            Buckets = Histogram.ExponentialBuckets(1, 2, 14),
        });


    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IConnectionManager _connection = default!;
    [Dependency] private IConfigurationManager _configuration = default!;
    [Dependency] private ILinkAccountManager _linkAccount = default!;
    [Dependency] private IServerNetManager _net = default!;
    [Dependency] private IAdminManager _adminManager = default!;
    [Dependency] private ILogManager _logManager = default!;

    private readonly System.Threading.SemaphoreSlim _queueLock = new(1, 1);
    private ISawmill _sawmill = default!;

    /// <summary>
    ///     Queue of active player sessions
    /// </summary>
    private readonly List<ICommonSession> _queue = new();

    /// <summary>
    ///     Queue for Patreon supporters.
    /// </summary>
    private readonly List<ICommonSession> _patronQueue = new();

    private bool _isEnabled = false;
    private bool _patreonIsEnabled = true;

    public int PlayerInQueueCount => _queue.Count + _patronQueue.Count;
    public int ActualPlayersCount
    {
        get
        {
            var players = _player.PlayerCount - PlayerInQueueCount;

            if (!_configuration.GetCVar(CCVars.AdminsCountForMaxPlayers))
            {
                players -= _adminManager.ActiveAdmins.Count(session =>
                    session.Status is not (SessionStatus.Disconnected or SessionStatus.Zombie));
            }

            return Math.Max(players, 0);
        }
    }


    public void Initialize()
    {
        _sawmill = _logManager.GetSawmill("join-queue");
        _net.RegisterNetMessage<QueueUpdateMessage>();

        _configuration.OnValueChanged(GoobCVars.QueueEnabled, OnQueueCVarChanged, true);
        _configuration.OnValueChanged(GoobCVars.PatreonSkip, OnPatronCvarChanged, true);
        _player.PlayerStatusChanged += OnPlayerStatusChanged;
    }


    private void OnQueueCVarChanged(bool value)
    {
        _isEnabled = value;

        if (!value)
        {
            var queuedSessions = _patronQueue.Concat(_queue).ToArray();
            _queue.Clear();
            _patronQueue.Clear();
            QueueCount.Set(0);

            foreach (var session in queuedSessions)
                session.Channel.Disconnect("Queue was disabled");
        }
    }

    private void OnPatronCvarChanged(bool value)
        => _patreonIsEnabled = value;


    private async void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        try
        {
            if (e.NewStatus == SessionStatus.Disconnected)
            {
                await _queueLock.WaitAsync();

                try
                {
                    var wasInQueue = _queue.Remove(e.Session) || _patronQueue.Remove(e.Session);

                    if (!wasInQueue && e.OldStatus != SessionStatus.InGame)
                        return;

                    ProcessQueue();

                    if (wasInQueue)
                        QueueTimings.WithLabels("Unwaited").Observe((DateTime.UtcNow - e.Session.ConnectedTime).TotalSeconds);
                }
                finally
                {
                    _queueLock.Release();
                }
            }
            else if (e.NewStatus == SessionStatus.Connected)
            {
                await OnPlayerConnected(e.Session);
            }
        }
        catch (Exception exception)
        {
            _sawmill.Error("Failed to update the join queue: {0}", exception);
        }
    }


    private async Task OnPlayerConnected(ICommonSession session)
    {
        if (!_isEnabled)
        {
            SendToGame(session);
            return;
        }

        var isPrivileged = await _connection.HasPrivilegedJoin(session.UserId);
        await _queueLock.WaitAsync();

        try
        {
            // The privilege lookup hits the database. The player may have disconnected or
            // the queue may have been disabled while it was in flight.
            if (session.Status is SessionStatus.Disconnected or SessionStatus.Zombie)
                return;

            if (!_isEnabled)
            {
                SendToGame(session);
                return;
            }

            var currentOnline = ActualPlayersCount - 1;
            var haveFreeSlot = currentOnline < _configuration.GetCVar(CCVars.SoftMaxPlayers);
            if (isPrivileged || haveFreeSlot)
            {
                SendToGame(session);

                if (isPrivileged && !haveFreeSlot)
                    QueueBypassCount.Inc();

                return;
            }

            if (_patreonIsEnabled && _linkAccount.IsPatron(session))
                _patronQueue.Add(session);
            else
                _queue.Add(session);

            ProcessQueue();
        }
        finally
        {
            _queueLock.Release();
        }
    }

    /// <summary>
    ///     If possible, takes the first player in the queue and sends him into the game
    /// </summary>
    private void ProcessQueue()
    {
        var players = ActualPlayersCount;
        var maxPlayers = _configuration.GetCVar(CCVars.SoftMaxPlayers);

        while (players < maxPlayers && (_patronQueue.Count > 0 || _queue.Count > 0))
        {
            ICommonSession session;
            if (_patronQueue.Count > 0)
            {
                session = _patronQueue[0];
                _patronQueue.RemoveAt(0);
            }
            else
            {
                session = _queue[0];
                _queue.RemoveAt(0);
            }

            if (session.Status is SessionStatus.Disconnected or SessionStatus.Zombie)
                continue;

            SendToGame(session);
            QueueTimings.WithLabels("Waited").Observe((DateTime.UtcNow - session.ConnectedTime).TotalSeconds);
            players++;
        }

        SendUpdateMessages();
        QueueCount.Set(_queue.Count + _patronQueue.Count);
    }

    /// <summary>
    ///     Sends messages to all players in the queue with the current state of the queue
    /// </summary>
    private void SendUpdateMessages()
    {
        var totalInQueue = _patronQueue.Count + _queue.Count;
        var currentPosition = 1;

        for (var i = 0; i < _patronQueue.Count; i++, currentPosition++)
        {
            _patronQueue[i].Channel.SendMessage(new QueueUpdateMessage
            {
                Total = totalInQueue,
                Position = currentPosition,
                IsPatron = true,
            });
        }

        for (var i = 0; i < _queue.Count; i++, currentPosition++)
        {
            _queue[i].Channel.SendMessage(new QueueUpdateMessage
            {
                Total = totalInQueue,
                Position = currentPosition,
                IsPatron = false,
            });
        }
    }

    /// <summary>
    ///     Letting player's session into game, change player state
    /// </summary>
    /// <param name="session">Player session that will be sent to game</param>
    private void SendToGame(ICommonSession session)
    {

        Timer.Spawn(0, () => _player.JoinGame(session));
    }
}
