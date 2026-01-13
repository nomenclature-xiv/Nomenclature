using NomenclatureClient.Services;

namespace NomenclatureClient.UI.Views.Register;

public class RegisterViewController(ConfigurationService configuration)
{
    public string SecretIdentifier = string.Empty;
    public string SecretKey = string.Empty;

    public void AddTemporary()
    {
        if (SecretIdentifier == string.Empty || SecretKey == string.Empty)
            return;
        
        configuration.Configuration.Secrets.TryAdd(SecretIdentifier, SecretKey);
        
        SecretIdentifier = string.Empty;
        SecretKey = string.Empty;
    }
}