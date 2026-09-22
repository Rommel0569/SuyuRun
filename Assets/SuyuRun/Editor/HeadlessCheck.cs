using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SuyuRun.Editor
{
    // Verifies Prototype_Costa.unity actually compiles and runs, without building a Player EXE
    // (the user explicitly asked not to generate another one this round) and without needing the
    // Editor GUI open by a human. Drives the SAME public AutomatedSmokeTest/CurrentState/
    // SmokeReport API the standalone-build smoke tests already use (RunnerPrototype.cs), just from
    // an EditorApplication.update callback instead of a built Player's command-line args - Play
    // mode's Application.Quit() is a no-op in the Editor, so this script has to own the stop/exit
    // itself instead of waiting for the game to call it.
    public static class HeadlessCheck
    {
        const string ScenePath = "Assets/SuyuRun/Scenes/Prototype_Costa.unity";
        const string OutDir = "Artifacts/HeadlessCheck";
        static RunnerPrototype proto;
        static double startTime;
        static bool shotTakenEarly, shotTakenRunning;

        [MenuItem("SuyuRun/QA/Chequeo headless de Prototype_Costa")]
        public static void Run()
        {
            Directory.CreateDirectory(OutDir);
            File.WriteAllText(Path.Combine(OutDir, "log.txt"), "start " + DateTime.Now + "\n");
            EditorSceneManager.OpenScene(ScenePath);
            startTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += Tick;
            EditorApplication.isPlaying = true;
        }

        static void Log(string s)
        {
            File.AppendAllText(Path.Combine(OutDir, "log.txt"), s + "\n");
            Debug.Log("SuyuRun HeadlessCheck: " + s);
        }

        static void Tick()
        {
            double elapsed = EditorApplication.timeSinceStartup - startTime;
            if (!EditorApplication.isPlayingOrWillChangePlaymode || !EditorApplication.isPlaying)
            {
                if (elapsed > 20) Finish("Never entered Play mode after 20s - likely a compile error.");
                return;
            }
            if (!proto)
            {
                proto = UnityEngine.Object.FindFirstObjectByType<RunnerPrototype>();
                if (!proto)
                {
                    if (elapsed > 15) Finish("RunnerPrototype not found in the running scene after 15s.");
                    return;
                }
                proto.AutomatedSmokeTest = true;
                Log("Found RunnerPrototype, AutomatedSmokeTest=true. State=" + proto.CurrentState);
            }
            if (!shotTakenEarly && proto.CurrentState == "Running" && proto.ProgressSeconds > 2f)
            {
                shotTakenEarly = true;
                ScreenCapture.CaptureScreenshot(Path.Combine(OutDir, "costa-early.png"));
                Log("Captured costa-early.png at ProgressSeconds=" + proto.ProgressSeconds);
            }
            if (!shotTakenRunning && proto.CurrentState == "Running" && proto.ProgressSeconds > 12f)
            {
                shotTakenRunning = true;
                ScreenCapture.CaptureScreenshot(Path.Combine(OutDir, "costa-running.png"));
                Log("Captured costa-running.png at ProgressSeconds=" + proto.ProgressSeconds);
            }
            if (proto.CurrentState == "Dead" || proto.CurrentState == "Complete")
            {
                Finish("Run ended: " + proto.CurrentState + "\n" + proto.SmokeReport);
                return;
            }
            if (elapsed > 200) Finish("Timed out after 200s real time without reaching Dead/Complete. Last state=" + proto.CurrentState);
        }

        static void Finish(string report)
        {
            EditorApplication.update -= Tick;
            Log("FINISHED: " + report);
            File.WriteAllText(Path.Combine(OutDir, "result.txt"), report);
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(0);
        }
    }
}
