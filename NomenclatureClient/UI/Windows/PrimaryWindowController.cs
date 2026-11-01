using System;
using System.Timers;
using NomenclatureClient.Managers;
using NomenclatureClient.Services;

namespace NomenclatureClient.UI.Windows;

public class PrimaryWindowController(ConnectionManager connectionManager, ConfigurationService configurationService)
{
    public bool IsConnected => connectionManager.IsConnected;
    public bool CharacterRegistered => configurationService.AnyCharacterRegistered;

}