using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace SuyuRun.Editor
{
    public static class HudLayoutAudit
    {
        [MenuItem("SuyuRun/QA/Verificar alineación HUD")]
        public static void Run()
        {
            var canvas=new Rect(0,0,320,180);
            for(int i=0;i<4;i++)
            {
                var slot=RunnerHud.PieceSlot(i);var icon=RunnerHud.PieceIcon(i);
                if(!canvas.Contains(slot.min)||!canvas.Contains(slot.max)||slot.Overlaps(RunnerHud.Message))throw new Exception("HUD slot out of bounds or overlaps message");
                if(!slot.Contains(icon.min)||!slot.Contains(icon.max))throw new Exception("Museum icon outside frame");
                for(int j=i+1;j<4;j++)if(slot.Overlaps(RunnerHud.PieceSlot(j)))throw new Exception("HUD frames overlap");
            }
            File.WriteAllText("Artifacts/VisualRefactor/hud_layout_audit.txt","PASS four aligned frames, inset icons, screen bounds and separate message zone.\nVisual Play review remains separate.\n");
        }
    }
}
