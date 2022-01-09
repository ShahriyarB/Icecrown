// <copyright file="Program.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot;

using System.Text.Json;

/// <summary>
/// Application entry point class.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Application version.
    /// </summary>
    internal const string Version = "0.0.1";

    private const ushort ListenPort = 16112;

    private static readonly List<Hostbot> Hostbots = new();
    private static readonly Thread ServerThread = new(ListenThread);
    private static readonly UdpClient UdpClient = new(new IPEndPoint(IPAddress.Any, ListenPort));
    private static ushort startPort = 6300;
    private static ushort hostCounter = 1;

    /// <summary>
    /// Application entry point function.
    /// </summary>
    private static async Task Main()
    {
        Console.Title = $"Icecrown Hostbot v{Version}";

        // Setup apps exit handler event.
        AppDomain.CurrentDomain.ProcessExit += (_, _) => CleanUp();

        // Setup and run the logger.
        InitLogger();

        // Initialize and load the app settings.
        _ = new Settings("settings.json");

        ServerThread.Start();

        while (true)
        {
            for (int i = 0; i < Settings.Current.Hostbots.Count; i++)
            {
                var config = Settings.Current.Hostbots[i];

                // Create required lobbies
                while (Hostbots.Count(h => h.ConfigId == i && h.GameState == GameState.Lobby) < config.MaxLobbies)
                {
                    var counter = FindLobbyCounter(i);

                    try
                    {
                        Hostbots.Add(new(config.Map, config.HCL, $"{config.GameName} #{counter}", "|cFF6495EDHost", hostCounter++, (ushort)(startPort + hostCounter), i));

                        if (hostCounter == ushort.MaxValue)
                        {
                            hostCounter = 1;
                        }
                    }
                    catch (SocketException)
                    {
                        // Try again until we find a usable port.
                        startPort++;
                        hostCounter--;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, $"Hostbot #{counter} encountered a fatal error during startup");
                        return;
                    }
                }
            }

            // Remove finished games.
            Hostbots.RemoveAll(h => h.GameState == GameState.Finished);

            await Task.Delay(5000);
        }
    }

    /// <summary>
    /// Initialize the logger.
    /// </summary>
    private static void InitLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/.log", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
            .WriteTo.Console()
            .CreateLogger();
    }

    private static async void ListenThread()
    {
        while (true)
        {
            try
            {
                if (UdpClient.Available > 0)
                {
                    IPEndPoint? sender = null;

                    var data = UdpClient.Receive(ref sender);

                    if (sender is null || sender.Port != 6112 || data.Length != 4)
                    {
                        continue;
                    }

                    // Check for magic value.
                    if (BitConverter.ToUInt32(data, 0) != 0xFEEDBABE)
                    {
                        continue;
                    }

                    // Find hostbots that are in lobby.
                    foreach (var hostbot in Hostbots.Where(h => h.GameState == GameState.Lobby))
                    {
                        var bytes = hostbot.GameInfo.FinalByteArray();

                        // Broadcast lobby info to sender.
                        UdpClient.Send(bytes, bytes.Length, sender);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while receiving UDP packet");
            }

            await Task.Delay(1000);
        }
    }

    private static byte FindLobbyCounter(int configId)
    {
        List<byte> used = new();

        foreach (var hostbot in Hostbots.Where(hb => hb.ConfigId == configId))
        {
            // Extract id
            var split = hostbot.GameName.Split('#');

            if (split.Length == 0)
            {
                continue;
            }

            var id = split[^1];

            if (byte.TryParse(id, out var bId))
            {
                used.Add(bId);
            }
        }

        for (byte i = 1; i <= byte.MaxValue; i++)
        {
            if (!used.Contains(i))
            {
                return i;
            }
        }

        return byte.MaxValue;
    }

    private static void CleanUp()
    {
        foreach (var hostbot in Hostbots)
        {
            hostbot.Close();
        }

        // Close and flush the log file.
        Log.Information("Application closed");
        Log.CloseAndFlush();
    }
}
