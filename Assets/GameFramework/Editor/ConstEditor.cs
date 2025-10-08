#if UNITY_EDITOR
using System.IO;
using UnityEngine;
namespace GameFramework.EditorTools
{

    /// <summary>
    /// 默认编辑器配置项
    /// </summary>
    public class ConstEditor
    {
        public const bool AutoScriptUTF8 = true;//新建脚本时自动修改脚本编码方式为utf-8以支持中文
        /// <summary>
        /// 打包资源前是否自动解决AB包重复依赖
        /// </summary>
        public const bool ResolveDuplicateAssets = true;


        internal static string AssetBundleOutputPath => UtilityBuiltin.AssetsPath.GetCombinePath(Directory.GetParent(Application.dataPath).FullName, "AB");
        public static readonly string UpdatePrefixUri = "http://127.0.0.1/1_0_0_1/";//默认资源下载地址
        internal static readonly string AppUpdateUrl = "https://play.google.com/store/apps/details?id=";

        /// <summary>
        /// 数据表Excel目录
        /// </summary>
        public static string DataTableExcelPath => UtilityBuiltin.AssetsPath.GetCombinePath(Directory.GetParent(Application.dataPath).FullName, "DataTables");
        /// <summary>
        /// 生成配置表代码的模板文件
        /// </summary>
        public const string DataTableCodeTemplate = "Assets/GameFramework/Editor/DataTableCodeTemplate.txt";
        /// <summary>
        /// 外部工具目录
        /// </summary>
        public static string ToolsPath = UtilityBuiltin.AssetsPath.GetCombinePath(Directory.GetParent(Application.dataPath).FullName, "Tools");

        //游戏设置 配置表 外部XLSX路径
        public const string DataTablePath = "Assets/Game/Tables";
        public const string GameConfigPath = "Assets/Game/Tables";
        //代码自动生成目录
        public const string DataTableCodePath = "Assets/Game/Scripts/DataTable";

        public const string ENABLE_HYBRIDCLR = "ENABLE_HYBRIDCLR";
        public const string ENABLE_OBFUZ = "ENABLE_OBFUZ";
        public const string Const_Editor = "Assets/GameFramework/Editor/ConstEditor.cs";
        /// <summary>
        /// DataTable,Config都支持AB测试,文件分为主文件和AB测试文件, AB测试文件名以'#'+ AB测试组名字结尾
        /// </summary>
        public const char AB_TEST_TAG = '#';
    }
}
#endif