using System;
using UnityEngine;

namespace SuyuRun
{
    [Serializable]
    public sealed class CostaMusicSection
    {
        public string nombre;
        public float inicio, fin;
        public string energia;
        public string uso;
    }

    [Serializable]
    public sealed class CostaMusicSeries
    {
        public float paso_s;
        public float[] valores;
    }

    // Parsed form of the user-provided Assets/costa_music_map.json (an automatic, low-confidence
    // energy/section analysis of a MIDI transcription of El Alcatraz - see its own "nota" field).
    // The source file is never edited; a byte-identical copy is staged into a Resources folder by
    // CostaMusicMapSetup so it can be loaded at runtime. Drives the animated sea's tide in
    // RunnerWater.cs.
    [Serializable]
    public sealed class CostaMusicMap
    {
        public string archivo;
        public float duracion_s;
        public string nota;
        public float bpm_candidato;
        public string bpm_confianza;
        public CostaMusicSection[] secciones_aprox;
        public CostaMusicSeries energia;
        public CostaMusicSeries graves;

        public static CostaMusicMap Load()
        {
            var text = Resources.Load<TextAsset>("costa_music_map");
            if (!text) return null;
            try { return JsonUtility.FromJson<CostaMusicMap>(text.text); }
            catch (Exception e) { Debug.LogWarning("SuyuRun: costa_music_map.json failed to parse - " + e.Message); return null; }
        }

        static float SampleSeries(CostaMusicSeries series, float seconds)
        {
            if (series?.valores == null || series.valores.Length == 0 || series.paso_s <= 0) return 0f;
            float index = seconds / series.paso_s;
            int lo = Mathf.Clamp(Mathf.FloorToInt(index), 0, series.valores.Length - 1);
            int hi = Mathf.Clamp(lo + 1, 0, series.valores.Length - 1);
            return Mathf.Lerp(series.valores[lo], series.valores[hi], Mathf.Clamp01(index - lo));
        }

        // The mp3 actually used in-game (~177.4s) runs slightly longer than the MIDI transcription
        // this map was analyzed from (duracion_s ~174.9s); seconds beyond the recorded series hold
        // the last sampled value rather than extrapolating past real data.
        public float EnergyAt(float seconds) => SampleSeries(energia, Mathf.Max(0, seconds));
        public float BassAt(float seconds) => SampleSeries(graves, Mathf.Max(0, seconds));

        public CostaMusicSection SectionAt(float seconds)
        {
            if (secciones_aprox == null || secciones_aprox.Length == 0) return null;
            foreach (var section in secciones_aprox)
                if (seconds >= section.inicio && seconds < section.fin) return section;
            return seconds < secciones_aprox[0].inicio ? secciones_aprox[0] : secciones_aprox[secciones_aprox.Length - 1];
        }
    }
}
