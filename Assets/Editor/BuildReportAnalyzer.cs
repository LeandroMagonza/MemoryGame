/*
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public class BuildReportAnalyzer
{
    [MenuItem("Tools/Analyze Build Report")]
    public static void AnalyzeBuildReport()
    {
        var report = BuildReport.GetLatestReport();
        if (report == null)
        {
            Debug.LogError("No build report found. Please build the project first.");
            return;
        }

        var sortedFiles = report.files.OrderByDescending(f => f.size);
        
        Debug.Log("Top 10 largest files in build:");
        foreach (var file in sortedFiles.Take(10))
        {
            Debug.Log($"{file.path}: {file.size / 1024f / 1024f:F2} MB");
        }
    }
}
#endif
*/