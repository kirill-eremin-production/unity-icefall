using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;

public class WebGLBuilder
{
    [MenuItem("Build/Build WebGL")]
    public static void BuildWebGL()
    {
        BuildWebGLInternal("Build/WebGL");
    }

    public static void BuildWebGLFromCommandLine()
    {
        string[] args = Environment.GetCommandLineArgs();
        string buildPath = "Build/WebGL";
        
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-buildPath" && i + 1 < args.Length)
            {
                buildPath = args[i + 1];
                break;
            }
        }
        
        BuildWebGLInternal(buildPath);
    }

    private static void BuildWebGLInternal(string buildPath)
    {
        Debug.Log($"Starting WebGL build to: {buildPath}");
        
        // Настройки сборки
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/SampleScene.unity" };
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.WebGL;
        buildPlayerOptions.options = BuildOptions.None;

        // Применяем оптимальные настройки для WebGL
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.memorySize = 256;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
        PlayerSettings.WebGL.dataCaching = true;
        
        Debug.Log("Building WebGL player...");
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {summary.totalSize} bytes");
            Debug.Log($"Build location: {buildPath}");
            EditorApplication.Exit(0);
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build failed");
            EditorApplication.Exit(1);
        }
    }
}