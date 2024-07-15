using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Turbo.Core;
using Turbo.Core.EventHandlers;
using Turbo.Core.Game;
using Turbo.Core.Game.Catalog;
using Turbo.Core.Game.Furniture;
using Turbo.Core.Game.Navigator;
using Turbo.Core.Game.Players;
using Turbo.Core.Game.Rooms;
using Turbo.Core.PacketHandlers;
using Turbo.Core.Plugins;
using Turbo.Core.Security;
using Turbo.Core.Storage;
using Turbo.Database.Context;
using Turbo.Networking;
using Turbo.Networking.Clients;

namespace Turbo.Main
{
    public class TurboEmulator(
        IHostApplicationLifetime _appLifetime,
        ILogger<TurboEmulator> _logger,
        IStorageQueue _storageQueue,
        IPluginManager _pluginManager,
        IServerManager _serverManager,
        ISecurityManager _securityManager,
        IFurnitureManager _furnitureManager,
        ICatalogManager _catalogManager,
        IRoomManager _roomManager,
        INavigatorManager _navigatorManager,
        IPlayerManager _playerManager,
        ISessionManager _sessionManager,
        IPacketHandlerManager _packetHandlers,
        IEventHandlerManager _eventHandlers) : IEmulator
    {
        public const int MAJOR = 0;
        public const int MINOR = 0;
        public const int PATCH = 0;

        private Task _storageCycle;
        private bool _storageCycleStarted;
        private bool _storageCycleRunning;

        private Task _gameCycle;
        private bool _gameCycleStarted;
        private bool _gameCycleRunning;

        /// <summary>
        /// This method is called by the .NET Generic Host.
        /// See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-5.0 for more information.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine(@"
                ████████╗██╗   ██╗██████╗ ██████╗  ██████╗ 
                ╚══██╔══╝██║   ██║██╔══██╗██╔══██╗██╔═══██╗
                   ██║   ██║   ██║██████╔╝██████╔╝██║   ██║
                   ██║   ██║   ██║██╔══██╗██╔══██╗██║   ██║
                   ██║   ╚██████╔╝██║  ██║██████╔╝╚██████╔╝
                   ╚═╝    ╚═════╝ ╚═╝  ╚═╝╚═════╝  ╚═════╝ 
            ");

            Console.WriteLine("Running {0}", GetVersion());
            Console.WriteLine();

            SetDefaultCulture(CultureInfo.InvariantCulture);

            // Register applicaton lifetime events
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);

            // Start services
            _pluginManager.LoadPlugins();

            await _securityManager.InitAsync();
            await _furnitureManager.InitAsync();
            await _catalogManager.InitAsync();
            await _roomManager.InitAsync();
            await _navigatorManager.InitAsync();

            StartStorageCycle();
            StartGameCycle();

            await _serverManager.StartServersAsync();
        }

        /// <summary>
        /// This method is called by the host application lifetime after the emulator started succesfully
        /// </summary>
        private void OnStarted() => _logger.LogInformation("Emulator started succesfully!");

        /// <summary>
        /// This method is called by the host application lifetime right before the emulator starts stopping
        /// Perform on-stopping activities here.
        /// This function is called before <see cref="StopAsync(CancellationToken)"/>
        /// </summary>
        private void OnStopping()
        {
            _storageCycleStarted = false;
            _gameCycleStarted = false;
            _logger.LogInformation("OnStopping has been called.");
        }

        /// <summary>
        /// This method is called by the .NET Generic Host.
        /// See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-5.0 for more information.
        /// </summary>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Shutting down. Disposing services...");

            _gameCycleStarted = false;
            _storageCycleStarted = false;

            // Todo: Dispose all services here
            await _catalogManager.DisposeAsync();
            await _roomManager.DisposeAsync();
            await _navigatorManager.DisposeAsync();
            await _furnitureManager.DisposeAsync();
            await _roomManager.DisposeAsync();
            await _playerManager.DisposeAsync();
        }

        /// <summary>
        /// This method is called by the host application lifetime after the emulator stopped succesfully
        /// </summary>
        private void OnStopped() => _logger.LogInformation("{Emulator} shut down gracefully.", GetVersion());

        private void SetDefaultCulture(CultureInfo culture)
        {
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        private void StartStorageCycle()
        {
            if (_storageCycleStarted) return;

            _storageCycleStarted = true;

            _storageCycle = Task.Run(async () =>
            {
                while (_storageCycleStarted)
                {
                    _storageCycleRunning = true;

                    try
                    {
                        await _storageQueue.SaveNow();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception caught! " + ex.ToString());
                    }

                    _storageCycleRunning = false;

                    await Task.Delay(DefaultSettings.StoreCycleMs);
                }
            });
        }

        private void StartGameCycle()
        {
            if (_gameCycleStarted) return;

            _gameCycleStarted = true;

            _gameCycle = Task.Run(async () =>
            {
                while (_gameCycleStarted)
                {
                    _gameCycleRunning = true;

                    try
                    {
                        Task.WaitAll(
                            Task.Run(async () => await _roomManager.Cycle()),
                            Task.Run(async () => await _sessionManager.Cycle())
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception caught! " + ex.ToString());
                    }

                    _gameCycleRunning = false;

                    await Task.Delay(DefaultSettings.GameCycleMs);
                }
            });
        }

        public string GetVersion() => ($"Turbo Emulator {MAJOR}.{MINOR}.{PATCH}");
    }
}
