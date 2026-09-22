using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SuyuRun.Editor
{
    // ART_BIBLE.md section 14: replace the hand-authored, repeated-template obstacle beats in
    // CostaAlcatraz.asset with real timing from the user's MIDI, per their explicit follow-up
    // request - "que tenga ritmo y que acompañe a las canciones", NOT an obstacle for every
    // single transcribed note. Tools/analyze_costa_midi.py already filtered ~12k dense
    // auto-transcribed notes (see its own docstring - this MIDI has no clean drum/bass
    // separation) down to a well-spaced, intensity-classified hit list; this script only
    // consumes that JSON and writes it into the course asset. Run
    // Tools/analyze_costa_midi.py again first if you want to retune the filter.
    public static class ApplyMidiRhythm
    {
        [Serializable] class RhythmHit { public float t; public float weight; public string kind; }
        [Serializable] class RhythmData { public string source; public float duration; public RhythmHit[] hits; }

        [MenuItem("SuyuRun/Audio/Aplicar ritmo MIDI a Costa (Alcatraz)")]
        public static void ApplyToCosta()
        {
            const string coursePath = "Assets/SuyuRun/Data/Resources/CostaAlcatraz.asset";
            const string jsonPath = "Artifacts/Audio/costa_rhythm.json";
            var course = AssetDatabase.LoadAssetAtPath<RunnerCourse>(coursePath);
            if (!course) throw new Exception("No se encontro " + coursePath);
            if (!File.Exists(jsonPath)) throw new Exception("Falta " + jsonPath + " - corre Tools/analyze_costa_midi.py primero.");
            var data = JsonUtility.FromJson<RhythmData>(File.ReadAllText(jsonPath));

            var encounters = new List<Encounter>();
            int n = 0;
            foreach (var hit in data.hits)
            {
                if (hit.kind != "ground") continue; // "air" hits are the song's lighter accents; the
                // existing coin-filler loop in RunnerPrototype.BuildCourse already scatters coins
                // along the floor independently, so they are not turned into encounters here.
                float beat = course.SecondsToBeat(hit.t);
                float seconds = course.BeatToSeconds(beat);
                if (seconds <= 2f || seconds >= course.duration - 2f) continue;
                float x = seconds * course.speed;
                bool inGap = false;
                foreach (var gap in course.gaps) if (x > gap.x - 1.5f && x < gap.x + gap.width + 1.5f) inGap = true;
                if (inGap) continue; // an obstacle floating over open water reads as a rendering bug, not rhythm.
                n++;
                // Every 7th ground accent is a Breakable crate instead of Jump/Slide - NOT for
                // variety, but because RunnerCoastalCombat.BuildCoastalEncounters only spawns the
                // gallinazo birds AND the museum-piece memory drops from Breakable encounters. A
                // rhythm-only Jump/Slide mix (my first pass) would have silently zeroed out both
                // systems - the user explicitly wants the existing birds kept. ~9 crates across the
                // course matches the previous template's density.
                var kind = n % 7 == 0 ? EncounterKind.Breakable : n % 4 == 0 ? EncounterKind.Slide : EncounterKind.Jump;
                encounters.Add(new Encounter { beat = beat, kind = kind, count = 1 });
            }
            encounters.Sort((a, b) => a.beat.CompareTo(b.beat));
            var deduped = new List<Encounter>();
            float previousBeat = -1;
            foreach (var e in encounters) { if (e.beat <= previousBeat + .02f) continue; deduped.Add(e); previousBeat = e.beat; }

            course.encounters = deduped.ToArray();
            course.ValidateCourse();
            EditorUtility.SetDirty(course);
            AssetDatabase.SaveAssets();

            Directory.CreateDirectory("Artifacts/Audio");
            File.WriteAllText("Artifacts/Audio/costa-rhythm-applied.txt",
                $"CostaAlcatraz.encounters reemplazado: {deduped.Count} obstaculos derivados del ritmo real de {data.source} " +
                $"(filtrado por intensidad, no nota por nota - ver Tools/analyze_costa_midi.py). " +
                $"Reemplaza el patron repetido anterior (3 copias de VerticalPrototype). Pendiente: validacion auditiva del usuario.");
            Debug.Log($"SuyuRun: {deduped.Count} obstaculos de Costa reemplazados con el ritmo de El Alcatraz (MIDI). Prueba en Play y ajusta Tools/analyze_costa_midi.py si se siente mal.");
        }
    }
}
