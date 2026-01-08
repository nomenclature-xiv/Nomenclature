using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NomenclatureClient.Managers;
using NomenclatureClient.Types.Configurations;

namespace NomenclatureClient.Services;

// ReSharper disable RedundantBoolCompare

/// <summary>
///     Contains and exposes methods to handling the various configuration objects for the plugin
/// </summary>
public class ConfigurationService : IHostedService
{
    // Base Configuration Path
    private readonly string _baseConfigurationFolderPath;
    
    // General Configuration
    private const string ConfigurationFileName = "Configuration.json";
    private readonly string _configurationFilePath;
    
    // Characters
    private const string CharactersFolderName = "Characters";
    private readonly string _charactersFolderPath;
    
    // Injected
    private readonly IPluginLog _pluginLog;
    
    /// <summary>
    ///     Plugin configuration
    /// </summary>
    public ConfigurationV2 Configuration { get; private set; } = new();
    
    /// <summary>
    ///     The current configuration for the logged in character, loaded on login by <see cref="LoadCharacterConfigurationAsync"/> by <see cref="LoginManager"/>
    /// </summary>
    public CharacterConfigurationV2? CharacterConfiguration { get; private set; }
    
    /// <summary>
    ///     <inheritdoc cref="ConfigurationService"/>
    /// </summary>
    public ConfigurationService(IDalamudPluginInterface dalamudPluginInterface, IPluginLog pluginLog)
    {
        _baseConfigurationFolderPath = dalamudPluginInterface.GetPluginConfigDirectory();
        _configurationFilePath = Path.Combine(_baseConfigurationFolderPath, ConfigurationFileName);
        _charactersFolderPath = Path.Combine(_baseConfigurationFolderPath, CharactersFolderName);
        
        _pluginLog = pluginLog;
    }

    /// <summary>
    ///     Load the configuration into this service instance
    /// </summary>
    public async Task LoadConfigurationAsync()
    {
        try
        {
            if (File.Exists(_configurationFilePath))
            {
                // Read & parse existing configuration file
                var raw = await File.ReadAllTextAsync(_configurationFilePath).ConfigureAwait(false);
                var json = JObject.Parse(raw);
                var version = json["Version"]?.Value<int>();
                switch (version)
                {
                    // Current
                    case ConfigurationV2.ExpectedVersion:
                        Configuration = json.ToObject<ConfigurationV2>() ?? new ConfigurationV2();
                        _pluginLog.Verbose($"[ConfigurationService.LoadConfigurationAsync] Loaded configuration {json}");
                        break;
                
                    // Update when / if new version of ConfigurationV2 are created
                    default:
                        Configuration = new ConfigurationV2();
                        _pluginLog.Warning($"[ConfigurationService.LoadConfigurationAsync] Unsupported version {version.ToString() ?? "null"}");
                        break;
                }
            }
            else
            {
                // New instance of configuration to flush out any previous / bad data
                Configuration = new ConfigurationV2();
            
                // Write
                await SaveConfigurationAsync().ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            _pluginLog.Error($"[ConfigurationService.LoadConfigurationAsync] {e}");
        }
    }

    /// <summary>
    ///     Saves the configuration in this service instance to the disk
    /// </summary>
    public async Task SaveConfigurationAsync()
    {
        try
        {
            // Create directory if not exists
            if (Directory.Exists(_baseConfigurationFolderPath) is false)
                Directory.CreateDirectory(_baseConfigurationFolderPath);
            
            // Convert to JSON
            var json = JsonConvert.SerializeObject(Configuration, Formatting.Indented);
            _pluginLog.Verbose($"[ConfigurationService.SaveConfigurationAsync] Saving configuration {json}");
            
            // Write to disk
            await File.WriteAllTextAsync(_configurationFilePath, json).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _pluginLog.Error($"[ConfigurationService.SaveConfigurationAsync] {e}");
        }
    }

    /// <summary>
    ///     Load a character configuration into this service instance
    /// </summary>
    public async Task<bool> LoadCharacterConfigurationAsync(string name, string world)
    {
        try
        {
            // Parse character filename
            var filename = string.Concat(name, " - ", world, ".json");
            var path = Path.Combine(_charactersFolderPath, filename);

            if (File.Exists(path))
            {
                var raw = await File.ReadAllTextAsync(path).ConfigureAwait(false);
                var json = JObject.Parse(raw);
                var version = json["Version"]?.Value<int>();
                switch (version)
                {
                    case CharacterConfigurationV2.ExpectedVersion:
                        CharacterConfiguration = json.ToObject<CharacterConfigurationV2>() ?? new CharacterConfigurationV2();
                        _pluginLog.Verbose($"[ConfigurationService.LoadCharacterConfigurationAsync] Loaded character configuration {json}");
                        return true;
                    
                    // Add new cases as needed
                    default:
                        CharacterConfiguration = null;
                        _pluginLog.Warning($"[ConfigurationService.LoadCharacterConfigurationAsync] Unsupported version {version.ToString() ?? "null"}");
                        return false;
                }
            }
            // New instance of character configuration
            CharacterConfiguration = new CharacterConfigurationV2 { Name = name, World = world };
            
            // Write
            await SaveCharacterConfigurationAsync().ConfigureAwait(false);
            return true;
        }
        catch (Exception e)
        {
            _pluginLog.Error($"[ConfigurationService.LoadCharacterConfigurationAsync] {e}");
            return false;
        }
    }

    public async Task SaveCharacterConfigurationAsync()
    {
        try
        {
            // Only save non-null configurations
            if (CharacterConfiguration is null)
                return;

            // Parse character filename
            var name = string.Concat(CharacterConfiguration.Name, " - ", CharacterConfiguration.World, ".json");
            var path = Path.Combine(_charactersFolderPath, name);
            
            // Create directory if not exists
            if (Directory.Exists(_charactersFolderPath) is false)
                Directory.CreateDirectory(_charactersFolderPath);
            
            // Convert to JSON
            var json = JsonConvert.SerializeObject(CharacterConfiguration, Formatting.Indented);
            _pluginLog.Verbose($"[ConfigurationService.SaveCharacterConfigurationAsync] Saving character configuration {json}");
        
            // Write to disk
            await File.WriteAllTextAsync(path, json).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _pluginLog.Error($"[ConfigurationService.SaveCharacterConfigurationAsync] {e}");
        }
    }

    /// <summary>
    ///     Sets the current character configuration to null
    /// </summary>
    public void ResetCharacterConfiguration()
    {
        CharacterConfiguration = null;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}