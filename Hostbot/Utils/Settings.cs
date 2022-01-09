// <copyright file="Settings.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Utils;

using System.Text.Json;

/// <summary>
/// This class contains all needed settigns and configurations.
/// </summary>
[Serializable]
internal class Settings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Settings"/> class.
    /// </summary>
    public Settings()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Settings"/> class.
    /// </summary>
    /// <param name="path">Settings file path.</param>
    internal Settings(string path)
    {
        // Settings class is initialized before.
        if (Current is not null)
        {
            return;
        }

        Current = this;

        try
        {
            var setting = JsonSerializer.Deserialize<Settings>(File.ReadAllText(path));

            // Setup fields
            this.DataPath = setting.DataPath;
            this.CommandDelimiter = setting.CommandDelimiter;
            this.WarcraftVersion = setting.WarcraftVersion;
            this.DownloadLimit = setting.DownloadLimit;
            this.Latency = setting.Latency;
            this.BroadcastLan = setting.BroadcastLan;

            if (this.Latency < 50)
            {
                this.Latency = 50;
                Log.Warning("Latency is set too low, setting it to 50");
            }
            else if (this.Latency > 500)
            {
                this.Latency = 500;
                Log.Warning("Latency is set too high, setting it to 500");
            }

            this.Hostbots = setting.Hostbots;
        }
        catch (Exception)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(new Settings()));
        }
    }

    /// <summary>
    /// Gets or sets data path.
    /// </summary>
    public string DataPath { get; set; } = ".\\data";

    /// <summary>
    /// Gets or sets chat command delimiter.
    /// </summary>
    public char CommandDelimiter { get; set; } = '/';

    /// <summary>
    /// Gets or sets warcraft version.
    /// </summary>
    public byte WarcraftVersion { get; set; } = 30;

    /// <summary>
    /// Gets or sets map download speed limit in KB/s.
    /// 0 means no limit on download speed.
    /// -1 means map download is disabled.
    /// </summary>
    public short DownloadLimit { get; set; } = 800;

    /// <summary>
    /// Gets or sets game latency.
    /// Can be changed with "latency" command.
    /// </summary>
    public ushort Latency { get; set; } = 80;

    /// <summary>
    /// Gets or sets a value indicating whether broadcast game info to lan or not.
    /// </summary>
    /// <remarks>
    /// If you set this to false you'll need to manually send the game info message to your clients.
    /// </remarks>
    public bool BroadcastLan { get; set; } = true;

    /// <summary>
    /// Gets or sets hostbot(s) configuration.
    /// </summary>
    public List<HostbotConfiguration> Hostbots { get; set; } = new () { new () };

    /// <summary>
    /// Gets current settings instance.
    /// </summary>
    internal static Settings? Current { get; private set; }
}
