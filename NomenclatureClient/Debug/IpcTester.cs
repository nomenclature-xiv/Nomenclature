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
        public string ChangedMessage;
        private readonly IDalamudPluginInterface _pluginInterface;

        private readonly ICallGateSubscriber<string, uint, object?> _setNomenclature;
        private readonly ICallGateSubscriber<string> _getNomenclature;
        private readonly ICallGateSubscriber<string, object> _nomenclatureChanged;

        private const string SetNomenclatureSelf = "Nomenclature.SetNomenclatureSelf";
        private const string GetNomenclature = "Nomenclature.GetNomenclature";
        private const string NomenclatureChanged = "Nomenclature.NomenclatureChanged";
        public IpcTester(IDalamudPluginInterface pluginInterface)
        {
            _pluginInterface = pluginInterface;

            _setNomenclature = _pluginInterface.GetIpcSubscriber<string, uint, object?>(SetNomenclatureSelf);
            _getNomenclature = _pluginInterface.GetIpcSubscriber<string>(GetNomenclature);
            _nomenclatureChanged = _pluginInterface.GetIpcSubscriber<string, object>(NomenclatureChanged);
            _nomenclatureChanged.Subscribe((message) =>
            {
                ChangedMessage = message;
            });
        }

        public string IpcGetNomenclature()
        {
            return _getNomenclature.InvokeFunc();
        }
        public void IpcSetNomenclature(string nomenclature, uint? flags = null)
        {
            if(flags is null)
                flags = 3;

            _setNomenclature.InvokeAction(nomenclature, flags.Value);
        }
        
    }
}
