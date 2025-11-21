using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using NomenclatureClient.Ipc;
using System.Numerics;

namespace NomenclatureClient.Debug
{
    public class IpcWindow : Window
    {
        private readonly IpcTester _tester;

        private string getnom = "";
        private string setnom1 = "";
        private string setnom2 = "";

        public IpcWindow(IpcTester tester) : base("Debug")
        {
            _tester = tester;
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(400, 400),
                MaximumSize = ImGui.GetIO().DisplaySize
            };
        }

        public override void Draw()
        {
            if(ImGui.Button("GetNom"))
            {
                getnom = _tester.IpcGetNomenclature();
            }
            ImGui.SameLine();
            ImGui.Text(getnom);
            if(ImGui.Button("SetNom1"))
            {
                _tester.IpcSetNomenclature(setnom1);
            }
            ImGui.SameLine();
            ImGui.InputText("##setnom1", ref setnom1, 64);
            if(ImGui.Button("SetNom2"))
            {
                _tester.IpcSetNomenclature(setnom2, (ushort)NomenclatureSetFlag.Lock);
            }
            ImGui.SameLine();
            ImGui.InputText("##setnom2", ref setnom2, 64);
            ImGui.Text(_tester.ChangedMessage);
        }
    }
}
