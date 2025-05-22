using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using NomenclatureClient.Ipc;

namespace NomenclatureClient.Debug
{
    public class IpcTester
    {
        private readonly IDalamudPluginInterface _pluginInterface;

        private readonly ICallGateSubscriber<string, ushort, object?> _setNomenclature;
        private readonly ICallGateSubscriber<string> _getNomenclature;

        private const string SetNomenclature = "Nomenclature.SetNomenclature";
        private const string GetNomenclature = "Nomenclature.GetNomenclature";
        public IpcTester(IDalamudPluginInterface pluginInterface)
        {
            _pluginInterface = pluginInterface;

            _setNomenclature = _pluginInterface.GetIpcSubscriber<string, ushort, object?>(SetNomenclature);
            _getNomenclature = _pluginInterface.GetIpcSubscriber<string>(GetNomenclature);
        }

        public string IpcGetNomenclature()
        {
            return _getNomenclature.InvokeFunc();
        }
        public void IpcSetNomenclature(string nomenclature, ushort? flags = null)
        {
            if(flags is null)
                flags = 0;

            _setNomenclature.InvokeAction(nomenclature, flags.Value);
        }
    }
}
