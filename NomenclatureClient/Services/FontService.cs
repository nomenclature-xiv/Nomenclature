using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Plugin;
using Microsoft.Extensions.Hosting;

namespace NomenclatureClient.Services;

public class FontService(IDalamudPluginInterface pluginInterface) : IHostedService
{
    private const int BigFontSize = 36;
    public static IFontHandle? BigFont;

    private const int MediumFontSize = 24;
    public static IFontHandle? MediumFont;
    
    /// <summary>
    ///     Initializes the two additional font sizes used in the plugin
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        MediumFont = pluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(toolkit =>
        {
            toolkit.OnPreBuild(preBuild => { preBuild.AddDalamudDefaultFont(MediumFontSize); });
        });
        
        BigFont = pluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(toolkit =>
        {
            toolkit.OnPreBuild(preBuild => { preBuild.AddDalamudDefaultFont(BigFontSize); });
        });
        
        await MediumFont.WaitAsync().ConfigureAwait(false);
        await BigFont.WaitAsync().ConfigureAwait(false);
        await pluginInterface.UiBuilder.FontAtlas.BuildFontsAsync().ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        MediumFont?.Dispose();
        BigFont?.Dispose();
        return Task.CompletedTask;
    }
}