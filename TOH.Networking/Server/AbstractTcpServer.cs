using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TOH.Networking.Abstractions;
using TOH.Networking.Common;

namespace TOH.Networking.Server
{
    public class ServerConfiguration
    {
        public int Port { get; set; }
    }

    public class TimerService
    {
        private Stopwatch stopwatch = new Stopwatch();

        private readonly ILogger _logger;

        public TimerService(ILogger<TimerService> logger)
        {
            _logger = logger;
        }

        public void Start(CancellationToken cancellationToken)
        {
            if (!stopwatch.IsRunning)
            {
                stopwatch.Start();
            }
            else
            {
                _logger.LogWarning("The stopwatch is already running.");
            }
        }

        public long GetTicks()
        {
            if (!stopwatch.IsRunning)
            {
                throw new Exception("The stopwatch is not running.");
            }

            return stopwatch.ElapsedTicks;
        }

        public void Stop()
        {
            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
            }
        }
    }

    public abstract class AbstractTcpServer
    {
        protected readonly ServerConfiguration _configuration;
        protected readonly ConnectionManager _connectionManager;
        protected readonly CancellationTokenSource _tasksCancellationTokenSource;
        protected readonly CancellationToken _tasksCancellationToken;
        protected readonly TimerService _timerService;
        protected readonly JsonPacketConverter _packetConverter;
        protected readonly ConcurrentDictionary<string, IPacketHandler<Packet>> _packetHandlers = new ConcurrentDictionary<string, IPacketHandler<Packet>>();

        protected readonly ConcurrentBag<Task> _serverTasks;

        protected ILogger _logger;
        protected TcpListener _listener;
        protected Task _listenerTask;
        protected IServiceProvider _serviceProvider;
        private IHost _host;

        public AbstractTcpServer(IHost host)
        {
            _host = host;

            _serviceProvider = _host.Services;

            _tasksCancellationTokenSource = new CancellationTokenSource();
            _tasksCancellationToken = _tasksCancellationTokenSource.Token;

            _packetConverter = _serviceProvider.GetRequiredService<JsonPacketConverter>();

            _configuration = _serviceProvider.GetRequiredService<IOptions<ServerConfiguration>>().Value;

            _timerService = _serviceProvider.GetRequiredService<TimerService>();

            _connectionManager = _serviceProvider.GetRequiredService<ConnectionManager>();

            _logger = _serviceProvider.GetRequiredService<ILogger<AbstractTcpServer>>();

            _serverTasks = new ConcurrentBag<Task>();
        }

        public virtual Task OnConnected(TcpConnection connection, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                connection.Close();
            }
            else
            {
                _connectionManager.AddConnection(connection);
            }

            return Task.CompletedTask;
        }

        public virtual Task OnDisconnected(TcpConnection connection)
        {
            _connectionManager.RemoveConnection(connection);

            return Task.CompletedTask;
        }

        public async Task SendPacket<T>(TcpConnection connection, T packet) where T : Packet
        {
            await connection.Send(packet);
        }

        public async Task SendPacket<T>(string connectionId, T packet) where T : Packet
        {
            var connection = _connectionManager.GetConnection(connectionId);

            if (connection != null)
            {
                await SendPacket(connection, packet);
            }
        }
        public async Task BroadcastPacket<T>(T packet) where T : Packet
        {
            foreach (var connection in _connectionManager.Connections)
            {
                await SendPacket(connection.Value, packet);
            }
        }

        protected async void ListenLoop(CancellationToken cancellationToken)
        {
            for (; ; )
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("The ListenLoop task was cancelled.");

                    break;
                }

                var socket = await _listener.AcceptSocketAsync();

                if (socket != null)
                {
                    var connection = new TcpConnection(socket, _packetConverter);

                    var handlerTask = Task.Factory.StartNew(() => HandleConnection(connection, cancellationToken), cancellationToken);

                    _serverTasks.Add(handlerTask);
                }
            }
        }

        protected async Task HandleConnection(TcpConnection connection, CancellationToken cancellationToken)
        {
            await OnConnected(connection, cancellationToken);

            while (!connection.IsClosed)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("The HandleSocket task was cancelled.");

                    break;
                }

                var packet = await connection.GetPacket();

                await OnPacketReceived(connection, packet);
            }

            await OnDisconnected(connection);
        }

        protected async Task OnPacketReceived(TcpConnection connection, Packet packet)
        {
            if (string.IsNullOrEmpty(packet.Type))
            {
                //Invalid packet.
                return;
            }

            if (_packetHandlers.ContainsKey(packet.Type))
            {
                var handler = _packetHandlers[packet.Type];

                await handler.Handle(connection, _packetConverter.Unwrap(packet));
            }
            else
            {
                _logger.LogWarning($"No Packet Handler has been registered for packet with 'Key'='{packet.Type}'.");
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting service");

            _timerService.Start(_tasksCancellationToken);

            var ipAddress = IPAddress.Loopback;

            _listener = new TcpListener(ipAddress, _configuration.Port);

            _listener.Start();

            _logger.LogInformation($"Listening on {ipAddress}:{_configuration.Port}");

            _listenerTask = Task.Factory.StartNew(() => ListenLoop(_tasksCancellationToken), _tasksCancellationToken);

            _serverTasks.Add(_listenerTask);

            _host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.Register(StopAsync);

            await _host.RunAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Stopping service");

            _tasksCancellationTokenSource.Cancel();

            try
            {
                Task.WaitAll(_serverTasks.ToArray());
            }
            catch (AggregateException ex)
            {
                _logger.LogInformation("AggregateException thrown with the following inner exceptions:");

                foreach (var exception in ex.InnerExceptions)
                {
                    if (exception is TaskCanceledException)
                    {
                        _logger.LogInformation($"TaskCanceledException: Task {((TaskCanceledException)exception).Task.Id}");
                    }
                    else
                    {
                        _logger.LogInformation($"Exception: {exception.GetType().Name}");
                    }
                }
            }
            finally
            {
                _tasksCancellationTokenSource.Dispose();
            }

            foreach (var task in _serverTasks)
            {
                _logger.LogInformation($"Task '{task.Id}' is now '{task.Status}'.");
            }

            return Task.CompletedTask;
        }

        public void StopAsync()
        {
            StopAsync(default);
        }
    }
}
