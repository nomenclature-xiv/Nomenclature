using System.Net;
using System.Security.Cryptography;
using System.Text.Json;

namespace NomenclatureServer;

public class Configuration
{
    public const int Port = 5007;
    private const string DefaultValue = "DEFAULT_VALUE";
    public static readonly IPAddress Ip = IPAddress.Parse("192.168.1.14");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public readonly string SigningKey;
    public readonly string CertificatePath;
    public readonly string CertificatePasswordPath;
    public readonly string ClientSecret;

    public Configuration()
    {
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "config");
        var filePath = Path.Combine(directoryPath, "database.config");

        if (Directory.Exists(directoryPath) is false)
            Directory.CreateDirectory(directoryPath);

        if (File.Exists(filePath) is false)
        {
            var defaultConfig = new ConfigurationData
            {
                CertificatePath = DefaultValue,
                CertificatePasswordPath = DefaultValue,
                SigningKey = DefaultValue,
                ClientSecret = DefaultValue
            };

            var defaultContent = JsonSerializer.Serialize(defaultConfig, JsonOptions);
            File.WriteAllText(filePath, defaultContent);
        }

        var json = File.ReadAllText(filePath);
        if (JsonSerializer.Deserialize<ConfigurationData>(json) is not { } config)
            throw new InvalidOperationException("Failed to deserialize server configuration data");
        
        if (config.CertificatePath is DefaultValue || config.CertificatePasswordPath is DefaultValue || config.SigningKey is DefaultValue)
            throw new InvalidOperationException("Configuration values must be set before running the server");
        
        CertificatePath = config.CertificatePath;
        CertificatePasswordPath = config.CertificatePasswordPath;
        SigningKey = config.SigningKey;
        ClientSecret = config.ClientSecret;

    }
    
    private class ConfigurationData
    {
        public string SigningKey { get; init; } = string.Empty;
        public string CertificatePath { get; init; } = string.Empty;
        public string CertificatePasswordPath { get; init; } = string.Empty;
        public string ClientSecret {  get; init; } = string.Empty;
    }
}