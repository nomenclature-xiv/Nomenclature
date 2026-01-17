using NomenclatureClient.Services;

namespace NomenclatureClient.UI.Views.Manage;

public class ManageViewController(ConfigurationService configuration)
{
    public bool ShowSecrets = false;

    public void DeleteSecret(string identifier)
    {
        configuration.Configuration.Secrets.Remove(identifier);
    }
}