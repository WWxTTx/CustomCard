using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using HybridCLR.Editor;
using UnityEngine;
using System.Collections.Generic;

public static class HybridCLRPostprocessor
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (SettingsUtil.Enable)
        {
            // 确保目标目录存在
            string targetDir = "Assets/Game/Aots";
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            string aotSourceDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(EditorUserBuildSettings.activeBuildTarget);
            ProcessDlls(aotSourceDir, SettingsUtil.AOTAssemblyNames, targetDir);

            targetDir = "Assets/Game/Dlls";
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            string hotUpdateSourceDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget);
            ProcessDlls(hotUpdateSourceDir, SettingsUtil.HotUpdateAssemblyFilesIncludePreserved, targetDir);

            AssetDatabase.Refresh();
        }
    }

    private static void ProcessDlls(string sourcePath, List<string> dllFiles, string targetDir)
    {
        if (!Directory.Exists(sourcePath))
        {
            Debug.LogWarning($"源目录不存在: {sourcePath}");
            return;
        }

        foreach (var dllFileName in dllFiles)
        {
            // 确保文件名有.dll扩展名
            string fileNameWithExtension = dllFileName;
            if (!dllFileName.EndsWith(".dll"))
            {
                fileNameWithExtension = dllFileName + ".dll";
            }

            string sourceFilePath = Path.Combine(sourcePath, fileNameWithExtension);

            if (File.Exists(sourceFilePath))
            {
                // 目标文件名保持原样或添加.byte扩展名（根据需要选择）
                string targetFileName = $"{dllFileName}.byte"; // 或者直接用 fileNameWithExtension
                string targetPath = Path.Combine(targetDir, targetFileName);

                File.Copy(sourceFilePath, targetPath, true);
                Debug.Log($"已复制DLL: {dllFileName} 到 {targetPath}");
            }
            else
            {
                Debug.LogWarning($"未找到DLL文件: {sourceFilePath}");
            }
        }
    }
}
