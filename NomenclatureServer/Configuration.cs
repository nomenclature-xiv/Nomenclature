using System.Net;
using System.Security.Cryptography;
using System.Text.Json;

namespace NomenclatureServer;

public class Configuration
{
    public const int Port = 5006;
    private const string DefaultValue = "DEFAULT_VALUE";
    public static readonly IPAddress Ip = IPAddress.Parse("127.0.0.1");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public readonly string SigningKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    public readonly string CertificatePath;
    public readonly string CertificatePasswordPath;

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
                CertificatePath = "DEFAULT",
                CertificatePasswordPath = "DEFAULT",
            };

            var defaultContent = JsonSerializer.Serialize(defaultConfig, JsonOptions);
            File.WriteAllText(filePath, defaultContent);
        }

        var json = File.ReadAllText(filePath);
        if (JsonSerializer.Deserialize<ConfigurationData>(json) is not { } config)
            throw new InvalidOperationException("Failed to deserialize server configuration data");
        
        if (config.CertificatePath is DefaultValue || config.CertificatePasswordPath is DefaultValue)
            throw new InvalidOperationException("Configuration values must be set before running the server");
        
        CertificatePath = config.CertificatePath;
        CertificatePasswordPath = config.CertificatePasswordPath;
    }
    
    private class ConfigurationData
    {
        public string CertificatePath { get; init; } = string.Empty;
        public string CertificatePasswordPath { get; init; } = string.Empty;
    }
}