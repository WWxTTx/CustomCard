using GameFramework;
using System.IO;
using UnityEngine;

/// <summary>
/// 路径相关的工具类
/// </summary>
public static class AssetsPath
{
    public static string GetCombinePath(params string[] args)
    {
        return Utility.Path.GetRegularPath(System.IO.Path.Combine(args));
    }

    public static readonly string PrefabsPath = "Assets/Game/Prefabs";
    public static readonly string ScenePath = "Assets/Game/Scene";
    public const string HotfixAssembly = "Assets/Game/Scripts/Hotfix.asmdef";

    public const string DataTableCodeTemplate = "Assets/GameFrameworkUnity/Editor/DataTableGenerator/DataTableCodeTemplate/DataTableCodeTemplate.txt"; //生成配置表代码的模板文件
    public const string BuiltinAssembly = "Assets/GameFrameworkUnity/Runtime/UnityGameFramework.Runtime.asmdef";
   
    public const string SharedAssetBundleName = "SharedAssets";//AssetBundle分包共用资源
    public static string AssetBundleOutputPath =>AssetsPath.GetCombinePath(Directory.GetParent(Application.dataPath).FullName, "AB");
}

