using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SuyuRun.Editor
{
    // The user provides Assets/costa_music_map.json directly and asked that its content never be
    // changed. This only stages a byte-identical copy into a Resources folder (so it is loadable
    // at runtime via Resources.Load<TextAsset>, the same convention the rest of Costa's data
    // uses) and writes a validation report to Artifacts/ so the sync can be checked without
    // trusting the parser blindly.
    public static class CostaMusicMapSetup
    {
        const string SourcePath = "Assets/costa_music_map.json";
        const string StagedPath = "Assets/SuyuRun/Data/Resources/costa_music_map.json";

        [MenuItem("SuyuRun/Audio/Sincronizar mapa musical de Costa")]
        public static void Sync()
        {
            if (!File.Exists(SourcePath))
                throw new System.Exception($"{SourcePath} not found. Place the user-provided music map there first.");
            var sourceBytes = File.ReadAllBytes(SourcePath);
            if (!File.Exists(StagedPath) || !FilesAreIdentical(sourceBytes, File.ReadAllBytes(StagedPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(StagedPath));
                File.WriteAllBytes(StagedPath, sourceBytes);
                AssetDatabase.ImportAsset(StagedPath, ImportAssetOptions.ForceUpdate);
            }
            AssetDatabase.Refresh();

            var map = JsonUtility.FromJson<CostaMusicMap>(Encoding.UTF8.GetString(sourceBytes));
            if (map == null) throw new System.Exception("costa_music_map.json failed to parse against CostaMusicMap's schema.");

            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/SuyuRun/Audio/El Alcatraz.mp3");
            var report = new StringBuilder();
            report.AppendLine("SuyuRun: costa_music_map.json validation (source file unmodified)");
            report.AppendLine($"archivo={map.archivo} bpm_candidato={map.bpm_candidato} bpm_confianza={map.bpm_confianza}");
            report.AppendLine($"duracion_s (mapa)={map.duracion_s:F2} vs longitud real El Alcatraz.mp3={(clip ? clip.length.ToString("F2") : "no importado")}");
            if (clip) report.AppendLine($"diferencia={(clip.length - map.duracion_s):F2}s - el mapa viene de una transcripcion MIDI, no del mp3; se sostiene el ultimo valor de la serie mas alla de duracion_s.");
            report.AppendLine($"secciones_aprox: {map.secciones_aprox?.Length ?? 0}");
            foreach (var section in map.secciones_aprox ?? System.Array.Empty<CostaMusicSection>())
                report.AppendLine($"  {section.nombre,-10} [{section.inicio,6:F1}s .. {section.fin,6:F1}s) energia={section.energia} uso={section.uso}");
            report.AppendLine($"energia: paso_s={map.energia?.paso_s} muestras={map.energia?.valores?.Length ?? 0}");
            report.AppendLine($"graves:  paso_s={map.graves?.paso_s} muestras={map.graves?.valores?.Length ?? 0}");
            // Spot-check a few timestamps so a human can compare these against the raw JSON by eye.
            report.AppendLine("Muestras de verificacion (segundo -> energia, graves, seccion):");
            foreach (float t in new[] { 0f, 16f, 48f, 96f, 130f, 152f, 172f, 180f })
            {
                var section = map.SectionAt(t);
                report.AppendLine($"  t={t,6:F1}s energia={map.EnergyAt(t):F3} graves={map.BassAt(t):F3} seccion={(section != null ? section.nombre : "-")}");
            }

            Directory.CreateDirectory("Artifacts/Audio");
            File.WriteAllText("Artifacts/Audio/costa-music-map-validation.txt", report.ToString());
            Debug.Log("SuyuRun: costa_music_map.json staged and validated. See Artifacts/Audio/costa-music-map-validation.txt");
        }

        static bool FilesAreIdentical(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
            return true;
        }
    }
}
